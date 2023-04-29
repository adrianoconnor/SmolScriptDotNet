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
     
            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("b"));
            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("c"));
        }

        [TestMethod]
        public void TernaryStatementExpression()
        {
            var program = @"
var a = 0;

function a1() { a = 1; }
function a2() { a = 2; }

a == 0 ? a1() : a2();
";

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void TernaryInsideTernary()
        {
            var program = @"
var a = 2;
var b = 0;
var c = 0;

b = ((a == 0) ? 1 : ((a == 1) ? 2 : 3)); // TODO: This only works with grouping, take that away and it fails
c = a == 0 ? 1 : a == 1 ? 2 : 3; "; // TODO: this should also work...

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(3.0, vm.GetGlobalVar<double>("b"));
            //Assert.AreEqual(3.0, ((SmolValue)vm.globalEnv.Get("c")!).value);
        }
    }
}

