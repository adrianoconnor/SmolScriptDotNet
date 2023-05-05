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

            Assert.AreEqual(6.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void BitWiseOr()
        {
            var vm = SmolVM.Init("var a = 11 | 4; var b = 12 | 4;");

            Assert.AreEqual(15, vm.GetGlobalVar<int>("a"));
            Assert.AreEqual(12, vm.GetGlobalVar<int>("b"));
        }

        [TestMethod]
        public void BitWiseAnd()
        {
            var vm = SmolVM.Init("var a = 11 & 4; var b = 12 & 4;");

            Assert.AreEqual(0, vm.GetGlobalVar<int>("a"));
            Assert.AreEqual(4, vm.GetGlobalVar<int>("b"));
        }

        [TestMethod]
        public void SubtractNumbers()
        {
            var program = SmolCompiler.Compile("var a = 3 - 1;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void DivideNumbers()
        {
            var program = SmolCompiler.Compile("var a = 6 / 2;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(3.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void Bidmas()
        {
            var program = SmolCompiler.Compile("var a = 4 * 2 + 1 / 2;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(8.5, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void Bidmas2()
        {
            var program = SmolCompiler.Compile("var a = 4 * ((3 + 1) / 2 + 1);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(12.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void Remainder()
        {
            var program = SmolCompiler.Compile("var a = 4 % 3;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void Power()
        {
            var program = SmolCompiler.Compile("var a = 4 ** 2;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(16.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void NegativeUnaryOperator()
        {
            var program = SmolCompiler.Compile("var a = -4; a = -a; var b = -a;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(4.0, vm.GetGlobalVar<double>("a"));
            Assert.AreEqual(-4.0, vm.GetGlobalVar<double>("b"));
        }

        [TestMethod]
        public void PlusPlus()
        {
            var program = SmolCompiler.Compile("var a = 1; var b = ++a; a++;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(3.0, vm.GetGlobalVar<double>("a"));
            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("b"));
        }

        [TestMethod]
        public void MinusMinus()
        {
            var program = SmolCompiler.Compile("var a = 3; var b = --a; var c = a--; --a; a--;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(-1.0, vm.GetGlobalVar<double>("a"));
            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("b"));
            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("c"));

        }
    }
}

