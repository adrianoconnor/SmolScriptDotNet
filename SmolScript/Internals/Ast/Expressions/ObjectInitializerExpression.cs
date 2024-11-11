using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class ObjectInitializerExpression : Expression
    {
        public readonly Token ObjectName;
        public readonly Expression Value;

        public ObjectInitializerExpression(Token objectName, Expression value)
        {
            this.ObjectName = objectName;
            this.Value = value;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

