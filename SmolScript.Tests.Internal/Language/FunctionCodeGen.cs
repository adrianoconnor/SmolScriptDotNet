using System;
using SmolScript;
using SmolScript.Internals;

namespace SmolTests
{
    [TestClass]
    public class FunctionCodeGen
    {
        public FunctionCodeGen()
        {


        }

        [TestMethod]
        public void TryCreatingByteCodeForAGlobalFunction()
        {
            var program = SmolCompiler.Compile("function addOne(num) { return num + 1; } var a = addOne(2);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(3.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void PassVariableAsParamToFunc()
        {
            var program = SmolCompiler.Compile("var a = 1; function addOne(num) { return num + 1; } var b = addOne(a);");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("a"));
            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("b"));
        }

        [TestMethod]
        public void FunctionReturningVoid()
        {
            var program = SmolCompiler.Compile(@"
var a = 1;
function a2() { a = 2; }
a2();");

            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("a"));
        }

    }
}

