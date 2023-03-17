using SmolScript.Internals;
using SmolScript.Internals.Ast.Expressions;

namespace SmolScript.Internals.Ast.Statements
{
    public class VarStatement : Statement
    {
        public readonly Token name;
        public readonly Expression? initializerExpression;

        public VarStatement(Token name, Expression? initializerExpression)
        {
            this.name = name;
            this.initializerExpression = initializerExpression;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

