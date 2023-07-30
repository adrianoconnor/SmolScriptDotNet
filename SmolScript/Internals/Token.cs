namespace SmolScript.Internals
{
    internal class Token
    {
        public TokenType type { get; private set; }

        public string lexeme { get; private set; }

        public object? literal { get; private set; }

        public int line { get; private set; }

        public bool followedByLineBreak { get; internal set; }

        public Token(TokenType type, string lexeme, object? literal, int line)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
            followedByLineBreak = false;
        }

        public override string ToString()
        {
            return $"Token: {type}, {lexeme}, {literal}";
        }
    }
}