using System;
namespace SmolScript.Internals.SmolStackTypes
{
	internal class SmolString: SmolStackValue, ISmolNativeCallable
	{
		internal readonly string value;

		internal SmolString(string value)
		{
			this.value = value;
		}

        internal override object? GetValue()
        {
            return this.value;
        }

        public override string ToString()
        {
            return $"(SmolString) {value}";
        }

        public SmolStackValue GetProp(string propName)
        {
            switch (propName)
            {
                case "length":
                    return new SmolNumber(this.value.Length);

                default:
                    throw new Exception($"{this.GetType()} cannot handle native property {propName}");
            }
        }
    }
}
