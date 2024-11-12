using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolCallSiteSaveState : SmolStackType
    {
        internal int CodeSection;
        internal int InstructionPointer;
        internal Environment Environment;
        internal bool CallIsExternal;

        internal SmolCallSiteSaveState(int codeSection, int instructionPointer, Environment environment, bool callIsExternal)
        {
            this.CodeSection = codeSection;
            this.InstructionPointer = instructionPointer;
            this.Environment = environment;
            this.CallIsExternal = callIsExternal;
        }
    }
}

