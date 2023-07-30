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

            Assert.AreEqual("test", vm.GetGlobalVar<string>("a"));
        }

        [TestMethod]
        public void ConcatStringAndNumber()
        {
            var program = SmolCompiler.Compile("var a = \"test\" + 1;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual("test1", vm.GetGlobalVar<string>("a"));
        }

        [TestMethod]
        public void CreateAndAssignStringUsingSingleQuotes()
        {
            var program = SmolCompiler.Compile("var a = 'test';");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual("test", vm.GetGlobalVar<string>("a"));
        }

        [TestMethod]
        public void CreateAndAssignStringWithSingleQuotesUsingDoubleQuotes()
        {
            var program = SmolCompiler.Compile("var a = \"'test'\";");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual("'test'", vm.GetGlobalVar<string>("a"));
        }

        [TestMethod]
        public void CreateAndAssignStringWithEscapedDoubleQuotes()
        {
            var program = SmolCompiler.Compile(@"var a = ""\""test\t\r\n123\"""";");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual("\"test\t\r\n123\"", vm.GetGlobalVar<string>("a"));
        }

        [TestMethod]
        public void AddTwoStrings()
        {
            var program = SmolCompiler.Compile("var a = 'test'; var b = '123'; var c = a + b;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual("test123", vm.GetGlobalVar<string>("c"));
        }

        [TestMethod]
        public void CreateAndAssignStringUsingBackticks()
        {
            var program = SmolCompiler.Compile("var n = 'x'; var a = `test ${n}`;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual("test x", vm.GetGlobalVar<string>("a"));
        }

        [TestMethod]
        public void InvalidStringFromBackticks()
        {
            // TODO: We really need to test the error handling to make sure we get a good error

            Assert.ThrowsException<ParseError>(() =>
            {
                var program = SmolCompiler.Compile("var n = 'x'; var a = `test ${!!!}`;");
            });
        }

        [TestMethod]
        public void BacktickStringHandlesEmbeddedRightBrace()
        {
            var program = SmolCompiler.Compile("var n = 'x'; var a = `test ${\"x}x\"}`;");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual("test x}x", vm.GetGlobalVar<string>("a"));
        }

        [TestMethod]
        public void IllegalMultilineStrings()
        {
            var source = @"var a = 'test
123';";

            /*
            var s = new Scanner(source);
            var (t, e) = s.ScanTokens();
            var p = new Parser(t);
            var x = p.Parse();
            */

            Assert.ThrowsException<ScannerError>(() => SmolVM.Compile(source));
        }
    }
}

