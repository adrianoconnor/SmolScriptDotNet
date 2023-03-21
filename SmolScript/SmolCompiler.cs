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

        private List<SmolFunctionDefn> function_table = new List<SmolFunctionDefn>();
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

            foreach(var fb in function_bodies)
            {
                code_sections.Add(fb);
            }

            return new SmolProgram()
            {
                constants = this.constants,
                code_sections = code_sections,
                function_table = function_table
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
                    chunk.AppendInstruction(OpCode.SUB);
                    break;

                case TokenType.DIVIDE:
                    chunk.AppendInstruction(OpCode.DIV);
                    break;

                case TokenType.MULTIPLY:
                    chunk.AppendInstruction(OpCode.MUL);
                    break;

                case TokenType.PLUS:
                    chunk.AppendInstruction(OpCode.ADD);
                    break;

                case TokenType.POW:
                    chunk.AppendInstruction(OpCode.POW);
                    break;

                case TokenType.REMAINDER:
                    chunk.AppendInstruction(OpCode.REM);
                    break;

                case TokenType.EQUAL_EQUAL:
                    chunk.AppendInstruction(OpCode.EQL);
                    break;

                case TokenType.NOT_EQUAL:
                    chunk.AppendInstruction(OpCode.NEQ);
                    break;

                case TokenType.GREATER:
                    chunk.AppendInstruction(OpCode.GT);
                    break;

                case TokenType.GREATER_EQUAL:
                    chunk.AppendInstruction(OpCode.GTE);
                    break;

                case TokenType.LESS:
                    chunk.AppendInstruction(OpCode.LT);
                    break;

                case TokenType.LESS_EQUAL:
                    chunk.AppendInstruction(OpCode.LTE);
                    break;

                /* We can live without these for now, let's do logical first
                case TokenType.BITWISE_AND:
                    chunk.AppendInstruction(OpCode.AND);
                    break;

                case TokenType.BITWISE_OR:
                    chunk.AppendInstruction(OpCode.OR);
                */
                default:
                    throw new NotImplementedException();
            }

            return chunk;
        }


        public object? Visit(LogicalExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            int shortcutLabel = nextLabel;
            int testCompleteLabel = nextLabel;

            switch (expr.op.type)
            {
                case TokenType.LOGICAL_AND:

                    chunk.AppendChunk(expr.left.Accept(this));

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.JMPFALSE,
                        operand1 = shortcutLabel
                    });

                    chunk.AppendChunk(expr.right.Accept(this));

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.JMP,
                        operand1 = testCompleteLabel
                    });

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.LABEL,
                        operand1 = shortcutLabel
                    });

                    // We arrived at this point from the shortcut, which had to be FALSE, and that Jump-not-true
                    // instruction popped the false result from the stack, so we need to put it back. I think a
                    // specific test instruction would make this nicer, but for now we can live with a few extra steps...

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.CONST,
                        operand1 = constants.FindIndex(e => e.type == SmolValueType.Bool && (bool)e.value! == false)
                    });

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.LABEL,
                        operand1 = testCompleteLabel
                    });


                    break;

                case TokenType.LOGICAL_OR:

                    chunk.AppendChunk(expr.left.Accept(this));

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.JMPTRUE,
                        operand1 = shortcutLabel
                    });

                    chunk.AppendChunk(expr.right.Accept(this));

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.JMP,
                        operand1 = testCompleteLabel
                    });

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.LABEL,
                        operand1 = shortcutLabel
                    });

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.CONST,
                        operand1 = constants.FindIndex(e => e.type == SmolValueType.Bool && (bool)e.value! == true)
                    });

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.LABEL,
                        operand1 = testCompleteLabel
                    });

                    break;
            }

            return chunk;
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
                opcode = OpCode.CONST,
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
                opcode = OpCode.FETCH,
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
            var chunk = new List<ByteCodeInstruction>();

            // Evalulate the arguments from left to right and pop them on the stack.

            foreach (var arg in expr.args)
            {
                // Maybe there's a better way to do this... feels a bit dumb
                switch (arg!.GetType().Name)
                {
                    case "LiteralExpression":
                        chunk.AppendChunk(((LiteralExpression)arg!).Accept(this));
                        break;

                    default:
                        throw new NotImplementedException($"Unable to process type '{arg!.GetType().Name}' as a function argument, we haven't implemented this yet");
                }
                
            }

            chunk.AppendChunk(expr.callee.Accept(this)); // Load the function name onto the stack

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.CALL,
                operand1 = expr.args.Count
            });

            return chunk;
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
            var chunk = new List<ByteCodeInstruction>();

            int startOfLoop = nextLabel;
            int notTrueLabel = nextLabel;

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.LABEL,
                operand1 = startOfLoop
            });

            chunk.AppendChunk(stmt.whileCondition.Accept(this));

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.JMPFALSE,
                operand1 = notTrueLabel
            });

            chunk.AppendChunk(stmt.executeStatement.Accept(this));

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.JMP,
                operand1 = startOfLoop
            });

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.LABEL,
                operand1 = notTrueLabel
            });

            return chunk;

        }

        public object? Visit(FunctionStatement stmt)
        {
            var function_index = function_bodies.Count() + 1;
            var function_name = stmt.name?.lexeme! ?? $"$_anon_{function_index}";

            function_table.Add(new SmolFunctionDefn()
            {
                globalFunctionName = function_name,
                code_section = function_index,
                arity = stmt.parameters.Count(),
                param_variable_names = stmt.parameters.Select(p => p.lexeme).ToList()
            });

            var body = (List<ByteCodeInstruction>)stmt.functionBody.Accept(this)!;

            body.AppendChunk(new ByteCodeInstruction()
            {
                // Not sure if this is the way to do this, seems like it should work.
                // Guess we can make it optional if the last statmeent of the body was already return?
                opcode = OpCode.RETURN
                // void?
            });

            function_bodies.Add(body);

            // We are declaring a function, we don't add anything to the byte stream at the current loc.
            // When we allow functions as expressions and assignments we'll need to do something
            // here, I guess something more like load constant but for functions
            return new ByteCodeInstruction()
            {
                opcode = OpCode.NOP
            };
        }
    }
}