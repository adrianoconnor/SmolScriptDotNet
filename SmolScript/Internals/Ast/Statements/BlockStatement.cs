namespace SmolScript.Internals.Ast.Statements
{
    public class BlockStatement : Statement
    {
        public readonly IList<Statement> statements;
        public bool isFunctionBody;

        public BlockStatement(IList<Statement> statements, bool isFunctionBody = false)
        {
            this.statements = statements;
            this.isFunctionBody = isFunctionBody;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

