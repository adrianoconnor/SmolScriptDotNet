using SmolScript.Internals.Ast.Expressions;

namespace SmolScript.Internals.Ast.Statements
{
    public class IfStatement : Statement
    {
        public readonly Expression testExpression;
        public readonly Statement thenStatement;
        public readonly Statement? elseStatement;

        public IfStatement(Expression testExpression, Statement thenStatement, Statement? elseStatement)
        {
            this.testExpression = testExpression;
            this.thenStatement = thenStatement;
            this.elseStatement = elseStatement;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

