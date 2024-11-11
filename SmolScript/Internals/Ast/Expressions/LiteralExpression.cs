using SmolScript.Internals.SmolVariableTypes;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class LiteralExpression : Expression
    {
        public readonly SmolVariableType Value;

        public LiteralExpression(SmolVariableType value)
        {
            this.Value = value;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

