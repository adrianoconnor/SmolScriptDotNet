using SmolScript;

namespace SmolScript.Tests.DotNetInterop;

[TestClass]
public class Basics
{
    [TestMethod]
    public void CallSimpleSmolGlobalFunctionFromDotNet()
    {
        var code = @"
var a = 1; // Declare a global variable

function aPlusPlus() { // Declare a function we can call from .net
  a++;
}

aPlusPlus(); // Call it once during initialization just to show this also is fine
";

        var vm = SmolVM.Compile(code);

        //vm.MaxStackSize = 4;

        vm.Run(); // this executes the code above -- declares a, sets to 1, declares a functiona and calls it

        var a = vm.GetGlobalVar<int>("a"); // verify that the var has the value we expect

        Assert.AreEqual(2, a);

        vm.Call("aPlusPlus"); // Now call the function with no params

        var a2 = vm.GetGlobalVar<string>("a"); // And get the new value in variable a

        Assert.AreEqual("3", a2);
    }

    [TestMethod]
    public void CallSmolGlobalFunctionWithArgsAndReturnFromDotNet()
    {
        var code = @"

var a = 1;

// Declare a global function that we can call from .net, with arguments and a return value
function aPlusNum(numToAdd) {
  a += numToAdd;
  return a;
}
";

        var vm = SmolVM.Compile(code);

        vm.Run(); // this executes the code above -- declares a and sets it to 1

        var a1 = vm.GetGlobalVar<int>("a"); // verify that the a has the value we expect

        Assert.AreEqual(1, a1);

        var returnedValue = vm.Call<int>("aPlusNum", 10); // Call the function with one param, returning an int

        var a2 = vm.GetGlobalVar<int>("a"); // Get the new value in variable a

        Assert.AreEqual(11, a2);
        Assert.AreEqual(11, returnedValue);
    }

    [TestMethod]
    public void CallSimpleSmolFunctionFromDotNetWithStringArgs()
    {
        var code = @"
function concat(strA, strB) {
  return strA + strB;
}
";

        var vm = SmolVM.Init(code);

        //        vm.Run();

        var result = vm.Call<string>("concat", "hello", "world");

        Assert.AreEqual("helloworld", result);
    }

    [TestMethod]
    public void LogDelegate()
    {
        var code = @"
function concat(strA, strB) {
  return strA + strB;
}
";

        var vm = SmolVM.Compile(code);

        var debugOutput = new System.Text.StringBuilder();

        vm.OnDebugLog = (string logMessage) => debugOutput.AppendLine(logMessage);

        vm.Run();

        var result = vm.Call<string>("concat", "hello", "world");

        Assert.AreEqual("helloworld", result);

        Assert.AreEqual(@"NOP          
EOF          
Done, stack size = 0
ENTER_SCOPE  
FETCH         [op1: strA]
              [Loaded $SmolScript.Internals.SmolVariableTypes.SmolString hello]
FETCH         [op1: strB]
              [Loaded $SmolScript.Internals.SmolVariableTypes.SmolString world]
ADD          
RETURN       
", debugOutput.ToString());

    }

    [TestMethod]
    public void RunProgramTwice()
    {
        var code = @"
var a = 1;
function getA() { return a; }
";

        var vm = SmolVM.Compile(code);

        vm.Run();

        Assert.ThrowsException<Exception>(vm.Run);
        Assert.ThrowsException<Exception>(vm.Run);
        Assert.ThrowsException<Exception>(vm.Step);
    }

    [TestMethod]
    public void StepThroughInDebug()
    {
        var code = @"
var a = 1;
debugger;
a++;
a++;
a++;
function getA() { return a; }
";

        var vm = SmolVM.Compile(code);

        vm.Run();

        Assert.AreEqual(1, vm.GetGlobalVar<int>("a"));

        vm.Step();

        Assert.AreEqual(2, vm.GetGlobalVar<int>("a"));

        vm.Run();

        Assert.AreEqual(4, vm.GetGlobalVar<int>("a"));
    }

}
