namespace SmolScript.Internals
{
    internal struct SmolProgram
    {
        public List<SmolValue> constants { get; set; }
        public List<List<ByteCodeInstruction>> code_sections { get; set; }
        public List<SmolFunctionDefn> function_table { get; set; }
        public Dictionary<string, string> class_table { get; set; }

        public IList<SmolScript.Internals.Ast.Statements.Statement> astStatements { get; set; }
    }
}

