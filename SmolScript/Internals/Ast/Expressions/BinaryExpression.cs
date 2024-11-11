using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class BinaryExpression : Expression
    {
        public readonly Expression LeftExpression;
        public readonly Token BinaryOperator;
        public readonly Expression RightExpression;

        public BinaryExpression(Expression leftExpression, Token binaryOperator, Expression rightExpression)
        {
            this.LeftExpression = leftExpression;
            this.BinaryOperator = binaryOperator;
            this.RightExpression = rightExpression;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

