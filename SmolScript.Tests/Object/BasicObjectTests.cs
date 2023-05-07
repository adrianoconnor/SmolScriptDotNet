using System;
namespace SmolScript.Tests.Array
{
    [TestClass]
    public class BasicObjectTests
    {
        [TestMethod]
        public void CreateObjectWithNew()
        {
            var code = @"
var g = [123];
var o = new Object();
o.x = function() { return g; };
var a = o.x()[0];
";

            var vm = SmolVM.Compile(code);

            Console.WriteLine(((SmolVM)vm).Decompile());

            vm.OnDebugLog = Console.WriteLine;

            vm.Run();

            Assert.AreEqual(123, vm.GetGlobalVar<int>("a"));

        }

        [TestMethod]
        public void CreateObjectWithShorthand()
        {
            var code = @"
var o = { z: 1 };
var b = o.z;
";

            var vm = SmolVM.Compile(code);

            Console.WriteLine(((SmolVM)vm).Decompile());

            vm.OnDebugLog = Console.WriteLine;

            vm.Run();

            Assert.AreEqual(1, vm.GetGlobalVar<int>("b"));
        }


        [TestMethod]
        public void CreateNestedObjects()
        {
            var code = @"
var o1 = {
    o1: {
      x: 0
    },

    o2: [ {}, {}, {}, {
        o4: {
            x: 99
        }
    }]
    
};

var a = o1.o2[3].o4.x;

o1.o2[3].o4.x += 1;

var b = o1.o2[3].o4.x;
";

            var vm = SmolVM.Compile(code);

            //Console.WriteLine(((SmolVM)vm).Decompile());

            vm.OnDebugLog = Console.WriteLine;

            vm.Run();

            Assert.AreEqual(99, vm.GetGlobalVar<int>("a"));
            Assert.AreEqual(100, vm.GetGlobalVar<int>("b"));
        }


    }
}

