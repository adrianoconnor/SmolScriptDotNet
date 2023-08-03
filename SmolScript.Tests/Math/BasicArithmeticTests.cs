using System;
namespace SmolScript.Tests.Math
{
    [TestClass]
    public class BasicArithmeticTests
    {
        [TestMethod]
        public void AddThreeNumbers()
        {
            var vm = SmolVM.Compile("var a = 1 + 2 + 3;");

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
            var vm = SmolVM.Compile("var a = 3 - 1;");

            vm.Run();

            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void DivideNumbers()
        {
            var vm = SmolVM.Compile("var a = 6 / 2;");

            vm.Run();

            Assert.AreEqual(3.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void Bidmas()
        {
            var vm = SmolVM.Compile("var a = 4 * 2 + 1 / 2;");

            vm.Run();

            Assert.AreEqual(8.5, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void Bidmas2()
        {
            var vm = SmolVM.Compile("var a = 4 * ((3 + 1) / 2 + 1);");

            vm.Run();

            Assert.AreEqual(12.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void Remainder()
        {
            var vm = SmolVM.Compile("var a = 4 % 3;");

            vm.Run();

            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void Power()
        {
            var vm = SmolVM.Compile("var a = 4 ** 2;");

            vm.Run();

            Assert.AreEqual(16.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void NegativeUnaryOperator()
        {
            var vm = SmolVM.Compile("var a = -4; a = -a; var b = -a;");

            vm.Run();

            Assert.AreEqual(4.0, vm.GetGlobalVar<double>("a"));
            Assert.AreEqual(-4.0, vm.GetGlobalVar<double>("b"));
        }

    }
}

