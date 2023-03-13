namespace SmolScript.Statements
{
    public interface IStatementVisitor
    {
        object? Visit(ExpressionStatement stmt);
        object? Visit(Statement.Print stmt);
        object? Visit(Statement.Return stmt);
        object? Visit(Statement.Var stmt);
        object? Visit(Statement.Block stmt);
        object? Visit(Statement.If stmt);
        object? Visit(Statement.Ternary stmt);
        object? Visit(Statement.While stmt);
        object? Visit(Statement.Break stmt);
        object? Visit(Statement.Function stmt);
    }
}

