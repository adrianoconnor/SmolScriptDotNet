﻿using System;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmolScript.Internals;
using SmolScript.Internals.Ast;
using SmolScript.Internals.SmolStackTypes;
using SmolScript.Internals.SmolVariableTypes;

[assembly: InternalsVisibleTo("SmolScript.Tests.Internal")]

namespace SmolScript
{
    public class SmolVm : ISmolRuntime
    {
        private class SmolThrownFromInstruction : Exception
        {

        }

        private enum RunMode
        {
            READY,
            RUN,
            PAUSED,
            STEP,
            DONE,
            ERROR
        }

        internal SmolProgram Program;

        private Action<string>? _debugLogDelegate;
        
        public Action<string> OnDebugLog
        {
            set
            {
                _debugLogDelegate = value;
                _debug = value != null;
            }
        }

        private int _maxStackSize = int.MaxValue;

        public int MaxStackSize
        {
            get => _maxStackSize;
            set => _maxStackSize = value;
        }

        private int _maxCycleCount = int.MaxValue;

        public int MaxCycleCount
        {
            get => _maxCycleCount;
            set => _maxCycleCount = value;
        }

        public void Reset()
        {
            stack.Clear();
            this.globalEnv = new Internals.Environment();
            environment = globalEnv;
            code_section = 0;
            PC = 0;
        }

        internal int code_section = 0;
        internal int PC = 0; // Program Counter / Instruction Pointer

        bool _debug = false;

        RunMode _runMode = RunMode.PAUSED;

        // This is a stack based VM, so this is for our working data.
        // We store everything here as SmolValue, which is a wrapper
        // for all of our types
        internal Stack<SmolStackType> stack = new Stack<SmolStackType>();

        

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

            if (v.GetType() == typeof(SmolObject) && typeof(T) == typeof(JObject))
            {
                Dictionary<string, object> values = new Dictionary<string, object>();

                foreach(var entry in ((SmolObject)v).object_env.variables)
                {
                    // For an array we need to do something like ((SmolArray)entry.value).elements, but it
                    // needs to be recursive

                    values.Add(entry.Key, entry.Value.GetValue());
                }

                // Stupid downcast to object and back resolves a compiler error

                return (T)(object)JObject.FromObject(values);
            }

            return (T)Convert.ChangeType(v.GetValue()!, typeof(T));
        }
        
        public List<T>? GetGlobalVarAsArray<T>(string variableName)
        {
            var v = (SmolVariableType)globalEnv.Get(variableName)!;
            
            if (v.GetType() == typeof(SmolArray))
            {
                var resultArray = new List<T>();

                foreach (var el in ((SmolArray)v).Elements)
                {
                    resultArray.Add((T)Convert.ChangeType(el.GetValue(), typeof(T)));
                }

                return resultArray;
            }

            throw new InvalidCastException();
        }

        public void Call(string functionName, params object[] args)
        {
            Call<object?>(functionName, args);
        }

        /// <summary>
        /// Call a method from .net that has been defined in the current SmolScript program.
        /// Requires that the current program has been initialized in the VM.
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T? Call<T>(string functionName, params object[] args)
        {
            if (this._runMode != RunMode.DONE)
            {
                throw new Exception("Init() should be used before calling a function, to ensure the vm state is prepared");
            }
            
            // Store the current state. This doesn't matter too much, because it shouldn't really
            // be runnable after we're done, but it doesn't hurt to do this.
            var state = new SmolCallSiteSaveState(
                codeSection: this.code_section,
                pc: this.PC,
                previousEnv: this.environment,
                callIsExtern: true
            );

            // Create an environment for the function
            var env = new SmolScript.Internals.Environment(this.globalEnv);
            this.environment = env;

            var fn = Program.FunctionTable.First(f => f.GlobalFunctionName == functionName);


            // Prime the new environment with variables for
            // the parameters in the function declaration (actual number
            // passed might be different)

            for (var i = 0; i < fn.Arity; i++)
            {
                if (args.Count() > i)
                {
                    try
                    {
                        env.Define(fn.ParamVariableNames[i], SmolVariableType.Create(args[i]));
                    }
                    catch(Exception)
                    {
                        env.Define(fn.ParamVariableNames[i], new SmolNativeTypeWrapper(args[i]));
                    }
                }
                else
                {
                    env.Define(fn.ParamVariableNames[i], new SmolUndefined());
                }
            }


            stack.Push(state!);

            PC = 0;
            code_section = fn.CodeSection;
            
            // Let the VM know that it's ok to proceed when we call Run
            this._runMode = RunMode.PAUSED;

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
            return new SmolVm(source);
        }

        public static ISmolRuntime Init(string source)
        {
            var vm = new SmolVm(source);

            vm.Run();

            return vm;
        }

        public string Decompile()
        {
            return ByteCodeDisassembler.Disassemble(this.Program);
        }

        public SmolVm(string source)
        {
            environment = globalEnv;

            this.Program = Compiler.Compile(source);

            CreateStdLib();
        }

        internal SmolVm(SmolProgram program)
        {
            environment = globalEnv;

            this.Program = program;

            CreateStdLib();
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
            if (this._runMode != RunMode.PAUSED)
            {
                throw new Exception("Step() is only allowed when the vm is paused (after invoking the debugger)");
            }

            _Run(RunMode.STEP);
        }

        public void Run()
        {
            if (this._runMode == RunMode.DONE)
            {
                throw new Exception("Program execution is complete, either call Reset() before Run(), or invoke a specific function");
            }

            _Run(RunMode.RUN);
        }

        private void Debug(string s)
        {
            if (_debug && _debugLogDelegate != null)
            {
                _debugLogDelegate(s);
            }
        }

        void _Run(RunMode newRunMode)
        {
            this._runMode = newRunMode;
            var hasExecutedAtLeastOnce = false; // Used to ensure Step-through trips after at least one instruction is executed
            var consumedCycles = 0;

            double t = System.Environment.TickCount;

            while (true)
            {
                if (this._runMode == RunMode.STEP
                    && (Program.CodeSections[code_section][PC].IsStatementStartpoint ?? false)
                    && hasExecutedAtLeastOnce)
                {
                    this._runMode = RunMode.PAUSED;
                    return;
                }

                var instr = Program.CodeSections[code_section][PC++]; // Increment PC after fetching the net (current) instruction

                Debug($"{instr}");//: {System.Environment.TickCount - t}");

                t = System.Environment.TickCount;

                try
                {
                    switch (instr.opcode)
                    {
                        case OpCode.NOP:
                            break;

                        case OpCode.CONST:
                            stack.Push(Program.Constants[(int)instr.operand1!]);

                            Debug($"              [Pushed ${Program.Constants[(int)instr.operand1!]}]");

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

                                // First create the env for our function -- we probably need to handle bind() here.

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

                                for (int i = 0; i < callData.Arity; i++)
                                {
                                    if (paramValues.Count > i)
                                    {
                                        env.Define(callData.ParamVariableNames[i], paramValues[i]);
                                    }
                                    else
                                    {
                                        env.Define(callData.ParamVariableNames[i], new SmolUndefined());
                                    }
                                }


                                // Store our current program/vm state so we can restor

                                var state = new SmolCallSiteSaveState(
                                    codeSection: this.code_section,
                                    pc: this.PC,
                                    previousEnv: this.environment,
                                    callIsExtern: false
                                );

                                // Switch the active env in the vm over to the one we prepared for the call

                                this.environment = env;

                                stack.Push(state);

                                // Finally set our PC to the start of the function we're about to execute

                                PC = 0;
                                code_section = callData.CodeSection;

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

                            Debug($"Done, stack size = {stack.Count}");
                            this._runMode = RunMode.DONE;

                            return;

                        case OpCode.RETURN:

                            // Return to the previous code section, putting
                            // a return value on the stack and restoring the PC

                            // Top value on the stack is the return value

                            var returnValue = stack.Pop();

                            // Next value should be the original pre-call state that we saved

                            var _savedCallState = stack.Pop();

                            if (_savedCallState.GetType() != typeof(SmolCallSiteSaveState))
                            {
                                throw new SmolRuntimeException("Tried to return but found something unexecpted on the stack");
                            }

                            var savedCallState = (SmolCallSiteSaveState)_savedCallState;

                            this.environment = savedCallState.PreviousEnv;
                            this.PC = savedCallState.Pc;
                            this.code_section = savedCallState.CodeSection;

                            // Return value needs to go back on the stack
                            stack.Push(returnValue);

                            if (savedCallState.CallIsExtern)
                            {
                                // Not sure what to do about return value here

                                this._runMode = RunMode.PAUSED; // TODO: Should be DONE I think, check this
                                
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

                                    // Not sure about this cast, might need to add an extra check for type

                                    name = ((SmolVariableType)stack.Pop()).GetValue()!.ToString();
                                }

                                var value = (SmolVariableType)stack.Pop(); // Hopefully always true...

                                var envInContext = environment;
                                var isPropertySetter = false;

                                if (instr.operand2 != null && (bool)instr.operand2)
                                {
                                    var objRef = stack.Pop();

                                    isPropertySetter = true;

                                    if (objRef.GetType() == typeof(SmolObject))
                                    {
                                        envInContext = ((SmolObject)objRef).object_env;
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

                                envInContext.Assign(name!, value, isPropertySetter);


                                Debug($"              [Saved ${value}]");

                                break;
                            }

                        case OpCode.FETCH:
                            {
                                // Could be a variable or a function
                                var name = (string)instr.operand1!;

                                var env_in_context = environment;

                                if (name == "@IndexerGet")
                                {
                                    // Special case for square brackets!

                                    name = ((SmolVariableType)stack.Pop()).GetValue()!.ToString();
                                }

                                if (instr.operand2 != null && (bool)instr.operand2)
                                {
                                    var objRef = stack.Pop();
                                    var peek_instr = Program.CodeSections[code_section][PC];

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

                                    Debug($"              [Loaded ${fetchedValue.GetType()} {((SmolVariableType)fetchedValue!).GetValue()}]");
                                }
                                else
                                {
                                    if (Program.FunctionTable.Any(f => f.GlobalFunctionName == name))
                                    {
                                        stack.Push(Program.FunctionTable.First(f => f.GlobalFunctionName == name));
                                    }
                                    else if (externalMethods.Keys.Contains(name))
                                    {
                                        var peekInstr = Program.CodeSections[code_section][PC];

                                        stack.Push(CallExternalMethod(name!, (int)peekInstr.operand1!));

                                        stack.Push(new SmolNativeFunctionResult()); // Call will use this to see that the call is already done
                                    }
                                    else
                                    {
                                        stack.Push(new SmolUndefined());
                                    }
                                }

                                break;
                            }

                        case OpCode.JMPFALSE:
                            
                            if (((SmolVariableType)stack.Pop()).IsFalsey())
                            {
                                PC = this.Program.JumpTable[(int)instr.operand1!];
                            }
                            
                            break;

                        case OpCode.JMPTRUE:

                            if (((SmolVariableType)stack.Pop()).IsTruthy())
                            {
                                PC = this.Program.JumpTable[(int)instr.operand1!];
                            }

                            break;


                        case OpCode.JMP:
                            this.PC = this.Program.JumpTable[(int)instr.operand1!];
                            break;

                        case OpCode.LABEL:
                            // This instruction is only here to support branching, it's a No-op
                            // TODO: Strip these during the compile stage?
                            break;

                        case OpCode.ENTER_SCOPE:
                            this.environment = new Internals.Environment(this.environment);
                            break;

                        case OpCode.LEAVE_SCOPE:
                            this.environment = this.environment.enclosing!;
                            break;

                        case OpCode.DEBUGGER:
                            this._runMode = RunMode.PAUSED;
                            return;

                        case OpCode.POP_AND_DISCARD:
                            stack.Pop();
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
                                    codeSection: this.code_section,
                                    programCounter: this.PC,
                                    thisEnv: this.environment,
                                    jumpException: this.Program.JumpTable[(int)instr.operand1!]
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
                                currentEnv: this.environment
                            ));

                            break;

                        case OpCode.LOOP_END:

                            stack.Pop();
                            break;

                        case OpCode.LOOP_EXIT:

                            while (stack.Any()) // Start removing items from the stack until we find our loop start marker
                            {
                                var next = stack.Pop();

                                if (next.GetType() == typeof(SmolLoopMarker))
                                {
                                    this.environment = ((SmolLoopMarker)next).CurrentEnv;

                                    stack.Push(next); // The loop marker needs to be left on the stack, it will eventually be removed by a LOOP_END

                                    if (instr.operand1 != null)
                                    {
                                        // Based on whether we are exiting the loop because of a break or a continue,
                                        // we will need to jump to a specific location in code.
                                        // The label index to jump to is stored in op1 for LOOP_EXIT instruction.
                                        
                                        this.PC = this.Program.JumpTable[(int)instr.operand1!];
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

                            var className = (string)instr.operand1!;

                            if (staticTypes.ContainsKey(className))
                            {
                                stack.Push(new SmolNativeFunctionResult());
                                break;
                            }

                            var obj_environment = new SmolScript.Internals.Environment(this.globalEnv);

                            foreach (var classFunc in Program.FunctionTable.Where(f => f.GlobalFunctionName!.StartsWith($"@{className}.")))
                            {
                                var funcName = classFunc.GlobalFunctionName!.Substring(className.Length + 2);

                                obj_environment.Define(funcName, new SmolFunction(
                                    arity: classFunc.Arity,
                                    codeSection: classFunc.CodeSection,
                                    globalFunctionName: classFunc.GlobalFunctionName,
                                    paramVariableNames: classFunc.ParamVariableNames
                                ));
                            }

                            stack.Push(new SmolObject(
                                    object_env: obj_environment,
                                    class_name: className
                                )
                            );

                            obj_environment.Define("this", (SmolVariableType)stack.Peek());

                            break;

                        case OpCode.DUPLICATE_VALUE:
    
                            // I feel like this is a bit of a hack, but it makes some scenarios possible and I'm
                            // not sure what the alternative options are. The compiler can basically inject this
                            // instruction to either duplicate the value on the top of the stack, or the value 'n'
                            // items down (using op1).
                            
                            // We only use this when initializing a new instance of an object or a class, so we
                            // might want to just revisit that code and work out a better approach, maybe with
                            // a dedicated instruction for new.
                            
                            int skip = instr.operand1 != null ? (int)instr.operand1 : 0;

                            var itemToDuplicate = stack.ElementAt(skip);

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

                            this.code_section = state.CodeSection;
                            this.PC = state.JumpException;
                            this.environment = state.ThisEnv;
                           
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

                if (this.stack.Count > _maxStackSize) throw new Exception("Stack overflow");
                
                hasExecutedAtLeastOnce = true;

                consumedCycles += 1;

                if (_maxCycleCount > -1 && consumedCycles > _maxCycleCount) throw new Exception("Too many cycles");
            }
        }
    }
}

