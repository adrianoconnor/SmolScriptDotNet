namespace SmolScript.Internals
{
    public enum OpCode
    {
        NOP,

        LABEL,
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

        LOAD_CONSTANT,
        LOAD_VARIABLE,

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

