namespace SmolScript.Internals.Ast.Expressions
{
    /// <summary>
    /// Represents an expression that has been explicity grouped, for the purpose
    /// of order of execution (e.g., 3 * (2 + 1)).
    /// 
    /// Also used for wrapping expressions embedded in string literals (e.g., `${a}${b+c}`)
    /// forcing a call to toString() before adding to the rest of the string (to avoid
    /// accidental math operations).
    /// </summary>
    internal class GroupingExpression : Expression
    {
        public readonly Expression GroupedExpression;
        public readonly bool CastToStringForEmbeddedStringExpression;

        public GroupingExpression(Expression groupedExpression, bool castToStringForEmbeddedStringExpression = false)
        {
            this.GroupedExpression = groupedExpression;
            this.CastToStringForEmbeddedStringExpression = castToStringForEmbeddedStringExpression;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

