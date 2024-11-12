using SmolScript.Internals.Ast.Expressions;

namespace SmolScript.Internals.Ast.Statements
{
    internal class IfStatement : Statement
    {
        public readonly Expression TestExpression;
        public readonly Statement StatementWhenTrue;
        public readonly Statement? ElseStatement;

        public IfStatement(Expression testExpression, Statement statementWhenTrue, Statement? elseStatement)
        {
            this.TestExpression = testExpression;
            this.StatementWhenTrue = statementWhenTrue;
            this.ElseStatement = elseStatement;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

