using SmolScript.Internals.Ast.Expressions;
using SmolScript.Internals.Ast.Statements;

namespace SmolScript.Internals
{
    public class ParseError : Exception
    {
        public int LineNumber { get; set; }
        public IList<ParseError>? Errors = null;

        public ParseError(Token token, string message) :
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
                   | logical_or ;

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

    public class Parser
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
                    while (peek().type == TokenType.SEMICOLON) consume(TokenType.SEMICOLON, "");

                    statements.Add(declaration());
                }
                catch (ParseError e)
                {
                    errors.Add(e);
                }
            }

            if (errors.Any())
            {
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

        private bool check(TokenType tokenType)
        {
            if (ReachedEnd()) return false;
            return peek().type == tokenType;
        }

        private Token peek()
        {
            return _tokens[_current];
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

            //Console.WriteLine(peek().type);

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

            Token? functionName = null;

            if (!check(TokenType.LEFT_BRACKET))
            {
                functionName = consume(TokenType.IDENTIFIER, "Expected function name");
            }

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

        private Statement statement()
        {
            if (match(TokenType.IF)) return ifStatement();
            if (match(TokenType.WHILE)) return whileStatement();
            if (match(TokenType.FOR)) return forStatement();
            if (match(TokenType.PRINT)) return printStatement();
            if (match(TokenType.RETURN)) return returnStatement();
            if (match(TokenType.BREAK)) return breakStatement();
            if (match(TokenType.LEFT_BRACE)) return block();

            return expressionStatement();
        }

        private Statement printStatement()
        {
            var expr = expression();
            consume(TokenType.SEMICOLON, "Expected ;");
            return new PrintStatement(expr);
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
            //Console.WriteLine("BREAK");
            //Console.WriteLine(String.Join('|', _statementCallStack.ToArray()));

            if (!_statementCallStack.Contains("WHILE"))
            {
                throw error(previous(), "Break should be inside a while or for loop");
            }

            consume(TokenType.SEMICOLON, "Expected ;");
            return new BreakStatement();
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
                condition = new LiteralExpression(true);
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

            if (match(TokenType.QUESTION_MARK))
            {
                var thenExpression = expression(); ;
                consume(TokenType.COLON, "Expected :");
                var elseExpression = expression(); ;
                consume(TokenType.SEMICOLON, "Expected ;");

                return new TernaryStatement(expr, thenExpression, elseExpression);
            }

            consume(TokenType.SEMICOLON, "Expected ;");
            return new ExpressionStatement(expr);
        }

        private Expression expression()
        {
            return assignment();
        }

        private Expression assignment()
        {
            var expr = logicalOr();

            if (match(TokenType.EQUAL))
            {
                var equals = previous();
                var value = assignment();

                if (expr.GetType() == typeof(VariableExpression))
                {
                    var name = ((VariableExpression)expr).name;
                    return new AssignExpression(name, value);
                }

                throw error(equals, "Invalid assignment target.");
            }

            if (match(TokenType.PLUS_EQUALS))
            {
                var equals = previous();
                var value = assignment();

                if (expr.GetType() == typeof(VariableExpression))
                {
                    var name = ((VariableExpression)expr).name;

                    // DESUGAR!!!
                    var x = new BinaryExpression(((VariableExpression)expr), new Token(TokenType.PLUS, "+=", null, 0), value);

                    return new AssignExpression(name, x);
                }

                throw error(equals, "Invalid assignment target.");
            }

            if (match(TokenType.MINUS_EQUALS))
            {
                var equals = previous();
                var value = assignment();

                if (expr.GetType() == typeof(VariableExpression))
                {
                    var name = ((VariableExpression)expr).name;

                    var x = new BinaryExpression(((VariableExpression)expr), new Token(TokenType.MINUS, "-=", null, 0), value);

                    return new AssignExpression(name, x);
                }

                throw error(equals, "Invalid assignment target.");
            }

            if (match(TokenType.POW_EQUALS))
            {
                var equals = previous();
                var value = assignment();

                if (expr.GetType() == typeof(VariableExpression))
                {
                    var name = ((VariableExpression)expr).name;

                    var x = new BinaryExpression(((VariableExpression)expr), new Token(TokenType.POW, "*=", null, 0), value);

                    return new AssignExpression(name, x);
                }

                throw error(equals, "Invalid assignment target.");
            }

            return expr;
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
                    expr = finishCall(expr);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expression finishCall(Expression callee)
        {
            var args = new List<object?>();

            if (!check(TokenType.RIGHT_BRACKET))
            {
                do
                {
                    if (match(TokenType.FUNC))
                    {
                        // Anonymous function -- I think this needs to be functionExpression?
                        args.Add(functionDeclaration());
                    }
                    else
                    {
                        args.Add(expression());
                    }
                } while (match(TokenType.COMMA));
            }

            var closingParen = consume(TokenType.RIGHT_BRACKET, "Expected )");

            return new CallExpression(callee, closingParen, args);
        }

        private Expression primary()
        {
            if (match(TokenType.FALSE)) return new LiteralExpression(false);
            if (match(TokenType.TRUE)) return new LiteralExpression(true);
            if (match(TokenType.NULL)) return new LiteralExpression(null);

            if (match(TokenType.NUMBER, TokenType.STRING))
            {
                return new LiteralExpression(previous().literal!);
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

            if (match(TokenType.LEFT_BRACKET))
            {
                Expression expr = expression();
                consume(TokenType.RIGHT_BRACKET, "Expect ')' after expression.");
                return new GroupingExpression(expr);
            }

            //while(match(TokenType.SEMICOLON));

            throw error(peek(), $"Parser did not expect to see '{peek().lexeme}' on line {peek().line}, sorry :(");
        }
    }
}