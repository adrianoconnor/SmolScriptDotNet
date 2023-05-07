using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolLoopMarker : SmolStackType
    {
        internal Environment current_env;

        internal SmolLoopMarker(Environment current_env)
        {
            this.current_env = current_env;
        }
    }
}

