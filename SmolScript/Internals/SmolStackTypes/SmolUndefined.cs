using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolUndefined : SmolStackValue
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

