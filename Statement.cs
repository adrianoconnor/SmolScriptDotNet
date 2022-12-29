namespace ABasic
{
    public interface IStatementVisitor
    {
        object? Visit(Statement.Expression stmt);
        object? Visit(Statement.Print stmt);
        object? Visit(Statement.Var stmt);
        object? Visit(Statement.Block stmt);
        object? Visit(Statement.If stmt);
        object? Visit(Statement.While stmt);
        object? Visit(Statement.Break stmt);
    }

    public abstract class Statement
    {
        public abstract object? Accept(IStatementVisitor visitor);

        public Statement? parent;

        public class Expression: Statement
        {
            public readonly ABasic.Expression expression;

            public Expression(ABasic.Expression expression)
            {
                this.expression = expression;
            }

            public override object? Accept(IStatementVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Print: Statement
        {
            public readonly ABasic.Expression expression;

            public Print(ABasic.Expression expression)
            {
                this.expression = expression;
            }

            public override object? Accept(IStatementVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Break: Statement
        {
            public Break()
            {
            }

            public override object? Accept(IStatementVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Var: Statement
        {
            public readonly Token name;
            public readonly ABasic.Expression? initializerExpression;

            public Var(Token name, ABasic.Expression? initializerExpression)
            {
                this.name = name;
                this.initializerExpression = initializerExpression;
            }

            public override object? Accept(IStatementVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Block: Statement
        {
            public readonly IList<Statement> statements;

            public Block(IList<Statement> statements)
            {
                this.statements = statements;
            }

            public override object? Accept(IStatementVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class If: Statement
        {
            public readonly ABasic.Expression testExpression;
            public readonly ABasic.Statement thenStatement;
            public readonly ABasic.Statement? elseStatement;

            public If(ABasic.Expression testExpression, ABasic.Statement thenStatement, ABasic.Statement? elseStatement)
            {
                this.testExpression = testExpression;
                this.thenStatement = thenStatement;
                this.elseStatement = elseStatement;
            }

            public override object? Accept(IStatementVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class While: Statement
        {
            public readonly ABasic.Expression whileCondition;
            public readonly ABasic.Statement executeStatement;

            public While(ABasic.Expression whileCondition, ABasic.Statement executeStatement)
            {
                this.whileCondition = whileCondition;
                this.executeStatement = executeStatement;
            }

            public override object? Accept(IStatementVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }
    }
}
