using System.Diagnostics.CodeAnalysis;

namespace SmolScript.Internals.SmolVariableTypes
{
    internal class SmolUndefined : SmolVariableType
    {
        internal SmolUndefined()
        {
        }

        internal override object? GetValue()
        {
            return this;
        }

        public override string ToString()
        {
            return $"(SmolUndefined)";
        }
    }
}

