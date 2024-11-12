using System;

namespace SmolScript.Internals
{
    internal class ByteCodeInstruction
    {
        // These are the classic bytecode-style values, though we're just storing them
        // as fields on an object
        public OpCode OpCode { get; set; }

        public object? Operand1;
        public object? Operand2;

        // This flag tells us that we're at the end of a single Statement, so this is where
        // the debugger would naturally step to if we're stepping through our program
        public bool? IsStatementStartpoint;

        // These attributes are used for mapping back to the original source code
        public int? TokenMapStartIndex;
        public int? TokenMapEndIndex;

        public override string ToString()
        {
            var str = $"{this.OpCode.ToString().PadRight(13)}";

            if (this.Operand1 != null)
            {
                str += $" [op1: {this.Operand1}]";
            }

            if (this.Operand2 != null)
            {
                str += $" [op2: {this.Operand2}]";
            }

            str += "";

            return str;
        }
    }

    /// <summary>
    /// This helper will take a null, a single instruction, or a list
    /// of instructions and append them as appropriate to whatever chunk we're
    /// currently compiling/building. This makes it so much easier to handle
    /// different types of data from the AST tree walker.
    /// </summary>
    internal static class ByteCodeChunkExtension
    {
        public static void AppendChunk(this List<ByteCodeInstruction> chunk, object? byteCodeChunkToAdd)
        {
            if (byteCodeChunkToAdd != null)
            {
                if (byteCodeChunkToAdd?.GetType() == typeof(ByteCodeInstruction))
                {
                    chunk.Add((ByteCodeInstruction)byteCodeChunkToAdd!);
                }
                else
                {
                    chunk.AddRange((List<ByteCodeInstruction>)byteCodeChunkToAdd!);
                }
            }
        }

        public static void AppendInstruction(this List<ByteCodeInstruction> chunk, OpCode opcode, object? operand1 = null, object? operand2 = null)
        {
            chunk.Add(new ByteCodeInstruction()
            {
                OpCode = opcode,
                Operand1 = operand1,
                Operand2 = operand2
            });
        }

        public static void MapTokens(this List<ByteCodeInstruction> chunk, int? firstTokenIndex, int? lastTokenIndex = null)
        {
            foreach(var instr in chunk)
            {
                if (instr.TokenMapStartIndex == null)
                {
                    instr.TokenMapStartIndex = firstTokenIndex!.Value;
                    instr.TokenMapEndIndex = lastTokenIndex.HasValue ? lastTokenIndex.Value : firstTokenIndex;
                }
            }
        }
    }
}

