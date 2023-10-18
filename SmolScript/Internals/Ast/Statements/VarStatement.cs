using SmolScript.Internals;
using SmolScript.Internals.Ast.Expressions;

namespace SmolScript.Internals.Ast.Statements
{
    internal class VarStatement : Statement
    {
        public readonly Token name;
        public readonly Expression? initializerExpression;

        public VarStatement(Token name, Expression? initializerExpression, int? firstTokenIndex = null, int? lastTokenIndex = null)
        {
            this.name = name;
            this.initializerExpression = initializerExpression;
            this.firstTokenIndex = firstTokenIndex;
            this.lastTokenIndex = lastTokenIndex;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }

        // The tokens that span the var declaration and initalization (if applicable)
        public int? firstTokenIndex;
        public int? lastTokenIndex;
    }
}

