using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class VariableExpression : Expression
    {
        public readonly Token name;
        public readonly TokenType? prepostfixop;

        public VariableExpression(Token name, TokenType? prepostfixop = null)
        {
            this.name = name;

            this.prepostfixop = prepostfixop;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

