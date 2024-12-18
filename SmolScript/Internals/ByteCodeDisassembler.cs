﻿using System;
using System.Text;

namespace SmolScript.Internals
{
    internal static class ByteCodeDisassembler
    {
        public static string Disassemble(SmolProgram program)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(".constants");

            for (int i = 0; i < program.Constants.Count; i++)
            {
                sb.AppendLine($"[{i}]: {program.Constants[i]}");
            }

            sb.AppendLine("");
            sb.AppendLine(".function_table");

            foreach (var fn in program.FunctionTable)
            {
                sb.AppendLine($"Name: {fn.GlobalFunctionName} (code_section = {fn.CodeSection}, arity = {fn.Arity})");
            }

            for (int i = 0; i < program.CodeSections.Count; i++)
            {
                sb.AppendLine($"");

                sb.AppendLine($".code_section {i}");

                WriteInstructions(program.CodeSections[i], sb);
            }

            sb.AppendLine($"");

            for (int i = 0; i < program.FunctionTable.Count; i++)
            {
                var f = program.FunctionTable[i];

                sb.AppendLine($".function {i}");
                sb.AppendLine($"GlobalFunctionName = {f.GlobalFunctionName}, CodeSection = {f.CodeSection}, Arity = {f.Arity}");
            }

            return sb.ToString();
        }

        private static void WriteInstructions(List<ByteCodeInstruction> instrs, StringBuilder to)
        {
            foreach (var instr in instrs)
            {
                to.AppendLine(instr.ToString());
            }
        }
    }
}

