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

        [TestMethod]
        public void Remainder()
        {
            var program = SmolCompiler.Compile("var a = 4 % 3;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(1.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

        [TestMethod]
        public void Power()
        {
            var program = SmolCompiler.Compile("var a = 4 ** 2;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(16.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

        [TestMethod]
        public void NegativeUnaryOperator()
        {
            var program = SmolCompiler.Compile("var a = -4; a = -a; var b = -a;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(4.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
            Assert.AreEqual(-4.0, ((SmolValue)vm.globalEnv.Get("b")!).value);
        }

        [TestMethod]
        public void PlusPlus()
        {
            var program = SmolCompiler.Compile("var a = 1; var b = ++a; a++;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(3.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
            Assert.AreEqual(2.0, ((SmolValue)vm.globalEnv.Get("b")!).value);
        }

        [TestMethod]
        public void MinusMinus()
        {
            var program = SmolCompiler.Compile("var a = 3; var b = --a; var c = a--; --a; a--;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(-1.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
            Assert.AreEqual(2.0, ((SmolValue)vm.globalEnv.Get("b")!).value);
            Assert.AreEqual(2.0, ((SmolValue)vm.globalEnv.Get("c")!).value);

        }
    }
}

