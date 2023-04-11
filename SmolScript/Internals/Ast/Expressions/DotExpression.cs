using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class DotExpression : Expression
    {
        public readonly Expression objectRefExpression;
        public readonly Expression nextExpressionInChain;

        public DotExpression(Expression objectRefExpression, Expression nextExpressionInChain)
        {
            this.objectRefExpression = objectRefExpression;
            this.nextExpressionInChain = nextExpressionInChain;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

