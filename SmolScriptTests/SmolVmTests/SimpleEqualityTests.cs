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

            Assert.AreEqual(true, ((SmolValue)vm.globalEnv.Get("a")!).value);
            Assert.AreEqual(false, ((SmolValue)vm.globalEnv.Get("b")!).value);
        }

        [TestMethod]
        public void NotEquals()
        {
            var program = SmolCompiler.Compile("var a = (1 != 2); var b = (1 != 1);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(true, ((SmolValue)vm.globalEnv.Get("a")!).value);
            Assert.AreEqual(false, ((SmolValue)vm.globalEnv.Get("b")!).value);
        }

        [TestMethod]
        public void GreaterThan()
        {
            var program = SmolCompiler.Compile("var a = (2 > 1); var b = (1 > 2);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(true, ((SmolValue)vm.globalEnv.Get("a")!).value);
            Assert.AreEqual(false, ((SmolValue)vm.globalEnv.Get("b")!).value);
        }

        [TestMethod]
        public void GreaterThanEquals()
        {
            var program = SmolCompiler.Compile("var a = (2 >= 1); var b = (1 >= 1);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(true, ((SmolValue)vm.globalEnv.Get("a")!).value);
            Assert.AreEqual(true, ((SmolValue)vm.globalEnv.Get("b")!).value);
        }

        [TestMethod]
        public void LessThan()
        {
            var program = SmolCompiler.Compile("var a = (1 < 2); var b = (2 < 1);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(true, ((SmolValue)vm.globalEnv.Get("a")!).value);
            Assert.AreEqual(false, ((SmolValue)vm.globalEnv.Get("b")!).value);
        }

        [TestMethod]
        public void LessThanEquals()
        {
            var program = SmolCompiler.Compile("var a = (1 <= 2); var b = (1 <= 1);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(true, ((SmolValue)vm.globalEnv.Get("a")!).value);
            Assert.AreEqual(true, ((SmolValue)vm.globalEnv.Get("b")!).value);
        }

        [TestMethod]
        public void UnaryNegation()
        {
            var program = SmolCompiler.Compile("var a = (!false); var b = (!true);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(true, ((SmolValue)vm.globalEnv.Get("a")!).value);
            Assert.AreEqual(false, ((SmolValue)vm.globalEnv.Get("b")!).value);
        }
    }
}