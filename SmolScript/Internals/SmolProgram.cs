namespace SmolScript.Internals
{
    public struct SmolProgram
    {
        public List<SmolValue> constants { get; set; }
        public List<List<ByteCodeInstruction>> code_sections { get; set; }
    }
}

