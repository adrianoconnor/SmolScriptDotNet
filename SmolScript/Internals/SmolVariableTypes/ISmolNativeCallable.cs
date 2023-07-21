using System;
namespace SmolScript.Internals.SmolVariableTypes
{
    /// <summary>
    /// ISmolNativeCallable is an interface that SmolVariableTypes can use if we want
    /// the VM to look for methods and properties in the native code (not just things
    /// that are in the compiled bytecode).
    ///
    /// This is only used for Smol internal classes/types, it's not used for interop
    /// with user code (for that we try and limit to simple lambdas and maybe later
    /// also reflection for dynamic access to native passed objects, tbc).
    /// </summary>
    internal interface ISmolNativeCallable
    {
        SmolVariableType GetProp(string propName) => throw new NotImplementedException();

        void SetProp(string propName, SmolVariableType value) => throw new NotImplementedException();

        SmolVariableType NativeCall(string funcName, List<SmolVariableType> parameters) => throw new NotImplementedException();

        static SmolVariableType StaticCall(string funcName, List<SmolVariableType> parameters) => throw new NotImplementedException();
    }
}

