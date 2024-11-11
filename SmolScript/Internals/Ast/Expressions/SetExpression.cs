using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class SetExpression : Expression
    {
        public readonly Expression TargetObject;
        public readonly Token AttributeName;
        public readonly Expression Value;

        public SetExpression(Expression targetObject, Token attributeName, Expression value)
        {
            this.TargetObject = targetObject;
            this.AttributeName = attributeName;
            this.Value = value;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

