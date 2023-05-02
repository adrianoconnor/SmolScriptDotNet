using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using SmolScript.Internals;
using SmolScript.Internals.Ast;
using SmolScript.Internals.SmolStackTypes;

[assembly: InternalsVisibleTo("SmolScript.Tests.Internal")]

namespace SmolScript
{
    public class SmolVM : ISmolRuntime
    {
        enum RunMode
        {
            Run,
            Paused,
            Step,
            Done
        }

        internal SmolProgram program;

        private Action<string>? _DebugLogDelegate;
        public Action<string> OnDebugLog { set { _DebugLogDelegate = value; } }

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

        public void Reset()
        {
            stack.Clear();
            this.globalEnv = new Internals.Environment();
            environment = globalEnv;
            code_section = 0;
            PC = 0;
        }

        int code_section = 0;
        int PC = 0; // Program Counter / Instruction Pointer

        bool _debug = false;

        RunMode runMode = RunMode.Paused;

        // This is a stack based VM, so this is for our working data.
        // We store everything here as SmolValue, which is a wrapper
        // for all of our types
        internal Stack<SmolStackValue> stack = new Stack<SmolStackValue>();

        Dictionary<int, int> jmplocs = new Dictionary<int, int>();

        internal SmolScript.Internals.Environment globalEnv = new SmolScript.Internals.Environment();
        internal SmolScript.Internals.Environment environment;

        public T GetGlobalVar<T>(string variableName)
        {
            var v = (SmolStackValue)globalEnv.Get(variableName)!;

            return (T)Convert.ChangeType(v.GetValue()!, typeof(T));
        }

        public void Call(string functionName, params object[] args)        
        {
            Call<object>(functionName, args);
        }

        public T Call<T>(string functionName, params object[] args)
        {
            if (this.runMode != RunMode.Done)
            {
                throw new Exception("Init() should be used before calling a function, to ensure the vm state is prepared");
            }

            // Let the VM know that it's ok to proceed from wherever the PC was pointing next
            this.runMode = RunMode.Paused;

            // Store the current state. This doesn't matter too much, because it shouldn't really
            // be runnable after we're done, but it doesn't hurt to do this.
            var state = new SmolCallSiteSaveState(
                code_section: this.code_section,
                PC: this.PC,
                previous_env: this.environment,
                call_is_extern: true
            );

            // Create an environment for the function
            var env = new SmolScript.Internals.Environment(this.globalEnv);
            this.environment = env;

            var fn = program.function_table.First(f => f.global_function_name == functionName);

        
            // Prime the new environment with variables for
            // the parameters in the function declaration (actual number
            // passed might be different)

            for (int i = 0; i < fn.arity; i++)
            {
                if (args.Count() > i)
                {
                    env.Define(fn.param_variable_names[i], SmolStackValue.Create(args[i]));
                }
                else
                {
                    env.Define(fn.param_variable_names[i], new SmolUndefined());
                }
            }


            stack.Push(state!);

            PC = 0;
            code_section = fn.code_section;

            Run();

            return (T)Convert.ChangeType(stack.Pop().GetValue()!, typeof(T));
        }


        public static ISmolRuntime Compile(string source)
        {
            return new SmolVM(source);
        }

        public static ISmolRuntime Init(string source)
        {
            var vm = new SmolVM(source);

            vm.Run();

            return vm;
        }


        public string Decompile()
        {
            return ByteCodeDisassembler.Disassemble(this.program);
        }

        public string DumpAst()
        {
            return new AstDump().Print(program.astStatements)!;
        }


        public SmolVM(string source)
        {
            environment = globalEnv;

            this.program = SmolCompiler.Compile(source);

            CreateStdLib();
            BuildJumpTable();
        }

        internal SmolVM(SmolProgram program)
        {
            environment = globalEnv;

            this.program = program;

            CreateStdLib();
            BuildJumpTable();
        }

        private void CreateStdLib()
        {
            staticTypes.Add("String", typeof(SmolString));
            staticTypes.Add("Array", typeof(SmolArray));
        }

        internal Dictionary<string, Type> staticTypes = new Dictionary<string, Type>();

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
            if (this.runMode != RunMode.Paused)
            {
                throw new Exception("Step() is only allowed when the vm is paused (after invoking the debugger)");
            }

            _Run(RunMode.Step);
        }

        public void Run()
        {
            if (this.runMode == RunMode.Done)
            {
                throw new Exception("Program execution is complete, either call Reset() before Run(), or invoke a specific function");
            }

            _Run(RunMode.Run);
        }

        public void RunInDebug()
        {
            if (this.runMode == RunMode.Done)
            {
                throw new Exception("Program execution is complete, either Reset() or call a specific function");
            }

            _debug = true;
            _Run(RunMode.Run);
        }

        private void debug(string s)
        {
            if (_debug && _DebugLogDelegate != null)
            {
                _DebugLogDelegate(s);
            }
        }

        void _Run(RunMode newRunMode)
        {
            this.runMode = newRunMode;

            double t = System.Environment.TickCount;

            while (true)
            {
                var instr = program.code_sections[code_section][PC++];

                debug($"{instr}");//: {System.Environment.TickCount - t}");

                t = System.Environment.TickCount;
                
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

                            debug($"              [Pushed ${program.constants[(int)instr.operand1!]}]");
                            
                            break;

                        case OpCode.CALL:
                            {
                                var untypedCallData = stack.Pop();

                                if (untypedCallData.GetType() == typeof(SmolNativeFunctionResult))
                                {
                                    // Everything was handled by the previous Fetch instruction, which made a native
                                    // call and left the result on the stack.
                                    break;
                                }

                                var callData = (SmolFunction)untypedCallData;

                                // First create the env for our function

                                var env = new SmolScript.Internals.Environment(this.globalEnv);

                                if ((bool)instr.operand2!)
                                {
                                    // If op2 is true, that means we're calling a method
                                    // on an object/class, so we need to get the objref
                                    // (from the next value on the stack) and use that
                                    // objects environment instead.

                                    env = ((SmolObject)stack.Pop()).object_env;
                                }

                                // Next pop args off the stack. Op1 is number of args.                    

                                List<SmolStackValue> paramValues = new List<SmolStackValue>();

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
                                        env.Define(callData.param_variable_names[i], new SmolUndefined());
                                    }
                                }


                                // Store our current program/vm state so we can restor

                                var state = new SmolCallSiteSaveState(
                                    code_section: this.code_section,
                                    PC: this.PC,
                                    previous_env:  this.environment,
                                    call_is_extern: false
                                );

                                // Switch the active env in the vm over to the one we prepared for the call

                                this.environment = env;

                                stack.Push(state);

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

                                stack.Push(SmolStackValue.Create(left.GetValue().Equals(right.GetValue())));

                                break;
                            }

                        case OpCode.NEQ:
                            {
                                var right = stack.Pop();
                                var left = stack.Pop();

                                stack.Push(SmolStackValue.Create(!left.GetValue().Equals(right.GetValue())));

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

                        case OpCode.BITWISE_OR:
                            {
                                var right = stack.Pop();
                                var left = stack.Pop();

                                stack.Push(left | right);

                                break;
                            }

                        case OpCode.BITWISE_AND:
                            {
                                var right = stack.Pop();
                                var left = stack.Pop();

                                stack.Push(left & right);

                                break;
                            }

                        case OpCode.EOF:

                            debug($"Done, stack size = {stack.Count}");
                            this.runMode = RunMode.Done;
                            
                            return;

                        case OpCode.RETURN:

                            // Return to the previous code section, putting
                            // a return value on the stack and restoring the PC

                            // Top value on the stack is the return value

                            var return_value = stack.Pop();

                            // Next value should be the original pre-call state that we saved

                            var _savedCallState = stack.Pop();

                            if (_savedCallState.GetType() != typeof(SmolCallSiteSaveState))
                            {
                                throw new Exception("Tried to return but found something unexecpted on the stack");
                            }

                            var savedCallState = (SmolCallSiteSaveState)_savedCallState;

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
                                    env_in_context = ((SmolObject)objRef).object_env;
                                }

                                env_in_context.Assign(((SmolVariableDefinition)instr.operand1!).name, value, isPropertySetter);


                                debug($"              [Saved ${value}]");
                                    
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
                                    var peek_instr = program.code_sections[code_section][PC];

                                    if (objRef.GetType() == typeof(SmolObject))
                                    {
                                        env_in_context = ((SmolObject)objRef).object_env;
                                     
                                        if (peek_instr.opcode == OpCode.CALL && (bool)peek_instr.operand2!)
                                        {
                                            stack.Push(objRef);
                                        }
                                    }
                                    else 
                                    {
                                        if (objRef is ISmolNativeCallable)
                                        {
                                            var isFuncCall = (peek_instr.opcode == OpCode.CALL && (bool)peek_instr.operand2!);

                                            if (isFuncCall)
                                            {
                                                // We need to get some arguments

                                                List<SmolStackValue> paramValues = new List<SmolStackValue>();

                                                for (int i = 0; i < (int)peek_instr.operand1!; i++)
                                                {
                                                    paramValues.Add(this.stack.Pop());
                                                }

                                                stack.Push(((ISmolNativeCallable)objRef).NativeCall(name, paramValues)!);
                                                stack.Push(new SmolNativeFunctionResult()); // Call will use this to see that the call is already done.
                                            }
                                            else
                                            {
                                                // For now won't work with Setter

                                                stack.Push(((ISmolNativeCallable)objRef).GetProp(name)!);
                                            }

                                            break;
                                        }
                                        else if (objRef is SmolNativeFunctionResult)
                                        {
                                            var classMethodRegEx = new Regex(@"\@([A-Za-z]+)[\.]([A-Za-z]+)");
                                            var rex = classMethodRegEx.Match(name);
                                            if (rex.Success)
                                            {
                                                List<object> parameters = new List<object>();

                                                parameters.Add(rex.Groups[2].Value);

                                                var functionArgs = new List<SmolStackValue>();

                                                for (int i = 0; i < (int)peek_instr.operand1!; i++)
                                                {
                                                    functionArgs.Add(this.stack.Pop());
                                                }

                                                parameters.Add(functionArgs);

                                                // Now we've got rid of the params we can get rid
                                                // of the dummy object that create_object left
                                                // on the stack

                                                _ = stack.Pop();

                                                // Put our actual new object on after calling the ctor:

                                                //var r = staticNativeClassMethods["@String.constructor"](paramValues);

                                                var r = (SmolStackValue)staticTypes[rex.Groups[1].Value].GetMethod("StaticCall")!.Invoke(null, parameters.ToArray());

                                                stack.Push(r);

                                                // And now fill in some fake object refs again:
                                                stack.Push(new SmolNativeFunctionResult()); // Call will use this to see that the call is already done.
                                                stack.Push(new SmolNativeFunctionResult()); // Pop and Discard following Call will discard this

                                                break;
                                            }

                                        }
                                        else
                                        {
                                            throw new Exception($"{objRef.GetType()} is not a valid target for this call");
                                        }
                                    }
                                }

                                var fetchedValue = env_in_context.TryGet(name);

                                if (fetchedValue?.GetType() == typeof(SmolFunction))
                                {
                                    fetchedValue = (SmolFunction)fetchedValue;
                                }

                                if (fetchedValue != null)
                                {
                                    stack.Push((SmolStackValue)fetchedValue!);

                                    debug($"              [Loaded ${fetchedValue.GetType()} {((SmolStackValue)fetchedValue!).GetValue()}]");                                    
                                }
                                else
                                {
                                    if (program.function_table.Any(f => f.global_function_name == name))
                                    {
                                        stack.Push(program.function_table.First(f => f.global_function_name == name));
                                    }
                                    else
                                    {
                                        stack.Push(new SmolUndefined());
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
                                this.environment = new Internals.Environment(this.environment);
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
                            // operand1 is optional bool, default true means fail if nothing to pop
                            if (stack.Count > 0 || instr.operand1 == null || (bool)instr.operand1)
                            {
                                stack.Pop();
                            }
                            break;

                        case OpCode.TRY:

                            SmolRuntimeException? exception = null;

                            if (instr.operand2 != null && (bool)instr.operand2)
                            {
                                // This is a special flag for the try instruction that tells us to
                                // take the exception that's already on the stack and leave it at the
                                // top after creating the try checkpoint.

                                exception = (SmolRuntimeException)stack.Pop();
                            }

                            stack.Push(new SmolTryRegionSaveState(
                                    code_section: this.code_section,
                                    PC: this.PC,
                                    this_env: this.environment,
                                    jump_exception: jmplocs[(int)instr.operand1!]
                                )
                            );

                            if (exception != null)
                            {
                                stack.Push(exception!);
                            }

                            break;

                        case OpCode.THROW:
                            if ((bool)instr.operand1!) // This flag means the user provided an object to throw, and it's already on the stack
                            {
                                throw new Exception(); // SmolRuntimeException("");
                            }
                            else
                            {
                                //stack.Push(new SmolValue()

                                throw new Exception();  // throw new SmolRuntimeException();
                            }

                        case OpCode.LOOP_START:

                            stack.Push(new SmolLoopMarker(
                                current_env: this.environment
                            ));

                            break;

                        case OpCode.LOOP_END:

                            stack.Pop();
                            break;

                        case OpCode.LOOP_EXIT:

                            while (stack.Any())
                            {
                                var next = stack.Pop();

                                if (next.GetType() == typeof(SmolLoopMarker))
                                {
                                    this.environment = ((SmolLoopMarker)next).current_env;

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

                            if (staticTypes.ContainsKey(class_name))
                            {
                                stack.Push(new SmolNativeFunctionResult());
                                break;
                            }

                            var obj_environment = new SmolScript.Internals.Environment(this.globalEnv);

                            foreach (var classFunc in program.function_table.Where(f => f.global_function_name!.StartsWith($"@{class_name}.")))
                            {
                                var funcName = classFunc.global_function_name!.Substring(class_name.Length + 2);

                                obj_environment.Define(funcName, new SmolFunction(
                                    arity: classFunc.arity,
                                    code_section: classFunc.code_section,
                                    global_function_name: classFunc.global_function_name,
                                    param_variable_names: classFunc.param_variable_names
                                ));
                            }

                            stack.Push(new SmolObject(
                                    object_env: obj_environment,
                                    class_name: class_name
                                )
                            );

                            obj_environment.Define("this", stack.Peek());

                            break;

                        case OpCode.DUPLICATE_VALUE:

                            int skip = instr.operand1 != null ? (int)instr.operand1 : 0;

                            var itemToDuplicate = stack.ElementAt(skip);

                            if (itemToDuplicate.GetType() != typeof(SmolNativeFunctionResult))
                            {
                            }

                            stack.Push(itemToDuplicate);

                            break;

                        case OpCode.PRINT:

                            var whatevs = stack.Pop();

                            Console.WriteLine(whatevs);

                            break;

                        default:
                            throw new Exception($"You forgot to handle an opcode: {instr.opcode}");
                    }
                }
                catch (Exception e) // (SmolRuntimeException e)
                {
                    bool handled = false;

                    while (stack.Any())
                    {
                        var next = stack.Pop();

                        if (next.GetType() == typeof(SmolTryRegionSaveState))
                        {
                            // We found the start of a try section, restore our state and jump to the exception handler location

                            var state = (SmolTryRegionSaveState)next;

                            this.code_section = state.code_section;
                            this.PC = state.jump_exception;
                            this.environment = state.this_env;

                            stack.Push(new SmolRuntimeException(e.Message));

                            handled = true;
                            break;
                        }
                    }

                    if (!handled)
                    {
                        throw;
                    }
                }

                if (this.stack.Count > _MaxStackSize) throw new Exception("Stack overflow");

                if (this.runMode == RunMode.Step && instr.StepCheckpoint == true)
                {
                    this.runMode = RunMode.Paused;
                    return;
                }
            }
        }
    }
}

