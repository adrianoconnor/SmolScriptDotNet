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
        public void ArraySquareBrackets()
        {
            var code = @"
var a = new Array(1, 2, 3);
a[1] *= 2;
a[1] /= 2;
a[1] += 2;
a[1] -= 2;
a[1] **= 2;
var b = a[0] + a[a[0]] + a[2];
";

            var vm = SmolVM.Compile(code);

            vm.Run();

            Assert.AreEqual(8, vm.GetGlobalVar<int>("b"));
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


        [TestMethod]
        public void PreviousFailingFromJsVersion()
        {
            var code = @"
    var a1 = new Array();
    var a2 = [];
    var l1 = a1.length;
    var l2 = a2.length;
    a1.push(1);
    a1[1] = 2;
    a1[2] = a1[0] + a1[1];
    var r1 = a1[2];
    a2[5] = 0;
    var l3 = a1.length;
    var l4 = a2.length;
";

            var vm = SmolVM.Compile(code);

            //vm.OnDebugLog = Console.WriteLine;
            vm.Run();

            Assert.AreEqual(0, vm.GetGlobalVar<int>("l1"));
            Assert.AreEqual(0, vm.GetGlobalVar<int>("l2"));
            Assert.AreEqual(3, vm.GetGlobalVar<int>("l3"));
            Assert.AreEqual(6, vm.GetGlobalVar<int>("l4"));
            Assert.AreEqual(3, vm.GetGlobalVar<int>("r1"));
        }

    }
}

