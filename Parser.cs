using System;

namespace ABasic
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

declaration    → varDecl
               | statement ;

statement      → exprStmt
               | ifStmt
               | whileStmt
               | printStmt
               | block ;

ifStmt         → "if" "(" expression ")" statement
               ( "else" statement )? ;

whileStmt      → "while" "(" expression ")" statement ;

block          → "{" declaration* "}" ;

exprStmt       → expression ";" ;
printStmt      → "print" expression ";" ;

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
               | primary ;
primary        → NUMBER | STRING | "true" | "false" | "nil"
               | "(" expression ")"
               | IDENTIFIER ;
*/

    public class Parser
    {
        private readonly Token[] _tokens;
        private int _current = 0;

        public Parser(IList<Token> tokens)
        {
            this._tokens = tokens.ToArray();
        }

        public IList<Statement> Parse()
        {
            var statements = new List<Statement>();
            var errors = new List<ParseError>();

            while(!ReachedEnd())
            {
                try
                {
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
            foreach(var tokenType in tokenTypes) {        
                if (check(tokenType)) {
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

        public Token previous()
        {
            return _tokens[_current - 1];
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

                switch (peek().type) {
                    case TokenType.CLASS:
                    case TokenType.FUN:
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
        }

        private bool ReachedEnd()
        {
            return peek().type == TokenType.EOF;
        }

        private Statement declaration()
        {
            try
            {
                if (match(TokenType.VAR)) return varDeclaration();

                return statement();
            }
            catch(ParseError)
            {
                synchronize();
                throw;
            }

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
            return new Statement.Var(name, initializer);
        }

        private Statement statement()
        {
            if (match(TokenType.IF)) return ifStatement();
            if (match(TokenType.WHILE)) return whileStatement();
            if (match(TokenType.PRINT)) return printStatement();
            if (match(TokenType.LEFT_BRACE)) return new Statement.Block(block());

            return expressionStatement();
        }

        private Statement printStatement()
        {
            var expr = expression();
            consume(TokenType.SEMICOLON, "Expected ;");
            return new Statement.Print(expr);
        }

        private IList<Statement> block()
        {
            IList<Statement> statements = new List<Statement>();

            while (!check(TokenType.RIGHT_BRACE) && !ReachedEnd()) {
                statements.Add(declaration());
            }

            consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Statement ifStatement()
        {
            consume(TokenType.LEFT_PAREN, "Expected (");
            var condition = expression();
            consume(TokenType.RIGHT_PAREN, "Expected )");
            var thenStatement = statement();

            Statement? elseStatement = null;

            if (match(TokenType.ELSE))
            {
                elseStatement = statement();
            }

            return new Statement.If(condition, thenStatement, elseStatement);
        }

        private Statement whileStatement()
        {
            consume(TokenType.LEFT_PAREN, "Expected (");
            var whileCondition = expression();
            consume(TokenType.RIGHT_PAREN, "Expected )");
            var whileStatement = statement();

            return new Statement.While(whileCondition, whileStatement);
        }

        private Statement expressionStatement()
        {
            var expr = expression();
            consume(TokenType.SEMICOLON, "Expected ;");
            return new Statement.Expression(expr);
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

                if (expr.GetType() == typeof(Expression.Variable))
                {
                    var name = ((Expression.Variable)expr).name;
                    return new Expression.Assign(name, value);
                }

                throw error(equals, "Invalid assignment target."); 
            }

            return expr;
        }

        private Expression logicalOr()
        {
            var expr = logicalAnd();

            while(match(TokenType.OR))
            {
                var op = previous();
                var right = logicalAnd();
                expr = new Expression.Logical(expr, op, right);
            }

            return expr;
        }

        private Expression logicalAnd()
        {
            var expr = equality();

            while(match(TokenType.AND))
            {
                var op = previous();
                var right = equality();
                expr = new Expression.Logical(expr, op, right);
            }

            return expr;
        }

        private Expression equality()
        {
            var expr = comparison();

            while(match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                var op = previous();
                var right = comparison();
                expr = new Expression.Binary(expr, op, right);
            }

            return expr;
        }

        private Expression comparison()
        {
            var expr = term(); // Was term

            while(match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                var op = previous();
                var right = term(); // Was term
                expr = new Expression.Binary(expr, op, right);
            }

            return expr;
        }

        private Expression term()
        {
            var expr = factor();

            while(match(TokenType.MINUS, TokenType.PLUS))
            {
                var op = previous();
                var right = factor();
                expr = new Expression.Binary(expr, op, right);
            }

            return expr;
        }

        private Expression factor()
        {
            var expr = pow();

            while(match(TokenType.STAR, TokenType.SLASH))
            {
                var op = previous();
                var right = pow();
                expr = new Expression.Binary(expr, op, right);
            }

            return expr;
        }

        private Expression pow()
        {
            var expr = unary();

            while(match(TokenType.POW))
            {
                var op = previous();
                var right = unary();
                expr = new Expression.Binary(expr, op, right);
            }

            return expr;
        }

        private Expression unary()
        {
            if(match(TokenType.BANG, TokenType.MINUS))
            {
                var op = previous();
                var right = unary();
                return new Expression.Unary(op, right);
            }

            return primary();
        }

        private Expression primary()
        {
            if (match(TokenType.FALSE)) return new Expression.Literal(false);
            if (match(TokenType.TRUE)) return new Expression.Literal(true);
            if (match(TokenType.NIL)) return new Expression.Literal(null);

            if(match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expression.Literal(previous().literal!);
            }

            if (match(TokenType.IDENTIFIER))
            {
                return new Expression.Variable(previous());
            }

            if (match(TokenType.LEFT_PAREN)) 
            {
                Expression expr = expression();
                consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expression.Grouping(expr);
            }

            throw error(peek(), "Parser could not deal with the data");
        }
    }
}