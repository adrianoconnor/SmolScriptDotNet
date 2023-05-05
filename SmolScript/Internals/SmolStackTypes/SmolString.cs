using System;

namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolString : SmolStackValue, ISmolNativeCallable
    {
        internal readonly string value;

        internal SmolString(string value)
        {
            this.value = value;
        }

        internal override object? GetValue()
        {
            return this.value;
        }

        public override string ToString()
        {
            return $"(SmolString) {value}";
        }

        public SmolStackValue GetProp(string propName)
        {
            switch (propName)
            {
                case "length":
                    return new SmolNumber(this.value.Length);

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
                case "indexOf":

                    var p1 = ((SmolString)parameters[0]).value;

                    return new SmolNumber(this.value.IndexOf(p1));

                default:
                    throw new Exception($"{this.GetType()} cannot handle native function {funcName}");
            }
        }

        public static SmolStackValue StaticCall(string funcName, List<SmolStackValue> parameters)
        {
            switch (funcName)
            {
                case "constructor":

                    if (parameters.Any())
                    {
                        return new SmolString(parameters.First().GetValue()!.ToString()!);
                    }
                    else
                    {
                        return new SmolString("");
                    }

                default:
                    throw new Exception($"String class cannot handle static function {funcName}");
            }
        }
    }
}
