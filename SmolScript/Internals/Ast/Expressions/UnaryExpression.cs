using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    public class UnaryExpression : Expression
    {
        public readonly Token op;
        public readonly Expression right;

        public UnaryExpression(Token op, Expression right)
        {
            this.op = op;
            this.right = right;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

