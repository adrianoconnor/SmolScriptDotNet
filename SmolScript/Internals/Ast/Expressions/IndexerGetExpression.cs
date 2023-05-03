using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class IndexerGetExpression : Expression
    {
        public readonly Expression obj;
        public readonly Expression indexerExpr;

        public IndexerGetExpression(Expression obj, Expression indexerExpr)
        {
            this.obj = obj;

            this.indexerExpr = indexerExpr;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

