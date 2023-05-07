using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal interface ISmolNativeCallable
    {
        SmolVariableType GetProp(string propName);
        void SetProp(string propName, SmolVariableType value);
        SmolVariableType NativeCall(string funcName, List<SmolVariableType> parameters);

        static SmolVariableType StaticCall(string funcName, List<SmolVariableType> parameters) => throw new NotImplementedException();
    }
}

