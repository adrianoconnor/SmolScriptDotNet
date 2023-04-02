﻿namespace SmolScript.Internals.Ast.Expressions
{
    internal class LiteralExpression : Expression
    {
        public readonly object? value;

        public LiteralExpression(object? value)
        {
            this.value = value;
        }

        public override object? Accept(IExpressionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}

