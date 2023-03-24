using SmolScript.Internals.Ast.Expressions;
using SmolScript.Internals.Ast.Statements;

namespace SmolScript.Internals.Ast.Interpreter
{
    public class RuntimeError : Exception
    {
        public RuntimeError(string message) : base(message)
        {

        }
    }

    public class ReturnFromUserDefinedFunction : Exception
    {
        public object? ReturnValue { get; private set; }

        public ReturnFromUserDefinedFunction(object? returnValue)
        {
            this.ReturnValue = returnValue;
        }
    }

    public class BreakFromLoop : Exception
    {

    }

    public class ContinueInLoop : Exception
    {

    }

    public class AstInterpreter : IExpressionVisitor, IStatementVisitor
    {
        public readonly Environment globalEnv = new Environment();
        private Environment environment;

        public AstInterpreter()
        {
            // For now just manually configure the global functions from stdlib

            environment = globalEnv;

            globalEnv.Define("ticks", new StdLib.Ticks());
        }

        public void Interpret(string source)
        {
            var scanner = new Scanner(source);
            var scanResult = scanner.ScanTokens();
            var parser = new Parser(scanResult.tokens);
            var statements = parser.Parse();

            this.Interpret(statements);
        }

        public void Interpret(IList<Statement> statements)
        {
            try
            {
                foreach (var stmt in statements)
                {
                    execute(stmt);
                }
            }
            catch (Exception ex)
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

        public object? Visit(ExpressionStatement stmt)
        {
            return evaluate(stmt.expression);

            // Was returning null, but doing this lets us get expression values for the repl
        }

        public object? Visit(PrintStatement stmt)
        {
            var value = evaluate(stmt.expression);
            Console.WriteLine(stringify(value));
            return null;
        }

        public object? Visit(ReturnStatement stmt)
        {
            var returnValue = evaluate(stmt.expression);

            throw new ReturnFromUserDefinedFunction(returnValue);
        }

        public object? Visit(BreakStatement stmt)
        {
            throw new BreakFromLoop();
        }

        public object? Visit(ContinueStatement stmt)
        {
            throw new ContinueInLoop();
        }

        public object? Visit(VarStatement stmt)
        {
            object? value = null;

            if (stmt.initializerExpression != null)
            {
                value = evaluate(stmt.initializerExpression);
            }

            environment.Define(stmt.name.lexeme, value);

            return null;
        }

        public object? Visit(BlockStatement stmt)
        {
            executeBlock(stmt.statements, new Environment(this.environment));
            return null;
        }

        public object? Visit(IfStatement stmt)
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

        public object? Visit(TernaryStatement stmt)
        {
            var test = evaluate(stmt.testExpression);

            if (isTruthy(test))
            {
                return evaluate(stmt.thenExpression);
            }
            else
            {
                return evaluate(stmt.elseExpression);
            }
        }

        public object? Visit(WhileStatement stmt)
        {
            try
            {
                while (isTruthy(evaluate(stmt.whileCondition)))
                {
                    try
                    {
                        execute(stmt.executeStatement);
                    }
                    catch(ContinueInLoop)
                    {
                    }
                }
            }
            catch (BreakFromLoop)
            {

            }

            return null;
        }

        public object? Visit(ThrowStatement stmt)
        {
            try
            {
                throw new Exception("Try/Catch not implemented in AST interpreter");
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {

            }

            return null;
        }

        public object? Visit(TryStatement stmt)
        {
            try
            {
                throw new Exception("Try/Catch not implemented in AST interpreter");
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {

            }

            return null;
        }

        public object? Visit(FunctionStatement stmt)
        {
            if (stmt.name == null)
            {
                throw new RuntimeError("Anonymous function not allowed here");
            }

            environment.Define(stmt.name.lexeme, new SmolFunctionWrapper(stmt, environment));

            return null;
        }

        public object? Visit(CallExpression expr)
        {
            var callee = evaluate(expr.callee);

            var function = callee as ICallable;

            if (function != null)
            {

                var args = new List<object?>();

                foreach (var arg in expr.args)
                {
                    var stmt = arg as Expression;

                    if (stmt != null)
                    {
                        args.Add(evaluate(stmt));
                        continue;
                    }

                    var fn = arg as FunctionStatement;

                    if (fn != null)
                    {
                        args.Add(fn);
                        continue;
                    }

                    throw new RuntimeError("Unable to process one or more arguments");
                }

                var result = function.call(args);

                return result;
            }

            var smolFunction = callee as ICallableSmolFunction;

            if (smolFunction != null)
            {

                var args = new List<object?>();

                foreach (var arg in expr.args)
                {
                    var stmt = arg as Expression;

                    if (stmt != null)
                    {
                        args.Add(evaluate(stmt));
                        continue;
                    }

                    var fn = arg as FunctionStatement;

                    if (fn != null)
                    {
                        args.Add(fn);
                        continue;
                    }

                    throw new RuntimeError("Unable to process one or more arguments");
                }

                var result = smolFunction.call(this, args);

                return result;
            }



            throw new RuntimeError("Unable to call function");
        }

        public object? Visit(BinaryExpression expr)
        {
            var left = evaluate(expr.left);
            var right = evaluate(expr.right);

            if (left == null || right == null)
            {
                throw new RuntimeError("Null reference");
            }

            switch (expr.op.type)
            {
                case TokenType.MINUS:
                    return (double)left - (double)right;
                case TokenType.DIVIDE:
                    return (double)left / (double)right;
                case TokenType.MULTIPLY:
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
                case TokenType.NOT_EQUAL:
                    return !isEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return isEqual(left, right);
                case TokenType.BITWISE_AND:
                    // A bit stupid, but we have to cast double>int>double...
                    return (double)((int)(double)left & (int)(double)right);
                case TokenType.BITWISE_OR:
                    return (double)((int)(double)left | (int)(double)right);
                case TokenType.REMAINDER:
                    return (double)left % (double)right;
            }

            return null;
        }

        public object? Visit(LogicalExpression expr)
        {
            var left = evaluate(expr.left);

            // Short circuit means we only evaluate the left side if that's enough

            if (!isTruthy(left) && expr.op.type == TokenType.LOGICAL_AND) return false;
            if (isTruthy(left) && expr.op.type == TokenType.LOGICAL_OR) return true;

            // It wasn't enough

            var right = evaluate(expr.right);

            switch (expr.op.type)
            {
                case TokenType.LOGICAL_AND:
                    return isTruthy(left) && isTruthy(right);
                case TokenType.LOGICAL_OR:
                    return isTruthy(left) || isTruthy(right);
            }

            return null;
        }

        public object? Visit(GroupingExpression expr)
        {
            return evaluate(expr.expr);
        }

        public object? Visit(LiteralExpression expr)
        {
            return expr.value;
        }

        public object? Visit(UnaryExpression expr)
        {
            var right = evaluate(expr.right);

            if (right == null)
            {
                throw new RuntimeError("Null reference");
            }

            switch (expr.op.type)
            {
                case TokenType.MINUS:
                    return 0 - (double)right;
                case TokenType.NOT:
                    return !isTruthy(right);
            }

            return null;
        }

        public object? Visit(VariableExpression expr)
        {
            if (expr.prepostfixop == null)
            {
                return environment.Get(expr.name.lexeme);
            }
            else
            {
                Console.WriteLine($"Getting var ${expr.name.lexeme}");

                var val = (double)environment.Get(expr.name.lexeme)!;

                switch (expr.prepostfixop!.Value)
                {
                    case TokenType.PREFIX_INCREMENT:
                        environment.Assign(expr.name.lexeme, val + 1);
                        return val + 1;
                    case TokenType.POSTFIX_INCREMENT:
                        environment.Assign(expr.name.lexeme, val + 1);
                        return val;
                    case TokenType.PREFIX_DECREMENT:
                        environment.Assign(expr.name.lexeme, val - 1);
                        return val - 1;
                    case TokenType.POSTFIX_DECREMENT:
                        environment.Assign(expr.name.lexeme, val - 1);
                        return val;
                }
            }

            throw new Exception();
        }

        public object? Visit(AssignExpression expr)
        {
            environment.Assign(expr.name.lexeme, evaluate(expr.value));
            return null;
        }

        private object? evaluate(Expression expr)
        {
            return expr.Accept(this);
        }

        private static bool isTruthy(object? value)
        {
            if (value == null) return false;
            if (value.GetType() == typeof(bool)) return (bool)value;
            return true;
        }

        private static bool isEqual(object? a, object? b)
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

                foreach (var statement in statements)
                {
                    execute(statement);
                }
            }
            finally // Restore env
            {
                this.environment = previous;
            }
        }

        public object? Visit(DebuggerStatement stmt)
        {
            return null;
        }
    }
}