using System;
namespace SmolScript.Internals
{
	public struct SmolTrySaveState
	{
        public int code_section;
		public int PC;
		public Environment this_env;
		public int jump_exception;
	}
}

