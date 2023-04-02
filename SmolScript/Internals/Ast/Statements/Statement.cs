namespace SmolScript.Internals.Ast.Statements
{
    internal abstract class Statement
    {
        public abstract object? Accept(IStatementVisitor visitor);
    }
}
