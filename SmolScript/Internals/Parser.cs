using SmolScript.Internals.Ast.Expressions;
using SmolScript.Internals.Ast.Statements;
using SmolScript.Internals.SmolVariableTypes;

namespace SmolScript.Internals
{
    /*
    program        → declaration* EOF ;

    declaration    → functionDecl
                   | varDecl
                   | classDecl
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
                    if (Peek().Type == TokenType.SEMICOLON)
                    {
                        Consume(TokenType.SEMICOLON, "");
                    }
                    else
                    {
                        statements.Add(Declaration());
                    }
                }
                catch (ParseError e)
                {
                    errors.Add(e);
                }
            }

            if (errors.Any())
            {
                throw new SmolCompilerError(errors, $"Encounted one or more errors while parsing (first error: {errors.First().Message})");
            }

            return statements;
        }

        private bool Match(params TokenType[] tokenTypes)
        {
            foreach (var tokenType in tokenTypes)
            {
                if (Check(tokenType))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType tokenType, int skip = 0)
        {
            if (ReachedEnd()) return false;
            return Peek(skip).Type == tokenType;
        }

        private Token Peek(int skip = 0)
        {
            return _tokens[_current + skip];
        }

        private Token Advance()
        {
            if (!ReachedEnd()) _current++;

            return Previous();
        }

        public Token Previous(int skip = 0)
        {
            return _tokens[_current - 1 - (skip * 1)];
        }

        public Token Consume(TokenType tokenType, string errorIfNotFound)
        {
            if (Check(tokenType)) return Advance();

            // If we expected a ; but got a newline, we just wave it through
            if (tokenType == TokenType.SEMICOLON && _tokens[_current - 1].IsFollowedByLineBreak)
            {
                // We need to return a token, so we'll make a fake semicolon
                return new Token(TokenType.SEMICOLON, "", "", -1, -1, -1, -1);
            }

            // If we expected a ; but got a }, we also wave that through
            if (tokenType == TokenType.SEMICOLON && (this.Check(TokenType.RIGHT_BRACE) || this.Peek().Type == TokenType.EOF))
            {
                return new Token(TokenType.SEMICOLON, "", "", -1, -1, -1, -1);
            }

            throw Error(Peek(), errorIfNotFound);
        }

        private ParseError Error(Token token, string errorMessage)
        {
            return new ParseError(token, $"{errorMessage} (Line {token.Line}, Col {token.Col})");
        }

        private void Synchronize()
        {
            Advance();

            while (!ReachedEnd())
            {
                if (Previous().Type == TokenType.SEMICOLON) return;

                switch (Peek().Type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUNC:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.RETURN:
                    case TokenType.TRY:
                        return;
                }

                Advance();
            }

            _statementCallStack.Clear();
        }

        private bool ReachedEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        private Statement Declaration()
        {
            try
            {
                if (Match(TokenType.FUNC)) return FunctionDeclaration();
                if (Match(TokenType.VAR)) return VarDeclaration();
                if (Match(TokenType.CLASS)) return ClassDeclaration();

                return Statement();
            }
            catch (ParseError)
            {
                Synchronize();
                throw;
            }

        }

        private Statement FunctionDeclaration()
        {
            // If this function is nested we're going to turn it into a var fnName = fnExpression() so that
            // the function variable becomes a regular variable in the enclosing functions environment.
            var firstTokenIndex = _current - 1;
            var isNestedFunction = _statementCallStack.Contains("FUNCTION");
            
            // Regular function statement code...
            
            _statementCallStack.Push("FUNCTION");

            var functionName = Consume(TokenType.IDENTIFIER, "Expected function name");

            var functionParams = new List<Token>();

            Consume(TokenType.LEFT_BRACKET, "Expected (");

            if (!Check(TokenType.RIGHT_BRACKET))
            {
                do
                {
                    if (functionParams.Count() >= 32_766)
                    {
                        Error(Peek(), "Can't define a function with more than 32,766 parameters.");
                    }

                    functionParams.Add(Consume(TokenType.IDENTIFIER, "Expected parameter name"));
                } while (Match(TokenType.COMMA));
            }

            Consume(TokenType.RIGHT_BRACKET, "Expected )");
            Consume(TokenType.LEFT_BRACE, "Expected {");

            var functionBody = Block();

            _ = _statementCallStack.Pop();

            if (isNestedFunction) // Switch out the function statement for a var declaration if it's nested
            {
                var skip = Consume(TokenType.SEMICOLON, "Expected either a value to be assigned or the end of the statement").StartPosition == -1 ? 1 : 2;
                var lastTokenIndex = _current - skip;
                
                return new VarStatement(functionName, new FunctionExpression(functionParams, functionBody), firstTokenIndex, lastTokenIndex);
            }
            else
            {
                return new FunctionStatement(functionName, functionParams, functionBody);
            }
        }

        private Statement VarDeclaration()
        {
            var firstTokenIndex = _current - 1;

            var name = Consume(TokenType.IDENTIFIER, "Expected variable name");

            Expression? initializer = null;

            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }
          
            var skip = Consume(TokenType.SEMICOLON, "Expected either a value to be assigned or the end of the statement").StartPosition == -1 ? 1 : 2;
            var lastTokenIndex = _current - skip;

            return new VarStatement(name, initializer, firstTokenIndex, lastTokenIndex);
        }

        private Statement ClassDeclaration()
        {
            var className = Consume(TokenType.IDENTIFIER, "Expected class name");
            Token? superclassName = null;
            var functions = new List<FunctionStatement>();

            if (Match(TokenType.COLON))
            {
                superclassName = Consume(TokenType.IDENTIFIER, "Expected superclass name");
            }

            Consume(TokenType.LEFT_BRACE, "Expected {");

            while (!Check(TokenType.RIGHT_BRACE) && !ReachedEnd())
            {
                if (Check(TokenType.IDENTIFIER) && Check(TokenType.LEFT_BRACKET, 1))
                {
                    functions.Add((FunctionStatement)FunctionDeclaration());
                }
                else
                {
                    throw new Exception($"Didn't expect to find {Peek()} in the class body");
                }
            }

            Consume(TokenType.RIGHT_BRACE, "Expected }");

            return new ClassStatement(className, superclassName, functions);
        }


        private Statement Statement()
        {
            if (Match(TokenType.IF)) return IfStatement();
            if (Match(TokenType.WHILE)) return WhileStatement();
            if (Match(TokenType.TRY)) return TryStatement();
            if (Match(TokenType.THROW)) return ThrowStatement();
            if (Match(TokenType.FOR)) return ForStatement();
            if (Match(TokenType.RETURN)) return ReturnStatement();
            if (Match(TokenType.BREAK)) return BreakStatement();
            if (Match(TokenType.CONTINUE)) return ContinueStatement();
            if (Match(TokenType.LEFT_BRACE)) return Block();
            if (Match(TokenType.DEBUGGER)) return DebuggerStatement();

            return ExpressionStatement();
        }

        private Statement ThrowStatement()
        {
            var expr = Expression();
            Consume(TokenType.SEMICOLON, "Expected ;");
            return new ThrowStatement(expr);
        }

        private Statement ReturnStatement()
        {
            if (!_statementCallStack.Contains("FUNCTION"))
            {
                throw Error(Previous(), "Return not in function.");
            }

            var expr = Expression();
            Consume(TokenType.SEMICOLON, "Expected ;");
            return new ReturnStatement(expr);
        }

        private Statement BreakStatement()
        {
            if (!_statementCallStack.Contains("WHILE"))
            {
                throw Error(Previous(), "Break should be inside a while or for loop");
            }

            Consume(TokenType.SEMICOLON, "Expected ;");
            return new BreakStatement();
        }

        private Statement ContinueStatement()
        {
            if (!_statementCallStack.Contains("WHILE"))
            {
                throw Error(Previous(), "Continue should be inside a while or for loop");
            }

            Consume(TokenType.SEMICOLON, "Expected ;");
            return new ContinueStatement();
        }

        private Statement DebuggerStatement()
        {
            Consume(TokenType.SEMICOLON, "Expected ;");
            return new DebuggerStatement();
        }

        private BlockStatement Block()
        {
            _statementCallStack.Push("BLOCK");

            IList<Statement> statements = new List<Statement>();

            while (!Check(TokenType.RIGHT_BRACE) && !ReachedEnd())
            {
                if (Peek().Type == TokenType.SEMICOLON)
                {
                    Consume(TokenType.SEMICOLON, "");
                }
                else
                {
                    statements.Add(Declaration());
                }
            }

            _ = _statementCallStack.Pop();

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return new BlockStatement(statements);
        }

        private Statement IfStatement()
        {
            _statementCallStack.Push("IF");

            Consume(TokenType.LEFT_BRACKET, "Expected (");
            var condition = Expression();
            Consume(TokenType.RIGHT_BRACKET, "Expected )");
            var thenStatement = Statement();

            Statement? elseStatement = null;

            if (Match(TokenType.ELSE))
            {
                elseStatement = Statement();
            }

            _ = _statementCallStack.Pop();

            return new IfStatement(condition, thenStatement, elseStatement);
        }

        private Statement WhileStatement()
        {
            _statementCallStack.Push("WHILE");

            Consume(TokenType.LEFT_BRACKET, "Expected (");
            var whileCondition = Expression();
            Consume(TokenType.RIGHT_BRACKET, "Expected )");
            var whileStatement = Statement();

            _ = _statementCallStack.Pop();

            return new WhileStatement(whileCondition, whileStatement);
        }

        private Statement TryStatement()
        {
            _statementCallStack.Push("TRY");

            Consume(TokenType.LEFT_BRACE, "Expected {");
            BlockStatement tryBody = Block();
            BlockStatement? catchBody = null;
            BlockStatement? finallyBody = null;

            Token? exceptionVarName = null;

            if (Match(TokenType.CATCH))
            {
                if (Match(TokenType.LEFT_BRACKET))
                {
                    exceptionVarName = Consume(TokenType.IDENTIFIER, "Expected a single variable name for exception variable");

                    Consume(TokenType.RIGHT_BRACKET, "Expected )");
                }

                Consume(TokenType.LEFT_BRACE, "Expected {");
                catchBody = Block();
            }

            if (Match(TokenType.FINALLY))
            {
                Consume(TokenType.LEFT_BRACE, "Expected {");
                finallyBody = Block();
            }

            if (catchBody == null && finallyBody == null)
            {
                Consume(TokenType.CATCH, "Expected catch or finally");
            }

            _ = _statementCallStack.Pop();

            return new TryStatement(tryBody, exceptionVarName, catchBody, finallyBody);
        }

        private Statement ForStatement()
        {
            _statementCallStack.Push("WHILE");

            Consume(TokenType.LEFT_BRACKET, "Expected (");

            Statement? initialiser = null;

            if (Match(TokenType.SEMICOLON))
            {
                initialiser = null;
            }
            else if (Match(TokenType.VAR))
            {
                initialiser = VarDeclaration();
            }
            else
            {
                initialiser = ExpressionStatement();
            }

            Expression? condition = null;

            if (!Check(TokenType.SEMICOLON))
            {
                condition = Expression();
            }
            else
            {
                condition = new LiteralExpression(new SmolBool(true));
            }

            Consume(TokenType.SEMICOLON, "Expected ;");

            Expression? increment = null;

            if (!Check(TokenType.RIGHT_BRACKET))
            {
                increment = Expression();
            }

            Consume(TokenType.RIGHT_BRACKET, "Expected )");

            var body = Statement();

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

        private Statement ExpressionStatement()
        {
            var expr = Expression();

            Consume(TokenType.SEMICOLON, "Expected ;");
            return new ExpressionStatement(expr);
        }

        private Expression Expression()
        {
            var expr = Assignment();

            if (Match(TokenType.QUESTION_MARK))
            {
                var thenExpression = Expression(); // This isn't right, need to work out correct order
                Consume(TokenType.COLON, "Expected :");
                var elseExpression = Expression();

                return new TernaryExpression(expr, thenExpression, elseExpression);
            }

            return expr;
        }

        private Expression Assignment()
        {
            var expr = FunctionExpression();

            if (Match(TokenType.EQUAL))
            {
                var equals = Previous();
                var value = Assignment();

                if (expr.GetType() == typeof(VariableExpression))
                {
                    var name = ((VariableExpression)expr).VariableName;
                    return new AssignExpression(name, value);
                }
                else if (expr.GetType() == typeof(GetExpression))
                {
                    var getExpr = (GetExpression)expr;
                    return new SetExpression(getExpr.TargetObject, getExpr.AttributeName, value);
                }
                else if (expr.GetType() == typeof(IndexerGetExpression))
                {
                    var getExpr = (IndexerGetExpression)expr;
                    return new IndexerSetExpression(getExpr.TargetObject, getExpr.IndexerExpression, value);
                }

                throw Error(equals, "Invalid assignment target.");
            }

            if (Match(TokenType.PLUS_EQUALS))
            {
                var originalToken = Previous();
                var value = Assignment();
                var additionExpr = new BinaryExpression(expr, new Token(TokenType.PLUS, "+=", null, originalToken.Line, originalToken.Col, originalToken.StartPosition, originalToken.EndPosition), value);

                if (expr.GetType() == typeof(VariableExpression))
                {
                    return new AssignExpression(((VariableExpression)expr).VariableName, additionExpr);
                }
                else if (expr.GetType() == typeof(GetExpression))
                {
                    var getExpr = (GetExpression)expr;

                    return new SetExpression(getExpr.TargetObject, getExpr.AttributeName, additionExpr);
                }
                else if (expr.GetType() == typeof(IndexerGetExpression))
                {
                    var getExpr = (IndexerGetExpression)expr;
                    return new IndexerSetExpression(getExpr.TargetObject, getExpr.IndexerExpression, additionExpr);
                }

                throw Error(originalToken, "Invalid assignment target.");
            }

            if (Match(TokenType.MINUS_EQUALS))
            {
                var originalToken = Previous();
                var value = Assignment();
                var subtractExpr = new BinaryExpression(expr, new Token(TokenType.MINUS, "-=", null, originalToken.Line, originalToken.Col, originalToken.StartPosition, originalToken.EndPosition), value);

                if (expr.GetType() == typeof(VariableExpression))
                {
                    return new AssignExpression(((VariableExpression)expr).VariableName, subtractExpr);
                }
                else if (expr.GetType() == typeof(GetExpression))
                {
                    var getExpr = (GetExpression)expr;

                    return new SetExpression(getExpr.TargetObject, getExpr.AttributeName, subtractExpr);
                }
                else if (expr.GetType() == typeof(IndexerGetExpression))
                {
                    var getExpr = (IndexerGetExpression)expr;
                    return new IndexerSetExpression(getExpr.TargetObject, getExpr.IndexerExpression, subtractExpr);
                }

                throw Error(originalToken, "Invalid assignment target.");
            }

            if (Match(TokenType.POW_EQUALS))
            {
                var originalToken = Previous();
                var value = Assignment();
                var powExpr = new BinaryExpression(expr, new Token(TokenType.POW, "*=", null, originalToken.Line, originalToken.Col, originalToken.StartPosition, originalToken.EndPosition), value);

                if (expr.GetType() == typeof(VariableExpression))
                {
                    return new AssignExpression(((VariableExpression)expr).VariableName, powExpr);
                }
                else if (expr.GetType() == typeof(GetExpression))
                {
                    var getExpr = (GetExpression)expr;
                    return new SetExpression(getExpr.TargetObject, getExpr.AttributeName, powExpr);
                }
                else if (expr.GetType() == typeof(IndexerGetExpression))
                {
                    var getExpr = (IndexerGetExpression)expr;
                    return new IndexerSetExpression(getExpr.TargetObject, getExpr.IndexerExpression, powExpr);
                }

                throw Error(originalToken, "Invalid assignment target.");
            }

            if (Match(TokenType.DIVIDE_EQUALS))
            {
                var originalToken = Previous();
                var value = Assignment();
                var divExpr = new BinaryExpression(expr, new Token(TokenType.DIVIDE, "/=", null, originalToken.Line, originalToken.Col, originalToken.StartPosition, originalToken.EndPosition), value);

                if (expr.GetType() == typeof(VariableExpression))
                {
                    return new AssignExpression(((VariableExpression)expr).VariableName, divExpr);
                }
                else if (expr.GetType() == typeof(GetExpression))
                {
                    var getExpr = (GetExpression)expr;

                    return new SetExpression(getExpr.TargetObject, getExpr.AttributeName, divExpr);
                }
                else if (expr.GetType() == typeof(IndexerGetExpression))
                {
                    var getExpr = (IndexerGetExpression)expr;
                    return new IndexerSetExpression(getExpr.TargetObject, getExpr.IndexerExpression, divExpr);
                }

                throw Error(originalToken, "Invalid assignment target.");
            }

            if (Match(TokenType.MULTIPLY_EQUALS))
            {
                var originalToken = Previous();
                var value = Assignment();
                var mulExpr = new BinaryExpression(expr, new Token(TokenType.MULTIPLY, "*=", null, originalToken.Line, originalToken.Col, originalToken.StartPosition, originalToken.EndPosition), value);

                if (expr.GetType() == typeof(VariableExpression))
                {
                    return new AssignExpression(((VariableExpression)expr).VariableName, mulExpr);
                }
                else if (expr.GetType() == typeof(GetExpression))
                {
                    var getExpr = (GetExpression)expr;

                    return new SetExpression(getExpr.TargetObject, getExpr.AttributeName, mulExpr);
                }
                else if (expr.GetType() == typeof(IndexerGetExpression))
                {
                    var getExpr = (IndexerGetExpression)expr;
                    return new IndexerSetExpression(getExpr.TargetObject, getExpr.IndexerExpression, mulExpr);
                }

                throw Error(originalToken, "Invalid assignment target.");
            }

            if (Match(TokenType.REMAINDER_EQUALS))
            {
                var originalToken = Previous();
                var value = Assignment();
                var remainderExpr = new BinaryExpression(expr, new Token(TokenType.REMAINDER, "/=", null, originalToken.Line, originalToken.Col, originalToken.StartPosition, originalToken.EndPosition), value);

                if (expr.GetType() == typeof(VariableExpression))
                {
                    return new AssignExpression(((VariableExpression)expr).VariableName, remainderExpr);
                }
                else if (expr.GetType() == typeof(GetExpression))
                {
                    var getExpr = (GetExpression)expr;

                    return new SetExpression(getExpr.TargetObject, getExpr.AttributeName, remainderExpr);
                }
                else if (expr.GetType() == typeof(IndexerGetExpression))
                {
                    var getExpr = (IndexerGetExpression)expr;
                    return new IndexerSetExpression(getExpr.TargetObject, getExpr.IndexerExpression, remainderExpr);
                }

                throw Error(originalToken, "Invalid assignment target.");
            }

            return expr;
        }

        private Expression FunctionExpression()
        {
            if ((Peek().Type == TokenType.LEFT_BRACKET || Peek().Type == TokenType.IDENTIFIER) && IsInFatArrow(false))
            {
                return FatArrowFunctionExpression(false);
            }
            else if (Match(TokenType.FUNC))
            {
                _statementCallStack.Push("FUNCTION");

                var functionParams = new List<Token>();

                Consume(TokenType.LEFT_BRACKET, "Expected (");

                if (!Check(TokenType.RIGHT_BRACKET))
                {
                    do
                    {
                        if (functionParams.Count() >= 127)
                        {
                            Error(Peek(), "Can't define a function with more than 127 parameters.");
                        }

                        functionParams.Add(Consume(TokenType.IDENTIFIER, "Expected parameter name"));
                    } while (Match(TokenType.COMMA));
                }

                Consume(TokenType.RIGHT_BRACKET, "Expected )");
                Consume(TokenType.LEFT_BRACE, "Expected {");

                var functionBody = Block();

                _ = _statementCallStack.Pop();

                return new FunctionExpression(functionParams, functionBody);
            }

            return LogicalOr();
        }

        private Expression LogicalOr()
        {
            var expr = LogicalAnd();

            while (Match(TokenType.LOGICAL_OR))
            {
                var op = Previous();
                var right = LogicalAnd();
                expr = new LogicalExpression(expr, op, right);
            }

            return expr;
        }

        private Expression LogicalAnd()
        {
            var expr = Equality();

            while (Match(TokenType.LOGICAL_AND))
            {
                var op = Previous();
                var right = Equality();
                expr = new LogicalExpression(expr, op, right);
            }

            return expr;
        }

        private Expression Equality()
        {
            var expr = Comparison();

            while (Match(TokenType.NOT_EQUAL, TokenType.EQUAL_EQUAL))
            {
                var op = Previous();
                var right = Comparison();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private Expression Comparison()
        {
            var expr = BitwiseOperation(); // Was term

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                var op = Previous();
                var right = BitwiseOperation(); // Was term
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private Expression BitwiseOperation()
        {
            var expr = Term();

            while (Match(TokenType.BITWISE_AND, TokenType.BITWISE_OR, TokenType.REMAINDER))
            {
                var op = Previous();
                var right = Term();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private Expression Term()
        {
            var expr = Factor();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                var op = Previous();
                var right = Factor();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private Expression Factor()
        {
            var expr = Pow();

            while (Match(TokenType.MULTIPLY, TokenType.DIVIDE))
            {
                var op = Previous();
                var right = Pow();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private Expression Pow()
        {
            var expr = Unary();

            while (Match(TokenType.POW))
            {
                var op = Previous();
                var right = Unary();
                expr = new BinaryExpression(expr, op, right);
            }

            return expr;
        }

        private Expression Unary()
        {
            if (Match(TokenType.NOT, TokenType.MINUS))
            {
                var op = Previous();
                var right = Unary();
                return new UnaryExpression(op, right);
            }
            
            return Call();
        }

        private Expression Call()
        {
            var expr = Primary();

            while (true)
            {
                if (Match(TokenType.LEFT_BRACKET))
                {
                    expr = FinishCall(expr, expr.GetType() == typeof(GetExpression));
                }
                else if (Match(TokenType.LEFT_SQUARE_BRACKET))
                {
                    var indexerExpression = Expression();

                    var closingParen = Consume(TokenType.RIGHT_SQUARE_BRACKET, "Expected ]");

                    expr = new IndexerGetExpression(expr, indexerExpression);
                }
                else if (Match(TokenType.DOT))
                {
                    Token name = Consume(TokenType.IDENTIFIER, "Expect property name after '.'.");
                    expr = new GetExpression(expr, name);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expression FinishCall(Expression callee, bool isFollowingGetter = false)
        {
            var args = new List<Expression>();

            if (!Check(TokenType.RIGHT_BRACKET))
            {
                do { args.Add(Expression()); } while (Match(TokenType.COMMA));
            }

            var closingParen = Consume(TokenType.RIGHT_BRACKET, "Expected )");

            return new CallExpression(callee, args, isFollowingGetter);
        }

        private Expression Primary()
        {
            if (Match(TokenType.FALSE)) return new LiteralExpression(new SmolBool(false));
            if (Match(TokenType.TRUE)) return new LiteralExpression(new SmolBool(true));
            if (Match(TokenType.NULL)) return new LiteralExpression(new SmolNull());
            if (Match(TokenType.UNDEFINED)) return new LiteralExpression(new SmolUndefined());

            if (Match(TokenType.NUMBER))
            {
                return new LiteralExpression(new SmolNumber((double)Previous().Literal!));
            }

            if (Match(TokenType.STRING))
            {
                return new LiteralExpression(new SmolString((string)Previous().Literal!));
            }

            if (Match(TokenType.PREFIX_INCREMENT))
            {
                if (Match(TokenType.IDENTIFIER))
                {
                    return new VariableExpression(Previous(), TokenType.PREFIX_INCREMENT);
                }
            }

            if (Match(TokenType.PREFIX_DECREMENT))
            {
                if (Match(TokenType.IDENTIFIER))
                {
                    return new VariableExpression(Previous(), TokenType.PREFIX_DECREMENT);
                }
            }

            if (Match(TokenType.IDENTIFIER))
            {
                if (Match(TokenType.POSTFIX_INCREMENT))
                {
                    return new VariableExpression(Previous(1), TokenType.POSTFIX_INCREMENT);
                }
                else if (Match(TokenType.POSTFIX_DECREMENT))
                {
                    return new VariableExpression(Previous(1), TokenType.POSTFIX_DECREMENT);
                }
                else
                {
                    return new VariableExpression(Previous());
                }
            }

            if (Match(TokenType.NEW))
            {
                var className = Consume(TokenType.IDENTIFIER, "Expected identifier after new");

                Consume(TokenType.LEFT_BRACKET, "Expect ')' after expression.");

                var args = new List<Expression>();

                if (!Check(TokenType.RIGHT_BRACKET))
                {
                    do { args.Add(Expression()); } while (Match(TokenType.COMMA));
                }

                var closingParen = Consume(TokenType.RIGHT_BRACKET, "Expected )");

                return new NewInstanceExpression(className, args);
            }

            if (Match(TokenType.LEFT_SQUARE_BRACKET))
            {
                var originalToken = Previous();
                var className = new Token(TokenType.IDENTIFIER, "Array", null, originalToken.Line, originalToken.Col, originalToken.StartPosition, originalToken.EndPosition);
                var args = new List<Expression>();

                if (!Check(TokenType.RIGHT_SQUARE_BRACKET))
                {
                    do { args.Add(Expression()); } while (Match(TokenType.COMMA));
                }

                var closingParen = Consume(TokenType.RIGHT_SQUARE_BRACKET, "Expected ]");

                return new NewInstanceExpression(className, args);
            }

            if (Match(TokenType.LEFT_BRACE))
            {
                var originalToken = Previous();
                var className = new Token(TokenType.IDENTIFIER, "Object", null, originalToken.Line, originalToken.Col, originalToken.StartPosition, originalToken.EndPosition);

                var args = new List<Expression>();

                if (!Check(TokenType.RIGHT_BRACE))
                {
                    do
                    {

                        var name = Consume(TokenType.IDENTIFIER, "Expected idetifier");
                        _ = Consume(TokenType.COLON, "Exepcted :");
                        var value = Expression();

                        args.Add(new ObjectInitializerExpression(name, value));

                    } while (Match(TokenType.COMMA));
                }

                var closingParen = Consume(TokenType.RIGHT_BRACE, "Expected }");

                return new NewInstanceExpression(className, args);
            }

            if (Match(TokenType.LEFT_BRACKET))
            {
                var expr = Expression();
                
                Consume(TokenType.RIGHT_BRACKET, "Expect ')' after expression.");
            
                return new GroupingExpression(expr);
            }
            
            if (Match(TokenType.START_OF_EMBEDDED_STRING_EXPRESSION))
            {
                var expr = Expression();
                
                Consume(TokenType.END_OF_EMBEDDED_STRING_EXPRESSION, "Expect internal special token 'END_OF_EMBEDDED_STRING_EXPRESSION' after expression.");
            
                return new GroupingExpression(expr, true);
            }
            
            throw Error(Peek(), $"Parser did not expect to see '{Peek().Lexeme}' here");
        }

        private FunctionExpression FatArrowFunctionExpression(bool openBracketConsumed = false)
        {
            _statementCallStack.Push("FUNCTION");
            
            if (!openBracketConsumed && Check(TokenType.LEFT_BRACKET))
            {
                Consume(TokenType.LEFT_BRACKET, "Expected (");
                
                openBracketConsumed = true;
            }

            var functionParams = new List<Token>();
                    
            if (!Check(TokenType.RIGHT_BRACKET))
            {
                do
                {
                    if (functionParams.Count() >= 127)
                    {
                        Error(Peek(), "Can't define a function with more than 127 parameters.");
                    }

                    functionParams.Add(Consume(TokenType.IDENTIFIER, "Expected parameter name"));
                            
                } while (Match(TokenType.COMMA));
            }

            if (openBracketConsumed)
            {
                Consume(TokenType.RIGHT_BRACKET, "Expected )");
            }

            Consume(TokenType.FAT_ARROW, "Expected =>");

            if (Check(TokenType.LEFT_BRACE))
            {
                Consume(TokenType.LEFT_BRACE, "Expected {");
 
                var functionBody = Block();
                        
                _ = _statementCallStack.Pop();

                return new FunctionExpression(functionParams, functionBody);
            }
            else
            {
                var funcExpr = Expression();
                
                // We need to remove this next check to allow function parameter style fat arrows to work...
                // e.g., my_func((x) => x + 1, param2)
                // In this case there's no ;, there's just an expression, but we know it's just one single
                // expression so in theory no need to check for any terminator...?
                
                // Consume(TokenType.SEMICOLON, "Expected ;");

                _ = _statementCallStack.Pop();
                        
                var functionBody = new BlockStatement(new List<Statement>()
                {
                    new ReturnStatement(funcExpr)
                }, true);

                return new FunctionExpression(functionParams, functionBody);
            }
        }
        
        private bool IsInFatArrow(bool openBracketConsumed = true)
        {
            // If we've jsut consumed an opening bracket we need to look ahead for
            //  (x) => 
            // or
            //  (x, y, z) =>
            
            var index = _current;
            
            // If we're looking at an expression, the current token could be an identifier and we just need to check if the next token is =>
            
            if (!openBracketConsumed)
            {
                if (!_tokens[_current].IsFollowedByLineBreak && _tokens[_current + 1].Type == TokenType.FAT_ARROW)
                {
                    return true;
                }
                else if (_tokens[_current].Type == TokenType.LEFT_BRACKET)
                {
                    index++; // pretend we consumed the left brack and next section can serve both needs
                }
                else
                {
                    return false;
                }
            }
            
            // The logic for brackets is a bit more involved...
            
            var previous = TokenType.LEFT_BRACKET;

            
            while (true)
            {
                if (_tokens[index].IsFollowedByLineBreak && _tokens[index].Type != TokenType.FAT_ARROW) // => has to be on same line as (...), but newline can come after =>
                {
                    break;
                }
                
                var next = _tokens[index];

                if (previous == TokenType.LEFT_BRACKET && next.Type == TokenType.RIGHT_BRACKET)
                {
                    // Valid, move on to the next token
                    index++;
                }
                else if (previous == TokenType.LEFT_BRACKET && next.Type == TokenType.IDENTIFIER)
                {
                    // Valid, move on to the next token
                    index++;
                }
                else if (previous == TokenType.IDENTIFIER && (next.Type == TokenType.COMMA || next.Type == TokenType.RIGHT_BRACKET))
                {
                    // Valid, move on to the next token
                    index++;
                }
                else if (previous == TokenType.COMMA && next.Type == TokenType.IDENTIFIER)
                {
                    // Valid, move on to the next token
                    index++;
                }
                else if (previous == TokenType.RIGHT_BRACKET && next.Type == TokenType.FAT_ARROW)
                {
                    // Valid, we're definitely dealing with a fat arrow
                    return true;
                }
                else
                {
                    break;
                }

                previous = next.Type;
            }

            return false;
        }
    }
}