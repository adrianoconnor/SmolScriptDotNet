using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class IndexerGetExpression : Expression
    {
        public readonly Expression TargetObject;
        public readonly Expression IndexerExpression;

        public IndexerGetExpression(Expression targetObject, Expression indexerExpression)
        {
            this.TargetObject = targetObject;

            this.IndexerExpression = indexerExpression;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

