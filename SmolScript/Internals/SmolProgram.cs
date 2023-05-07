using SmolScript.Internals.Ast.Statements;
using SmolScript.Internals.SmolStackTypes;
using SmolScript.Internals.SmolVariableTypes;

namespace SmolScript.Internals
{
    /// <summary>
    /// This data structure holds the compiled smol program
    /// that can be executed by the VM. It is only an internal
    /// intermediate state, it is not meant to be persisted
    /// (but it could be if that ever becomes a requirement)
    /// </summary>
    internal struct SmolProgram
    {
        internal List<SmolVariableType> constants { get; set; }
        internal List<List<ByteCodeInstruction>> code_sections { get; set; }
        internal List<SmolFunction> function_table { get; set; }
    }
}

