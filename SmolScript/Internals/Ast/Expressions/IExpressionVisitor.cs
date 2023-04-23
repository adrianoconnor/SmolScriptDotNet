namespace SmolScript.Internals.Ast.Expressions
{
    internal interface IExpressionVisitor
    {
        object? Visit(BinaryExpression expr);
        object? Visit(LogicalExpression expr);
        object? Visit(GroupingExpression expr);
        object? Visit(LiteralExpression expr);
        object? Visit(UnaryExpression expr);
        object? Visit(VariableExpression expr);
        object? Visit(AssignExpression expr);
        object? Visit(TernaryExpression expr);
        object? Visit(CallExpression expr);
        object? Visit(NewInstanceExpression expr);
        //object? Visit(DotExpression expr);
        object? Visit(GetExpression expr);
        object? Visit(SetExpression expr);
    }
}
