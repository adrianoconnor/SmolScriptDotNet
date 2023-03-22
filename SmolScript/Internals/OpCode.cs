namespace SmolScript.Internals
{
    public enum OpCode
    {
        NOP,

        LABEL,
        CALL,
        RETURN,

        ADD,
        SUB,
        DIV,
        MUL,
        POW,
        REM,

        EQL,
        NEQ,
        LT,
        LTE,
        GT,
        GTE,

        BITWISE_AND,
        BITWISE_OR,

        JMPTRUE,
        JMPFALSE,
        JMP,

        DECLARE,

        CONST,
        FETCH,

        STORE,

        ENTER_SCOPE,
        LEAVE_SCOPE,

        TRY,
        CATCH,
        THROW,

        NEW,

        POP_AND_DISCARD,

        PRINT,
        DEBUGGER,

        EOF
    }
}

