namespace SmolScript.Internals.Ast.Expressions
{
    internal class GroupingExpression : Expression
    {
        public readonly Expression GroupedExpression;

        public GroupingExpression(Expression groupedExpression)
        {
            this.GroupedExpression = groupedExpression;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

