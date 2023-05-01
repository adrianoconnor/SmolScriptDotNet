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
//var b = a.length
";

            var vm = SmolVM.Init(code);

            //Assert.AreEqual(0, vm.GetGlobalVar<int>("b"));
        }
	}
}

