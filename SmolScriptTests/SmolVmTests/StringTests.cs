using System;
using SmolScript;
using SmolScript.Internals;

namespace SmolTests
{
    [TestClass]
    public class StringTests
	{
        public StringTests()
		{


		}

        [TestMethod]
        public void CreateAndAssignStringUsingDoubleQuotes()
        {
            var program = SmolCompiler.Compile("var a = \"test\";");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual("test", ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

        [TestMethod]
        public void CreateAndAssignStringUsingSingleQuotes()
        {
            var program = SmolCompiler.Compile("var a = 'test';");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual("test", ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

        [TestMethod]
        public void CreateAndAssignStringWithSingleQuotesUsingDoubleQuotes()
        {
            var program = SmolCompiler.Compile("var a = \"'test'\";");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual("'test'", ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

        [TestMethod]
        public void CreateAndAssignStringWithEscapedDoubleQuotes()
        {
            var program = SmolCompiler.Compile(@"var a = ""\""test\t\r\n123\"""";");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual("\"test\t\r\n123\"", ((SmolValue)vm.globalEnv.Get("a")!).value);
        }
    }
}

