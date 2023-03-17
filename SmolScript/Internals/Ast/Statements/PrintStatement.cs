using SmolScript.Internals.Ast.Expressions;

namespace SmolScript.Internals.Ast.Statements
{
    public class PrintStatement : Statement
    {
        public readonly Expression expression;

        public PrintStatement(Expression expression)
        {
            this.expression = expression;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

