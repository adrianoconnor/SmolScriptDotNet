using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class AssignExpression : Expression
    {
        public readonly Token VariableName;
        public readonly Expression ValueExpression;
        
        public AssignExpression(Token variableName, Expression valueExpression)
        {
            this.VariableName = variableName;
            this.ValueExpression = valueExpression;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

