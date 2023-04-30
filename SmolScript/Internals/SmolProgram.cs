using SmolScript.Internals.Ast.Statements;
using SmolScript.Internals.SmolStackTypes;

namespace SmolScript.Internals
{
    internal struct SmolProgram
    {
        internal List<SmolStackValue> constants { get; set; }
        internal List<List<ByteCodeInstruction>> code_sections { get; set; }
        internal List<SmolFunction> function_table { get; set; }
        internal Dictionary<string, string?> class_table { get; set; }

        internal IList<Statement> astStatements { get; set; }
    }
}

