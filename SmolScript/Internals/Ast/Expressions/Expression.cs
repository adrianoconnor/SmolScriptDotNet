namespace SmolScript.Internals.Ast.Expressions
{
    public abstract class Expression
    {
        public abstract object? Accept(IExpressionVisitor visitor);
    }
}