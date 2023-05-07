using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolError : SmolVariableType, ISmolNativeCallable
    {
        internal readonly string message;

        internal SmolError(string message)
        {
            this.message = message;
        }

        internal override object? GetValue()
        {
            return this.message;
        }

        public override string ToString()
        {
            return $"Error: {message}";
        }


        public SmolVariableType GetProp(string propName)
        {
            switch (propName)
            {
                case "message":
                    return new SmolString(this.message);

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
            throw new Exception($"{this.GetType()} cannot handle native function {funcName}");
        }

        public static SmolVariableType StaticCall(string funcName, List<SmolVariableType> parameters)
        {
            switch (funcName)
            {
                case "constructor":

                    if (parameters.Any())
                    {
                        return new SmolError(parameters.First().GetValue()!.ToString()!);
                    }
                    else
                    {
                        return new SmolError("");
                    }

                default:
                    throw new Exception($"Error class cannot handle static function {funcName}");
            }
        }
    }
}

