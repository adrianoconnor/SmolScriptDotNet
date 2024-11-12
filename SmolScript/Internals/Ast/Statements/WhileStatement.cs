using SmolScript.Internals.Ast.Expressions;

namespace SmolScript.Internals.Ast.Statements
{
    internal class WhileStatement : Statement
    {
        public readonly Expression WhileCondition;
        public readonly Statement ExecuteStatement;

        public WhileStatement(Expression whileCondition, Statement executeStatement)
        {
            this.WhileCondition = whileCondition;
            this.ExecuteStatement = executeStatement;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}