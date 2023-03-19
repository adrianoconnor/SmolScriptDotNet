using System;

namespace SmolScript.Internals
{
    public struct ByteCodeInstruction
    {
        public OpCode opcode { get; set; }

        public object? operand1;
        public object? operand2;

        public override string ToString()
        {
            var str = $"{this.opcode.ToString().PadRight(13)}";

            if (this.operand1 != null)
            {
                str += $" {this.operand1}";
            }

            if (this.operand2 != null)
            {
                str += $" {this.operand2}";
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
    public static class ByteCodeChunkExtension
    {
        public static void AppendChunk(this List<ByteCodeInstruction> chunk, object? byteCodeChunkToAdd)
        {
            if (byteCodeChunkToAdd != null) {
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
    }
}

