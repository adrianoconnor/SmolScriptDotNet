using System;
using SmolScript;
using SmolScript.Internals;

namespace SmolTests
{
    [TestClass]
    public class FunctionCodeGen
	{
        public FunctionCodeGen()
		{


		}

        [TestMethod]
        public void TryCreatingByteCodeForAGlobalFunction()
        {
            var program = SmolCompiler.Compile("function addOne(num) { return num + 1; } var a = addOne(2);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(3.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

        [TestMethod]
        public void PassVariableAsParamToFunc()
        {
            var program = SmolCompiler.Compile("var a = 1; function addOne(num) { return num + 1; } var b = addOne(a);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(1.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
            Assert.AreEqual(2.0, ((SmolValue)vm.globalEnv.Get("b")!).value);
        }

    }
}

