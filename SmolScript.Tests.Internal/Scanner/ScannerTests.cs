using SmolScript.Internals;

namespace SmolScript.Tests.Internal.Types;

/// <summary>
/// This test class is here to support TDD refactor of the scanner, but I probably
/// won't leave any tests here unless they feel particularly useful to hold on to.
/// </summary>
[TestClass]
public class ScannerTests
{
    [TestMethod]
    public void TestStartOfToken()
    {
        var scanner = new Scanner("var a = 1;      var b = 2; // var c = 2, d = 1;");

        var tokens = scanner.ScanTokens();

        Assert.AreEqual(16, tokens[5].StartPosition);

        var parser = new Parser(tokens);

        parser.Parse();
    }

}