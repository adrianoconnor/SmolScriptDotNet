using System;
using SmolScript;
using SmolScript.Internals;

namespace SmolScript.Tests.Debugger
{
    [TestClass]
    public class DebuggerTests
    {
        [TestMethod]
        public void StepThroughBasics()
        {
            var source = @"
debugger;
var a = 10;
function addOne(num) {
    return num + 1;
}
a = addOne(a);
";
            var vm = SmolVm.Compile(source);

            vm.Run();

            vm.Step();

            Assert.AreEqual(10.0, vm.GetGlobalVar<double>("a"));

            vm.Run();

            Assert.AreEqual(11.0, vm.GetGlobalVar<double>("a"));
        }
    }
}

