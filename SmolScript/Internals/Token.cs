namespace SmolScript.Internals
{
    internal class Token
    {
        public TokenType type { get; private set; }
        public string lexeme { get; private set; }
        public object? literal { get; private set; }

        public int line { get; private set; }
        public int col { get; private set; }
        public int start_pos { get; private set; }
        public int end_pos { get; private set; }

        public bool followed_by_line_break { get; internal set; }

        public Token(TokenType type, string lexeme, object? literal, int line, int col, int start_pos, int end_pos)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;

            this.line = line;
            this.col = col;
            this.start_pos = start_pos;
            this.end_pos = end_pos;

            followed_by_line_break = false;
        }

        public override string ToString()
        {
            return $"Token: {type}, {lexeme}, {literal}";
        }
    }
}