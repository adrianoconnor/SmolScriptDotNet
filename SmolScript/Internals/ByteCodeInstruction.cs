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
    /// Helper that can take either a null, a single instruction or a list
    /// of instructions and add them to an existig list (a chunk). This makes
    /// it easier to just keep appending whatever we get back as we built the
    /// code
    /// </summary>
    public static class ByteCodeChunkExtension
    {
        public static void AppendChunk(this List<ByteCodeInstruction> chunk, object? byteCodeChunkToAdd)
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
}

