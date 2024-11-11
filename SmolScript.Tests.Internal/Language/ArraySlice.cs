using SmolScript.Internals;
using SmolScript.Internals.Ast;
using SmolScript.Internals.SmolStackTypes;
using SmolScript.Internals.SmolVariableTypes;

namespace SmolScript.Tests.Internal.Language;

[TestClass]
public class ArraySliceTests
{
    [TestMethod]
    public void ArraySlice()
    {
        var vm = SmolVm.Init(@"
var animals = ['ant', 'bison', 'camel', 'duck', 'elephant'];

var a = animals.slice(2);
// Expected output: Array [""camel"", ""duck"", ""elephant""]

var b = animals.slice(2, 4);
// Expected output: Array [""camel"", ""duck""]

var c = animals.slice(1, 5);
// Expected output: Array [""bison"", ""camel"", ""duck"", ""elephant""]

var d = animals.slice(-2);
// Expected output: Array [""duck"", ""elephant""]

var e = animals.slice(2, -1);
// Expected output: Array [""camel"", ""duck""]

var f = animals.slice();
// Expected output: Array [""ant"", ""bison"", ""camel"", ""duck"", ""elephant""]

");
        var a = vm.GetGlobalVarAsArray<string>("a"); 
        Assert.AreEqual(3, a.Count);
        Assert.AreEqual("camel", a[0]);
        Assert.AreEqual("elephant", a[2]);
        
        var b = vm.GetGlobalVarAsArray<string>("b"); 
        Assert.AreEqual(2, b.Count);
        Assert.AreEqual("camel", b[0]);
        Assert.AreEqual("duck", b[1]);
        
        var c = vm.GetGlobalVarAsArray<string>("c"); 
        Assert.AreEqual(4, c.Count);
        Assert.AreEqual("bison", c[0]);
        Assert.AreEqual("elephant", c[3]);

        var d = vm.GetGlobalVarAsArray<string>("d"); 
        Assert.AreEqual(2, d.Count);
        Assert.AreEqual("duck", d[0]);
        Assert.AreEqual("elephant", d[1]);
        
        var e = vm.GetGlobalVarAsArray<string>("e"); 
        Assert.AreEqual(2, e.Count);
        Assert.AreEqual("camel", e[0]);
        Assert.AreEqual("duck", e[1]);
        
        var f = vm.GetGlobalVarAsArray<string>("f"); 
        Assert.AreEqual(5, f.Count);
        Assert.AreEqual("ant", f[0]);
        Assert.AreEqual("elephant", f[4]);
    }
    
}