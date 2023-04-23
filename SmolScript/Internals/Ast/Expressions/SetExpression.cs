using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class SetExpression : Expression
    {
        public readonly Expression obj;
        public readonly Token name;
        public readonly Expression value;

        public SetExpression(Expression obj, Token name, Expression value)
        {
            this.obj = obj;
            this.name = name;
            this.value = value;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

