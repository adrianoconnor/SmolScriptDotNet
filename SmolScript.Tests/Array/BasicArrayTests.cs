﻿using System;
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

			vm.RunInDebug();

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

            //Console.WriteLine(((SmolVM)vm).Decompile());

            //vm.OnDebugLog = Console.WriteLine;

            vm.RunInDebug();

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
";

            var vm = SmolVM.Compile(code);

            //Console.WriteLine(((SmolVM)vm).Decompile());

            //vm.OnDebugLog = Console.WriteLine;

            vm.RunInDebug();

            Assert.AreEqual(3, vm.GetGlobalVar<int>("b"));
            //Assert.IsNull(vm.GetGlobalVar<int>("c")); // It's undefined, but no way to tell .net this yet
        }
    }
}
