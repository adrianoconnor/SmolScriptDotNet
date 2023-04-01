using System;
using SmolScript;
using SmolScript.Internals;

namespace SmolTests.SmolVmTests
{
    [TestClass]
    public class TernaryExpressions
    {
        public TernaryExpressions()
        {
        }

        [TestMethod]
        public void AssignWithTernary()
        {
            var program = SmolCompiler.Compile(@"
var a = true;
var b = a ? 1 : 2;
var c = !a ? 1 : 2;
");

            var vm = new SmolVM(program);

            vm.Run();
     
            Assert.AreEqual(1.0, ((SmolValue)vm.globalEnv.Get("b")!).value);
            Assert.AreEqual(2.0, ((SmolValue)vm.globalEnv.Get("c")!).value);
        }

        [TestMethod]
        public void TernaryStatementExpression()
        {
            var program = @"
var a = 0;

function a1() { a = 1; }
function a2() { a = 2; }

a == 0 ? 1 : 2; // a1(), a2() Fails because we can't return void yet
";

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(0.0, ((SmolValue)vm.globalEnv.Get("a")!).value);
        }
    }
}

