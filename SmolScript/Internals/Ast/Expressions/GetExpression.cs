using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class GetExpression : Expression
    {
        public readonly Expression TargetObject;
        public readonly Token AttributeName;

        public GetExpression(Expression targetObject, Token attributeName)
        {
            this.TargetObject = targetObject;
            this.AttributeName = attributeName;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

