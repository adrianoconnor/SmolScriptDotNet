using SmolScript.Internals.Ast.Statements;
using SmolScript.Internals.SmolStackTypes;

namespace SmolScript.Internals
{
    internal struct SmolProgram
    {
        public List<SmolStackValue> constants { get; set; }
        public List<List<ByteCodeInstruction>> code_sections { get; set; }
        public List<SmolFunction> function_table { get; set; }
        public Dictionary<string, string?> class_table { get; set; }

        public IList<Statement> astStatements { get; set; }
    }
}

