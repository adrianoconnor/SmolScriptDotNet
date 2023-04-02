using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class CallExpression : Expression
    {
        public readonly Expression callee;
        public readonly Token paren;
        public readonly IList<object?> args;

        public CallExpression(Expression callee, Token paren, IList<object?> args)
        {
            this.callee = callee;
            this.paren = paren;
            this.args = args;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

