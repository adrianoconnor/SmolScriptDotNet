using System;
namespace SmolScript.Internals.SmolVariableTypes
{
    internal class SmolFunction : SmolVariableType
    {
        // When we compile our program we'll get a list of functions (some of which wil be anonymous, some instance methods,
        // some regular functions) and we need a way to refer to them globally. For example, a function called a() might
        // be declared in a few places, so we need a way to know which function we're talking about when we look up the code
        // location at run-time.
        internal string? GlobalFunctionName;
        internal int CodeSection;
        internal int Arity;
        internal List<string> ParamVariableNames = new List<string>();

        internal SmolFunction(string? globalFunctionName, int codeSection, int arity, List<string> paramVariableNames)
        {
            this.GlobalFunctionName = globalFunctionName;
            this.CodeSection = codeSection;
            this.Arity = arity;
            this.ParamVariableNames = paramVariableNames;
        }

        internal override object? GetValue()
        {
            return this;
        }

        public override string ToString()
        {
            return "[Function]";
        }
    }
}

