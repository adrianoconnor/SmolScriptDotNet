using SmolScript.Internals;
using SmolScript.Internals.Ast;
using SmolScript.Internals.SmolStackTypes;
using SmolScript.Internals.SmolVariableTypes;

namespace SmolScript.Tests.Internal.Language;

[TestClass]
public class Functions
{
    [TestMethod]
    public void NestedFunctionsWithTheSameNameNotCalled()
    {
        var source = @"
var x = 0;
function b() {
    function a() {
        x = x + 1;
    }
    //a();
}

function c() {
    function a() {
        x = x + 2;
    }// add ; to create another error :(
    //a();
};;;;

b();
c();
";

        var vm = SmolVm.Init(source);
        Assert.AreEqual(0, vm.GetGlobalVar<int>("x"));
    }
    
    [TestMethod]
    public void NestedFunctionsWithTheDifferentNamesNotCalled()
    {
        var vm = SmolVm.Init(@"
var x = 0;
function b() {
    var y = function () {
        x = x + 1;
    }
    //a();
}

function c() {
    var z = function () {
        x = x + 2;
    }
    //z();
}

b();
c();
");
        
        Assert.AreEqual(0, vm.GetGlobalVar<int>("x"));
    }
    
    
    [TestMethod]
    public void NestedFunctionsWithTheSameNameCalled()
    {
        var source = @"
var x = 0;
function b() {
    function a() {
        x = x + 1;
    }
    a();
    x+=10;
}

function c() {
    function a() {
        x = x + 2;;;;;
    };;;;;
    a();;;;;
    x+=100;;;;;
};;;;
;;;;
b();;;;
c();;;;;
";
        var vm = SmolVm.Init(source);
  
        Console.WriteLine(vm.Decompile());
        
        Assert.AreEqual(113, vm.GetGlobalVar<int>("x"));
    }
    
    [TestMethod]
    public void NestedFunctionsWithSameVarNames()
    {
        var vm = SmolVm.Init(@"
var x = 0;
function b() {
    var y = function () {
        x = x + 1;
    }
    y();
}

function c() {
    var y = function () {
        x = x + 2;
    }
    y();
}

b();
c();
");
        
        Assert.AreEqual(3, vm.GetGlobalVar<int>("x"));
    }
}