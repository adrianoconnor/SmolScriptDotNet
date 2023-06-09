﻿using System;

namespace SmolScript.Internals
{
    internal struct ByteCodeInstruction
    {
        public OpCode opcode { get; set; }

        public object? operand1;
        public object? operand2;

        public bool StepCheckpoint;

        public override string ToString()
        {
            var str = $"{this.opcode.ToString().PadRight(13)}";

            if (this.operand1 != null)
            {
                str += $" [op1: {this.operand1}]";
            }

            if (this.operand2 != null)
            {
                str += $" [op2: {this.operand2}]";
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
                opcode = opcode,
                operand1 = operand1,
                operand2 = operand2
            });
        }
    }
}

