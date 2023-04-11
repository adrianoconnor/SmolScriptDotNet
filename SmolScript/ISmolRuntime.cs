using System;
namespace SmolScript
{
	public interface ISmolRuntime
	{
		public abstract static ISmolRuntime Compile(string sourceCode);

        int MaxStackSize { get; set; }

		T GetGlobalVar<T>(string variableName);
		void Call(string functionName);

        void Run();
		void RunInDebug();
		//Task RunAsync();

		string Decompile();
		string DumpAst();
	}
}

