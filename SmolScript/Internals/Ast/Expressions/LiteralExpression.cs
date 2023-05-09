using SmolScript.Internals.SmolVariableTypes;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class LiteralExpression : Expression
    {
        public readonly SmolVariableType value;

        public LiteralExpression(SmolVariableType value)
        {
            this.value = value;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

