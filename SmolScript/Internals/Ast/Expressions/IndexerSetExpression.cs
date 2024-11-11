using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class IndexerSetExpression : Expression
    {
        public readonly Expression TargetObject;
        public readonly Expression IndexerExpression;
        public readonly Expression Value;

        public IndexerSetExpression(Expression targetObject, Expression indexerExpression, Expression value)
        {
            this.TargetObject = targetObject;
            this.IndexerExpression = indexerExpression;
            this.Value = value;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

