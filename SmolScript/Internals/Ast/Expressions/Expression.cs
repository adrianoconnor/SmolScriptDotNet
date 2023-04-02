namespace SmolScript.Internals.Ast.Expressions
{
    internal abstract class Expression
    {
        public abstract object? Accept(IExpressionVisitor visitor);
    }
}