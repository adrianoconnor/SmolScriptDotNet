using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolTryRegionSaveState : SmolStackType
    {
        internal int code_section;
        internal int PC;
        internal Environment this_env;
        internal int jump_exception;

        internal SmolTryRegionSaveState(int code_section, int PC, Environment this_env, int jump_exception)
        {
            this.code_section = code_section;
            this.PC = PC;
            this.this_env = this_env;
            this.jump_exception = jump_exception;
        }
    }
}

