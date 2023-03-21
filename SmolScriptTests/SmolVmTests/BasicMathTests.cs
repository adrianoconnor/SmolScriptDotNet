using System;
using SmolScript;
using SmolScript.Internals;

namespace SmolTests
{
    [TestClass]
    public class BasicMathTests
	{
        public BasicMathTests()
		{


		}

        [TestMethod]
        public void AddThreeNumbers()
        {
            var program = SmolCompiler.Compile("var a = 1 + 2 + 3;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(6.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

        [TestMethod]
        public void Bidmas()
        {
            var program = SmolCompiler.Compile("var a = 4 * 2 + 1 / 2;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(8.5, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

        [TestMethod]
        public void Bidmas2()
        {
            var program = SmolCompiler.Compile("var a = 4 * ((3 + 1) / 2 + 1);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(12.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }
    }
}

