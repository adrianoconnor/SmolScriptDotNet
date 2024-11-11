namespace SmolScript.Internals.SmolVariableTypes
{
    internal class SmolBool : SmolVariableType
    {
        internal readonly bool BoolValue;

        internal SmolBool(bool boolValue)
        {
            this.BoolValue = boolValue;
        }

        internal override object? GetValue()
        {
            return this.BoolValue;
        }

        public override string ToString()
        {
            return $"(SmolBool) {BoolValue}";
        }
    }
}

