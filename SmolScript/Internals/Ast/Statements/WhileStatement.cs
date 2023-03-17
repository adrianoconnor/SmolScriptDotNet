using SmolScript.Internals.Ast.Expressions;

namespace SmolScript.Internals.Ast.Statements
{
    public class WhileStatement : Statement
    {
        public readonly Expression whileCondition;
        public readonly Statement executeStatement;

        public WhileStatement(Expression whileCondition, Statement executeStatement)
        {
            this.whileCondition = whileCondition;
            this.executeStatement = executeStatement;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}