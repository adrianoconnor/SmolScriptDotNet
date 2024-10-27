using SmolScript.Internals.SmolStackTypes;
using SmolScript.Internals.SmolVariableTypes;

namespace SmolScript.Tests.Internal.Types;

[TestClass]
public class Equality
{
    [TestMethod]
    public void SmolNumberEquality()
    {
        var vm = SmolVm.Init("var a = 1; var b = 2; var c = 1;");

        Assert.AreEqual(1, vm.GetGlobalVar<int>("a"));
        Assert.AreEqual(2, vm.GetGlobalVar<int>("b"));
        Assert.AreEqual(1, vm.GetGlobalVar<int>("c"));

        // We will get the values as a mixture of base type (which is how
        // they're defined in the stack) and actual type (which is how
        // they're often used internally) -- this let's us check that
        // comparison when using base types is ok
        var a = ((SmolVm)vm).globalEnv.Get("a") as SmolNumber;
        var b = ((SmolVm)vm).globalEnv.Get("b") as SmolNumber;
        var c = ((SmolVm)vm).globalEnv.Get("c") as SmolVariableType;

        Assert.IsInstanceOfType<SmolNumber>(a);
        Assert.IsInstanceOfType<SmolNumber>(b);
        Assert.IsInstanceOfType<SmolNumber>(c);

        Assert.IsInstanceOfType<SmolVariableType>(a);
        Assert.IsInstanceOfType<SmolVariableType>(b);
        Assert.IsInstanceOfType<SmolVariableType>(c);

        Assert.AreEqual(a, c);
        Assert.AreEqual(a.GetValue(), c.GetValue());

        Assert.IsTrue(a == c);
        Assert.IsFalse(a != c);

        // GetValue returns an (optional) object, so we need to cast to
        // the actual type if we want to compare directly, otherwise will
        // never be equal (becuase they aren't the same instance)
        Assert.IsTrue((double)a.GetValue()! == (double)c.GetValue()!);

        Assert.AreNotEqual(a, b);
        Assert.IsTrue(a != b);
        Assert.IsFalse(a == b);
    }

    [TestMethod]
    public void SmolNumberEqualityFloat()
    {
        var vm = SmolVm.Init("var a = 0.1; var b = 0.2; var c = 0.1; var d = a + b; var e = d == 0.3;");

        Assert.AreEqual(0.1M, vm.GetGlobalVar<decimal>("a"));
        Assert.AreEqual(0.2M, vm.GetGlobalVar<decimal>("b"));
        Assert.AreEqual(0.1M, vm.GetGlobalVar<decimal>("c"));
        Assert.AreEqual(0.3M, vm.GetGlobalVar<decimal>("d"));

        Assert.AreEqual(0.1, vm.GetGlobalVar<double>("a"));
        Assert.AreEqual(0.2, vm.GetGlobalVar<double>("b"));
        Assert.AreEqual(0.1, vm.GetGlobalVar<double>("c"));
        Assert.AreEqual(0.30000000000000004, vm.GetGlobalVar<double>("d"));

        // We will get the values as a mixture of base type (which is how
        // they're defined in the stack) and actual type (which is how
        // they're often used internally) -- this let's us check that
        // comparison when using base types is ok
        var a = ((SmolVm)vm).globalEnv.Get("a") as SmolNumber;
        var b = ((SmolVm)vm).globalEnv.Get("b") as SmolNumber;
        var c = ((SmolVm)vm).globalEnv.Get("c") as SmolVariableType;
        var d = ((SmolVm)vm).globalEnv.Get("d") as SmolNumber;
        var e = ((SmolVm)vm).globalEnv.Get("e") as SmolBool;
        
        Assert.IsInstanceOfType<SmolNumber>(a);
        Assert.IsInstanceOfType<SmolNumber>(b);
        Assert.IsInstanceOfType<SmolNumber>(c);

        Assert.IsInstanceOfType<SmolVariableType>(a);
        Assert.IsInstanceOfType<SmolVariableType>(b);
        Assert.IsInstanceOfType<SmolVariableType>(c);

        Assert.AreEqual(a, c);
        Assert.AreEqual(a.GetValue(), c.GetValue());

        Assert.IsTrue(a == c);
        Assert.IsFalse(a != c);

        // GetValue returns an (optional) object, so we need to cast to
        // the actual type if we want to compare directly, otherwise will
        // never be equal (becuase they aren't the same instance)
        Assert.IsTrue((double)a.GetValue()! == (double)c.GetValue()!);

        Assert.AreNotEqual(a, b);
        Assert.IsTrue(a != b);
        Assert.IsFalse(a == b);

        Assert.AreEqual(0.30000000000000004, d.value);
        Assert.IsFalse(e.value);
    }

    [TestMethod]
    public void SmolBoolEquality()
    {
        var vm = SmolVm.Init("var a = true; var b = false; var c = a == b; var d = true;");

        Assert.IsTrue(vm.GetGlobalVar<bool>("a"));
        Assert.IsFalse(vm.GetGlobalVar<bool>("b"));
        Assert.IsFalse(vm.GetGlobalVar<bool>("c"));
        Assert.IsTrue(vm.GetGlobalVar<bool>("d"));

        // We will get the values as a mixture of base type (which is how
        // they're defined in the stack) and actual type (which is how
        // they're often used internally) -- this let's us check that
        // comparison when using base types is ok
        var a = ((SmolVm)vm).globalEnv.Get("a") as SmolBool;
        var b = ((SmolVm)vm).globalEnv.Get("b") as SmolBool;
        var c = ((SmolVm)vm).globalEnv.Get("c") as SmolVariableType;
        var d = ((SmolVm)vm).globalEnv.Get("d") as SmolVariableType;

        Assert.IsInstanceOfType<SmolBool>(a);
        Assert.IsInstanceOfType<SmolBool>(b);
        Assert.IsInstanceOfType<SmolBool>(c);
        Assert.IsInstanceOfType<SmolBool>(d);

        Assert.IsInstanceOfType<SmolVariableType>(a);
        Assert.IsInstanceOfType<SmolVariableType>(b);
        Assert.IsInstanceOfType<SmolVariableType>(c);
        Assert.IsInstanceOfType<SmolVariableType>(d);

        Assert.AreEqual(a, d);
        Assert.AreEqual(a.GetValue(), d.GetValue());

        Assert.IsTrue(a == d);
        Assert.IsFalse(a != d);

        // GetValue returns an (optional) object, so we need to cast to
        // the actual type if we want to compare directly, otherwise will
        // never be equal (becuase they aren't the same instance)
        Assert.IsTrue((bool)a.GetValue()! == (bool)d.GetValue()!);

        Assert.AreNotEqual(a, b);
        Assert.IsTrue(a != b);
        Assert.IsFalse(a == b);
    }

}