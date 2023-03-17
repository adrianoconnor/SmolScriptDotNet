using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    public class LogicalExpression : Expression
    {
        public readonly Expression left;
        public readonly Token op;
        public readonly Expression right;

        public LogicalExpression(Expression left, Token op, Expression right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

