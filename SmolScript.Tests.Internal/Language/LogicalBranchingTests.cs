using System;
using SmolScript;
using SmolScript.Internals;

namespace SmolTests.SmolVmTests
{
    [TestClass]
    public class LogicalBranchingTests
    {
        public LogicalBranchingTests()
        {
        }

        [TestMethod]
        public void LogicalAnd()
        {
            var program = SmolCompiler.Compile(@"
var a = 0;

var alwaysTrueCalled = 0;
function alwaysTrue() {
    alwaysTrueCalled = alwaysTrueCalled + 1;
    return true;
}

var alwaysFalseCalled = 0;
function alwaysFalse() {
    alwaysFalseCalled = alwaysFalseCalled + 1;
    return false;
}

if (alwaysTrue() && alwaysFalse())
  a = 1;
else
  a = 2;
");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("a"));
            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("alwaysTrueCalled"));
            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("alwaysFalseCalled"));
        }

        [TestMethod]
        public void LogicalAndShortCircuit()
        {
            var program = SmolCompiler.Compile(@"
var a = 0;

var alwaysTrueCalled = 0;
function alwaysTrue() {
    alwaysTrueCalled = alwaysTrueCalled + 1;
    return true;
}

var alwaysFalseCalled = 0;
function alwaysFalse() {
    alwaysFalseCalled = alwaysFalseCalled + 1;
    return false;
}

if (alwaysFalse() && alwaysTrue())
  a = 1;
else
  a = 2;
");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("a"));
            Assert.AreEqual(0.0, vm.GetGlobalVar<double>("alwaysTrueCalled"));
            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("alwaysFalseCalled"));
        }


        [TestMethod]
        public void LogicalOr()
        {
            var program = SmolCompiler.Compile(@"
var a = 0;

var alwaysTrueCalled = 0;
function alwaysTrue() {
    alwaysTrueCalled = alwaysTrueCalled + 1;
    return true;
}

var alwaysFalseCalled = 0;
function alwaysFalse() {
    alwaysFalseCalled = alwaysFalseCalled + 1;
    return false;
}

if (alwaysFalse() || alwaysTrue())
  a = 1;
else
  a = 2;
");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("a"));
            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("alwaysTrueCalled"));
            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("alwaysFalseCalled"));
        }

        [TestMethod]
        public void LogicalOrShortCircuit()
        {
            var program = SmolCompiler.Compile(@"
var a = 0;

var alwaysTrueCalled = 0;
function alwaysTrue() {
    alwaysTrueCalled = alwaysTrueCalled + 1;
    return true;
}

var alwaysFalseCalled = 0;
function alwaysFalse() {
    alwaysFalseCalled = alwaysFalseCalled + 1;
    return false;
}

if (alwaysTrue() || alwaysFalse())
  a = 1;
else
  a = 2;
");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("a"));
            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("alwaysTrueCalled"));
            Assert.AreEqual(0.0, vm.GetGlobalVar<double>("alwaysFalseCalled"));
        }
    }
}

