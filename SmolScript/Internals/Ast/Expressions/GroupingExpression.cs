namespace SmolScript.Internals.Ast.Expressions
{
    public class GroupingExpression : Expression
    {
        public readonly Expression expr;

        public GroupingExpression(Expression expr)
        {
            this.expr = expr;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

