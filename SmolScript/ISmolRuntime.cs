using System;
namespace SmolScript
{
    public delegate object NativeFunctionDelegate(params object[] parameters);

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

        /// <summary>
        /// Sets a limit on the number of cycles in the VM that a single program execution
        /// is allowed to consume. Prevents infinite loops etc.
        /// </summary>
        int MaxCycleCount { get; set; }

        /// <summary>
        /// Retrieve the value of a global variable from the VM after execution
        /// </summary>
        /// <typeparam name="T">The generic type to cast to</typeparam>
        /// <param name="variableName">The name of the variable to get</param>
        /// <returns>The value of the variable. If the variable is not defined returns null for nullable types, and throws if not nullable</returns>
        T? GetGlobalVar<T>(string variableName);
        List<T>? GetGlobalVarAsArray<T>(string variableName);

        void Call(string functionName, params object[] args);
        T? Call<T>(string functionName, params object[] args);

        void RegisterMethod(string methodName, object lambda);

        void Run();
        void Reset();
        void Step();

        Action<string> OnDebugLog { set; }

	    string Decompile();
    }
}

