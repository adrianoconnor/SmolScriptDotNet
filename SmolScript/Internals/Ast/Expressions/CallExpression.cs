using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class CallExpression : Expression
    {
        public readonly Expression callee;
        public readonly Token paren;
        public readonly IList<Expression> args;
        public readonly bool useObjectRef;

        public CallExpression(Expression callee, Token paren, IList<Expression> args, bool useObjectRef = false)
        {
            this.callee = callee;
            this.paren = paren;
            this.args = args;
            this.useObjectRef = useObjectRef;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

