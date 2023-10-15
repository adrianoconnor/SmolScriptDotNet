using System.Reflection;
using Newtonsoft.Json.Linq;
using SmolScript;

namespace SmolScript.Tests.JSON;

[TestClass]
public class Basics
{
    [TestMethod]
    public void GetObjectFromVMAsJObject()
    {
        var code = @"
var obj = {val:1} // JSON.parse('{val:1}');
";

        var vm = SmolVM.Compile(code);

        vm.Run(); // this executes the code above -- declares a, sets to 1, declares a functiona and calls it

        var obj = vm.GetGlobalVar<JObject>("obj"); // verify that the var has the value we expect

        Assert.AreEqual(1.0, obj?.GetValue("val"));
    }
}
