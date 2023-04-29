using System;
namespace SmolScript.Internals.SmolStackTypes
{
	internal class SmolLoopMarker : SmolStackValue
    {
        internal Environment current_env;

        internal SmolLoopMarker(Environment current_env)
		{
            this.current_env = current_env;
		}

        internal override object? GetValue()
        {
            return null;
        }
    }
}

