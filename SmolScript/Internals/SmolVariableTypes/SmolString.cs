using SmolScript.Internals.SmolStackTypes;

namespace SmolScript.Internals.SmolVariableTypes
{
    internal class SmolString : SmolVariableType, ISmolNativeCallable
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

        public SmolVariableType GetProp(string propName)
        {
            switch (propName)
            {
                case "length":
                    return new SmolNumber(this.value.Length);

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
                case "indexOf":

                    var p1 = ((SmolString)parameters[0]).value;

                    return new SmolNumber(this.value.IndexOf(p1));

                default:
                    throw new Exception($"{this.GetType()} cannot handle native function {funcName}");
            }
        }

        public static SmolVariableType StaticCall(string funcName, List<SmolVariableType> parameters)
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
