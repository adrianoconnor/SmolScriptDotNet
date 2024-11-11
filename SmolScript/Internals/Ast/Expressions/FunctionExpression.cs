using SmolScript.Internals;
using SmolScript.Internals.Ast.Statements;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class FunctionExpression : Expression
    {
        public readonly IList<Token> Parameters;
        public readonly BlockStatement FunctionBody;

        public FunctionExpression(IList<Token> parameters, BlockStatement functionBody)
        {
            this.Parameters = parameters;
            this.FunctionBody = functionBody;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

