using SmolScript.Internals;

namespace SmolScript.Tests.ErrorHandling
{
    [TestClass]
    public class StringErrors
    {
        [TestMethod]
        public void InvalidStringFromBackticks()
        {
            var ex = Assert.ThrowsException<SmolCompilerError>(() =>
            {
                var program = SmolVm.Compile("var n = 'x'; var a = `test ${!!!}`;");
            });
            
            Assert.AreEqual("Encounted one or more errors while parsing (first error: Parser did not expect to see '`test ${!!!}' here (Line 1, Col 22))", ex.Message);
            Assert.AreEqual("Parser did not expect to see '`test ${!!!}' here (Line 1, Col 22)", ex.ParserErrors.FirstOrDefault().Message);
        }


        [TestMethod]
        public void IllegalMultilineStrings()
        {
            var source = @"var a = 'test
123';";
            var ex = Assert.ThrowsException<SmolCompilerError>(() => SmolVm.Compile(source));
            
            Assert.AreEqual("Unexpected Line break in string (line 1)", ex.Message);
        }
    }
}

