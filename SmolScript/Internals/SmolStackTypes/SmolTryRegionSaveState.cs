using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolTryRegionSaveState : SmolStackType
    {
        internal int CodeSection;
        internal int ProgramCounter;
        internal Environment ThisEnv;
        internal int JumpException;

        internal SmolTryRegionSaveState(int codeSection, int programCounter, Environment thisEnv, int jumpException)
        {
            this.CodeSection = codeSection;
            this.ProgramCounter = programCounter;
            this.ThisEnv = thisEnv;
            this.JumpException = jumpException;
        }
    }
}

