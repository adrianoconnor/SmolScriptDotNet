using SmolScript;

namespace SmolScript.Tests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void StringLength()
    {
        var src = @"
            var a = 'Test String';
            var b = a.length;
        ";

        var vm = SmolVM.Compile(src);

        vm.RunInDebug();

        var a = vm.GetGlobalVar<int>("b");

        Assert.AreEqual(11, a);
    }

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
       
        vm.RunInDebug(); // this executes the code above -- declares a, sets to 1, declares a functiona and calls it

        var a = vm.GetGlobalVar<int>("a"); // verify that the var has the value we expect

        Assert.AreEqual(2, a);

        vm.Call("aPlusPlus"); // Now call the function with no params

        var a2 = vm.GetGlobalVar<string>("a"); // And get the new value in variable a

        Assert.AreEqual("3", a2);       
    }
    
    [TestMethod]
    public void TestMethod3()
    {
        var code = @"function fibonacci(num) {
  if (num <= 1) return num;

  return fibonacci(num - 1) + fibonacci(num - 2);
}

var f = fibonacci(30);
";

        var vm = new SmolVM(code);

        vm.Run();

        var f = vm.GetGlobalVar<int>("f");

        Assert.AreEqual(832040, f);
    }

    public int fib(int n)
    {
        if (n <= 1) return n;

        return fib(n - 1) + fib(n - 2);
    }

    [TestMethod]
    public void TestMethod4()
    {
        var s = Environment.TickCount;

        var f = fib(30);

        Assert.AreEqual(832040, f);

        Console.WriteLine($"TOOK {Environment.TickCount - s}");
    }

    [TestMethod]
    public void TestMethod5()
    {
        var code = @"
var a = 1; // Declare a global variable

function aPlusNum(numToAdd) { // Declare a function we can call from .net, with args and return value
  a += numToAdd;

  return a;
}

";

        ISmolRuntime vm = SmolVM.Compile(code);

        vm.RunInDebug(); // this executes the code above -- declares a, sets to 1, declares a functiona and calls it

        var a = vm.GetGlobalVar<int>("a"); // verify that the var has the value we expect

        Assert.AreEqual(1, a);

        var x = vm.Call<int>("aPlusNum", 10); // Now call the function with no params

        var a2 = vm.GetGlobalVar<string>("a"); // And get the new value in variable a

        Assert.AreEqual("11", a2);
        Assert.AreEqual(11, x);
    }

    [TestMethod]
    public void TestMethod6()
    {
        var code = @"var a = function() { return 1; };
                     var b = a();";

        ISmolRuntime vm = SmolVM.Compile(code);

        Console.WriteLine(((SmolVM)vm).Decompile());

        vm.Run();

        var a = vm.GetGlobalVar<int>("b");

        Assert.AreEqual(1, a);
    }

    [TestMethod]
    public void TestMethod7()
    {
        var code = @"

var a = 1;

function f(x) {
  x(10);
};

f(function(z) { a += z; });
";

        ISmolRuntime vm = SmolVM.Compile(code);

        Console.WriteLine(((SmolVM)vm).Decompile());

        vm.Run();

        var a = vm.GetGlobalVar<int>("a");

        Assert.AreEqual(11, a);
    }
}
