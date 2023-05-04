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

			vm.RunInDebug();

            Assert.AreEqual(123, vm.GetGlobalVar<int>("a"));
   
        }

    }
}

