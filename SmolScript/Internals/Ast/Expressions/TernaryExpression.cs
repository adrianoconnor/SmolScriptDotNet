using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class TernaryExpression : Expression
    {
        public readonly Expression EvaluationExpression;
        public readonly Expression TrueValue;
        public readonly Expression FalseValue;

        public TernaryExpression(Expression evaluationExpression, Expression trueValue, Expression falseValue)
        {
            this.EvaluationExpression = evaluationExpression;
            this.TrueValue = trueValue;
            this.FalseValue = falseValue;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

