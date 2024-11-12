namespace SmolScript.Internals
{
    internal enum OpCode
    {
        /// <summary>
        /// No Operation
        /// </summary>
        NOP,

        LABEL,
        
        /// <summary>
        /// Call a function or method (including external) using function details on the stack. Op1 is number of args (int), Op2 indicates we're calling a method on an object/class (bool)
        /// </summary>
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

        /// <summary>
        /// Load a pre-defined Constant onto the stack. Op1 is the index of the constant in the lookup table.
        /// </summary>
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
        DUPLICATE_VALUE,

        LOOP_EXIT,
        LOOP_START,
        LOOP_END,

        CREATE_OBJECT,
        
        DEBUGGER,

        EOF
    }
}

