using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class VariableExpression : Expression
    {
        public readonly Token VariableName;
        public readonly TokenType? prepostfixop; // TODO: This should probably be PrefixUnary or PostfixUnary

        public VariableExpression(Token variableName, TokenType? prepostfixop = null)
        {
            this.VariableName = variableName;
            this.prepostfixop = prepostfixop;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

