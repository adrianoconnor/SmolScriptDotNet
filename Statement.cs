namespace SmolScript
{
    public interface IStatementVisitor
    {
        object? Visit(Statement.Expression stmt);
        object? Visit(Statement.Print stmt);
        object? Visit(Statement.Return stmt);
        object? Visit(Statement.Var stmt);
        object? Visit(Statement.Block stmt);
        object? Visit(Statement.If stmt);
        object? Visit(Statement.While stmt);
        object? Visit(Statement.Break stmt);
        object? Visit(Statement.Function stmt);
    }

    public abstract class Statement
    {
        public abstract object? Accept(IStatementVisitor visitor);

        public Statement? parent;

        public class Expression: Statement
        {
            public readonly SmolScript.Expression expression;

            public Expression(SmolScript.Expression expression)
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
            public readonly SmolScript.Expression expression;

            public Print(SmolScript.Expression expression)
            {
                this.expression = expression;
            }

            public override object? Accept(IStatementVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Return: Statement
        {
            public readonly SmolScript.Expression expression;

            public Return(SmolScript.Expression expression)
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
            public readonly SmolScript.Expression? initializerExpression;

            public Var(Token name, SmolScript.Expression? initializerExpression)
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
            public readonly SmolScript.Expression testExpression;
            public readonly SmolScript.Statement thenStatement;
            public readonly SmolScript.Statement? elseStatement;

            public If(SmolScript.Expression testExpression, SmolScript.Statement thenStatement, SmolScript.Statement? elseStatement)
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
            public readonly SmolScript.Expression whileCondition;
            public readonly SmolScript.Statement executeStatement;

            public While(SmolScript.Expression whileCondition, SmolScript.Statement executeStatement)
            {
                this.whileCondition = whileCondition;
                this.executeStatement = executeStatement;
            }

            public override object? Accept(IStatementVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class Function: Statement
        {
            public readonly Token name;
            public readonly IList<Token> parameters;
            public readonly Statement.Block functionBody;

            public Function(Token name, IList<Token> parameters, Statement.Block functionBody)
            {
                this.name = name;
                this.parameters = parameters;
                this.functionBody = functionBody;
            }

            public override object? Accept(IStatementVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }
    }
}
