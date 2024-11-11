using System.Text.RegularExpressions;
using SmolScript.Internals.SmolStackTypes;

namespace SmolScript.Internals.SmolVariableTypes
{
    internal class SmolRegExp : SmolVariableType, ISmolNativeCallable
    {
        internal readonly string Pattern;
        internal readonly Regex Regex;

        internal SmolRegExp(string pattern)
        {
            this.Pattern = pattern;
            this.Regex = new Regex(pattern);
        }

        internal override object? GetValue()
        {
            return this.Pattern;
        }

        public override string ToString()
        {
            return $"(SmolRegExp) {Pattern}";
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
                case "test":
                    return new SmolBool(this.Regex.IsMatch(((SmolString)parameters[0]).StringValue));

                default:
                    throw new Exception($"{this.GetType()} cannot handle native function {funcName}");
            }
        }

        public static SmolVariableType StaticCall(string funcName, List<SmolVariableType> parameters)
        {
            switch (funcName)
            {
                case "constructor":

                    return new SmolRegExp(parameters.First().GetValue()!.ToString()!);

                default:
                    throw new Exception($"String class cannot handle static function {funcName}");
            }
        }
    }
}
