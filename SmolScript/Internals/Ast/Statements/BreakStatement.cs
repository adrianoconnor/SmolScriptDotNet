namespace SmolScript.Internals.Ast.Statements
{
    internal class BreakStatement : Statement
    {
        public BreakStatement()
        {
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

