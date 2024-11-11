using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Expressions
{
    internal class NewInstanceExpression : Expression
    {
        public readonly Token ClassName;
        public readonly IList<Expression> ConstructorArgs;

        public NewInstanceExpression(Token className, IList<Expression> constructorArgs)
        {
            this.ClassName = className;
            this.ConstructorArgs = constructorArgs;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

