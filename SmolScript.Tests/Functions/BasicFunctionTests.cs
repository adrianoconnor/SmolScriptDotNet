using System;
namespace SmolScript.Tests.Functions
{
	[TestClass]
	public class BasicFunctionTests
	{
        [TestMethod]
        public void TryCreatingByteCodeForAGlobalFunction()
        {
            var vm = SmolVM.Compile("function addOne(num) { return num + 1; } var a = addOne(2);");

            vm.Run();

            Assert.AreEqual(3.0, vm.GetGlobalVar<double>("a")); 
        }

        [TestMethod]
        public void PassVariableAsParamToFunc()
        {
            var source = @"
var a = 1;
function addOne(num) {
  return num + 1;
}
var b = addOne(a);
";

            var vm = SmolVM.Init(source);

            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("a"));
            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("b"));
        }

        [TestMethod]
        public void FunctionReturningVoid()
        {
            var vm = SmolVM.Compile(@"
var a = 1;
function a2() { a = 2; }
a2();");

            vm.Run();

            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("a"));
        }

        [TestMethod]
        public void FunctionVariable ()
        {
            var source = @"
var a = 1;
var addOne = function(num) {
  return num + 1;
}; // Not sure this semi-colon should be needed?!
var b = addOne(a);
";

            var vm = SmolVM.Init(source);

            Assert.AreEqual(1.0, vm.GetGlobalVar<double>("a"));
            Assert.AreEqual(2.0, vm.GetGlobalVar<double>("b"));
        }

        [TestMethod]
        public void FunctionVariableEquality()
        {
            var source = @"
var a = 1;
var addOne = function(num) {
  return num + 1;
};
var addOne2 = addOne;
var b = addOne == addOne2;
var c = addOne == true;
var d = addOne == false;
";

            var vm = SmolVM.Init(source);

            Assert.AreEqual(true, vm.GetGlobalVar<bool>("b"));
            Assert.AreEqual(false, vm.GetGlobalVar<bool>("c"));
            Assert.AreEqual(false, vm.GetGlobalVar<bool>("d"));
        }

    }
}

