using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class LogicalExpression : Expression
    {
        public readonly Expression LeftExpression;
        public readonly Token Operator;
        public readonly Expression RightExpression;

        public LogicalExpression(Expression leftExpression, Token @operator, Expression rightExpression)
        {
            this.LeftExpression = leftExpression;
            this.Operator = @operator;
            this.RightExpression = rightExpression;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

