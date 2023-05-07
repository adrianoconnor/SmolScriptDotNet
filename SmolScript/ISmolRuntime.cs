using System;
namespace SmolScript
{
    public interface ISmolRuntime
    {
        /// <summary>
        /// Static method that takes a smol source string
        /// and compiles the program but does not execute
        /// any code
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns>A SmolVM instance</returns>
        public static ISmolRuntime Compile(string sourceCode) => throw new NotImplementedException();

        /// <summary>
        /// Static method that takes a smol source string,
		/// compiles it and then immediately executes the
		/// top level statements, preparing the global
		/// environment and making it ready to call from .net
        /// </summary>
        /// <param name="sourceCode"></param>
        /// <returns>A SmolVM instance</returns>
        public static ISmolRuntime Init(string sourceCode) => throw new NotImplementedException();


        /// <summary>
        /// Set a limit on the maximum stack size for the smol vm,
        /// constraining the amount of resouces a smol program can
        /// consume
        /// </summary>
        int MaxStackSize { get; set; }

        T GetGlobalVar<T>(string variableName);

        void Call(string functionName, params object[] args);
        T Call<T>(string functionName, params object[] args);

        void Run();
        void Reset();
        void Step();

        Action<string> OnDebugLog { set; }
    }
}

