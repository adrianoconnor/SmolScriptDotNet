using SmolScript.Internals.Ast.Expressions;

namespace SmolScript.Internals.Ast.Statements
{
    internal class ThrowStatement : Statement
    {
        public readonly Expression ThrownValueExpression;

        public ThrowStatement(Expression thrownValueExpression)
        {
            this.ThrownValueExpression = thrownValueExpression;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

