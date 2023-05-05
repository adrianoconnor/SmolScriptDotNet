using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal interface ISmolNativeCallable
    {
        SmolStackValue GetProp(string propName);
        void SetProp(string propName, SmolStackValue value);
        SmolStackValue NativeCall(string funcName, List<SmolStackValue> parameters);

        static SmolStackValue StaticCall(string funcName, List<SmolStackValue> parameters) => throw new NotImplementedException();
    }
}

