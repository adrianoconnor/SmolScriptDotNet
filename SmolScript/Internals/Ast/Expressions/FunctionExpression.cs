using SmolScript.Internals;
using SmolScript.Internals.Ast.Statements;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class FunctionExpression : Expression
    {
        public readonly IList<Token> parameters;
        public readonly BlockStatement functionBody;

        public FunctionExpression(IList<Token> parameters, BlockStatement functionBody)
        {
            this.parameters = parameters;
            this.functionBody = functionBody;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

