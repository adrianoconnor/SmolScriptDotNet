namespace SmolScript.Statements
{
    public abstract class Statement
    {
        public abstract object? Accept(IStatementVisitor visitor);

        public Statement? parent;
       
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
            public readonly Expression testExpression;
            public readonly Statement thenStatement;
            public readonly Statement? elseStatement;

            public If(SmolScript.Expression testExpression, Statement thenStatement, Statement? elseStatement)
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

        public class Ternary: Statement
        {
            public readonly SmolScript.Expression testExpression;
            public readonly SmolScript.Expression thenExpression;
            public readonly SmolScript.Expression elseExpression;

            public Ternary(SmolScript.Expression testExpression, SmolScript.Expression thenExpression, SmolScript.Expression elseExpression)
            {
                this.testExpression = testExpression;
                this.thenExpression = thenExpression;
                this.elseExpression = elseExpression;
            }

            public override object? Accept(IStatementVisitor visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class While: Statement
        {
            public readonly Expression whileCondition;
            public readonly Statement executeStatement;

            public While(SmolScript.Expression whileCondition, Statement executeStatement)
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
            public readonly Token? name;
            public readonly IList<Token> parameters;
            public readonly Statement.Block functionBody;

            public Function(Token? name, IList<Token> parameters, Statement.Block functionBody)
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
