using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolCallSiteSaveState : SmolStackType
    {
        internal int CodeSection;
        internal int Pc;
        internal Environment PreviousEnv;
        internal bool CallIsExtern;

        internal SmolCallSiteSaveState(int codeSection, int pc, Environment previousEnv, bool callIsExtern)
        {
            this.CodeSection = codeSection;
            this.Pc = pc;
            this.PreviousEnv = previousEnv;
            this.CallIsExtern = callIsExtern;
        }
    }
}

