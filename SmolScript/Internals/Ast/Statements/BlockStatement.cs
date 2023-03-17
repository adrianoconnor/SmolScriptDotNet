namespace SmolScript.Internals.Ast.Statements
{
    public class BlockStatement : Statement
    {
        public readonly IList<Statement> statements;

        public BlockStatement(IList<Statement> statements)
        {
            this.statements = statements;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

