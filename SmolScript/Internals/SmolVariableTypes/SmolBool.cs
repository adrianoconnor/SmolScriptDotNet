namespace SmolScript.Internals.SmolVariableTypes
{
    internal class SmolBool : SmolVariableType
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

