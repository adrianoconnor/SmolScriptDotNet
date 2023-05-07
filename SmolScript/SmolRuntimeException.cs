using System;
namespace SmolScript
{
    public class SmolRuntimeException : Exception
    {
        public SmolRuntimeException(string message) : base(message)
        {
        }

        public SmolRuntimeException(string message, Exception innerException) : base(message, innerException)
        {            
        }
    }
}

