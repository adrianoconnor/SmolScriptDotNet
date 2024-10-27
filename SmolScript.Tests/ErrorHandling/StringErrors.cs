using SmolScript.Internals;

namespace SmolScript.Tests.ErrorHandling
{
    [TestClass]
    public class StringErrors
    {
        [TestMethod]
        public void InvalidStringFromBackticks()
        {
            Assert.Inconclusive(); // Need to refactor exceptions -- right now they're internal

            // TODO: We really need to test the error handling to make sure we get a good error

            Assert.ThrowsException<Exception>(() =>
            {
                var program = SmolVm.Compile("var n = 'x'; var a = `test ${!!!}`;");
            });
        }


        [TestMethod]
        public void IllegalMultilineStrings()
        {
            //Assert.Inconclusive();

            var source = @"var a = 'test
123';";

            Assert.ThrowsException<ScannerError>(() => SmolVm.Compile(source));
        }
    }
}

