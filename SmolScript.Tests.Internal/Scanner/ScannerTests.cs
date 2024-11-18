using SmolScript.Internals;
using SmolScript.Internals.Ast;

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
    
    [TestMethod]
    public void TestScannerError()
    {
        var ex = Assert.ThrowsException<SmolCompilerError>(() =>
        {
            var vm = SmolVm.Compile("var a = '");
        });
        
        Assert.AreEqual("Unterminated string (line 1)", ex.Message);
        Assert.AreEqual(CompilerErrorSource.SCANNER, ex.ErrorSource);
    }
    
    [TestMethod]
    public void TestScannerErrorInEmbeddedExpression()
    {
        var ex = Assert.ThrowsException<SmolCompilerError>(() =>
        {
            var vm = SmolVm.Compile("var a = `${x.}`");
        });
        
        Assert.AreEqual("Encounted one or more errors while parsing (first error: Expect property name after '.'. (Line 1, Col 9))", ex.Message);
        Assert.AreEqual(CompilerErrorSource.PARSER, ex.ErrorSource);
    }

    
    [TestMethod]
    public void TestRandomTodoErrorNote()
    {
        var src = "var a = 1; var b = 2; var c = `${a}${b}`; // var c = `${a.toString()}${b.toString()}`;";
        
        var vm = SmolVm.Init(src);
        Assert.AreEqual("12", vm.GetGlobalVar<string>("c"));
    }
    
    [TestMethod]
    public void TestRandomTodoErrorNoteBonus()
    {
        var src = @"var a = 1; 
var b = 2;
var x = 'a' + 'b';
var c = `${a}-${b}`;";
        
        var scanner = new Scanner(src);
        var tokens = scanner.ScanTokens();
        foreach(var t in tokens)
        Console.WriteLine(t);
        
        var parser = new Parser(tokens);
        var ast = new AstDump();
        Console.WriteLine(ast.Print(parser.Parse()));
        
        var vm = SmolVm.Init(src);
        
        Assert.AreEqual("1-2", vm.GetGlobalVar<string>("c"));
    }

}