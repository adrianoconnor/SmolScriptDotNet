namespace SmolScript.Internals.SmolVariableTypes
{
    internal class SmolNumber : SmolVariableType
    {
        internal readonly double NumberValue;

        internal SmolNumber(double numberValue)
        {
            this.NumberValue = numberValue;
        }

        internal override object? GetValue()
        {
            return this.NumberValue;
        }

        public override string ToString()
        {
            return $"(Number) {NumberValue}";
        }

        public override int GetHashCode()
        {
            return this.NumberValue.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (this.GetType() == typeof(SmolNumber) && obj?.GetType() == typeof(SmolNumber))
            {
                return ((SmolNumber)this).NumberValue.Equals(((SmolNumber)obj!).NumberValue);
            }
            else
            {
                return base.Equals(obj);
            }
        }
    }
}

