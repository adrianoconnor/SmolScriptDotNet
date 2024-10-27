using SmolScript.Internals;
using SmolScript.Internals.Ast;
using SmolScript.Internals.SmolStackTypes;
using SmolScript.Internals.SmolVariableTypes;

namespace SmolScript.Tests.Internal.Language;

[TestClass]
public class FatArrows
{
    [TestMethod]
    public void FatArrows1()
    {
        var vm = SmolVm.Init(@"
var x = 0;
var f = () => 1;
x = f();
");
        
        Assert.AreEqual(1, vm.GetGlobalVar<int>("x"));
    }
    
    [TestMethod]
    public void FatArrows1b()
    {
        var vm = SmolVm.Init(@"
var x = 0;
var f = b => 1;
x = f();
");
        
        Assert.AreEqual(1, vm.GetGlobalVar<int>("x"));
    }
    
    [TestMethod]
    public void FatArrows2()
    {
        var vm = SmolVm.Init(@"
var x = 0;
var f = () => { return 1 };
x = f();
");
        
        Assert.AreEqual(1, vm.GetGlobalVar<int>("x"));
    }
    
    [TestMethod]
    public void FatArrows3()
    {
        var vm = SmolVm.Init(@"
var x = 0;
var f = (n) => { return n * n; };
x = f(2);
");
        
        Assert.AreEqual(4, vm.GetGlobalVar<int>("x"));
    }
    
    [TestMethod]
    public void FatArrows4()
    {
        var vm = SmolVm.Init(@"
var x = 0;
var f = (n) => n * n;
x = f(2);
");
        
        Assert.AreEqual(4, vm.GetGlobalVar<int>("x"));
    }
    
    [TestMethod]
    public void FatArrows5a()
    {
        var vm = SmolVm.Init(@"
var x = 0;
function f(z) {
    return z(2);
}
x = f((n) => { return n * n; });
");
        
        Assert.AreEqual(4, vm.GetGlobalVar<int>("x"));
    }
    
    [TestMethod]
    public void FatArrows5b()
    {
        var vm = SmolVm.Init(@"
var x = 0;
function f(z) {
    return z(2);
}
x = f((n) => n * n );
");
        
        Assert.AreEqual(4, vm.GetGlobalVar<int>("x"));
    }
    
    [TestMethod]
    public void FatArrows6()
    {
        var vm = SmolVm.Init(@"
var x = 0;
function f(z) {
    return z(2);
}
x = f(n => n * n);
");
        
        Assert.AreEqual(4, vm.GetGlobalVar<int>("x"));
    }
    
    [TestMethod]
    public void FatArrows0()
    {
        var vm = SmolVm.Init(@"var f = (n) => n * n; 
var x = f(2);");
        
        Assert.AreEqual(4, vm.GetGlobalVar<int>("x"));
    }
    
    [TestMethod]
    public void FatArrows0a()
    {
        var vm = SmolVm.Init(@"var f = (n,m) => n * m; var x = f(2, 3);");
        
        Assert.AreEqual(6, vm.GetGlobalVar<int>("x"));
    }
    
    [TestMethod]
    public void FatArrows0b()
    {
        var vm = SmolVm.Init(@"var f = n => n * n; var x = f(3);");
        
        Assert.AreEqual(9, vm.GetGlobalVar<int>("x"));
    }
}