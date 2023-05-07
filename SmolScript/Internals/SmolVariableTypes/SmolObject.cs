
using SmolScript.Internals.SmolStackTypes;

namespace SmolScript.Internals.SmolVariableTypes
{
    internal class SmolObject : SmolVariableType, ISmolNativeCallable
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

        public SmolVariableType GetProp(string propName)
        {
            switch (propName)
            {
                default:
                    throw new Exception($"{this.GetType()} cannot handle native property {propName}");
            }
        }

        public void SetProp(string propName, SmolVariableType value)
        {
            throw new Exception($"Not a valid target");
        }

        public SmolVariableType NativeCall(string funcName, List<SmolVariableType> parameters)
        {
            switch (funcName)
            {
                default:
                    throw new Exception($"{this.GetTypeName()} cannot handle native function {funcName}");
            }
        }

        public static SmolVariableType StaticCall(string funcName, List<SmolVariableType> parameters)
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

