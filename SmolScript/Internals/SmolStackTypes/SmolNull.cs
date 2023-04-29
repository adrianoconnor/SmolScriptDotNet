using System;
namespace SmolScript.Internals.SmolStackTypes
{
	internal class SmolNull: SmolStackValue
	{
		internal SmolNull()
		{
		}

        internal override object? GetValue()
        {
            return null;
        }

        public override string ToString()
        {
            return "(SmolNull)";
        }
    }
}

