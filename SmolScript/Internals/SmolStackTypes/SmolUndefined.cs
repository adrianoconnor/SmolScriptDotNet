using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolUndefined : SmolVariableType
    {
        internal SmolUndefined()
        {
        }

        internal override object? GetValue()
        {
            return null;
        }

        public override string ToString()
        {
            return $"(SmolUndefined)";
        }
    }
}

