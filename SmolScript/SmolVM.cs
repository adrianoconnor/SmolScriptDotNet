using System;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SmolScript.Internals;
using SmolScript.Internals.Ast;
using SmolScript.Internals.SmolStackTypes;
using SmolScript.Internals.SmolVariableTypes;

[assembly: InternalsVisibleTo("SmolScript.Tests.Internal")]

namespace SmolScript
{
    public class SmolVM : ISmolRuntime
    {
        private class SmolThrownFromInstruction : Exception
        {

        }

        enum RunMode
        {
            Ready,
            Run,
            Paused,
            Step,
            Done,
            Error
        }

        internal SmolProgram program;

        private Action<string>? _DebugLogDelegate;
        public Action<string> OnDebugLog
        {
            set
            {
                _DebugLogDelegate = value;
                _debug = value != null;
            }
        }

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

        private int _MaxCycleCount = int.MaxValue;

        public int MaxCycleCount
        {
            get
            {
                return _MaxCycleCount;
            }
            set
            {
                _MaxCycleCount = value;
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
        internal Stack<SmolStackType> stack = new Stack<SmolStackType>();

        Dictionary<int, int> jmplocs = new Dictionary<int, int>();

        internal Internals.Environment globalEnv = new();
        internal Internals.Environment environment;

        public T? GetGlobalVar<T>(string variableName)
        {
            var v = (SmolVariableType)globalEnv.Get(variableName)!;

            if (v.GetType() == typeof(SmolUndefined) || v.GetType() == typeof(SmolNull))
            {
                // Check if the default for type T is null, and if it is return that. We need to do this check
                // because the default of a non-nullable type (like int) will be something that looks real (e.g., zero)
                // and that would be potentially very misleading
                if (default(T) == null)
                {
                    return default;
                }
                else
                {
                    throw new NullReferenceException($"Variable '{variableName}' is undefined or null and target type '{typeof(T).Name}' is not Nullable");
                }
            }

            return (T)Convert.ChangeType(v.GetValue()!, typeof(T));
        }

        public void Call(string functionName, params object[] args)
        {
            Call<object?>(functionName, args);
        }

        public T? Call<T>(string functionName, params object[] args)
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
                    try
                    {
                        env.Define(fn.param_variable_names[i], SmolVariableType.Create(args[i]));
                    }
                    catch(Exception)
                    {
                        env.Define(fn.param_variable_names[i], new SmolNativeTypeWrapper(args[i]));
                    }
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

            var returnValue = stack.Pop();

            if (returnValue.GetType() == typeof(SmolUndefined) || returnValue.GetType() == typeof(SmolNull))
            {
                return default(T);
            }
            else
            {
                return (T)Convert.ChangeType(((SmolVariableType)returnValue).GetValue()!, typeof(T));
            }
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
            // These are the class types that are supported by native
            // code. We can cast these to a certain interface and call
            // a static method to 
            staticTypes.Add("String", typeof(SmolString));
            staticTypes.Add("Array", typeof(SmolArray));
            staticTypes.Add("Object", typeof(SmolObject));
            staticTypes.Add("RegExp", typeof(SmolRegExp));
            staticTypes.Add("Error", typeof(SmolError));
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

        internal Dictionary<string, object> externalMethods = new Dictionary<string, object>();

        public void RegisterMethod(string methodName, object lambda)
        {
            var methodInfos = lambda.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var methodTypeName = methodInfos[0].DeclaringType!.Name;

            var typeCheck = new Regex("^(Func|Action)(`[0-9]+){0,1}$");

            if (!typeCheck.IsMatch(methodTypeName))
            {
                throw new Exception($"External method '{methodName}' could not be registered because the second parameter was not a lambda (we expect a Func or Action, but an object with type {methodTypeName} was received)");
            }

            externalMethods.Add(methodName, lambda);
        }

        internal SmolVariableType CallExternalMethod(string methodName, int numberOfPassedArgs)
        {
            var lambdaObj = externalMethods[methodName];
            var methodInfos = lambdaObj.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var methodTypeName = methodInfos[0].DeclaringType!.Name;
            var returnType = ((MethodInfo)methodInfos[0]).ReturnType.Name;
            var argTypes = methodInfos[0].ReflectedType!.GenericTypeArguments;
            var args = new List<object>();
            var numberOfParams = Math.Max(argTypes.Count() - (returnType == "Void" ? 0 : 1), 0);

            var typeCheck = new Regex("^(Func|Action)(`[0-9]+){0,1}$");

            if (!typeCheck.IsMatch(methodTypeName))
            {
                throw new Exception($"External method '{methodName}' is not a lambda (should be a Func or Action only)");
            }

            if (numberOfParams != numberOfPassedArgs)
            {
                throw new Exception($"{methodName} expects {numberOfParams} args, but got {numberOfPassedArgs}");
            }

            for (int i = 0; i < numberOfPassedArgs; i++)
            {
                var argInfo = argTypes[i];
                var value = stack.Pop() as SmolVariableType;

                if (argInfo.Name == "String")
                {
                    args.Add(Convert.ChangeType(value!.GetValue()!, typeof(string)));
                }
                else if (argInfo.Name == "Double")
                {
                    args.Add(Convert.ChangeType(value!.GetValue()!, typeof(double)));
                }
                else if (argInfo.Name == "Int32" || argInfo.Name == "Int64")
                {
                    args.Add(Convert.ChangeType(value!.GetValue()!, typeof(int)));
                }
                else if (argInfo.Name == "Boolean")
                {
                    args.Add(Convert.ChangeType(value!.GetValue()!, typeof(bool)));
                }
                else
                {
                    throw new Exception($"Failed to process argument {i + 1} when calling {methodName} (type {argInfo.Name})");
                }
            }

            var result = methodInfos[0].Invoke(lambdaObj, args.ToArray());

            if (returnType == "Void")
            {
                return new SmolUndefined();
            }
            else
            {
                return SmolVariableType.Create(result);
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
            var hasExecutedAtLeastOnce = false; // Used to ensure Step-through trips after at least one instruction is executed
            var consumedCycles = 0;

            double t = System.Environment.TickCount;

            while (true)
            {
                if (this.runMode == RunMode.Step
                    && (program.code_sections[code_section][PC].IsStatementStartpoint ?? false)
                    && hasExecutedAtLeastOnce)
                {
                    this.runMode = RunMode.Paused;
                    return;
                }

                var instr = program.code_sections[code_section][PC++]; // Increment PC after fetching the net (current) instruction

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

                                List<SmolVariableType> paramValues = new List<SmolVariableType>();

                                for (int i = 0; i < (int)instr.operand1!; i++)
                                {
                                    paramValues.Add((SmolVariableType)this.stack.Pop());
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
                                    previous_env: this.environment,
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
                                var right = (SmolVariableType)stack.Pop();
                                var left = (SmolVariableType)stack.Pop();

                                stack.Push(left + right);

                                break;
                            }

                        case OpCode.SUB:
                            {
                                var right = (SmolVariableType)stack.Pop();
                                var left = (SmolVariableType)stack.Pop();

                                stack.Push(left - right);

                                break;
                            }

                        case OpCode.MUL:
                            {
                                var right = (SmolVariableType)stack.Pop();
                                var left = (SmolVariableType)stack.Pop();

                                stack.Push(left * right);

                                break;
                            }

                        case OpCode.DIV:
                            {
                                var right = (SmolVariableType)stack.Pop();
                                var left = (SmolVariableType)stack.Pop();

                                stack.Push(left / right);

                                break;
                            }

                        case OpCode.REM:
                            {
                                var right = (SmolVariableType)stack.Pop();
                                var left = (SmolVariableType)stack.Pop();

                                stack.Push(left % right);

                                break;
                            }

                        case OpCode.POW:
                            {
                                var right = (SmolVariableType)stack.Pop();
                                var left = (SmolVariableType)stack.Pop();

                                stack.Push(left.Power(right));

                                break;
                            }


                        case OpCode.EQL:
                            {
                                var right = (SmolVariableType)stack.Pop();
                                var left = (SmolVariableType)stack.Pop();

                                stack.Push(SmolVariableType.Create(left.GetValue()!.Equals(right.GetValue())));

                                break;
                            }

                        case OpCode.NEQ:
                            {
                                var right = (SmolVariableType)stack.Pop();
                                var left = (SmolVariableType)stack.Pop();

                                stack.Push(SmolVariableType.Create(!left.GetValue()!.Equals(right.GetValue())));

                                break;
                            }

                        case OpCode.GT:
                            {
                                var right = (SmolVariableType)stack.Pop();
                                var left = (SmolVariableType)stack.Pop();

                                stack.Push(left > right);

                                break;
                            }

                        case OpCode.LT:
                            {
                                var right = (SmolVariableType)stack.Pop();
                                var left = (SmolVariableType)stack.Pop();

                                stack.Push(left < right);

                                break;
                            }

                        case OpCode.GTE:
                            {
                                var right = (SmolVariableType)stack.Pop();
                                var left = (SmolVariableType)stack.Pop();

                                stack.Push(left >= right);

                                break;
                            }

                        case OpCode.LTE:
                            {
                                var right = (SmolVariableType)stack.Pop();
                                var left = (SmolVariableType)stack.Pop();

                                stack.Push(left <= right);

                                break;
                            }

                        case OpCode.BITWISE_OR:
                            {
                                var right = (SmolVariableType)stack.Pop();
                                var left = (SmolVariableType)stack.Pop();

                                stack.Push(left | right);

                                break;
                            }

                        case OpCode.BITWISE_AND:
                            {
                                var right = (SmolVariableType)stack.Pop();
                                var left = (SmolVariableType)stack.Pop();

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
                            environment.Define((string)instr.operand1!, new SmolUndefined());
                            break;

                        case OpCode.STORE:
                            {
                                var name = (string)instr.operand1!;

                                if (name == "@IndexerSet")
                                {
                                    // Special case for square brackets!

                                    // Not sure abotu this cast, might need to add an extra check for type

                                    name = ((SmolVariableType)stack.Pop()).GetValue()!.ToString();
                                }

                                var value = (SmolVariableType)stack.Pop(); // Hopefully always true...

                                var env_in_context = environment;
                                bool isPropertySetter = false;

                                if (instr.operand2 != null && (bool)instr.operand2)
                                {
                                    var objRef = stack.Pop();

                                    isPropertySetter = true;

                                    if (objRef.GetType() == typeof(SmolObject))
                                    {
                                        env_in_context = ((SmolObject)objRef).object_env;
                                    }
                                    else if (objRef is ISmolNativeCallable)
                                    {
                                        ((ISmolNativeCallable)objRef).SetProp(name!, value);
                                        break;
                                    }
                                    else
                                    {
                                        throw new Exception($"{objRef.GetType()} is not a valid target for this call");
                                    }
                                }

                                env_in_context.Assign(name!, value, isPropertySetter);


                                debug($"              [Saved ${value}]");

                                break;
                            }

                        case OpCode.FETCH:
                            {
                                // Could be a variable or a function
                                var name = (string)instr.operand1!;

                                var env_in_context = environment;

                                if (name == "@IndexerGet" || name == "@zIndxerSet")
                                {
                                    // Special case for square brackets!

                                    name = ((SmolVariableType)stack.Pop()).GetValue()!.ToString();
                                }

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

                                                List<SmolVariableType> paramValues = new List<SmolVariableType>();

                                                for (int i = 0; i < (int)peek_instr.operand1!; i++)
                                                {
                                                    paramValues.Add((SmolVariableType)this.stack.Pop());
                                                }

                                                stack.Push(((ISmolNativeCallable)objRef).NativeCall(name!, paramValues)!);
                                                stack.Push(new SmolNativeFunctionResult()); // Call will use this to see that the call is already done.
                                            }
                                            else
                                            {
                                                // For now won't work with Setter

                                                stack.Push(((ISmolNativeCallable)objRef).GetProp(name!)!);
                                            }

                                            break;
                                        }
                                        else if (objRef is SmolNativeFunctionResult)
                                        {
                                            var classMethodRegEx = new Regex(@"\@([A-Za-z]+)[\.]([A-Za-z]+)");
                                            var rex = classMethodRegEx.Match(name!);

                                            if (rex.Success)
                                            {
                                                List<object> parameters = new List<object>();

                                                parameters.Add(rex.Groups[2].Value);

                                                var functionArgs = new List<SmolVariableType>();

                                                if (name != "@Object.constructor")
                                                {
                                                    for (int i = 0; i < (int)peek_instr.operand1!; i++)
                                                    {
                                                        functionArgs.Add((SmolVariableType)this.stack.Pop());
                                                    }
                                                }

                                                parameters.Add(functionArgs);

                                                // Now we've got rid of the params we can get rid
                                                // of the dummy object that create_object left
                                                // on the stack

                                                _ = stack.Pop();

                                                // Put our actual new object on after calling the ctor:

                                                var r = (SmolVariableType)staticTypes[rex.Groups[1].Value].GetMethod("StaticCall")!.Invoke(null, parameters.ToArray())!;

                                                if (name == "@Object.constructor")
                                                {
                                                    // Hack alert!!!
                                                    ((SmolObject)r!).object_env = new Internals.Environment(this.globalEnv);
                                                }

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

                                var fetchedValue = env_in_context.TryGet(name!);

                                if (fetchedValue?.GetType() == typeof(SmolFunction))
                                {
                                    fetchedValue = (SmolFunction)fetchedValue;
                                }

                                if (fetchedValue != null)
                                {
                                    stack.Push((SmolStackType)fetchedValue!);

                                    debug($"              [Loaded ${fetchedValue.GetType()} {((SmolVariableType)fetchedValue!).GetValue()}]");
                                }
                                else
                                {
                                    if (program.function_table.Any(f => f.global_function_name == name))
                                    {
                                        stack.Push(program.function_table.First(f => f.global_function_name == name));
                                    }
                                    else if (externalMethods.Keys.Contains(name))
                                    {
                                        var peek_instr = program.code_sections[code_section][PC];

                                        stack.Push(CallExternalMethod(name!, (int)peek_instr.operand1!));

                                        stack.Push(new SmolNativeFunctionResult()); // Call will use this to see that the call is already done.
                                        //stack.Push(new SmolNativeFunctionResult()); // Pop and Discard following Call will discard this
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
                                var value = (SmolVariableType)stack.Pop();

                                if (value.IsFalsey())
                                {
                                    PC = jmplocs[(int)instr.operand1!];
                                }

                                break;
                            }

                        case OpCode.JMPTRUE:

                            if (((SmolVariableType)stack.Pop()).IsTruthy())
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
                            //if (stack.Count > 0 || instr.operand1 == null || (bool)instr.operand1)
                            //{
                                stack.Pop();
                            //}
                            // TODO: Whatabout else, why is there no else here? I've commented the if out for now because
                            // I'm not convinced it's valid...

                            break;

                        case OpCode.TRY:

                            SmolVariableType? exception = null;

                            if (instr.operand2 != null && (bool)instr.operand2)
                            {
                                // This is a special flag for the try instruction that tells us to
                                // take the exception that's already on the stack and leave it at the
                                // top after creating the try checkpoint.

                                exception = (SmolVariableType)stack.Pop();
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
                            // We throw a custom exception and let the default Exception handler deal with
                            // jumping to the correct location etc -- this is because it's the exact same
                            // behaviour whether it's a user-defined throw or just something going wrong
                            // in the internals, it still needs to look for a parent try/catch block...
                            throw new SmolThrownFromInstruction();                            

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

                            obj_environment.Define("this", (SmolVariableType)stack.Peek());

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
                catch (Exception e)
                {
                    bool handled = false;

                    // If we're here because we're responding to a exception that was thrown by the user in their code,
                    // then the argument to pass to the catch block is the next value on the stack

                    bool isUserThrown = e.GetType() == typeof(SmolThrownFromInstruction);

                    SmolVariableType? userThrownArgument = isUserThrown ? (SmolVariableType)stack.Pop() : null;

                    while (stack.Any())
                    {
                        var next = stack.Pop(); // Keep didscarding whatever is on the stack until we find a try/catch state object (or reach the end)

                        if (next.GetType() == typeof(SmolTryRegionSaveState))
                        {
                            // We found the start of a try section, restore our state and jump to the catch or finally location

                            var state = (SmolTryRegionSaveState)next;

                            this.code_section = state.code_section;
                            this.PC = state.jump_exception;
                            this.environment = state.this_env;
                           
                            stack.Push(isUserThrown ? userThrownArgument! : new SmolError(e.Message));

                            handled = true;
                            break;
                        }
                    }

                    if (!handled)
                    {
                        throw new SmolRuntimeException(e.Message, e);
                    }
                }

                if (this.stack.Count > _MaxStackSize) throw new Exception("Stack overflow");
                
                hasExecutedAtLeastOnce = true;

                consumedCycles += 1;

                if (_MaxCycleCount > -1 && consumedCycles > _MaxCycleCount) throw new Exception("Too many cycles");
            }
        }
    }
}

