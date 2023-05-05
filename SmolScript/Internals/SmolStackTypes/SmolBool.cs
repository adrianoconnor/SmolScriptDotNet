using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolBool : SmolStackValue
    {
        internal readonly bool value;

        internal SmolBool(bool value)
        {
            this.value = value;
        }

        internal override object? GetValue()
        {
            return this.value;
        }

        public override string ToString()
        {
            return $"(SmolBool) {value}";
        }
    }
}

