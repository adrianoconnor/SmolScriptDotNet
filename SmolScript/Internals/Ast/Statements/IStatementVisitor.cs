namespace SmolScript.Internals.Ast.Statements
{
    public interface IStatementVisitor
    {
        object? Visit(ExpressionStatement stmt);
        object? Visit(PrintStatement stmt);
        object? Visit(ReturnStatement stmt);
        object? Visit(VarStatement stmt);
        object? Visit(BlockStatement stmt);
        object? Visit(IfStatement stmt);
        object? Visit(TernaryStatement stmt);
        object? Visit(WhileStatement stmt);
        object? Visit(BreakStatement stmt);
        object? Visit(FunctionStatement stmt);
    }
}

