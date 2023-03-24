namespace SmolScript.Internals.Ast.Statements
{
    public class ContinueStatement : Statement
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

