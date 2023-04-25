using System;
using System.Runtime.CompilerServices;
using SmolScript.Internals;
using SmolScript.Internals.Ast;

[assembly: InternalsVisibleTo("SmolScript.Tests.Internal")]

namespace SmolScript
{
    public class SmolVM : ISmolRuntime
    {
        enum RunMode
        {
            Run,
            Paused,
            Step
        }

        internal SmolProgram program;


        private int _MaxStackSize = int.MaxValue;

        public int MaxStackSize
        {
            get
            {
                return _MaxStackSize;
            }
            set
            {
                _MaxStackSize = value;
            }
        }

        int code_section = 0;
        int PC = 0; // Program Counter / Instruction Pointer

        bool debug = false;

        RunMode runMode = RunMode.Paused;

        // This is a stack based VM, so this is for our working data.
        // We store everything here as SmolValue, which is a wrapper
        // for all of our types
        internal Stack<SmolValue> stack = new Stack<SmolValue>();

        Dictionary<int, int> jmplocs = new Dictionary<int, int>();

        internal readonly SmolScript.Internals.Environment globalEnv = new SmolScript.Internals.Environment();
        internal SmolScript.Internals.Environment environment;

        public T GetGlobalVar<T>(string variableName)
        {
            var v = (SmolValue)globalEnv.Get(variableName)!;
            return (T)Convert.ChangeType(v.value!, typeof(T));
        }

        public void Call(string functionName, params object[] args)        
        {
            Call<object>(functionName, args);
        }

        public T Call<T>(string functionName, params object[] args)
        {
            var state = new SmolCallSaveState()
            {
                code_section = this.code_section,
                PC = this.PC,
                previous_env = this.environment,
                call_is_extern = true
            };

            var env = new SmolScript.Internals.Environment(this.globalEnv);
            this.environment = env;

            var fn = program.function_table.First(f => f.globalFunctionName == functionName);

        
            // Prime the new environment with variables for
            // the parameters in the function declaration (actual number
            // passed might be different)

            for (int i = 0; i < fn.arity; i++)
            {
                if (args.Count() > i)
                {
                    env.Define(fn.param_variable_names[i], new SmolValue(args[i]));
                }
                else
                {
                    env.Define(fn.param_variable_names[i], new SmolValue()
                    {
                        type = SmolValueType.Undefined
                    });
                }
            }


            stack.Push(new SmolValue()
            {
                type = SmolValueType.SavedCallState,
                value = state
            });

            PC = 0;
            code_section = fn.code_section;

            Run();

            return (T)Convert.ChangeType(stack.Pop().value!, typeof(T));
        }


        public static ISmolRuntime Compile(string source)
        {
            return new SmolVM(source);
        }

        internal string Decompile()
        {
            return ByteCodeDisassembler.Disassemble(this.program);
        }

        internal string DumpAst()
        {
            return new AstDump().Print(program.astStatements)!;
        }


        public SmolVM(string source)
        {
            environment = globalEnv;

            this.program = SmolCompiler.Compile(source);

            BuildJumpTable();
        }

        internal SmolVM(SmolProgram program)
        {
            environment = globalEnv;

            this.program = program;

            BuildJumpTable();
        }

        private void BuildJumpTable()
        {
            // Loop through all labels in all code sections, capturing
            // the label number (always unique) and the location/index
            // in the instructions for that section so we can jump
            // if we need to.

            for (int i = 0; i < this.program.code_sections.Count; i++)
            {
                // Not sure if this will hold up, might be too simplistic

                for (int j = 0; j < this.program.code_sections[i].Count; j++)
                {
                    var instr = this.program.code_sections[i][j];

                    if (instr.opcode == OpCode.LABEL)
                    {
                        // We're not storing anything about the section
                        // number but this should be ok becuase we should
                        // only ever jump inside the current section...
                        // Jumps to other sections are handled in a different
                        // way using the CALL instruction
                        jmplocs[(int)instr.operand1!] = j;
                    }
                }
            }
        }

        public void Step()
        {
            _Run(RunMode.Step);
        }

        public void Run()
        {
            _Run(RunMode.Run);
        }

        public void RunInDebug()
        {
            debug = true;
            _Run(RunMode.Run);
        }

        void _Run(RunMode newRunMode)
        {
            this.runMode = newRunMode;

            double t = System.Environment.TickCount;

            while (true)
            {
                var instr = program.code_sections[code_section][PC++];

                if (debug)
                {
                    Console.WriteLine($"{instr}");//: {System.Environment.TickCount - t}");

                    t = System.Environment.TickCount;
                }

                try
                {
                    switch (instr.opcode)
                    {
                        case OpCode.NOP:
                            // Just skip over this instruction, no-op
                            break;

                        case OpCode.CONST:
                            // Load a value from the data section at specified index
                            // and place it on the stack
                            stack.Push(program.constants[(int)instr.operand1!]);

                            if (debug)
                            {
                                Console.WriteLine($"              [Pushed ${program.constants[(int)instr.operand1!]}]");
                            }

                            break;

                        case OpCode.CALL:
                            {
                                var callData = (SmolFunctionDefn)stack.Pop().value!;

                                // First create the env for our function

                                var env = new SmolScript.Internals.Environment(this.globalEnv);

                                if ((bool)instr.operand2!)
                                {
                                    // If op2 is true, that means we're calling a method
                                    // on an object/class, so we need to get the objref
                                    // (from the next value on the stack) and use that
                                    // objects environment instead.

                                    env = ((SmolObjectRef)stack.Pop().value!).object_env;
                                }

                                // Next pop args off the stack. Op1 is number of args.                    

                                List<SmolValue> paramValues = new List<SmolValue>();

                                for (int i = 0; i < (int)instr.operand1!; i++)
                                {
                                    paramValues.Add(this.stack.Pop());
                                }

                                // Now prime the new environment with variables for
                                // the parameters in the function declaration (actual number
                                // passed might be different)

                                for (int i = 0; i < callData.arity; i++)
                                {
                                    if (paramValues.Count > i)
                                    {
                                        env.Define(callData.param_variable_names[i], paramValues[i]);
                                    }
                                    else
                                    {
                                        env.Define(callData.param_variable_names[i], new SmolValue()
                                        {
                                            type = SmolValueType.Undefined
                                        });
                                    }
                                }


                                // Store our current program/vm state so we can restor

                                var state = new SmolCallSaveState()
                                {
                                    code_section = this.code_section,
                                    PC = this.PC,
                                    previous_env = this.environment,
                                };

                                // Switch the active env in the vm over to the one we prepared for the call

                                this.environment = env;

                                stack.Push(new SmolValue()
                                {
                                    type = SmolValueType.SavedCallState,
                                    value = state
                                });

                                // Finally set our PC to the start of the function we're about to execute

                                PC = 0;
                                code_section = callData.code_section;

                                break;
                            }

                        case OpCode.ADD:
                            {
                                var right = stack.Pop();
                                var left = stack.Pop();

                                stack.Push(left + right);

                                break;
                            }

                        case OpCode.SUB:
                            {
                                var right = stack.Pop();
                                var left = stack.Pop();

                                stack.Push(left - right);

                                break;
                            }

                        case OpCode.MUL:
                            {
                                var right = stack.Pop();
                                var left = stack.Pop();

                                stack.Push(left * right);

                                break;
                            }

                        case OpCode.DIV:
                            {
                                var right = stack.Pop();
                                var left = stack.Pop();

                                stack.Push(left / right);

                                break;
                            }

                        case OpCode.REM:
                            {
                                var right = stack.Pop();
                                var left = stack.Pop();

                                stack.Push(left % right);

                                break;
                            }

                        case OpCode.POW:
                            {
                                var right = stack.Pop();
                                var left = stack.Pop();

                                stack.Push(left.Power(right));

                                break;
                            }


                        case OpCode.EQL:
                            {
                                var right = stack.Pop();
                                var left = stack.Pop();

                                stack.Push(new SmolValue()
                                {
                                    type = SmolValueType.Bool,
                                    value = left.Equals(right)
                                });

                                break;
                            }

                        case OpCode.NEQ:
                            {
                                var right = stack.Pop();
                                var left = stack.Pop();

                                stack.Push(new SmolValue()
                                {
                                    type = SmolValueType.Bool,
                                    value = !left.Equals(right)
                                });

                                break;
                            }

                        case OpCode.GT:
                            {
                                var right = stack.Pop();
                                var left = stack.Pop();

                                stack.Push(left > right);

                                break;
                            }

                        case OpCode.LT:
                            {
                                var right = stack.Pop();
                                var left = stack.Pop();

                                stack.Push(left < right);

                                break;
                            }

                        case OpCode.GTE:
                            {
                                var right = stack.Pop();
                                var left = stack.Pop();

                                stack.Push(left >= right);

                                break;
                            }

                        case OpCode.LTE:
                            {
                                var right = stack.Pop();
                                var left = stack.Pop();

                                stack.Push(left <= right);

                                break;
                            }

                        case OpCode.EOF:

                            //if (debug)
                            {
                                Console.WriteLine($"DONE, TOOK {System.Environment.TickCount - t}");
                            }

                            return;

                        case OpCode.RETURN:

                            // Return to the previous code section, putting
                            // a return value on the stack and restoring the PC

                            // Top value on the stack is the return value

                            var return_value = stack.Pop();

                            // Next value should be the original pre-call state that we saved

                            var _savedCallState = stack.Pop();

                            if (_savedCallState.type != SmolValueType.SavedCallState)
                            {
                                throw new Exception("Tried to return but found something unexecpted on the stack");
                            }

                            var savedCallState = (SmolCallSaveState)_savedCallState.value!;

                            this.environment = savedCallState.previous_env;
                            this.PC = savedCallState.PC;
                            this.code_section = savedCallState.code_section;

                            // Return value needs to go back on the stack
                            stack.Push(return_value);

                            if (savedCallState.call_is_extern)
                            {
                                // Not sure what to do about return value here

                                this.runMode = RunMode.Paused;
                                return; // Don't like this, error prone
                            }

                            break;

                        case OpCode.DECLARE:
                            environment.Define(((SmolVariableDefinition)instr.operand1!).name, null);
                            break;

                        case OpCode.STORE:
                            {
                                var value = stack.Pop();

                                var env_in_context = environment;
                                bool isPropertySetter = false;

                                if (instr.operand2 != null && (bool)instr.operand2)
                                {
                                    var objRef = stack.Pop();

                                    isPropertySetter = true;
                                    env_in_context = ((SmolObjectRef)objRef.value!).object_env;
                                }

                                env_in_context.Assign(((SmolVariableDefinition)instr.operand1!).name, value, isPropertySetter);


                                if (debug)
                                {
                                    Console.WriteLine($"              [Saved ${value}]");
                                }

                                break;
                            }

                        case OpCode.FETCH:
                            {
                                // Could be a variable or a function
                                var name = ((SmolVariableDefinition)instr.operand1!).name;

                                var env_in_context = environment;

                                if (instr.operand2 != null && (bool)instr.operand2)
                                {
                                    var objRef = stack.Pop();

                                    env_in_context = ((SmolObjectRef)objRef.value!).object_env;

                                    var peek_instr = program.code_sections[code_section][PC];

                                    if (peek_instr.opcode == OpCode.CALL && (bool)peek_instr.operand2!)
                                    {
                                        stack.Push(objRef);
                                    }
                                }

                                var fetchedValue = env_in_context.TryGet(name);

                                if (fetchedValue?.GetType() == typeof(SmolFunctionDefn))
                                {
                                    fetchedValue = new SmolValue()
                                    {
                                        type = SmolValueType.FunctionRef,
                                        value = fetchedValue
                                    };
                                }

                                if (fetchedValue != null)
                                {
                                    stack.Push((SmolValue)fetchedValue);

                                    if (debug)
                                    {
                                        Console.WriteLine($"              [Loaded ${fetchedValue}]");
                                    }
                                }
                                else
                                {
                                    if (program.function_table.Any(f => f.globalFunctionName == name))
                                    {
                                        stack.Push(new SmolValue()
                                        {
                                            type = SmolValueType.FunctionRef,
                                            value = program.function_table.First(f => f.globalFunctionName == name)
                                        });
                                    }
                                    else
                                    {
                                        stack.Push(new SmolValue() { type = SmolValueType.Undefined });
                                    }
                                }

                                break;
                            }

                        case OpCode.JMPFALSE:
                            {
                                var value = stack.Pop();

                                if (value.IsFalsey())
                                {
                                    PC = jmplocs[(int)instr.operand1!];
                                }

                                break;
                            }

                        case OpCode.JMPTRUE:

                            if (stack.Pop().IsTruthy())
                            {
                                PC = jmplocs[(int)instr.operand1!];
                            }

                            break;


                        case OpCode.JMP:
                            PC = jmplocs[(int)instr.operand1!];
                            break;

                        case OpCode.LABEL:
                            // Just skip over this instruction, it's only here
                            // to support branching
                            break;

                        case OpCode.ENTER_SCOPE:
                            {
                                this.environment = new SmolScript.Internals.Environment(this.environment);
                                break;
                            }

                        case OpCode.LEAVE_SCOPE:
                            {
                                this.environment = this.environment.enclosing!;
                                break;
                            }

                        case OpCode.DEBUGGER:
                            {
                                //break;
                                this.runMode = RunMode.Paused;
                                return;
                            }

                        case OpCode.POP_AND_DISCARD:
                            // operand1 is optional bool, default true means fail if nothing to pop
                            if (stack.Count > 0 || instr.operand1 == null || (bool)instr.operand1)
                            {
                                stack.Pop();
                            }
                            break;

                        case OpCode.TRY:

                            SmolValue? exception = null;

                            if (instr.operand2 != null && (bool)instr.operand2)
                            {
                                // This is a special flag for the try instruction that tells us to
                                // take the exception that's already on the stack and leave it at the
                                // top after creating the try checkpoint.

                                exception = stack.Pop();
                            }

                            stack.Push(new SmolValue()
                            {
                                type = SmolValueType.TryCheckPoint,
                                value = new SmolTrySaveState()
                                {
                                    code_section = this.code_section,
                                    PC = this.PC,
                                    this_env = this.environment,
                                    jump_exception = jmplocs[(int)instr.operand1!]
                                }
                            });

                            if (exception != null)
                            {
                                stack.Push((SmolValue)exception!);
                            }

                            break;

                        case OpCode.THROW:
                            if ((bool)instr.operand1!) // This flag means the user provided an object to throw, and it's already on the stack
                            {
                                throw new SmolRuntimeException();
                            }
                            else
                            {
                                stack.Push(new SmolValue()
                                {
                                    type = SmolValueType.String,
                                    value = "Error"
                                });

                                throw new SmolRuntimeException();
                            }

                        case OpCode.LOOP_START:

                            stack.Push(new SmolValue()
                            {
                                type = SmolValueType.LoopMarker,
                                value = new SmolLoopMarker()
                                {
                                    current_env = this.environment
                                }
                            });

                            break;

                        case OpCode.LOOP_END:

                            stack.Pop();
                            break;

                        case OpCode.LOOP_EXIT:

                            while (stack.Any())
                            {
                                var next = stack.Pop();

                                if (next.type == SmolValueType.LoopMarker)
                                {
                                    this.environment = ((SmolLoopMarker)next.value!).current_env;

                                    stack.Push(next); // Needs to still be on the stack

                                    if (instr.operand1 != null)
                                    {
                                        this.PC = jmplocs[(int)instr.operand1!];
                                    }

                                    break;
                                }
                            }

                            break;

                        case OpCode.CREATE_OBJECT:

                            // Create a new environment and store it as an instance/ref variable
                            // For now we'll just have it 'inherit' the global env, but scope is
                            // a thing we need to think about, but I'll work out how JS does it
                            // first and try and do the same (I think class hierarchies all share
                            // a single env?!

                            var class_name = (string)instr.operand1!;

                            var obj_environment = new SmolScript.Internals.Environment(this.globalEnv);

                            foreach (var classFunc in program.function_table.Where(f => f.globalFunctionName!.StartsWith($"@{class_name}.")))
                            {
                                var funcName = classFunc.globalFunctionName!.Substring(class_name.Length + 2);

                                obj_environment.Define(funcName, new SmolFunctionDefn()
                                {
                                    arity = classFunc.arity,
                                    code_section = classFunc.code_section,
                                    globalFunctionName = classFunc.globalFunctionName,
                                    param_variable_names = classFunc.param_variable_names
                                });
                            }

                            stack.Push(new SmolValue()
                            {
                                type = SmolValueType.ObjectRef,
                                value = new SmolObjectRef()
                                {
                                    object_env = obj_environment,
                                    class_name = class_name
                                }
                            });

                            obj_environment.Define("this", stack.Peek());

                            break;

                        case OpCode.DUPLICATE_VALUE:

                            stack.Push(stack.Peek());

                            break;

                        case OpCode.PRINT:

                            var whatevs = stack.Pop();

                            Console.WriteLine(whatevs);

                            break;

                        default:
                            throw new Exception($"You forgot to handle an opcode: {instr.opcode}");
                    }
                }
                catch (SmolRuntimeException e)
                {
                    bool handled = false;

                    while (stack.Any())
                    {
                        var next = stack.Pop();

                        if (next.type == SmolValueType.TryCheckPoint)
                        {
                            // We found the start of a try section, restore our state and jump to the exception handler location

                            var state = (SmolTrySaveState)next.value!;

                            this.code_section = state.code_section;
                            this.PC = state.jump_exception;
                            this.environment = state.this_env;

                            stack.Push(new SmolValue()
                            {
                                type = SmolValueType.Exception,
                                value = e.Message
                            });

                            handled = true;
                            break;
                        }
                    }

                    if (!handled)
                    {
                        throw;
                    }
                }

                if (this.runMode == RunMode.Step && instr.StepCheckpoint == true)
                {
                    this.runMode = RunMode.Paused;
                    return;
                }

                if (this.stack.Count > _MaxStackSize) throw new Exception("Stack overflow");
            }
        }
    }
}

