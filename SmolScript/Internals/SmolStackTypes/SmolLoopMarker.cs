using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolLoopMarker : SmolStackType
    {
        internal Environment SavedEnvironment;

        internal SmolLoopMarker(Environment savedEnvironment)
        {
            this.SavedEnvironment = savedEnvironment;
        }
    }
}

