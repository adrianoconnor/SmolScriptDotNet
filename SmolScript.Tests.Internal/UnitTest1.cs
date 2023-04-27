using SmolScript;
using SmolScript.Internals;
//using SmolScript.Internals.Ast.Interpreter;

namespace SmolTests;

[TestClass]
public class UnitTest1
{
    private class CustomFunc : ICallable
    {
        public int calls = 0;

        public object? call(IList<object?> args)
        {
            return ++calls;
        }
    }

    /*
    [TestMethod]
    public void ReallySimpleAstInterpreterTest()
    {
        var interpreter = new AstInterpreter();

        interpreter.Interpret("var a = 1 + 2; a++;");

       

        Assert.AreEqual(4.0, interpreter.globalEnv.Get("a"));
    }

    [TestMethod]
    public void NewScopeForIfAstInterpreterTest()
    {
        var interpreter = new AstInterpreter();

        interpreter.Interpret(@"
        var a = 1;

        if (true)
        {
            var a = 2;
        }
        ");

        Assert.AreEqual(1.0, interpreter.globalEnv.Get("a"));
    }

    [TestMethod]
    public void ReallySimpleAstInterpreterTest2()
    { 
        var cf = new CustomFunc();

        var interpreter = new AstInterpreter();

        interpreter.globalEnv.Define("cf", cf);

        interpreter.Interpret("var x = cf(); cf();");

        Assert.AreEqual(1, interpreter.globalEnv.Get("x"));
        Assert.AreEqual(2, cf.calls);
    }
    */

    [TestMethod]
    public void ReallySimpleByteCodeCompilationTest()
    { 
        var program = SmolCompiler.Compile("5 * (3 - 1);");

        Assert.AreEqual(OpCode.CONST, program.code_sections[0][0].opcode);
        Assert.AreEqual(5.0, ((SmolValue)program.constants[(int)((program.code_sections[0][0]).operand1!)]).value!);
        Assert.AreEqual(OpCode.CONST, program.code_sections[0][1].opcode);
        Assert.AreEqual(OpCode.CONST, program.code_sections[0][2].opcode);
        Assert.AreEqual(OpCode.SUB, program.code_sections[0][3].opcode);
        Assert.AreEqual(OpCode.MUL, program.code_sections[0][4].opcode);
    }

    [TestMethod]
    public void ReallySimpleByteCodeCompilationTest2()
    {
        var program = SmolCompiler.Compile("var a = 5 * (3 - 1);");

        Assert.AreEqual(OpCode.DECLARE, program.code_sections[0][0].opcode);
        Assert.AreEqual("a", ((SmolVariableDefinition)program.code_sections[0][0].operand1!).name);
        Assert.AreEqual(OpCode.CONST, program.code_sections[0][1].opcode);
        Assert.AreEqual(5.0, ((SmolValue)program.constants[(int)((program.code_sections[0][1]).operand1!)]).value!);
        Assert.AreEqual(OpCode.CONST, program.code_sections[0][2].opcode);
        Assert.AreEqual(OpCode.CONST, program.code_sections[0][3].opcode);
        Assert.AreEqual(OpCode.SUB, program.code_sections[0][4].opcode);
        Assert.AreEqual(OpCode.MUL, program.code_sections[0][5].opcode);
        Assert.AreEqual(OpCode.STORE, program.code_sections[0][6].opcode);
        Assert.AreEqual("a", ((SmolVariableDefinition)program.code_sections[0][0].operand1!).name);
    }

    [TestMethod]
    public void ReallySimpleVmTest()
    {
        var program = SmolCompiler.Compile("5 + (3 + 1); 1+1;1+1;1+1;1+1;1+1;1+1;1+1;1+1;1+1;1+1;");

        var vm = new SmolVM(program);

        vm.Run();
    }

    [TestMethod]
    public void ReallySimpleVmTest2()
    {
        var program = SmolCompiler.Compile("var a = 1; if (true) a = 2;");

        var vm = new SmolVM(program);

        vm.Run();

        Assert.AreEqual(2.0, (double)(((SmolValue)vm.globalEnv.Get("a")!).value!));
    }

    [TestMethod]
    public void ReallySimpleVmTest3()
    {
        var program = SmolCompiler.Compile("var a = 1; a = a + 1;");

        var vm = new SmolVM(program);

        vm.Run();

        Assert.AreEqual(2.0, (double)(((SmolValue)vm.globalEnv.Get("a")!).value!));
    }

    [TestMethod]
    public void ReallySimpleVmTest4()
    {
        var program = SmolCompiler.Compile("var a = 1; if (true) { a = 2; }");

        var vm = new SmolVM(program);

        vm.Run();

        Assert.AreEqual(2.0, (double)(((SmolValue)vm.globalEnv.Get("a")!).value!));
    }

    [TestMethod]
    public void SimpleVmFunctionTest()
    {
        var program = SmolCompiler.Compile(@"function f() { return 2; } var a = f();");

        var vm = new SmolVM(program);

        vm.Run();

        Assert.AreEqual(2.0, (double)(((SmolValue)vm.globalEnv.Get("a")!).value!));
    }

    [TestMethod]
    public void SimpleDisassembleTest()
    {
        var program = SmolCompiler.Compile(@"function f(p1) { return p1 * 2; } var a = f(1);");

        var output = ByteCodeDisassembler.Disassemble(program);

        Console.WriteLine(output);
    }

    /*

    Commented out, we expect this test to fail because we haven't written function expressions yet

    [TestMethod]
    public void SimpleDisassembleTest2()
    {
        var program = SmolCompiler.Compile(@"f(function() {});");

        var output = ByteCodeDisassembler.Disassemble(program);

        Console.WriteLine(output);
    }
    */

    [TestMethod]
    public void StackIsEmptyAfterExpression()
    {
        var program = SmolCompiler.Compile(@"function f() { return 1 == 2; } f(); 1 == 2; 2 == 3; f();");

        var vm = new SmolVM(program);

        vm.Run();

        Assert.AreEqual(0, vm.stack.Count);
    }

    [TestMethod]
    public void StackIsEmptyAfterExpression2()
    {
        var program = SmolCompiler.Compile(@"var a = 1; a++;");

        var vm = new SmolVM(program);

        vm.Run();

        Assert.AreEqual(0, vm.stack.Count);
    }

    [TestMethod]
    public void UndefinedVar()
    {
        var program = SmolCompiler.Compile(@"var b = 0; function f() { return a == 2; } 1 == 2; 2 == 3; f();");

        var vm = new SmolVM(program);

        vm.Run();

        Assert.AreEqual(0, vm.stack.Count);
    }
}