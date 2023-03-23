using SmolScript.Internals.Ast.Expressions;

namespace SmolScript.Internals.Ast.Statements
{
	public class ThrowStatement : Statement
	{
        public readonly Expression? expression;

        public ThrowStatement(Expression? expression = null)
        {
            this.expression = expression;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

