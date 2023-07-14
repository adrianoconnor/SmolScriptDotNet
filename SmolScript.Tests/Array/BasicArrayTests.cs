using System;
namespace SmolScript.Tests.Array
{
    [TestClass]
    public class BasicArrayTests
    {
        [TestMethod]
        public void CreateArrayWithNew()
        {
            var code = @"
var a = new Array(1, 2, 3);
var b = a.length;
var c = a.pop();
var d = a.length;
a.push('x');
var e = a.length;
";

            var vm = SmolVM.Compile(code);

            Console.WriteLine(((SmolVM)vm).Decompile());

            vm.OnDebugLog = Console.WriteLine;

            vm.Run();

            Assert.AreEqual(3, vm.GetGlobalVar<int>("b"));
            Assert.AreEqual(3, vm.GetGlobalVar<int>("c"));
            Assert.AreEqual(2, vm.GetGlobalVar<int>("d"));
            Assert.AreEqual(3, vm.GetGlobalVar<int>("e"));
        }


        [TestMethod]
        public void ImplicityGrowArray()
        {
            var code = @"
var a = new Array();
a[0] = 1;
a[2] = 3;
var b = a.length;
var c = a[1];
var d = (c == undefined);
";

            var vm = SmolVM.Compile(code);

            vm.Run();

            Assert.AreEqual(3, vm.GetGlobalVar<int>("b"));
            Assert.AreEqual(true, vm.GetGlobalVar<bool>("d"));
            //Assert.IsNull(vm.GetGlobalVar<int>("c")); // It's undefined, but no way to tell .net this yet
        }

        [TestMethod]
        public void CreateArrayWithShorthandNoParams()
        {
            var code = @"
var a = [];
a[0] = 1;
a[2] = 3;
var b = a.length;
var c = a[2];
";

            var vm = SmolVM.Compile(code);

            vm.Run();

            Assert.AreEqual(3, vm.GetGlobalVar<int>("b"));
            Assert.AreEqual(3, vm.GetGlobalVar<int>("c"));
        }

        [TestMethod]
        public void CreateArrayWithShorthandAndElements()
        {
            var code = @"
var a = [1, 2, 3, []];
var b = a.length;
var c = a[2];
";

            var vm = SmolVM.Compile(code);

            vm.Run();

            Assert.AreEqual(4, vm.GetGlobalVar<int>("b"));
            Assert.AreEqual(3, vm.GetGlobalVar<int>("c"));
        }
    }
}

