using SmolScript.Internals;

namespace SmolScript
{
    public class SmolVM
    {
        SmolProgram program;

        int PC = 0;
        int code_section = 0;

        Stack<SmolValue> stack = new Stack<SmolValue>();
        Dictionary<int, int> jmplocs = new Dictionary<int, int>();

        public readonly Environment globalEnv = new Environment();
        private Environment environment;

        public SmolVM(string source)
        {
            environment = globalEnv;

            this.program = SmolCompiler.Compile(source);

            BuildJumpTable();
        }

        public SmolVM(SmolProgram program)
        {
            environment = globalEnv;

            this.program = program;

            BuildJumpTable();
        }

        private void BuildJumpTable()
        {
            // Loop through all labels in all code sections, capturing
            // the label number (always unique) and the location/index
            // in the instructions for that section so we can jump
            // if we need to.

            for (int i = 0; i < this.program.code_sections.Count; i++)
            {
                // Not sure if this will hold up, might be too simplistic

                for (int j = 0; j < this.program.code_sections[i].Count; j++)
                {
                    var instr = this.program.code_sections[i][j];

                    if (instr.opcode == OpCode.LABEL)
                    {
                        // We're not storing anything about the section
                        // number but this should be ok becuase we should
                        // only ever jump inside the current section...
                        // Jumps to other sections are handled in a different
                        // way using the CALL instruction
                        jmplocs[(int)instr.operand1!] = j;
                    }
                }
            }
        }

        public void Run()
        {
            double t = System.Environment.TickCount;

            while (true)
            {
                var instr = program.code_sections[code_section][PC++];

                Console.WriteLine($"{instr}: {System.Environment.TickCount - t}");
                t = System.Environment.TickCount;

                switch (instr.opcode)
                {
                    case OpCode.NOP:
                        // Just skip over this instruction, no-op
                        break;

                    case OpCode.LOAD_CONSTANT:
                        stack.Push(program.constants[(int)instr.operand1!]);
                        break;

                    case OpCode.ADD:
                        {
                            var right = stack.Pop();
                            var left = stack.Pop();

                            stack.Push(left + right);

                            break;
                        }

                    case OpCode.EOF:
                        // Needs to handle call stack scenario for functions that
                        // implicitly return void
                        return;

                    case OpCode.RETURN:
                        // Needs to return to the previous code section, putting
                        // a return value on the stack and restoring the PC
                        break;

                    case OpCode.DECLARE:
                        environment.Define(((SmolVariableDefinition)instr.operand1!).name, null);
                        break;

                    case OpCode.STORE:
                        {
                            var value = stack.Pop();

                            environment.Assign(((SmolVariableDefinition)instr.operand1!).name, value);
                            break;
                        }

                    case OpCode.LOAD_VARIABLE:
                        stack.Push((SmolValue)environment.Get(((SmolVariableDefinition)instr.operand1!).name)!);
                        break;

                    case OpCode.JMPFALSE:
                        {
                            var value = stack.Pop();

                            if (value.IsFalsey())
                            {
                                PC = jmplocs[(int)instr.operand1!];
                            }

                            break;
                        }

                    case OpCode.JMP:
                        PC = jmplocs[(int)instr.operand1!];
                        break;

                    case OpCode.LABEL:
                        // Just skip over this instruction, it's only here
                        // to support branching
                        break;

                    case OpCode.ENTER_SCOPE:
                        break;

                    case OpCode.LEAVE_SCOPE:
                        break;

                    default:
                        throw new Exception($"You forgot to handle an opcode: {instr.opcode}");
                }
            }
        }
    }
}

