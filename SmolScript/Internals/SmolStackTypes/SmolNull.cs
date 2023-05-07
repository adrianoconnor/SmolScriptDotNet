﻿namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolNull : SmolVariableType
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

