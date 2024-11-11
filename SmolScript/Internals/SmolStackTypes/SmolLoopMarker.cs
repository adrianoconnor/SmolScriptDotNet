using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolLoopMarker : SmolStackType
    {
        internal Environment CurrentEnv;

        internal SmolLoopMarker(Environment currentEnv)
        {
            this.CurrentEnv = currentEnv;
        }
    }
}

