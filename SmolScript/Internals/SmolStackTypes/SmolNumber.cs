using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolNumber: SmolStackValue
	{
        internal readonly double value;

        internal SmolNumber(double value)
        {
            this.value = value;
        }

        internal override object? GetValue()
        {
            return this.value;
        }

        public override string ToString()
        {
            return $"(SmolNumber) {value}";
        }
    }
}

