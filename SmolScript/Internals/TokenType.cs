namespace SmolScript.Internals
{
    public enum TokenType
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
        MULTIPY_EQUALS,
        POW_EQUALS,
        DIVIDE_EQUALS,
        REMAINER_EQUALS,

        POSTFIX_INCREMENT,
        PREFIX_INCREMENT,
        POSTFIX_DECREMENT,
        PREFIX_DECREMENT,        

        // Literals

        IDENTIFIER,
        STRING,
        NUMBER,

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
        PRINT,
        RETURN,
        SUPER,
        SWITCH,
        THIS,
        TRUE,
        VAR,
        WHILE,
        UNDEFINED,

        EOF
    }
}