namespace SmolScript.Internals.Ast.Statements
{
    internal class ContinueStatement : Statement
    {
        public ContinueStatement()
        {
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

