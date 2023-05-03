using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class IndexerSetExpression : Expression
    {
        public readonly Expression obj;
        public readonly Expression indexerExpr;
        public readonly Expression value;

        public IndexerSetExpression(Expression obj, Expression indexerExpr, Expression value)
        {
            this.obj = obj;
            this.indexerExpr = indexerExpr;
            this.value = value;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

