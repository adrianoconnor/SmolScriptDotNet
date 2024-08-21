namespace SmolScript.Internals.Ast.Statements
{
    internal class BlockStatement : Statement
    {
        public readonly IList<Statement> statements;

        public bool insertedByParser;

        public BlockStatement(IList<Statement> statements, bool insertedByParser = false)
        {
            this.statements = statements;
            this.insertedByParser = insertedByParser;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

