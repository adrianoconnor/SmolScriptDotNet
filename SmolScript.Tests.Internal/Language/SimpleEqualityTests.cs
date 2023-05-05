using System;
using SmolScript;
using SmolScript.Internals;

namespace SmolTests
{
    [TestClass]
    public class SimpleEqualityTests
    {
        public SimpleEqualityTests()
        {
        }

        [TestMethod]
        public void EqualsEquals()
        {
            var program = SmolCompiler.Compile("var a = (1 == 1); var b = (1 == 2);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(true, vm.GetGlobalVar<bool>("a"));
            Assert.AreEqual(false, vm.GetGlobalVar<bool>("b"));
        }

        [TestMethod]
        public void NotEquals()
        {
            var program = SmolCompiler.Compile("var a = (1 != 2); var b = (1 != 1);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(false, vm.GetGlobalVar<bool>("b"));
            Assert.AreEqual(true, vm.GetGlobalVar<bool>("a"));
        }

        [TestMethod]
        public void GreaterThan()
        {
            var program = SmolCompiler.Compile("var a = (2 > 1); var b = (1 > 2);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(true, vm.GetGlobalVar<bool>("a"));
            Assert.AreEqual(false, vm.GetGlobalVar<bool>("b"));
        }

        [TestMethod]
        public void GreaterThanEquals()
        {
            var program = SmolCompiler.Compile("var a = (2 >= 1); var b = (1 >= 1);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(true, vm.GetGlobalVar<bool>("a"));
            Assert.AreEqual(true, vm.GetGlobalVar<bool>("b"));
        }

        [TestMethod]
        public void LessThan()
        {
            var program = SmolCompiler.Compile("var a = (1 < 2); var b = (2 < 1);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(true, vm.GetGlobalVar<bool>("a"));
            Assert.AreEqual(false, vm.GetGlobalVar<bool>("b"));
        }

        [TestMethod]
        public void LessThanEquals()
        {
            var program = SmolCompiler.Compile("var a = (1 <= 2); var b = (1 <= 1);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(true, vm.GetGlobalVar<bool>("a"));
            Assert.AreEqual(true, vm.GetGlobalVar<bool>("b"));
        }

        [TestMethod]
        public void UnaryNegation()
        {
            var program = SmolCompiler.Compile("var a = (!false); var b = (!true);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(true, vm.GetGlobalVar<bool>("a"));
            Assert.AreEqual(false, vm.GetGlobalVar<bool>("b"));
        }
    }
}