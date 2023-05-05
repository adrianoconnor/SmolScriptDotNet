using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolRuntimeException : SmolStackValue
    {
        internal string message;

        internal SmolRuntimeException(string message)
        {
            this.message = message;
        }

        internal override object? GetValue()
        {
            return null;
        }
    }
}

