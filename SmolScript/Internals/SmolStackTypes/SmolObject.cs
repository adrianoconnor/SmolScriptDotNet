using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolObject : SmolStackValue, ISmolNativeCallable
    {
        internal Environment object_env;
        internal string class_name;

        internal SmolObject(Environment object_env, string class_name)
        {
            this.object_env = object_env;
            this.class_name = class_name;
        }

        internal override object? GetValue()
        {
            return null;
        }

        public SmolStackValue GetProp(string propName)
        {
            switch (propName)
            {
                default:
                    throw new Exception($"{this.GetType()} cannot handle native property {propName}");
            }
        }

        public void SetProp(string propName, SmolStackValue value)
        {
            throw new Exception($"Not a valid target");
        }

        public SmolStackValue NativeCall(string funcName, List<SmolStackValue> parameters)
        {
            switch (funcName)
            {
                default:
                    throw new Exception($"{this.GetTypeName()} cannot handle native function {funcName}");
            }
        }

        public static SmolStackValue StaticCall(string funcName, List<SmolStackValue> parameters)
        {
            switch (funcName)
            {
                case "constructor":

                    return new SmolObject(new Environment(), "Object");


                default:
                    throw new Exception($"Object class cannot handle static function {funcName}");
            }
        }
    }
}

