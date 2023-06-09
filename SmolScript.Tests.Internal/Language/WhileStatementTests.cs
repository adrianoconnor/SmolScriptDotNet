﻿using System;
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

            Assert.AreEqual(100000.0, vm.GetGlobalVar<double>("a"));
            Assert.AreEqual(100000.0, vm.GetGlobalVar<double>("alwaysTrueCalled"));
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

            Assert.AreEqual(10.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void BreakInWhileTest()
        {
            var program = SmolCompiler.Compile(@"
var a = 0;
var b = 0;

while(a <= 10) {

  b++;

  if (a >= 5) {
    break;
  }

  a = a + 1;
}
");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(5.0, vm.GetGlobalVar<double>("a"));
            Assert.AreEqual(6.0, vm.GetGlobalVar<double>("b"));
        }


        [TestMethod]
        public void ContinueInWhileTest()
        {
            var program = SmolCompiler.Compile(@"
var a = 0;
var b = 0;

while(++a <= 10) {

  if (a >= 5) {
    continue;
  }

  b++;
}
");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(11.0, vm.GetGlobalVar<double>("a"));
            Assert.AreEqual(4.0, vm.GetGlobalVar<double>("b"));
        }

    }
}

