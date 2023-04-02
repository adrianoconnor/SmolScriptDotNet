using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class AssignExpression : Expression
    {
        public readonly Token name;
        public readonly Expression value;

        public AssignExpression(Token name, Expression value)
        {
            this.name = name;
            this.value = value;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

