using System.Text;

namespace SmolScript.Internals
{
    public class ScannerError : Exception
    {
        public int line { get; set; }
        public string message { get; set; }

        public ScannerError(int line, string message)
        {
            this.line = line;
            this.message = message;
        }
    }

    internal class Scanner
    {
        private string _source;
        private char[] _sourceRaw;

        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        private IList<Token> _tokens = new List<Token>();

        private IDictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>()
        {
            { "break", TokenType.BREAK },
            { "class", TokenType.CLASS },
            { "case", TokenType.CASE },
            { "const", TokenType.CONST },
            { "continue", TokenType.CONTINUE },
            { "debugger", TokenType.DEBUGGER },
            { "do", TokenType.DO },
            { "else", TokenType.ELSE },
            { "false", TokenType.FALSE },
            { "for", TokenType.FOR },
            { "function", TokenType.FUNC },
            { "if", TokenType.IF },
            { "null", TokenType.NULL },
            { "new", TokenType.NEW },
            { "print", TokenType.PRINT },
            { "return", TokenType.RETURN },
            { "super", TokenType.SUPER },
            { "switch", TokenType.SWITCH },
            //{ "this", TokenType.THIS },
            { "true", TokenType.TRUE },
            { "var", TokenType.VAR },
            { "let", TokenType.VAR },
            { "while", TokenType.WHILE },
            { "undefined", TokenType.UNDEFINED },
            { "try", TokenType.TRY },
            { "catch", TokenType.CATCH },
            { "finally", TokenType.FINALLY },
            { "throw", TokenType.THROW }
        };

        public Scanner(string source)
        {
            this._source = source;
            this._sourceRaw = source.ToCharArray();
        }

        public IList<Token> ScanTokens()
        {
            while (!ReachedEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, _line));

            return _tokens;
        }

        private void ScanToken()
        {
            char c = NextChar();

            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_BRACKET); break;
                case ')': AddToken(TokenType.RIGHT_BRACKET); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case '[': AddToken(TokenType.LEFT_SQUARE_BRACKET); break;
                case ']': AddToken(TokenType.RIGHT_SQUARE_BRACKET); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '?': AddToken(TokenType.QUESTION_MARK); break;
                case ':': AddToken(TokenType.COLON); break;
                case '-':
                    if (MatchNext('-'))
                    {
                        if (_tokens.LastOrDefault()?.type == TokenType.IDENTIFIER && _tokens.LastOrDefault()?.followedByLineBreak == false)
                        {
                            AddToken(TokenType.POSTFIX_DECREMENT);
                        }
                        else
                        {
                            AddToken(TokenType.PREFIX_DECREMENT);
                        }
                    }
                    else if (MatchNext('='))
                    {
                        AddToken(TokenType.MINUS_EQUALS);
                    }
                    else
                    {
                        AddToken(TokenType.MINUS);
                    }
                    break;
                case '+':
                    if (MatchNext('+'))
                    {
                        if (_tokens.LastOrDefault()?.type == TokenType.IDENTIFIER && _tokens.LastOrDefault()?.followedByLineBreak == false)
                        {
                            AddToken(TokenType.POSTFIX_INCREMENT);
                        }
                        else
                        {
                            AddToken(TokenType.PREFIX_INCREMENT);
                        }
                    }
                    else if (MatchNext('='))
                    {
                        AddToken(TokenType.PLUS_EQUALS);
                    }
                    else
                    {
                        AddToken(TokenType.PLUS);
                    }
                    break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*':
                    if (MatchNext('*'))
                    {
                        if (MatchNext('='))
                        {
                            AddToken(TokenType.POW_EQUALS);
                        }
                        else
                        {
                            AddToken(TokenType.POW);
                        }
                    }
                    else if (MatchNext('='))
                    {
                        AddToken(TokenType.MULTIPLY_EQUALS);
                    }
                    else
                    {
                        AddToken(TokenType.MULTIPLY);
                    }
                    break;
                case '!':
                    if (MatchNext('='))
                    {
                        AddToken(TokenType.NOT_EQUAL);
                    }
                    else
                    {
                        AddToken(TokenType.NOT);
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
                        while (Peek() != '\n' && !ReachedEnd()) _ = NextChar();
                    }
                    else if (MatchNext('='))
                    {
                        AddToken(TokenType.DIVIDE_EQUALS);
                    }
                    else if (MatchNext('*'))
                    {
                        while (Peek() != '*' || Peek(1) != '/')
                        {
                            if (ReachedEnd())
                            {
                                //_errors.Add(
                                throw new ScannerError(_line, $"Expected end of comment block");
                            }
                            else
                            {
                                c = NextChar();
                                //_current = NextChar();
                            }
                        }

                        MatchNext('*');
                        MatchNext('/');
                    }
                    else
                    {
                        AddToken(TokenType.DIVIDE);
                    }
                    break;
                case '%':
                    AddToken(TokenType.REMAINDER);
                    break;
                case '&':
                    if (MatchNext('&'))
                    {
                        AddToken(TokenType.LOGICAL_AND);
                    }
                    else
                    {
                        AddToken(TokenType.BITWISE_AND);
                    }
                    break;
                case '|':
                    if (MatchNext('|'))
                    {
                        AddToken(TokenType.LOGICAL_OR);
                    }
                    else
                    {
                        AddToken(TokenType.BITWISE_OR);
                    }
                    break;

                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace
                    break;

                case '\n':

                    _line++;

                    if (_tokens.Any())
                    {
                        _tokens[_tokens.Count()- 1].followedByLineBreak = true;
                    }

                    break;

                case '\'':
                    ProcessString('\'');
                    break;

                case '"':
                    ProcessString('"');
                    break;

                case '`':
                    ProcessBacktickString();
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
                        //_errors.Add(
                        throw new ScannerError(_line, $"Unexpected character '{c}'");
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

        private void ProcessString(char quoteChar)
        {
            StringBuilder sb = new StringBuilder();

            while (Peek() != quoteChar && !ReachedEnd())
            {

                if (MatchNext('\n')) // Peek() == '\n')
                {
                    _line++;
                    //_errors.Add(new ScannerError(_line, "Unexpected Line break in string"));
                    throw new ScannerError(_line, "Unexpected Line break in string");
                    //return;
                }

                if (Peek() == '\\')
                {
                    var next = Peek(1);

                    if (next == '\'' || next == '"' || next == '\\')
                    {
                        _ = NextChar();
                        sb.Append(NextChar());
                    }
                    else if (next == 't')
                    {
                        _ = NextChar();
                        _ = NextChar();
                        sb.Append('\t');
                    }
                    else if (next == 'r')
                    {
                        _ = NextChar();
                        _ = NextChar();
                        sb.Append('\r');
                    }
                    else if (next == 'n')
                    {
                        _ = NextChar();
                        _ = NextChar();
                        sb.Append('\n');
                    }
                    else
                    {
                        sb.Append(NextChar());
                    }
                }
                else
                {
                    sb.Append(NextChar());
                }
            }

            if (ReachedEnd())
            {
                throw new ScannerError(_line, "Unterminated string");

                //_errors.Add(new ScannerError(_line, "Unterminated string"));
                //return;
            }

            // Consume the closing "
            _ = NextChar();

            AddToken(TokenType.STRING, sb.ToString());
        }

        private void ProcessBacktickString()
        {
            StringBuilder sb = new StringBuilder();
            bool hasProducedAtLeastOneToken = false; // Use this to know whether we need to inject a + before each new token

            while (Peek() != '`' && !ReachedEnd())
            {
                // TODO: Need to refactor this so somehow it uses the same code as regular string parsing -- need it to
                // match those rules exactly, an handle all of the same cases for escaping etc.

                if (Peek() == '\n') _line++;

                if (Peek() == '\\')
                {
                    var next = Peek(1);

                    if (next == '\'' || next == '"' || next == '\\' || next == '{')
                    {
                        _ = NextChar();
                        sb.Append(NextChar());
                    }
                    else if (next == 't')
                    {
                        _ = NextChar();
                        _ = NextChar();
                        sb.Append('\t');
                    }
                    else if (next == 'r')
                    {
                        _ = NextChar();
                        _ = NextChar();
                        sb.Append('\r');
                    }
                    else if (next == 'n')
                    {
                        _ = NextChar();
                        _ = NextChar();
                        sb.Append('\n');
                    }
                    else
                    {
                        sb.Append(NextChar());
                    }
                }
                else
                {
                    if (Peek() == '$' && Peek(1) == '{')
                    {
                        // We've just entered the ${} section, so whatever we've got so far, create
                        // a string token and add it to the stream, and then start a new string part

                        if (sb.Length > 0)
                        {
                            AddToken(TokenType.STRING, sb.ToString());
                            sb = new StringBuilder();
                            hasProducedAtLeastOneToken = true;
                        }

                        // Now we'll loop through collecting whatever is inside the ${}

                        _ = NextChar();
                        _ = NextChar();

                        StringBuilder embeddedExpr = new StringBuilder();

                        bool inEmbeddedString = false;
                        char? embeddedStringChar = null;

                        while ((Peek() != '}' || inEmbeddedString) && !ReachedEnd())
                        {
                            // Bug here, ${"}"} will currently not do so well

                            if ((embeddedStringChar == null && (Peek() == '\'' || Peek() == '"'))
                                    || embeddedStringChar != null && Peek() == embeddedStringChar) // Also `
                            {
                                embeddedStringChar = Peek();
                                inEmbeddedString = !inEmbeddedString;
                            }

                            embeddedExpr.Append(NextChar());
                        }

                        _ = NextChar();

                        if (embeddedExpr.Length > 0)
                        {
                            // We've just extracted the contents inside the ${}.
                            // Now we create a new scanner and pass it that string and
                            // get back tokens. We will wrap those in parens and, if we've already
                            // generated at least one token so far, we insert a + to concat them.

                            if (hasProducedAtLeastOneToken)
                            {
                                // There is actually a potential bug here... I think
                                // `${a}${b}` might actually print the result of a+b if they're numbers.

                                AddToken(TokenType.PLUS);
                            }

                            Scanner embeddedScanner = new Scanner(embeddedExpr.ToString());

                            var embeddedTokens = embeddedScanner.ScanTokens();

                            //TODO: Handle errors from embedded scanner

                            AddToken(TokenType.LEFT_BRACKET);

                            foreach (var t in embeddedTokens)
                            {
                                if (t.type == TokenType.EOF)
                                {
                                    break;
                                }

                                this._tokens.Add(t);
                            }

                            AddToken(TokenType.RIGHT_BRACKET);

                            hasProducedAtLeastOneToken = true;
                        }

                    }
                    else
                    {
                        sb.Append(NextChar());
                    }
                }
            }

            if (ReachedEnd())
            {
                throw new ScannerError(_line, "Unterminated string");
                //_errors.Add(new ScannerError(_line, "Unterminated string"));
                //return;
            }

            // Consume the closing `
            _ = NextChar();

            if (sb.Length > 0 || !hasProducedAtLeastOneToken) // If we haven't produced a token yet, even if it's an empty string, we still need that string token
            {
                if (hasProducedAtLeastOneToken)
                {
                    AddToken(TokenType.PLUS);
                }

                AddToken(TokenType.STRING, sb.ToString());// stringValue);
            }
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
            while (CharIsDigit(Peek())) _ = NextChar();

            if (Peek() == '.' && CharIsDigit(Peek(1)))
            {
                // Consume the .
                _ = NextChar();

                while (CharIsDigit(Peek())) _ = NextChar();
            }

            var stringValue = _source.Substring(_start, _current - _start);
            AddToken(TokenType.NUMBER, Double.Parse(stringValue));
        }

        private void ProcessIdentifier()
        {
            while (CharIsAlphaNumeric(Peek())) _ = NextChar();

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