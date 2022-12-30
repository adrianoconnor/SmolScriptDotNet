using System;

namespace SmolScript
{
    public record ScannerError
    {
        public int line { get; set; }
        public string message { get; set; }

        public ScannerError(int line, string message)
        {
            this.line = line;
            this.message = message;
        }
    }

    public class Scanner
    {
        private string _source; 
        private char[] _sourceRaw;

        private int _start =  0;
        private int _current = 0;
        private int _line = 1;

        private IList<Token> _tokens = new List<Token>();
        private IList<ScannerError> _errors = new List<ScannerError>();

        private IDictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>()
        {
            { "and", TokenType.AND },
            { "break", TokenType.BREAK },
            { "class", TokenType.CLASS },
            { "else", TokenType.ELSE },
            { "false", TokenType.FALSE },
            { "for", TokenType.FOR },
            { "function", TokenType.FUNC },
            { "if", TokenType.IF },
            { "nil", TokenType.NIL },
            { "or", TokenType.OR },
            { "print", TokenType.PRINT },
            { "return", TokenType.RETURN },
            { "super", TokenType.SUPER },
            { "this", TokenType.THIS },
            { "true", TokenType.TRUE },
            { "var", TokenType.VAR },
            { "while", TokenType.WHILE }    
        };

        public Scanner(string source)
        {
            this._source = source;
            this._sourceRaw = source.ToCharArray();
        }

        public (IList<Token> tokens, IList<ScannerError> errors) ScanTokens()
        {
            while(!ReachedEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, _line));

            return (_tokens, _errors);
        }

        private void ScanToken()
        {
            char c = NextChar();

            switch(c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case '^': AddToken(TokenType.POW); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '!':
                    if (MatchNext('='))
                    {
                        AddToken(TokenType.BANG_EQUAL);
                    }
                    else
                    {
                        AddToken(TokenType.BANG);
                    }
                    break;
                case '=':
                    if (MatchNext('='))
                    {
                        AddToken(TokenType.EQUAL_EQUAL);
                    }
                    else
                    {
                        AddToken(TokenType.EQUAL);
                    }
                    break;
                case '<':
                    if (MatchNext('='))
                    {
                        AddToken(TokenType.LESS_EQUAL);
                    }
                    else
                    {
                        AddToken(TokenType.LESS);
                    }
                    break;
                case '>':
                    if (MatchNext('='))
                    {
                        AddToken(TokenType.GREATER_EQUAL);
                    }
                    else
                    {
                        AddToken(TokenType.GREATER);
                    }
                    break;
                case '/':
                    if (MatchNext('/'))
                    {
                        while(Peek() != '\n' && !ReachedEnd()) _ = NextChar();
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case '&':
                    if (MatchNext('&'))
                    {
                        AddToken(TokenType.AND);
                    }
                    break;
                case '|':
                    if (MatchNext('|'))
                    {
                        AddToken(TokenType.OR);
                    }
                    break;

                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace
                    break;

                case '\n':
                    _line++;
                    break;

                case '"':
                    ProcessString();
                    break;

                default:
                    if (CharIsDigit(c))
                    {
                        ProcessNumber();
                    }
                    else if (CharIsAlpha(c))
                    {
                        ProcessIdentifier();
                    }
                    else
                    {
                        _errors.Add(new ScannerError(_line, "Unexpected character"));
                    }

                    break;
            }
        }

        private void AddToken(TokenType tokenType)
        {
            AddToken(tokenType, null);
        }

        private void AddToken(TokenType tokenType, object? literal)
        {
            //Console.WriteLine($"**ADD TOKEN**");
            
            //Console.WriteLine($"Token Type: {tokenType}");
            //Console.WriteLine($"Start: {_start}");
            //Console.WriteLine($"Current: {_current}");

            var text = _source.Substring(_start, _current - _start);

            //Console.WriteLine($"Text: {text}");

            _tokens.Add(new Token(tokenType, text, literal, _line));
        }

        private bool ReachedEnd()
        {
            return _current >= _source.Length;
        }

        private char NextChar()
        {
            return _sourceRaw[_current++];
        }

        private char Peek(int peekAhead = 0)
        {
            if (ReachedEnd()) return '\0';
            return _sourceRaw[_current + peekAhead];
        }

        private bool MatchNext(char charToMatch)
        {
            if (Peek() == charToMatch)
            {
                _ = NextChar();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ProcessString()
        {
            while (Peek() != '"' && !ReachedEnd()) {
                if (Peek() == '\n') _line++;
                _ = NextChar();
            }

            if (ReachedEnd()) {
                _errors.Add(new ScannerError(_line, "Unterminated string"));
                return;
            }

            // Consume the closing "
            _ = NextChar();

            // Trim the surrounding quotes
            var stringStart = _start + 1;
            var stringEnd = _current - 1;

            var stringValue = _source.Substring(stringStart, stringEnd - stringStart);
            AddToken(TokenType.STRING, stringValue);
        }

        private bool CharIsDigit(char c)
        {
            return (c >= '0' && c <= '9');
        }

        private bool CharIsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
        }

        private bool CharIsAlphaNumeric(char c)
        {
            return CharIsAlpha(c) || CharIsDigit(c);
        }

        private void ProcessNumber()
        {
            while(CharIsDigit(Peek())) _ = NextChar();

            if (Peek() == '.' && CharIsDigit(Peek(1)))
            {
                // Consume the .
                _ = NextChar();
            
                while(CharIsDigit(Peek())) _ = NextChar();
            }

            var stringValue = _source.Substring(_start, _current - _start);
            AddToken(TokenType.NUMBER, Double.Parse(stringValue));
        }

        private void ProcessIdentifier()
        {
            while(CharIsAlphaNumeric(Peek())) _ = NextChar();

            var stringValue = _source.Substring(_start, _current - _start);

            if (_keywords.ContainsKey(stringValue))
            {
                AddToken(_keywords[stringValue]);
            }
            else
            {
                AddToken(TokenType.IDENTIFIER);
            }
        }
    }
}