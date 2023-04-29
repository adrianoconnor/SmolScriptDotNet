using System;
using SmolScript;
using SmolScript.Internals;

namespace SmolTests.SmolVmTests
{
	[TestClass]
	public class SimpleScopeTests
	{
		public SimpleScopeTests()
		{
		}

		[TestMethod]
		public void NewScopeForIf()
		{
            var program = SmolCompiler.Compile(@"
var a = 1;
var b = 0;

if (true) {
  var a = 2;
  if (true) {
    a = 3;
    b = a;
  }
}
");
            var vm = new SmolVM(program);

            vm.Run();

            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("a"));
            Assert.AreEqual(3.0, vm.GetGlobalVar<double>("b"));
        }
    }
}

