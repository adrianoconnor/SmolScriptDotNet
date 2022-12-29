namespace ABasic
{
    public interface IExpressionVisitor
    {
        object? Visit(Expression.Binary expr);
        object? Visit(Expression.Logical expr);
        object? Visit(Expression.Grouping expr);
        object? Visit(Expression.Literal expr);
        object? Visit(Expression.Unary expr);
        object? Visit(Expression.Variable expr);
        object? Visit(Expression.Assign expr);
    }

    public abstract class Expression
    {
        public abstract object? Accept(IExpressionVisitor visitor);

        public class Binary: Expression
        {
            public readonly Expression left;
            public readonly Token op;
            public readonly Expression right;

            public Binary(Expression left, Token op, Expression right)
            {
                this.left = left;
                this.op = op;
                this.right = right;
            }

            public override object? Accept(IExpressionVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Logical: Expression
        {
            public readonly Expression left;
            public readonly Token op;
            public readonly Expression right;

            public Logical(Expression left, Token op, Expression right)
            {
                this.left = left;
                this.op = op;
                this.right = right;
            }

            public override object? Accept(IExpressionVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Grouping: Expression
        {
            public readonly Expression expr;

            public Grouping(Expression expr)
            {
                this.expr = expr;
            }

            public override object? Accept(IExpressionVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Literal: Expression
        {
            public readonly object? value;

            public Literal(object? value)
            {
                this.value = value;
            }

            public override object? Accept(IExpressionVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Unary: Expression
        {
            public readonly Token op;
            public readonly Expression right;

            public Unary(Token op, Expression right)
            {
                this.op = op;
                this.right = right;
            }

            public override object? Accept(IExpressionVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Variable: Expression
        {
            public readonly Token name;

            public Variable(Token name)
            {
                this.name = name;
            }

            public override object? Accept(IExpressionVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Assign: Expression
        {
            public readonly Token name;
            public readonly Expression value;

            public Assign(Token name, Expression value)
            {
                this.name = name;
                this.value = value;
            }

            public override object? Accept(IExpressionVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }
    }
}