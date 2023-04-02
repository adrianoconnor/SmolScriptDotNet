namespace SmolScript.Internals.Ast.Statements
{
    internal interface IStatementVisitor
    {
        object? Visit(ExpressionStatement stmt);
        object? Visit(PrintStatement stmt);
        object? Visit(ReturnStatement stmt);
        object? Visit(VarStatement stmt);
        object? Visit(BlockStatement stmt);
        object? Visit(IfStatement stmt);
        object? Visit(WhileStatement stmt);
        object? Visit(BreakStatement stmt);
        object? Visit(ContinueStatement stmt);
        object? Visit(FunctionStatement stmt);
        object? Visit(DebuggerStatement stmt);
        object? Visit(TryStatement stmt);
        object? Visit(ThrowStatement stmt);
    }
}

