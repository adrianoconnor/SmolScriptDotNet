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
	}
}

