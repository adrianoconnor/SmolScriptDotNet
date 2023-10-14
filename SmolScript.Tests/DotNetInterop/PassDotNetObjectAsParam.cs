using System.Reflection;
using SmolScript;

namespace SmolScript.Tests.DotNetInterop;

internal class TestPassObj
{
    public int valueField = 999;

    public int valueGetterSetter
    {
        get
        {
            return valueField;
        }

        set
        {
            this.valueField = value;
        }
    }

    private int privateValue = 111;

    public int getPrivateValue()
    {
        return privateValue;
    }

    public void setPrivateValue(int newValue)
    {
        this.privateValue = newValue;
    }
}

[TestClass]
public class PassDotNetObjectAsParam
{
    [TestMethod]
    public void CallSimpleSmolGlobalFunctionFromDotNet()
    {
        var code = @"var result1 = null;
var result2 = null;
var result3 = null;
var result4 = null;
var result5 = null;
var result6 = null;

function test(obj) {
    result1 = obj.valueField;
    result2 = obj.valueGetterSetter
    obj.valueGetterSetter = 123;
    result3 = obj.valueField;
    obj.valueField = 0;
    result4 = obj.valueGetterSetter;

    result5 = obj.getPrivateValue();
    obj.setPrivateValue(321);
    result6 = obj.getPrivateValue();
}";

        var vm = SmolVM.Init(code);

        var obj = new TestPassObj();

        vm.Call("test", obj);

        Assert.AreEqual(999, vm.GetGlobalVar<int>("result1"));
        Assert.AreEqual(999, vm.GetGlobalVar<int>("result2"));
        Assert.AreEqual(123, vm.GetGlobalVar<int>("result3"));
        Assert.AreEqual(0, vm.GetGlobalVar<int>("result4"));
        Assert.AreEqual(111, vm.GetGlobalVar<int>("result5"));
        Assert.AreEqual(321, vm.GetGlobalVar<int>("result6"));
    }
}