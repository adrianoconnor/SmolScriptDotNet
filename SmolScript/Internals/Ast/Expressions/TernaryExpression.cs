using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    public class TernaryExpression : Expression
    {
        public readonly Expression evaluationExpression;
        public readonly Expression expressionIfTrue;
        public readonly Expression expresisonIfFalse;

        public TernaryExpression(Expression evaluationExpression, Expression expressionIfTrue, Expression expresisonIfFalse)
        {
            this.evaluationExpression = evaluationExpression;
            this.expressionIfTrue = expressionIfTrue;
            this.expresisonIfFalse = expresisonIfFalse;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

