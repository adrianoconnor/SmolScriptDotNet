using SmolScript.Internals.Ast.Expressions;

namespace SmolScript.Internals.Ast.Statements
{
    public class TernaryStatement : Statement
    {
        public readonly Expression testExpression;
        public readonly Expression thenExpression;
        public readonly Expression elseExpression;

        public TernaryStatement(Expression testExpression, Expression thenExpression, Expression elseExpression)
        {
            this.testExpression = testExpression;
            this.thenExpression = thenExpression;
            this.elseExpression = elseExpression;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

