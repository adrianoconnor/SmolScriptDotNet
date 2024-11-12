﻿namespace SmolScript.Internals.Ast.Statements
{
    internal class BlockStatement : Statement
    {
        public readonly IList<Statement> Statements;

        public bool IsAutoGeneratedByParser;

        public BlockStatement(IList<Statement> statements, bool isAutoGeneratedByParser = false)
        {
            this.Statements = statements;
            this.IsAutoGeneratedByParser = isAutoGeneratedByParser;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

