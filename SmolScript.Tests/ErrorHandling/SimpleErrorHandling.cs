using System;
namespace SmolScript.Tests.ErrorHandling
{
    [TestClass]
    public class SimpleErrorHandling
    {
        [TestMethod]
        public void ReferenceUndefinedVariable()
        {
            var code = @"var a = b * 2;";

            var e = Assert.ThrowsException<SmolRuntimeException>(() => SmolVm.Init(code));

            Assert.AreEqual("Unable to multiply Undefined and Number", e.Message);
        }
    }
}

