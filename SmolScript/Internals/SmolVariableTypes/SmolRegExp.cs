using System.Text.RegularExpressions;
using SmolScript.Internals.SmolStackTypes;

namespace SmolScript.Internals.SmolVariableTypes
{
    internal class SmolRegExp : SmolVariableType, ISmolNativeCallable
    {
        internal readonly string pattern;
        internal readonly Regex regex;

        internal SmolRegExp(string pattern)
        {
            this.pattern = pattern;
            this.regex = new Regex(pattern);
        }

        internal override object? GetValue()
        {
            return this.pattern;
        }

        public override string ToString()
        {
            return $"(SmolRegExp) {pattern}";
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
