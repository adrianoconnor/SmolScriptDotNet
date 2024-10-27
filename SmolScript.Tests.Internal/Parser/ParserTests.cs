using SmolScript.Internals;

namespace SmolScript.Tests.Internal.Types;

/// <summary>
/// This test class is here to support TDD refactor of the parser, but I probably
/// won't leave any tests here unless they feel particularly useful to hold on to.
/// </summary>
[TestClass]
public class ParserTests
{
    [TestMethod]
    public void TestMultiVarDeclarations()
    {
        //var vm = SmolVM.Init("var a = 1, b = 2, c;");

        //Assert.AreEqual(1, vm.GetGlobalVar<int>("a"));
        //Assert.AreEqual(2, vm.GetGlobalVar<int>("b"));
        //Assert.IsNull(vm.GetGlobalVar<int>("c"));
    }

    [TestMethod]
    public void TestMeaninglessSemiColons()
    {
        var vm = SmolVm.Init(@"var a = 1;;;
var b = 2;
;var c = 0;
;");
        Assert.AreEqual(1, vm.GetGlobalVar<int>("a"));
        Assert.AreEqual(2, vm.GetGlobalVar<int>("b"));
        Assert.AreEqual(0, vm.GetGlobalVar<int>("c"));
    }

}