using SmolScript.Internals.Ast.Expressions;
using SmolScript.Internals.Ast.Statements;
using SmolScript.Internals.SmolVariableTypes;

namespace SmolScript.Internals
{
    public class ParseError : Exception
    {
        public int LineNumber { get; set; }
        public IList<ParseError>? Errors = null;

        internal ParseError(Token token, string message) :
           base(message)
        {
            this.LineNumber = token.line;
        }

        public ParseError(IList<ParseError>? errors, string message) :
           base(message)
        {
            this.Errors = errors;
        }

    }

    /*
    program        → declaration* EOF ;

    declaration    → functionDecl
                   | varDecl
                   | statement ;

    functionDecl   → "function" function ;

    function       → IDENTIFIER "(" parameters? ")" block ;
    parameters     → IDENTIFIER ( "," IDENTIFIER )* ;

    statement      → exprStmt
                   | ifStmt
                   | whileStmt
                   | forStmt
                   | printStmt
                   | returnStmt
                   | breakStmt
                   | block ;

    ifStmt         → "if" "(" expression ")" statement
                   ( "else" statement )? ;

    whileStmt      → "while" "(" expression ")" statement ;

    forStmt        → "for" "(" ( varDecl | exprStmt | ";" )
                     expression? ";"
                     expression? ")" statement ;

    block          → "{" declaration* "}" ;

    exprStmt       → expression ";" ;
    printStmt      → "print" expression ";" ;
    returnStmt     → "return" expression ";" ;

    expression     → assignment ;
    assignment     → IDENTIFIER "=" assignment
                   | function

    function       -> "function" function | logical_or

    logical_or     → logical_and ( ( AND | OR ) logical_and )* ; // ?????
    logical_and    → equality ( ( AND | OR ) equality )* ; // ?????

    equality       → comparison ( ( "!=" | "==" ) comparison )* ;

    comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;

    term           → factor ( ( "-" | "+" ) factor )* ;
    factor         → power ( ( "/" | "*" ) power )* ;
    power          → unary ( ( "^" ) unary )* ;
    unary          → ( "!" | "-" ) unary
                   | call ;

    call           → primary ( "(" arguments? ")" )* ;

    primary        → NUMBER | STRING | "true" | "false" | "nil"
                   | "(" expression ")"
                   | IDENTIFIER ;
    */

    internal class Parser
    {
        private readonly Token[] _tokens;
        private int _current = 0;

        // Maintain a basic call stack of enclosing statement types, so we can do some validation
        // on certain commands that can only appear inside certain other statements (while/break)
        private Stack<string> _statementCallStack = new Stack<string>();

        public Parser(IList<Token> tokens)
        {
            this._tokens = tokens.ToArray();
        }

        public IList<Statement> Parse()
        {
            var statements = new List<Statement>();
            var errors = new List<ParseError>();

            while (!ReachedEnd())
            {
                try
                {
                    //while (peek().type == TokenType.SEMICOLON) consume(TokenType.SEMICOLON, "");

                    statements.Add(declaration());
                }
                catch (ParseError e)
                {
                    errors.Add(e);
                }
            }

            if (errors.Any())
            {
                Console.WriteLine("Errors:");
                foreach (var error in errors)
                {
                    Console.WriteLine(error.Message);
                }

                throw new ParseError(errors, "Encounted one or more errors parsing");
            }

            return statements;
        }

        private bool match(params TokenType[] tokenTypes)
        {
            foreach (var tokenType in tokenTypes)
            {
                if (check(tokenType))
                {
                    advance();
                    return true;
                }
            }

            return false;
        }

        private bool check(TokenType tokenType, int skip = 0)
        {
            if (ReachedEnd()) return false;
            return peek(skip).type == tokenType;
        }

        private Token peek(int skip = 0)
        {
            return _tokens[_current + skip];
        }

        private Token advance()
        {
            if (!ReachedEnd()) _current++;

            return previous();
        }

        public Token previous(int skip = 0)
        {
            return _tokens[_current - 1 - (skip * 1)];
        }

        public Token consume(TokenType tokenType, string errorIfNotFound)
        {
            if (check(tokenType)) return advance();

            // If we expected a ; but got a newline, we just wave it through
            if (tokenType == TokenType.SEMICOLON && _tokens[_current - 1].followedByLineBreak)
            {
                // We need to return a token, so we'll make a fake semicolon
                return new Token(TokenType.SEMICOLON, "", "", -1);
            }

            // If we expected a ; but got a }, we also wave that through
            if (tokenType == TokenType.SEMICOLON && (this.check(TokenType.RIGHT_BRACE) || this.peek().type == TokenType.EOF))
            {
                return new Token(TokenType.SEMICOLON, "", "", -1);//, -1, -1, -1);
            }

            throw error(peek(), errorIfNotFound);
        }

        private ParseError error(Token token, string errorMessage)
        {
            return new ParseError(token, errorMessage);
        }

        private void synchronize()
        {
            advance();

            while (!ReachedEnd())
            {
                if (previous().type == TokenType.SEMICOLON) return;

                switch (peek().type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUNC:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                    case TokenType.TRY:
                        return;
                }

                advance();
            }

            _statementCallStack.Clear();
        }

        private bool ReachedEnd()
        {
            return peek().type == TokenType.EOF;
        }

        private Statement declaration()
        {
            try
            {
                if (match(TokenType.FUNC)) return functionDeclaration();
                if (match(TokenType.VAR)) return varDeclaration();
                if (match(TokenType.CLASS)) return classDeclaration();

                return statement();
            }
            catch (ParseError)
            {
                synchronize();
                throw;
            }

        }

        private Statement functionDeclaration()
        {
            _statementCallStack.Push("FUNCTION");

            Token functionName = consume(TokenType.IDENTIFIER, "Expected function name");

            var functionParams = new List<Token>();

            consume(TokenType.LEFT_BRACKET, "Expected (");

            if (!check(TokenType.RIGHT_BRACKET))
            {
                do
                {
                    if (functionParams.Count() >= 127)
                    {
                        error(peek(), "Can't define a function with more than 127 parameters.");
                    }

                    functionParams.Add(consume(TokenType.IDENTIFIER, "Expected parameter name"));
                } while (match(TokenType.COMMA));
            }

            consume(TokenType.RIGHT_BRACKET, "Expected )");
            consume(TokenType.LEFT_BRACE, "Expected {");

            var functionBody = block();

            _ = _statementCallStack.Pop();

            return new FunctionStatement(functionName, functionParams, functionBody);
        }

        private Statement varDeclaration()
        {
            var name = consume(TokenType.IDENTIFIER, "Expected variable name");

            Expression? initializer = null;

            if (match(TokenType.EQUAL))
            {
                initializer = expression();
            }

            consume(TokenType.SEMICOLON, "Expected ;");
            return new VarStatement(name, initializer);
        }

        private Statement classDeclaration()
        {
            Token className = consume(TokenType.IDENTIFIER, "Expected class name");
            Token? superclassName = null;
            List<FunctionStatement> functions = new List<FunctionStatement>();

            if (match(TokenType.COLON))
            {
                superclassName = consume(TokenType.IDENTIFIER, "Expected superclass name");
            }

            consume(TokenType.LEFT_BRACE, "Expected {");

            while (!check(TokenType.RIGHT_BRACE) && !ReachedEnd())
            {
                if (check(TokenType.IDENTIFIER) && check(TokenType.LEFT_BRACKET, 1))
                {
                    functions.Add((FunctionStatement)functionDeclaration());
                }
                else
                {
                    throw new Exception($"Didn't expect to find {peek()} in the class body");
                }
            }

            consume(TokenType.RIGHT_BRACE, "Expected }");

            return new ClassStatement(className, superclassName, functions);
        }


        private Statement statement()
        {
            if (match(TokenType.IF)) return ifStatement();
            if (match(TokenType.WHILE)) return whileStatement();
            if (match(TokenType.TRY)) return tryStatement();
            if (match(TokenType.THROW)) return throwStatement();
            if (match(TokenType.FOR)) return forStatement();
            if (match(TokenType.PRINT)) return printStatement();
            if (match(TokenType.RETURN)) return returnStatement();
            if (match(TokenType.BREAK)) return breakStatement();
            if (match(TokenType.CONTINUE)) return continueStatement();
            if (match(TokenType.LEFT_BRACE)) return block();
            if (match(TokenType.DEBUGGER)) return debuggerStatement();
            //if (match(TokenType.CLASS)) return classDeclaration();

            return expressionStatement();
        }

        private Statement printStatement()
        {
            var expr = expression();
            consume(TokenType.SEMICOLON, "Expected ;");
            return new PrintStatement(expr);
        }

        private Statement throwStatement()
        {
            var expr = expression();
            consume(TokenType.SEMICOLON, "Expected ;");
            return new ThrowStatement(expr);
        }

        private Statement returnStatement()
        {
            if (!_statementCallStack.Contains("FUNCTION"))
            {
                throw error(previous(), "Return not in function.");
            }

            var expr = expression();
            consume(TokenType.SEMICOLON, "Expected ;");
            return new ReturnStatement(expr);
        }

        private Statement breakStatement()
        {
            if (!_statementCallStack.Contains("WHILE"))
            {
                throw error(previous(), "Break should be inside a while or for loop");
            }

            consume(TokenType.SEMICOLON, "Expected ;");
            return new BreakStatement();
        }

        private Statement continueStatement()
        {
            if (!_statementCallStack.Contains("WHILE"))
            {
                throw error(previous(), "Continue should be inside a while or for loop");
            }

            consume(TokenType.SEMICOLON, "Expected ;");
            return new ContinueStatement();
        }

        private Statement debuggerStatement()
        {
            consume(TokenType.SEMICOLON, "Expected ;");
            return new DebuggerStatement();
        }

        private BlockStatement block()
        {
            _statementCallStack.Push("BLOCK");

            IList<Statement> statements = new List<Statement>();

            while (!check(TokenType.RIGHT_BRACE) && !ReachedEnd())
            {
                statements.Add(declaration());
            }

            _ = _statementCallStack.Pop();

            consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return new BlockStatement(statements);
        }

        private Statement ifStatement()
        {
            _statementCallStack.Push("IF");

            consume(TokenType.LEFT_BRACKET, "Expected (");
            var condition = expression();
            consume(TokenType.RIGHT_BRACKET, "Expected )");
            var thenStatement = statement();

            Statement? elseStatement = null;

            if (match(TokenType.ELSE))
            {
                elseStatement = statement();
            }

            _ = _statementCallStack.Pop();

            return new IfStatement(condition, thenStatement, elseStatement);
        }

        private Statement whileStatement()
        {
            _statementCallStack.Push("WHILE");

            consume(TokenType.LEFT_BRACKET, "Expected (");
            var whileCondition = expression();
            consume(TokenType.RIGHT_BRACKET, "Expected )");
            var whileStatement = statement();

            _ = _statementCallStack.Pop();

            return new WhileStatement(whileCondition, whileStatement);
        }

        private Statement tryStatement()
        {
            _statementCallStack.Push("TRY");

            consume(TokenType.LEFT_BRACE, "Expected {");
            BlockStatement tryBody = block();
            BlockStatement? catchBody = null;
            BlockStatement? finallyBody = null;

            Token? exceptionVarName = null;

            if (match(TokenType.CATCH))
            {
                if (match(TokenType.LEFT_BRACKET))
                {
                    exceptionVarName = consume(TokenType.IDENTIFIER, "Expected a single variable name for exception variable");

                    consume(TokenType.RIGHT_BRACKET, "Expected )");
                }

                consume(TokenType.LEFT_BRACE, "Expected {");
                catchBody = block();
            }

            if (match(TokenType.FINALLY))
            {
                consume(TokenType.LEFT_BRACE, "Expected {");
                finallyBody = block();
            }

            if (catchBody == null && finallyBody == null)
            {
                consume(TokenType.CATCH, "Expected catch or finally");
            }

            _ = _statementCallStack.Pop();

            return new TryStatement(tryBody, exceptionVarName, catchBody, finallyBody);
        }

        private Statement forStatement()
        {
            _statementCallStack.Push("WHILE");

            consume(TokenType.LEFT_BRACKET, "Expected (");

            Statement? initialiser = null;

            if (match(TokenType.SEMICOLON))
            {
                initialiser = null;
            }
            else if (match(TokenType.VAR))
            {
                initialiser = varDeclaration();
            }
            else
            {
                initialiser = expressionStatement();
            }

            Expression? condition = null;

            if (!check(TokenType.SEMICOLON))
            {
                condition = expression();
            }
            else
            {
                condition = new LiteralExpression(new SmolBool(true));
            }

            consume(TokenType.SEMICOLON, "Expected ;");

            Expression? increment = null;

            if (!check(TokenType.RIGHT_BRACKET))
            {
                increment = expression();
            }

            consume(TokenType.RIGHT_BRACKET, "Expected )");

            var body = statement();

            if (increment != null)
            {
                body = new BlockStatement(new List<Statement>() {
                    body,
                    new ExpressionStatement(increment)
                });
            }

            body = new WhileStatement(condition, body);

            if (initialiser != null)
            {
                body = new BlockStatement(new List<Statement>() {
                    initialiser,
                    body
                });
            }

            _ = _statementCallStack.Pop();

            return body;
        }

        private Statement expressionStatement()
        {
            var expr = expression();

            consume(TokenType.SEMICOLON, "Expected ;");
            return new ExpressionStatement(expr);
        }

        private Expression expression()
        {
            var expr = assignment();

            if (match(TokenType.QUESTION_MARK))
            {
                var thenExpression = expression(); // This isn't right, need to work out correct order
                consume(TokenType.COLON, "Expected :");
                var elseExpression = expression();

                return new TernaryExpression(expr, thenExpression, elseExpression);
            }

            return expr;
        }

        private Expression assignment()
        {
            var expr = functionExpression();

            if (match(TokenType.EQUAL))
            {
                var equals = previous();
                var value = assignment();

                if (expr.GetType() == typeof(VariableExpression))
                {
                    var name = ((VariableExpression)expr).name;
                    return new AssignExpression(name, value);
                }
                else if (expr.GetType() == typeof(GetExpression))
                {
                    var getExpr = (GetExpression)expr;
                    return new SetExpression(getExpr.obj, getExpr.name, value);
                }
                else if (expr.GetType() == typeof(IndexerGetExpression))
                {
                    var getExpr = (IndexerGetExpression)expr;
                    return new IndexerSetExpression(getExpr.obj, getExpr.indexerExpr, value);
                }

                throw error(equals, "Invalid assignment target.");
            }

            if (match(TokenType.PLUS_EQUALS))
            {
                var equals = previous();
                var value = assignment();
                var additionExpr = new BinaryExpression(expr, new Token(TokenType.PLUS, "+=", null, 0), value);

                if (expr.GetType() == typeof(VariableExpression))
                {
                    return new AssignExpression(((VariableExpression)expr).name, additionExpr);
                }
                else if (expr.GetType() == typeof(GetExpression))
                {
                    var getExpr = (GetExpression)expr;

                    return new SetExpression(getExpr.obj, getExpr.name, additionExpr);
                }
                else if (expr.GetType() == typeof(IndexerGetExpression))
                {
                    var getExpr = (IndexerGetExpression)expr;
                    return new IndexerSetExpression(getExpr.obj, getExpr.indexerExpr, additionExpr);
                }

                throw error(equals, "Invalid assignment target.");
            }

            if (match(TokenType.MINUS_EQUALS))
            {
                var equals = previous();
                var value = assignment();
                var subtractExpr = new BinaryExpression(expr, new Token(TokenType.MINUS, "-=", null, 0), value);

                if (expr.GetType() == typeof(VariableExpression))
                {
                    return new AssignExpression(((VariableExpression)expr).name, subtractExpr);
                }
                else if (expr.GetType() == typeof(GetExpression))
                {
                    var getExpr = (GetExpression)expr;

                    return new SetExpression(getExpr.obj, getExpr.name, subtractExpr);
                }
                else if (expr.GetType() == typeof(IndexerGetExpression))
                {
                    var getExpr = (IndexerGetExpression)expr;
                    return new IndexerSetExpression(getExpr.obj, getExpr.indexerExpr, subtractExpr);
                }

                throw error(equals, "Invalid assignment target.");
            }

            if (match(TokenType.POW_EQUALS))
            {
                var equals = previous();
                var value = assignment();
                var powExpr = new BinaryExpression(expr, new Token(TokenType.POW, "*=", null, 0), value);

                if (expr.GetType() == typeof(VariableExpression))
                {
                    return new AssignExpression(((VariableExpression)expr).name, powExpr);
                }
                else if (expr.GetType() == typeof(GetExpression))
                {
                    var getExpr = (GetExpression)expr;
                    return new SetExpression(getExpr.obj, getExpr.name, powExpr);
                }
                else if (expr.GetType() == typeof(IndexerGetExpression))
                {
                    var getExpr = (IndexerGetExpression)expr;
                    return new IndexerSetExpression(getExpr.obj, getExpr.indexerExpr, powExpr);
                }

                throw error(equals, "Invalid assignment target.");
            }

            if (match(TokenType.DIVIDE_EQUALS))
            {
                var equals = previous();
                var value = assignment();
                var divExpr = new BinaryExpression(expr, new Token(TokenType.DIVIDE, "/=", null, 0), value);

                if (expr.GetType() == typeof(VariableExpression))
                {
                    return new AssignExpression(((VariableExpression)expr).name, divExpr);
                }
                else if (expr.GetType() == typeof(GetExpression))
                {
                    var getExpr = (GetExpression)expr;

                    return new SetExpression(getExpr.obj, getExpr.name, divExpr);
                }
                else if (expr.GetType() == typeof(IndexerGetExpression))
                {
                    var getExpr = (IndexerGetExpression)expr;
                    return new IndexerSetExpression(getExpr.obj, getExpr.indexerExpr, divExpr);
                }

                throw error(equals, "Invalid assignment target.");
            }

            if (match(TokenType.MULTIPLY_EQUALS))
            {
                var equals = previous();
                var value = assignment();
                var mulExpr = new BinaryExpression(expr, new Token(TokenType.MULTIPLY, "*=", null, 0), value);

                if (expr.GetType() == typeof(VariableExpression))
                {
                    return new AssignExpression(((VariableExpression)expr).name, mulExpr);
                }
                else if (expr.GetType() == typeof(GetExpression))
                {
                    var getExpr = (GetExpression)expr;

                    return new SetExpression(getExpr.obj, getExpr.name, mulExpr);
                }
                else if (expr.GetType() == typeof(IndexerGetExpression))
                {
                    var getExpr = (IndexerGetExpression)expr;
                    return new IndexerSetExpression(getExpr.obj, getExpr.indexerExpr, mulExpr);
                }

                throw error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expression functionExpression()
        {
            if (match(TokenType.FUNC))
            {
                _statementCallStack.Push("FUNCTION");

                var functionParams = new List<Token>();

                consume(TokenType.LEFT_BRACKET, "Expected (");

                if (!check(TokenType.RIGHT_BRACKET))
                {
                    do
                    {
                        if (functionParams.Count() >= 127)
                        {
                            error(peek(), "Can't define a function with more than 127 parameters.");
                        }

                        functionParams.Add(consume(TokenType.IDENTIFIER, "Expected parameter name"));
                    } while (match(TokenType.COMMA));
                }

                consume(TokenType.RIGHT_BRACKET, "Expected )");
                consume(TokenType.LEFT_BRACE, "Expected {");

                var functionBody = block();

                _ = _statementCallStack.Pop();

                return new FunctionExpression(functionParams, functionBody);
            }

            return logicalOr();
        }

        private Expression logicalOr()
        {
            var expr = logicalAnd();

            while (match(TokenType.LOGICAL_OR))
            {
                var op = previous();
                var right = logicalAnd();
                expr = new LogicalExpression(expr, op, right);
            }

            return expr;
        }

        private Expression logicalAnd()
        {
            var expr = equality();

            while (match(TokenType.LOGICAL_AND))
            {
                var op = previous();
                var right = equality();
                expr = new LogicalExpression(expr, op, right);
            }

            return expr;
        }

        private Expression equality()
        {
            var expr = comparison();

            while (match(TokenType.NOT_EQUAL, TokenType.EQUAL_EQUAL))
            {
                var op = previous();
                var right = comparison();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private Expression comparison()
        {
            var expr = bitwise_op(); // Was term

            while (match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                var op = previous();
                var right = bitwise_op(); // Was term
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private Expression bitwise_op()
        {
            var expr = term();

            while (match(TokenType.BITWISE_AND, TokenType.BITWISE_OR, TokenType.REMAINDER))
            {
                var op = previous();
                var right = term();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private Expression term()
        {
            var expr = factor();

            while (match(TokenType.MINUS, TokenType.PLUS))
            {
                var op = previous();
                var right = factor();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private Expression factor()
        {
            var expr = pow();

            while (match(TokenType.MULTIPLY, TokenType.DIVIDE))
            {
                var op = previous();
                var right = pow();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private Expression pow()
        {
            var expr = unary();

            while (match(TokenType.POW))
            {
                var op = previous();
                var right = unary();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private Expression unary()
        {
            if (match(TokenType.NOT, TokenType.MINUS))
            {
                var op = previous();
                var right = unary();
                return new UnaryExpression(op, right);
            }

            return call();
        }

        private Expression call()
        {
            var expr = primary();

            while (true)
            {
                if (match(TokenType.LEFT_BRACKET))
                {
                    expr = finishCall(expr, expr.GetType() == typeof(GetExpression));
                }
                else if (match(TokenType.LEFT_SQUARE_BRACKET))
                {
                    var indexerExpression = expression();

                    var closingParen = consume(TokenType.RIGHT_SQUARE_BRACKET, "Expected ]");

                    expr = new IndexerGetExpression(expr, indexerExpression);
                }
                else if (match(TokenType.DOT))
                {
                    Token name = consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                    expr = new GetExpression(expr, name);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expression finishCall(Expression callee, bool isFollowingGetter = false)
        {
            var args = new List<Expression>();

            if (!check(TokenType.RIGHT_BRACKET))
            {
                do { args.Add(expression()); } while (match(TokenType.COMMA));
            }

            var closingParen = consume(TokenType.RIGHT_BRACKET, "Expected )");

            return new CallExpression(callee, args, isFollowingGetter);
        }

        private Expression primary()
        {
            if (match(TokenType.FALSE)) return new LiteralExpression(new SmolBool(false));
            if (match(TokenType.TRUE)) return new LiteralExpression(new SmolBool(true));
            if (match(TokenType.NULL)) return new LiteralExpression(new SmolNull());
            if (match(TokenType.UNDEFINED)) return new LiteralExpression(new SmolUndefined());

            if (match(TokenType.NUMBER))
            {
                return new LiteralExpression(new SmolNumber((double)previous().literal!));
            }

            if (match(TokenType.STRING))
            {
                return new LiteralExpression(new SmolString((string)previous().literal!));
            }

            if (match(TokenType.PREFIX_INCREMENT))
            {
                if (match(TokenType.IDENTIFIER))
                {
                    return new VariableExpression(previous(), TokenType.PREFIX_INCREMENT);
                }
            }

            if (match(TokenType.PREFIX_DECREMENT))
            {
                if (match(TokenType.IDENTIFIER))
                {
                    return new VariableExpression(previous(), TokenType.PREFIX_DECREMENT);
                }
            }

            if (match(TokenType.IDENTIFIER))
            {
                if (match(TokenType.POSTFIX_INCREMENT))
                {
                    return new VariableExpression(previous(1), TokenType.POSTFIX_INCREMENT);
                }
                else if (match(TokenType.POSTFIX_DECREMENT))
                {
                    return new VariableExpression(previous(1), TokenType.POSTFIX_DECREMENT);
                }
                else
                {
                    return new VariableExpression(previous());
                }
            }

            if (match(TokenType.NEW))
            {
                var className = consume(TokenType.IDENTIFIER, "Expected identifier after new");

                consume(TokenType.LEFT_BRACKET, "Expect ')' after expression.");

                var args = new List<Expression>();

                if (!check(TokenType.RIGHT_BRACKET))
                {
                    do { args.Add(expression()); } while (match(TokenType.COMMA));
                }

                var closingParen = consume(TokenType.RIGHT_BRACKET, "Expected )");

                return new NewInstanceExpression(className, args);
            }

            if (match(TokenType.LEFT_SQUARE_BRACKET))
            {
                var className = new Token(TokenType.IDENTIFIER, "Array", null, peek().line);

                var args = new List<Expression>();

                if (!check(TokenType.RIGHT_SQUARE_BRACKET))
                {
                    do { args.Add(expression()); } while (match(TokenType.COMMA));
                }

                var closingParen = consume(TokenType.RIGHT_SQUARE_BRACKET, "Expected ]");

                return new NewInstanceExpression(className, args);
            }

            if (match(TokenType.LEFT_BRACE))
            {
                var className = new Token(TokenType.IDENTIFIER, "Object", null, peek().line);

                var args = new List<Expression>();

                if (!check(TokenType.RIGHT_BRACE))
                {
                    do
                    {

                        var name = consume(TokenType.IDENTIFIER, "Expected idetifier");
                        _ = consume(TokenType.COLON, "Exepcted :");
                        var value = expression();

                        args.Add(new ObjectInitializerExpression(name, value));

                    } while (match(TokenType.COMMA));
                }

                var closingParen = consume(TokenType.RIGHT_BRACE, "Expected }");

                return new NewInstanceExpression(className, args);
            }

            if (match(TokenType.LEFT_BRACKET))
            {
                Expression expr = expression();
                consume(TokenType.RIGHT_BRACKET, "Expect ')' after expression.");
                return new GroupingExpression(expr);
            }

            throw error(peek(), $"Parser did not expect to see '{peek().lexeme}' on line {peek().line}, sorry :(");
        }
    }
}