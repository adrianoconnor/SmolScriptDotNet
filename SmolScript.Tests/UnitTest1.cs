﻿using SmolScript;

namespace SmolScript.Tests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        var code = @"var a = 1; a++; a*=2; a = a % 3;";

        var vm = new SmolVM(code);

        vm.Run();

        var a = vm.GetGlobalVar<int>("a");

        Assert.AreEqual(1, a);
    }

    [TestMethod]
    public void TestMethod2()
    {
        var code = @"
var a = 1; // Declare a global variable

function aPlusPlus() { // Declare a function we can call from .net
  a++;
}

aPlusPlus(); // Call it once during initialization just to show this also is fine
";

        ISmolRuntime vm = SmolVM.Compile(code);

        vm.MaxStackSize = 4;
       
        vm.Run(); // this executes the code above -- declares a, sets to 1, declares a functiona and calls it

        var a = vm.GetGlobalVar<int>("a"); // verify that the var has the value we expect

        Assert.AreEqual(2, a);

        vm.Call("aPlusPlus"); // Now call the function with no params

        var a2 = vm.GetGlobalVar<string>("a"); // And get the new value in variable a

        Assert.AreEqual("3", a2);       
    }
    /*
    [TestMethod]
    public void TestMethod3()
    {
        var code = @"function fibonacci(num) {
  if (num <= 1) return 1;

  return fibonacci(num - 1) + fibonacci(num - 2);
}

var f = fibonacci(50);

print f;";

        var vm = new SmolVM(code);

        vm.Run();

        var f = vm.GetGlobalVar<int>("f");

        Assert.AreEqual(1, f);
    }*/
}
