using SmolScript.Internals;

namespace SmolScript
{
    public enum RunMode {
        Run,
        Paused,
        Step
    }

    public class SmolVM
    {
        SmolProgram program;

        int code_section = 0;
        int PC = 0; // Program Counter / Instruction Pointer

        bool debug = true; 

        RunMode runMode = RunMode.Paused;

        // This is a stack based VM, so this is for our working data.
        // We store everything here as SmolValue, which is a wrapper
        // for all of our types
        Stack<SmolValue> stack = new Stack<SmolValue>();

        Dictionary<int, int> jmplocs = new Dictionary<int, int>();

        public readonly Environment globalEnv = new Environment();
        private Environment environment;

        public SmolVM(string source)
        {
            environment = globalEnv;

            this.program = SmolCompiler.Compile(source);

            BuildJumpTable();
        }

        public SmolVM(SmolProgram program)
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
            this.Run(RunMode.Step);
        }

        public void Run(RunMode newRunMode = RunMode.Run)
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
                            var callData = (SmolFunctionDefn)stack.Pop().value!;

                            // First we need to pop the args off the stack                        

                            List<SmolValue> paramValues = new List<SmolValue>();

                            for (int i = 0; i < (int)instr.operand1!; i++)
                            {
                                paramValues.Add(this.stack.Pop());
                                //env.Define(callData.variableNames[i], param_value);
                            }

                            var state = new SmolCallSaveState()
                            {
                                code_section = this.code_section,
                                PC = this.PC,
                                previous_env = this.environment,
                                treat_call_as_expression = true // Needs to come from function dfn, I guess
                            };

                            var env = new Environment(this.globalEnv);
                            this.environment = env;

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

                            stack.Push(new SmolValue()
                            {
                                type = SmolValueType.SavedCallState,
                                value = state
                            });

                            PC = 0;
                            code_section = callData.code_section;

                            break;

                        case OpCode.ADD:
                            {
                                var right = stack.Pop();
                                var left = stack.Pop();

                                stack.Push(left + right);

                                break;
                            }

                        case OpCode.SUB:
                            stack.Push(stack.Pop() - stack.Pop());
                            break;

                        case OpCode.MUL:
                            stack.Push(stack.Pop() * stack.Pop());
                            break;

                        case OpCode.DIV:
                            stack.Push(stack.Pop() / stack.Pop());
                            break;

                        case OpCode.REM:
                            stack.Push(stack.Pop() % stack.Pop());
                            break;

                        case OpCode.POW:
                            stack.Push(stack.Pop().Power(stack.Pop()));
                            break;


                        case OpCode.EQL:
                            stack.Push(new SmolValue()
                            {
                                type = SmolValueType.Bool,
                                value = stack.Pop()!.Equals(stack.Pop()!)
                            });
                            break;

                        case OpCode.NEQ:
                            stack.Push(new SmolValue()
                            {
                                type = SmolValueType.Bool,
                                value = !stack.Pop()!.Equals(stack.Pop()!)
                            });
                            break;

                        case OpCode.GT:
                            stack.Push(stack.Pop() > stack.Pop());
                            break;

                        case OpCode.LT:
                            stack.Push(stack.Pop() < stack.Pop());
                            break;

                        case OpCode.GTE:
                            stack.Push(stack.Pop() >= stack.Pop());
                            break;

                        case OpCode.LTE:
                            stack.Push(stack.Pop() <= stack.Pop());
                            break;

                        case OpCode.EOF:
                            // Needs to handle call stack scenario for functions that
                            // implicitly return void

                            //if (debug)
                            {
                                Console.WriteLine($"DONE, TOOK {System.Environment.TickCount - t}");
                            }

                            return;

                        case OpCode.RETURN:
                            // Needs to return to the previous code section, putting
                            // a return value on the stack and restoring the PC

                            // Top value on the stack is the return value

                            var return_value = stack.Pop();

                            var _savedCallState = stack.Pop();

                            if (_savedCallState.type != SmolValueType.SavedCallState)
                            {
                                throw new Exception("Tried to return but found something unexecpted on the stack");
                            }

                            var savedCallState = (SmolCallSaveState)_savedCallState.value!;

                            if (savedCallState.treat_call_as_expression)
                            {
                                // Means we're using the return value as an expression
                                // in some other statement
                                stack.Push(return_value);
                            }

                            this.environment = savedCallState.previous_env;
                            this.PC = savedCallState.PC;
                            this.code_section = savedCallState.code_section;


                            break;

                        case OpCode.DECLARE:
                            environment.Define(((SmolVariableDefinition)instr.operand1!).name, null);
                            break;

                        case OpCode.STORE:
                            {
                                var value = stack.Pop();

                                environment.Assign(((SmolVariableDefinition)instr.operand1!).name, value);


                                if (debug)
                                {
                                    Console.WriteLine($"              [Saved ${value}]");
                                }

                                break;
                            }

                        case OpCode.FETCH:
                            // Could be a variable or a function
                            var name = ((SmolVariableDefinition)instr.operand1!).name;

                            var fetchedValue = environment.TryGet(name);

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
                            }

                            break;

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
                                this.environment = new Environment(this.environment);
                                break;
                            }

                        case OpCode.LEAVE_SCOPE:
                            {
                                this.environment = this.environment.enclosing!;
                                break;
                            }

                        case OpCode.DEBUGGER:
                            {
                                this.runMode = RunMode.Paused;
                                return;
                            }

                        case OpCode.POP_AND_DISCARD:
                            stack.Pop();
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

                        default:
                            throw new Exception($"You forgot to handle an opcode: {instr.opcode}");
                    }
                }
                catch(SmolRuntimeException e)
                {
                    bool handled = false;

                    while(stack.Any())
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

                if (this.stack.Count > 20) throw new Exception("Stack too big!");
            }
        }
    }
}

