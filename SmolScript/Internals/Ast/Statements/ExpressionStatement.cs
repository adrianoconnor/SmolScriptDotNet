using SmolScript.Internals.Ast.Expressions;

namespace SmolScript.Internals.Ast.Statements
{
    internal class ExpressionStatement : Statement
    {
        public readonly Expression expression;

        public ExpressionStatement(Expression expression)
        {
            this.expression = expression;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

