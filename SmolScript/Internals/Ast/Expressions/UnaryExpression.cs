using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class UnaryExpression : Expression
    {
        public readonly Token Operator;
        public readonly Expression RightExpression;

        public UnaryExpression(Token @operator, Expression rightExpression)
        {
            this.Operator = @operator;
            this.RightExpression = rightExpression;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

