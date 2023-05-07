using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolRuntimeException : SmolStackType
    {
        internal string message;

        internal SmolRuntimeException(string message)
        {
            this.message = message;
        }
    }
}

