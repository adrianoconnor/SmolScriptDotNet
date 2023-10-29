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
        var vm = SmolVM.Init("var a = 1;      var b = 2; var c = 2, d = 1;");

        Assert.AreEqual(2, vm.GetGlobalVar<int>("c"));
        Assert.AreEqual(1, vm.GetGlobalVar<int>("d"));
    }

}