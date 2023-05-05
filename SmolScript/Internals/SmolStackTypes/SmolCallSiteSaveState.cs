using System;
namespace SmolScript.Internals.SmolStackTypes
{
    internal class SmolCallSiteSaveState : SmolStackValue
    {
        internal int code_section;
        internal int PC;
        internal Environment previous_env;
        internal bool call_is_extern;

        internal SmolCallSiteSaveState(int code_section, int PC, Environment previous_env, bool call_is_extern)
        {
            this.code_section = code_section;
            this.PC = PC;
            this.previous_env = previous_env;
            this.call_is_extern = call_is_extern;
        }

        internal override object? GetValue()
        {
            return null;
        }
    }
}

