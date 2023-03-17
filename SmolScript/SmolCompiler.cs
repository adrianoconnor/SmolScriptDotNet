using System.Text;

using SmolScript.Internals.Ast.Expressions;
using SmolScript.Internals.Ast.Statements;
using SmolScript.Internals;

namespace SmolScript
{
    public class SmolCompiler : IExpressionVisitor, IStatementVisitor
    {
        private int _nextLabel = 1;

        private int nextLabel {
            get {
                return _nextLabel++;
            }
        }

        private List<List<ByteCodeInstruction>> function_bodies = new List<List<ByteCodeInstruction>>();

        private List<SmolValue> constants = new List<SmolValue>()
        {
            new SmolValue(true),
            new SmolValue(false),
            new SmolValue(0.0),
            new SmolValue(1.0)
        };

        private List<ByteCodeInstruction> EmitChunk(IList<Statement> stmts) {

            var chunk = new List<ByteCodeInstruction>();

            foreach(var stmt in stmts)
            {
                chunk.AppendChunk(stmt.Accept(this));
            }

            return chunk;
        }

        public static SmolProgram Compile(string source)
        {
            var compiler = new SmolCompiler();

            return compiler._Compile(source);
        }

        private SmolProgram _Compile(string source)
        {
            var scanner = new Scanner(source);
            var scanResult = scanner.ScanTokens();
            var parser = new Parser(scanResult.tokens);
            var statements = parser.Parse();

            // Creating the main chunk will populate the constants
            var main_chunk = this.EmitChunk(statements);

            main_chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.EOF
            });

            var code_sections = new List<List<ByteCodeInstruction>>();
            
            code_sections.Add(main_chunk);
            // Add function blocks

            return new SmolProgram()
            {
                constants = this.constants,
                code_sections = code_sections
            };
        }

        public object? Visit(BinaryExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(expr.left.Accept(this));
            chunk.AppendChunk(expr.right.Accept(this));

            switch (expr.op.type)
            {
                case TokenType.MINUS:
                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.SUB
                    });
                    break;
                case TokenType.DIVIDE:
                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.DIV
                    });
                    break;
                case TokenType.MULTIPLY:
                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.MUL
                    });
                    break;
                case TokenType.PLUS:
                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.ADD
                    });
                    break;

                case TokenType.POW:
                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.POW
                    });
                    break;
                    /*
                case TokenType.GREATER:
                       return (double)left > (double)right;
                   case TokenType.GREATER_EQUAL:
                       return (double)left >= (double)right;
                   case TokenType.LESS:
                       return (double)left < (double)right;
                   case TokenType.LESS_EQUAL:
                       return (double)left <= (double)right;
                   case TokenType.NOT_EQUAL:
                       return !isEqual(left, right);
                   case TokenType.EQUAL_EQUAL:
                       return isEqual(left, right);
                   case TokenType.BITWISE_AND:
                       // A bit stupid, but we have to cast double>int>double...
                       return (double)((int)(double)left & (int)(double)right);
                   case TokenType.BITWISE_OR:
                       return (double)((int)(double)left | (int)(double)right);
                   case TokenType.REMAINDER:
                       return (double)left % (double)right;*/
                default:
                    throw new NotImplementedException();
            }

            return chunk;
        }
        

        public object? Visit(LogicalExpression expr)
        {
            return $"({expr.op.lexeme} {expr.left.Accept(this)} {expr.right.Accept(this)})";
        }

        public object? Visit(GroupingExpression expr)
        {
            return expr.expr.Accept(this);
        }

        public object? Visit(LiteralExpression expr)
        {
            // Literal is always a constant, so see if we've got this
            // literal in our list of constants and add it if we need to

            var value = new SmolValue(expr.value);

            var cIndex = constants.IndexOf(value);

            if (cIndex == -1)
            {
                constants.Add(value);
                cIndex = constants.Count - 1;
            }

            return new ByteCodeInstruction()
            {
                opcode = OpCode.LOAD_CONSTANT,
                operand1 = cIndex
            };
        }

        public object? Visit(UnaryExpression expr)
        {
            return $"({expr.op.lexeme} {expr.right.Accept(this)})";
        }

        public object? Visit(VariableExpression expr)
        {
            return new ByteCodeInstruction()
            {
                opcode = OpCode.LOAD_VARIABLE,
                operand1 = new SmolVariableDefinition() {
                    name = (string)(expr.name.lexeme)
                }
            };
        }

        public object? Visit(AssignExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(expr.value.Accept(this));

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.STORE,
                operand1 = new SmolVariableDefinition()
                {
                    name = (string)(expr.name.lexeme)
                }
            });

            return chunk;
        }

        public object? Visit(CallExpression expr)
        {
            return $"(call {expr.callee.Accept(this)} {expr.paren.lexeme} args[{expr.args.Count}])";
        }

        public object? Visit(VarStatement stmt)
        {
            if (stmt.initializerExpression != null)
            {
                var chunk = new List<ByteCodeInstruction>();

                chunk.AppendChunk(new ByteCodeInstruction()
                {
                    opcode = OpCode.DECLARE,
                    operand1 = new SmolVariableDefinition()
                    {
                        name = (string)(stmt.name.lexeme)
                    }
                });

                chunk.AppendChunk(stmt.initializerExpression.Accept(this));

                chunk.AppendChunk(new ByteCodeInstruction()
                {
                    opcode = OpCode.STORE,
                    operand1 = new SmolVariableDefinition()
                    {
                        name = (string)(stmt.name.lexeme)
                    }
                });

                return chunk;
            }
            else 
            {
                return new ByteCodeInstruction()
                {
                    opcode = OpCode.DECLARE,
                    operand1 = new SmolVariableDefinition()
                    {
                        name = (string)(stmt.name.lexeme)
                    }
                };
            }
        }

        public object? Visit(ExpressionStatement stmt)
        {
            return stmt.expression.Accept(this);
        }

        public object? Visit(BlockStatement stmt)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.ENTER_SCOPE
            });

            foreach (var blockStmt in stmt.statements)
            {
                chunk.AppendChunk(blockStmt.Accept(this));
            }

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.LEAVE_SCOPE
            });

            return chunk;
        }

        public object? Visit(PrintStatement stmt)
        {
            return new ByteCodeInstruction()
            {
                opcode = OpCode.PRINT
            };
        }

        public object? Visit(ReturnStatement stmt)
        {
            var chunk = new List<ByteCodeInstruction>();

            if (stmt.expression != null)
            { 
                chunk.AppendChunk(stmt.expression.Accept(this));
            }
            else
            {
                /*chunk.AppendChunk(new ByteCodeInstruction()
                {
                    opcode = OpCode. // NULL // Check what JS does
                });*/
            }

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.RETURN
            });

            return chunk;
        }

        public object? Visit(BreakStatement stmt)
        {
            return new ByteCodeInstruction()
            {
                opcode = OpCode.JMP,
                operand1 = -1 // Needs to be label from top of break stack
            };
        }

        public object? Visit(IfStatement stmt)
        {
            var chunk = new List<ByteCodeInstruction>();

            int notTrueLabel = nextLabel;

            chunk.AppendChunk(stmt.testExpression.Accept(this));

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.JMPFALSE,
                operand1 = notTrueLabel
            });

            chunk.AppendChunk(stmt.thenStatement.Accept(this));


            if (stmt.elseStatement == null)
            {
                chunk.AppendChunk(new ByteCodeInstruction()
                {
                    opcode = OpCode.LABEL,
                    operand1 = notTrueLabel
                });
            }
            else
            {
                int skipElseLabel = nextLabel;

                chunk.AppendChunk(new ByteCodeInstruction()
                {
                    opcode = OpCode.JMP,
                    operand1 = skipElseLabel
                });

                chunk.AppendChunk(new ByteCodeInstruction()
                {
                    opcode = OpCode.LABEL,
                    operand1 = notTrueLabel
                });

                chunk.AppendChunk(stmt.elseStatement!.Accept(this));

                chunk.AppendChunk(new ByteCodeInstruction()
                {
                    opcode = OpCode.LABEL,
                    operand1 = skipElseLabel
                });
            }

            return chunk;
        }

        public object? Visit(TernaryStatement stmt)
        {
            var s = new StringBuilder();
            
            s.AppendLine($"[ternary {stmt.testExpression.Accept(this)}]");

            s.AppendLine($"[then]");
            //s.Append($"{stmt.thenStatement.Accept(this)}");
            s.AppendLine($"[/then]");
        
            s.AppendLine($"[else]");
            //s.Append($"{stmt.elseStatement!.Accept(this)}");
            s.AppendLine($"[/else]");
        
            s.AppendLine("[end ternary]");

            return s.ToString();
        }

        public object? Visit(WhileStatement stmt)
        {
            var s = new StringBuilder();
            
            s.AppendLine($"[while {stmt.whileCondition.Accept(this)}]");

            s.Append($"{stmt.executeStatement.Accept(this)}");

            s.AppendLine("[end while]");

            return s.ToString();
        }

        public object? Visit(FunctionStatement stmt)
        {            
            //s.AppendLine($"[declare function {stmt.name?.lexeme ?? ""} ()]");

            //s.Append($"{stmt.functionBody.Accept(this)}");

            //s.AppendLine("[end function declaration]");

            return null;
        }

    }
}