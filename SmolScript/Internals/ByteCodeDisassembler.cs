using System;
using System.Text;

namespace SmolScript.Internals
{
	public static class ByteCodeDisassembler
	{
		public static string Disassemble(SmolProgram program)
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine(".constants");

			for(int i = 0; i < program.constants.Count; i++)
			{
                sb.AppendLine($"[{i}]: {program.constants[i]}");
            }

            sb.AppendLine("");
            sb.AppendLine(".function_table");

			foreach (var fn in program.function_table)
			{
                sb.AppendLine($"Name: {fn.globalFunctionName} (code_section = {fn.code_section}, arity = {fn.arity})");
            }

            for (int i = 0; i < program.code_sections.Count; i++)
            {
				sb.AppendLine($"");

                sb.AppendLine($".code_section {i}");

                WriteInstructions(program.code_sections[i], sb);
            }


            return sb.ToString();
		}

		private static void WriteInstructions(List<ByteCodeInstruction> instrs, StringBuilder to)
		{
			foreach(var instr in instrs)
			{
				to.AppendLine(instr.ToString());
            }
        }
	}
}

