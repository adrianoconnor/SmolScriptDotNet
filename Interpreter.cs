namespace SmolScript
{
    public class RuntimeError : Exception {

        public RuntimeError(string message) : base(message)
        {

        }
    }

    public class Interpreter : IExpressionVisitor, IStatementVisitor {

        public static readonly Environment globalEnv = new Environment();
        private Environment environment = globalEnv;

        private bool _break_while = false;

        public Interpreter()
        {
            globalEnv.Define("ticks", new StdLib.Ticks());
        }

        public void Interpret(IList<Statement> statements)
        {
            try
            {
                foreach(var stmt in statements)
                {
                    execute(stmt);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void execute(Statement stmt)
        {
            var result = stmt.Accept(this);

            if (result != null)
            {
                Console.WriteLine(result);
            }
        }

        public string stringify(object? value)
        {
            if (value == null) 
            {
                return "nil";
            }
            else
            {
                if (value.GetType() == typeof(double))
                {
                    return ((double)value).ToString();
                }
                else if (value.GetType() == typeof(string))
                {
                    return (string)value;
                }
                else if (value.GetType() == typeof(bool))
                {
                    return (bool)value ? "true" : "false";
                }
                else if ((value as ICallable) != null)
                {
                    return "function()";
                }

                throw new RuntimeError($"Unable to stringify variable with type {value.GetType()}");
            }
        }

        public object? Visit(Statement.Expression stmt)
        {
            return evaluate(stmt.expression);

            // Was returning null, but doing this lets us get expression values for the repl
        }

        public object? Visit(Statement.Print stmt)
        {   
            var value = evaluate(stmt.expression);
            Console.WriteLine(stringify(value));
            return null;
        }

        public object? Visit(Statement.Return stmt)
        {   
            var value = evaluate(stmt.expression);

            if (environment.returnVaue != null)
            {
                throw new RuntimeError("ReturnValue already set?");
            }

            environment.returnVaue = value;

            return null;
        }

        public object? Visit(Statement.Break stmt)
        {   
            _break_while = true;

            return null;
        }

        public object? Visit(Statement.Var stmt)
        {   
            object? value = null;
            
            if (stmt.initializerExpression != null)
            {
                value = evaluate(stmt.initializerExpression);
            }

            environment.Define(stmt.name.lexeme, value);

            return null;
        }

        public object? Visit(Statement.Block stmt)
        {   
            executeBlock(stmt.statements, new Environment(this.environment));
            return null;
        }

        public object? Visit(Statement.If stmt)
        {
            var test = evaluate(stmt.testExpression);

            if (isTruthy(test))
            {
                execute(stmt.thenStatement);
            }
            else if (stmt.elseStatement != null)
            {
                execute(stmt.elseStatement);
            }

            return null;
        }

        public object? Visit(Statement.While stmt)
        {
            while (!_break_while && isTruthy(evaluate(stmt.whileCondition)))
            {
                execute(stmt.executeStatement);
            }

            _break_while = false;

            return null;
        }

        public object? Visit(Statement.Function stmt)
        {
            environment.Define(stmt.name.lexeme, new UserDefinedFunction(stmt));

            return null;
        }

        public object? Visit(Expression.Call expr)
        {
            var callee = evaluate(expr.callee);
            var function = callee as ICallable;

            if (function == null)
            {
                throw new RuntimeError("Unable to call function, bad type");
            }

            var args = new List<object?>();
 
            foreach (var arg in expr.args)
            { 
                args.Add(evaluate(arg));
            }
        
            return function.call(this, args);
        }

        public object? Visit(Expression.Binary expr)
        {
            var left = evaluate(expr.left);
            var right = evaluate(expr.right);

            if (left == null || right == null)
            {
                throw new RuntimeError("Null reference");
            }

            switch(expr.op.type)
            {
                case TokenType.MINUS:
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    return (double)left / (double)right;
                case TokenType.STAR:
                    return (double)left * (double)right;
                case TokenType.PLUS:
                    if (left.GetType() == typeof(double) && right.GetType() == left.GetType())
                    {
                        return (double)left + (double)right;
                    }

                    return left.ToString() + right.ToString();
                case TokenType.POW:
                    return Double.Pow((double)left, (double)right);
                case TokenType.GREATER:
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    return (double)left <= (double)right;
                case TokenType.BANG_EQUAL:
                    return !isEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return isEqual(left, right);                          
            }   

            return null;
        }

        public object? Visit(Expression.Logical expr)
        {
            var left = evaluate(expr.left);

            // Short circuit means we only evaluate the left side if that's enough

            if (!isTruthy(left) && expr.op.type == TokenType.AND) return false;
            if (isTruthy(left) && expr.op.type == TokenType.OR) return true;

            // It wasn't enough

            var right = evaluate(expr.right);

            switch(expr.op.type)
            {
                case TokenType.AND:
                    return isTruthy(left) && isTruthy(right);
                case TokenType.OR:
                    return isTruthy(left) || isTruthy(right);
            }

            return null;
        }

        public object? Visit(Expression.Grouping expr)
        {
            return evaluate(expr.expr);
        }

        public object? Visit(Expression.Literal expr)
        {
            return expr.value;
        }

        public object? Visit(Expression.Unary expr)
        {
            var right = evaluate(expr.right);

            if (right == null)
            {
                throw new RuntimeError("Null reference");
            }

            switch(expr.op.type)
            {
                case TokenType.MINUS:
                    return 0-(double)right;
                case TokenType.BANG:
                    return !isTruthy(right);
            }   

            return null;
        }

        public object? Visit(Expression.Variable expr)
        {
            return environment.Get(expr.name.lexeme);
        }

        public object? Visit(Expression.Assign expr)
        {
            environment.Assign(expr.name.lexeme, evaluate(expr.value));
            return null;
        }

        private object? evaluate(Expression expr)
        {
            return expr.Accept(this);
        }

        private bool isTruthy(object? value)
        {
            if (value == null) return false;
            if (value.GetType() == typeof(bool)) return (bool)value;
            return true;
        }

        private bool isEqual(object? a, object? b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;
            return a.Equals(b);
        }

        public void executeBlock(IList<Statement> statements, Environment environment)
        {
            var previous = this.environment;

            try
            {
                this.environment = environment;

                foreach(var statement in statements)
                {
                    execute(statement);

                    if (_break_while) break;
                }
            }
            finally // Restore env
            {
                this.environment = previous;
            }
        }
    }
}