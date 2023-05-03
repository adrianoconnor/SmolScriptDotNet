﻿using System;
namespace SmolScript.Internals.SmolStackTypes
{
	internal interface ISmolNativeCallable
	{
        SmolStackValue GetProp(string propName);
        SmolStackValue NativeCall(string funcName, List<SmolStackValue> parameters);
        abstract static SmolStackValue StaticCall(string funcName, List<SmolStackValue> parameters);
    }
}
