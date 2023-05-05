using System;
using SmolScript;
using SmolScript.Internals;

namespace SmolTests
{
    [TestClass]
    public class DebuggerTests
    {
        public DebuggerTests()
        {


        }

        [TestMethod]
        public void TryCreatingByteCodeForAGlobalFunction()
        {
            var program = SmolCompiler.Compile("debugger; var a = 10; function addOne(num) { return num + 1; } a = addOne(a);");

            var vm = new SmolVM(program);

            vm.Run();

            vm.Step();

            Assert.AreEqual(10.0, vm.GetGlobalVar<double>("a"));

            vm.Run();

            Assert.AreEqual(11.0, vm.GetGlobalVar<double>("a"));
        }

    }
}

