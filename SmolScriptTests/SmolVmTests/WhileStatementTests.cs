using System;
using SmolScript;
using SmolScript.Internals;

namespace SmolTests.SmolVmTests
{
    [TestClass]
    public class WhileStatementTests
    {
        public WhileStatementTests()
        {
        }

        [TestMethod]
        public void BasicWhileStatementTest()
        {
            var program = SmolCompiler.Compile(@"
var a = 0;

var alwaysTrueCalled = 0;
function alwaysTrue() {
    alwaysTrueCalled = alwaysTrueCalled + 1;
    return true;
}

while(a < 100000 && alwaysTrue()) {
  a = a + 1;
}
");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(100000.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
            Assert.AreEqual(100000.0, ((SmolValue)vm.globalEnv.Get("alwaysTrueCalled")!).value);
        }

        [TestMethod]
        public void ForLoopTest()
        {
            var program = SmolCompiler.Compile(@"
var a = 0;

for(var b = 0; b < 10; b++) {
  a = a + 1;
}
");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(10.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }

    }
}

