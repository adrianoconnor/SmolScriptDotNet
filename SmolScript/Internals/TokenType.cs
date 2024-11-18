namespace SmolScript.Internals
{
    internal enum TokenType
    {
        LEFT_BRACKET,
        RIGHT_BRACKET,
        LEFT_BRACE,
        RIGHT_BRACE,
        LEFT_SQUARE_BRACKET,
        RIGHT_SQUARE_BRACKET,
        COMMA,
        DOT,
        MINUS,
        PLUS,
        POW,
        SEMICOLON,
        DIVIDE,
        REMAINDER,

        MULTIPLY,
        LOGICAL_AND,
        LOGICAL_OR,

        BITWISE_AND,
        BITWISE_OR,
        BITWISE_XOR,
        BITWISE_NOT,

        QUESTION_MARK,
        COLON,

        NOT,
        NOT_EQUAL,
        EQUAL,
        EQUAL_EQUAL,
        GREATER,
        GREATER_EQUAL,
        LESS,
        LESS_EQUAL,

        PLUS_EQUALS,
        MINUS_EQUALS,
        MULTIPLY_EQUALS,
        POW_EQUALS,
        DIVIDE_EQUALS,
        REMAINDER_EQUALS,

        POSTFIX_INCREMENT,
        PREFIX_INCREMENT,
        POSTFIX_DECREMENT,
        PREFIX_DECREMENT,
        
        FAT_ARROW,

        // Literals

        IDENTIFIER,
        STRING,
        NUMBER,
        
        START_OF_EMBEDDED_STRING_EXPRESSION,
        END_OF_EMBEDDED_STRING_EXPRESSION,

        // Keywords

        BREAK,
        CASE,
        CLASS,
        CONST,
        CONTINUE,
        DEBUGGER,
        DO,
        ELSE,
        FALSE,
        FUNC,
        FOR,
        IF,
        NEW,
        NULL,
        RETURN,
        SUPER,
        SWITCH,
        TRUE,
        VAR,
        WHILE,
        UNDEFINED,
        TRY,
        CATCH,
        FINALLY,
        THROW,

        EOF
    }
}