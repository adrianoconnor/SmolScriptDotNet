using System;
namespace SmolScript.Internals
{
	internal struct SmolCallSaveState
	{
        public int code_section;
		public int PC;
		public Environment previous_env;
		public bool call_is_extern;
	}
}

