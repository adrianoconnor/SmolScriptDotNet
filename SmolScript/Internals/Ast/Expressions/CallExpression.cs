using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class CallExpression : Expression
    {
        public readonly Expression Callee;
        public readonly IList<Expression> Arguments;
        public readonly bool UseFetchedObjectEnvironment;

        public CallExpression(Expression callee, IList<Expression> arguments, bool useFetchedObjectEnvironment = false)
        {
            this.Callee = callee;
            this.Arguments = arguments;
            this.UseFetchedObjectEnvironment = useFetchedObjectEnvironment;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

