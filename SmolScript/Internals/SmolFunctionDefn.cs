using System;
namespace SmolScript.Internals
{
	internal class SmolFunctionDefn
	{
        public string? globalFunctionName;
        public int code_section;
		public int arity;
		public List<string> param_variable_names = new List<string>();

		public SmolFunctionDefn()
		{
		}
	}
}

