using SmolScript.Internals.Ast.Expressions;

namespace SmolScript.Internals.Ast.Statements
{
    internal class ReturnStatement : Statement
    {
        public readonly Expression ReturnValueExpression;

        public ReturnStatement(Expression returnValueExpression)
        {
            this.ReturnValueExpression = returnValueExpression;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

