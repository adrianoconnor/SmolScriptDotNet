namespace SmolScript.Internals
{
    internal class Token
    {
        public TokenType Type { get; private set; }
        public string Lexeme { get; private set; }
        public object? Literal { get; private set; }

        public int Line { get; private set; }
        public int Col { get; private set; }
        public int StartPosition { get; private set; }
        public int EndPosition { get; private set; }

        public bool IsFollowedByLineBreak { get; internal set; }

        public Token(TokenType type, string lexeme, object? literal, int line, int col, int startPosition, int endPosition)
        {
            this.Type = type;
            this.Lexeme = lexeme;
            this.Literal = literal;

            this.Line = line;
            this.Col = col;
            this.StartPosition = startPosition;
            this.EndPosition = endPosition;
            
            IsFollowedByLineBreak = false;
        }

        public override string ToString()
        {
            return $"Token: {Type}, {Lexeme}, {Literal}";
        }
    }
}