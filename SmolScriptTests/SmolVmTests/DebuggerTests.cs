using System;
using SmolScript;
using SmolScript.Internals;

namespace SmolTests
{
    [TestClass]
    public class DebuggerTests
	{
        public DebuggerTests()
		{


		}

        [TestMethod]
        public void TryCreatingByteCodeForAGlobalFunction()
        {
            var program = SmolCompiler.Compile("debugger; var a = 10; function addOne(num) { return num + 1; } a = addOne(a);");

            var vm = new SmolVM(program);

            vm.Run();

            //Assert.AreEqual(3.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

    }
}

