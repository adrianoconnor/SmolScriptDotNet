using System;
using SmolScript;
using SmolScript.Internals;
using SmolScript.Internals.SmolStackTypes;
using SmolScript.Internals.SmolVariableTypes;

namespace SmolTests
{
    [TestClass]
    public class CheckForJunkOnStackAfterNew
    {
        [TestMethod]
        public void EmptyStackAfterNewOnClass()
        {
            var source = @"

class testClass1 {
  constructor()
  {
    var a = 1;
    1 + 2;
    a += 1;
    return a - 1;
  }
}

3 + 4;
var c = new testClass1(1, 2, 3);

";
            var program = Compiler.Compile(source);

            var vm = new SmolVm(program);

            Console.WriteLine(((SmolVm)vm).Decompile());

            vm.Run();

            Assert.IsNotNull(vm.globalEnv.Get("c"));
            Assert.AreEqual(0, vm.stack.Count);
        }
    }
}

