using SmolScript.Internals.SmolVariableTypes;

namespace SmolScript.Internals
{
    /// <summary>
    /// This data structure holds the compiled smol program
    /// that can be executed by the VM. It is only an internal
    /// intermediate state, it is not meant to be persisted
    /// (but it could be if that ever becomes a requirement)
    /// </summary>
    internal class SmolProgram
    {
        internal required List<SmolVariableType> Constants { get; set; }
        internal required List<List<ByteCodeInstruction>> CodeSections { get; set; }
        internal required List<SmolFunction> FunctionTable { get; set; }
        
        
        
        internal required IList<Token> Tokens { get; set; }
        internal required string Source { get;  set; }
        
        internal Dictionary<int, int> JumpTable = new();
        
        internal void BuildJumpTable()
        {
            // Loop through all labels in all code sections, capturing
            // the label number (always unique) and the location/index
            // in the instructions for that section so we can jump
            // if we need to.

            for (int i = 0; i < this.CodeSections.Count; i++)
            {
                // Not sure if this will hold up, might be too simplistic

                for (int j = 0; j < this.CodeSections[i].Count; j++)
                {
                    var instr = this.CodeSections[i][j];

                    if (instr.OpCode == OpCode.LABEL)
                    {
                        // We're not storing anything about the section
                        // number but this should be ok becuase we should
                        // only ever jump inside the current section...
                        // Jumps to other sections are handled in a different
                        // way using the CALL instruction
                        JumpTable[(int)instr.Operand1!] = j;
                    }
                }
            }
        }
    }
}

