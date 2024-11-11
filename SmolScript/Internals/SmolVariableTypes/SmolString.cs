using System.Text.RegularExpressions;
using SmolScript.Internals.SmolStackTypes;

namespace SmolScript.Internals.SmolVariableTypes
{
    internal class SmolString : SmolVariableType, ISmolNativeCallable
    {
        internal readonly string StringValue;

        internal SmolString(string stringValue)
        {
            this.StringValue = stringValue;
        }

        internal override object? GetValue()
        {
            return this.StringValue;
        }

        public override string ToString()
        {
            return $"(SmolString) {StringValue}";
        }

        public SmolVariableType GetProp(string propName)
        {
            switch (propName)
            {
                case "length":
                    return new SmolNumber(this.StringValue.Length);

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
                    {
                        var p1 = ((SmolString)parameters[0]).StringValue;

                        return new SmolNumber(this.StringValue.IndexOf(p1));
                    }
                case "search":
                    {
                        var regex = (SmolRegExp)parameters.First();

                        return new SmolNumber(regex.Regex.Match(this.StringValue).Index);
                    }
                case "substring":
                    {
                        var p1 = (SmolNumber)parameters[0];

                        if (parameters.Count == 1)
                        {
                            return new SmolString(this.StringValue.Substring(Convert.ToInt32(p1.NumberValue)));
                        }
                        else
                        {
                            var p2 = (SmolNumber)parameters[1];

                            return new SmolString(this.StringValue.Substring(Convert.ToInt32(p1.NumberValue), Convert.ToInt32(p2.NumberValue)));
                        }
                    }
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
