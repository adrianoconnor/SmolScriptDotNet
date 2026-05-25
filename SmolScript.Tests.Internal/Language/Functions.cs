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
    // var x = 0; // If we add this line to both 'parent' functions, the result should be zero, but it is not being captured/hoisted correctly and global x ends up = 3
    function a() {
        x = x + 1;
    };
    a();
}

function c() {
    // var x = 0;
    function a() {
        x = x + 2;
    };
    a();
};

b();
c();
";

        var vm = SmolVm.Init(source);
        Assert.AreEqual(3, vm.GetGlobalVar<int>("x"));
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
    
    [TestMethod]
    public void FunctionOnObjectWithThis ()
    {
        var source = """
             var x = 0;

             var o = {
                 z: 999,
                 getZ: function() { return this.z; }
             }

             x = o.getZ();
        """;
        
        var vm = SmolVm.Init(source);
        Assert.AreEqual(999, vm.GetGlobalVar<int>("x"));
   
    }
    
    [TestMethod]
    public void ObjRef()
    {
        var source = @"

var a = [];
a.push(1);
var b = a;
b.push(2);
var a_len = a.length;
var b_len = b.length;
";

        var vm = SmolVm.Init(source);
        Assert.AreEqual(2, vm.GetGlobalVar<int>("a_len"));
        Assert.AreEqual(2, vm.GetGlobalVar<int>("b_len"));
    }

}