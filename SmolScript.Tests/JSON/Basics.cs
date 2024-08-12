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
        var code = @"var obj = {one:1, two:'2', t:true, arr:[1, 2, 3]} // originally wanted to do JSON.parse('{val:1}') but that comes later...";

        var vm = SmolVM.Compile(code);

        vm.Run(); // this executes the code above -- declares a, sets to 1, declares a functiona and calls it

        var obj = vm.GetGlobalVar<JObject>("obj"); // verify that the var has the value we expect

        Assert.AreEqual(1.0, obj?.GetValue("one"));
        Assert.AreEqual("2", obj?.GetValue("two"));
        Assert.AreEqual(true, obj?.GetValue("t"));
        //Assert.AreEqual(0, ((JArray)(JToken)obj?.GetValue("arr")).Count); // Not working yet
    }
}
