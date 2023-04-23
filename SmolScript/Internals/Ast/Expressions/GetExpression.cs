using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class GetExpression : Expression
    {
        public readonly Expression obj;
        public readonly Token name;

        public GetExpression(Expression obj, Token name)
        {
            this.obj = obj;

            this.name = name;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

