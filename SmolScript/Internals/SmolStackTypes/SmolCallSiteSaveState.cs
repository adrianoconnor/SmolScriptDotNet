using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolCallSiteSaveState : SmolStackType
    {
        internal int SavedCodeSection;
        internal int SavedInstructionPointer;
        internal Environment SavedEnvironment;
        internal bool CallIsExternal;

        internal SmolCallSiteSaveState(int savedCodeSection, int savedInstructionPointer, Environment savedEnvironment, bool callIsExternal)
        {
            this.SavedCodeSection = savedCodeSection;
            this.SavedInstructionPointer = savedInstructionPointer;
            this.SavedEnvironment = savedEnvironment;
            this.CallIsExternal = callIsExternal;
        }
    }
}

