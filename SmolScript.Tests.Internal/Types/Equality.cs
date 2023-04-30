﻿using SmolScript.Internals.SmolStackTypes;

namespace SmolScript.Tests.Internal.Types;

[TestClass]
public class Equality
{
    [TestMethod]
    public void SmolNumberEquality()
    {
        var vm = SmolVM.Init("var a = 1; var b = 2; var c = 1;");
     
        Assert.AreEqual(1, vm.GetGlobalVar<int>("a"));
        Assert.AreEqual(2, vm.GetGlobalVar<int>("b"));
        Assert.AreEqual(1, vm.GetGlobalVar<int>("c"));

        // We will get the values as a mixture of base type (which is how
        // they're defined in the stack) and actual type (which is how
        // they're often used internally) -- this let's us check that
        // comparison when using base types is ok
        var a = ((SmolVM)vm).globalEnv.Get("a") as SmolNumber;
        var b = ((SmolVM)vm).globalEnv.Get("b") as SmolNumber;
        var c = ((SmolVM)vm).globalEnv.Get("c") as SmolStackValue;

        Assert.IsInstanceOfType<SmolNumber>(a);
        Assert.IsInstanceOfType<SmolNumber>(b);
        Assert.IsInstanceOfType<SmolNumber>(c);

        Assert.IsInstanceOfType<SmolStackValue>(a);
        Assert.IsInstanceOfType<SmolStackValue>(b);
        Assert.IsInstanceOfType<SmolStackValue>(c);

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

}