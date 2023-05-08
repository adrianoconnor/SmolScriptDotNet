using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class NewInstanceExpression : Expression
    {
        public readonly Token className;
        public readonly IList<Expression> ctorArgs;

        public NewInstanceExpression(Token className, IList<Expression> ctorArgs)
        {
            this.className = className;
            this.ctorArgs = ctorArgs;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

