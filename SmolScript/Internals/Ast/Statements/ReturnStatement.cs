using SmolScript.Internals.Ast.Expressions;

namespace SmolScript.Internals.Ast.Statements
{
    internal class ReturnStatement : Statement
    {
        public readonly Expression expression;

        public ReturnStatement(Expression expression)
        {
            this.expression = expression;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

