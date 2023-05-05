using System;
using System.Diagnostics.CodeAnalysis;

namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolNumber : SmolStackValue
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

        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (this.GetType() == typeof(SmolNumber) && obj?.GetType() == typeof(SmolNumber))
            {
                return ((SmolNumber)this).value.Equals(((SmolNumber)obj!).value);
            }
            else
            {
                return base.Equals(obj);
            }
        }
    }
}

