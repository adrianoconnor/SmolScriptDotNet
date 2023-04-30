using System;
namespace SmolScript.Internals.SmolStackTypes
{
	internal class SmolString: SmolStackValue
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

    
    }
}

