using SmolScript;

namespace SmolScript.Tests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestMethod1()
    {
        var code = @"var a = 1; a++;";

        var vm = new SmolVM(code);

        vm.Run();
    }
}
