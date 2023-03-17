namespace SmolScript.Internals.Ast.Statements
{
    public abstract class Statement
    {
        public abstract object? Accept(IStatementVisitor visitor);
    }
}
