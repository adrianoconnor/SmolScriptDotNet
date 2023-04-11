using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class NewInstanceExpression : Expression
    {
        public readonly Token className;

        public NewInstanceExpression(Token className)
        {
            this.className = className;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

