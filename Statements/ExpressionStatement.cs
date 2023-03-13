using System;
namespace SmolScript.Statements
{
    public class ExpressionStatement : Statement
    {
        public readonly SmolScript.Expression expression;

        public ExpressionStatement(SmolScript.Expression expression)
        {
            this.expression = expression;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

