using SmolScript.Internals;
using SmolScript.Internals.Ast.Expressions;

namespace SmolScript.Internals.Ast.Statements
{
    internal class VarStatement : Statement
    {
        public readonly Token VariableName;
        public readonly Expression? InitialValueExpression;
        
        public int? FirstTokenIndex;
        public int? LastTokenIndex;
        
        public VarStatement(Token variableName, Expression? initialValueExpression, int? firstTokenIndex = null, int? lastTokenIndex = null)
        {
            this.VariableName = variableName;
            this.InitialValueExpression = initialValueExpression;
            
            this.FirstTokenIndex = firstTokenIndex;
            this.LastTokenIndex = lastTokenIndex;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

