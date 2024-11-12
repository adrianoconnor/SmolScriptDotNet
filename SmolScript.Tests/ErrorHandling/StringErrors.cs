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
            
            Assert.AreEqual("Encounted one or more errors while parsing", ex.Message);
            Assert.AreEqual("Parser did not expect to see '`test ${!!!}' here (Line 1, Col 22)", ex.ParserErrors.FirstOrDefault().Message);
        }


        [TestMethod]
        public void IllegalMultilineStrings()
        {
            //Assert.Inconclusive();

            var source = @"var a = 'test
123';";
            var ex = Assert.ThrowsException<ScannerError>(() => SmolVm.Compile(source));
            
            Assert.AreEqual("Unexpected Line break in string", ex.Message);
            Assert.AreEqual(2, ex.LineNumber);
        }
    }
}

