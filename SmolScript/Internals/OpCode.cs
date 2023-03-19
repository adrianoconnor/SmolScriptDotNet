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
        LT,
        LTE,
        GT,
        GTE,

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

        PRINT,
        DEBUGGER,

        EOF
    }
}

