using System.Text;

using SmolScript.Internals.Ast.Expressions;
using SmolScript.Internals.Ast.Statements;
using SmolScript.Internals.SmolStackTypes;
using SmolScript.Internals.SmolVariableTypes;

namespace SmolScript.Internals
{
    internal class SmolCompiler : IExpressionVisitor, IStatementVisitor
    {
        private int _nextLabel = 1;

        private int reserveLabelId()
        {
            return _nextLabel++;
        }

        private List<SmolFunction> function_table = new List<SmolFunction>();
        private List<List<ByteCodeInstruction>> function_bodies = new List<List<ByteCodeInstruction>>();

        private List<SmolVariableType> constants = new List<SmolVariableType>()
        {
            new SmolBool(true),
            new SmolBool(false),
            new SmolNumber(0.0),
            new SmolNumber(1.0),
            new SmolUndefined(),
            new SmolNull()
        };

        private List<ByteCodeInstruction> EmitChunk(IList<Statement> stmts)
        {

            var chunk = new List<ByteCodeInstruction>();

            foreach (var stmt in stmts)
            {
                chunk.AppendChunk(stmt.Accept(this));

                var c = chunk.Last();
                c.StepCheckpoint = true;
                chunk[chunk.Count() - 1] = c;
            }

            return chunk;
        }

        internal static SmolProgram Compile(string source)
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

            foreach (var fb in function_bodies)
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

                case TokenType.BITWISE_AND:
                    chunk.AppendInstruction(OpCode.BITWISE_AND);
                    break;

                case TokenType.BITWISE_OR:
                    chunk.AppendInstruction(OpCode.BITWISE_OR);
                    break;

                default:
                    throw new NotImplementedException();
            }

            return chunk;
        }


        public object? Visit(LogicalExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            int shortcutLabel = reserveLabelId();
            int testCompleteLabel = reserveLabelId();

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
                        operand1 = constants.FindIndex(e => e.GetType() == typeof(SmolBool) && (bool)((SmolVariableType)e).GetValue()! == false)
                    });

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.LABEL,
                        operand1 = testCompleteLabel
                    });


                    break;

                case TokenType.LOGICAL_OR:

                    chunk.AppendChunk(expr.left.Accept(this));

                    chunk.AppendInstruction(OpCode.JMPTRUE, shortcutLabel);

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
                        operand1 = constants.FindIndex(e => e.GetType() == typeof(SmolBool) && (bool)((SmolVariableType)e).GetValue()! == true)
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

            var value = SmolVariableType.Create(expr.value);

            // Todo: Got some real weirdness with comparison of SmolStackTypes, not sure what I've done here..
            var cIndex = constants.FindIndex(c => c.GetType() == value.GetType() && (((SmolVariableType)c).GetValue()?.Equals(value.GetValue()) ?? false)!);

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
            var chunk = new List<ByteCodeInstruction>();

            switch (expr.op.type)
            {
                case TokenType.NOT:
                    {
                        chunk.AppendChunk(expr.right.Accept(this));

                        int isTrueLabel = reserveLabelId();
                        int endLabel = reserveLabelId();

                        chunk.AppendChunk(new ByteCodeInstruction()
                        {
                            opcode = OpCode.JMPTRUE,
                            operand1 = isTrueLabel
                        });

                        // If we're here it was false, so now it's true
                        chunk.AppendChunk(new ByteCodeInstruction()
                        {
                            opcode = OpCode.CONST,
                            operand1 = constants.FindIndex(e => e.GetType() == typeof(SmolBool) && (bool)((SmolVariableType)e).GetValue()! == true)
                        });

                        chunk.AppendChunk(new ByteCodeInstruction()
                        {
                            opcode = OpCode.JMP,
                            operand1 = endLabel
                        });

                        chunk.AppendChunk(new ByteCodeInstruction()
                        {
                            opcode = OpCode.LABEL,
                            operand1 = isTrueLabel
                        });

                        // If we're here it was true, so now it's false
                        chunk.AppendChunk(new ByteCodeInstruction()
                        {
                            opcode = OpCode.CONST,
                            operand1 = constants.FindIndex(e => e.GetType() == typeof(SmolBool) && (bool)((SmolVariableType)e).GetValue()! == false)
                        });

                        chunk.AppendChunk(new ByteCodeInstruction()
                        {
                            opcode = OpCode.LABEL,
                            operand1 = endLabel
                        });

                        break;
                    }

                case TokenType.MINUS:

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.CONST,
                        operand1 = constants.FindIndex(e => e.GetType() == typeof(SmolNumber) && (double)((SmolVariableType)e).GetValue()! == 0.0)
                    });

                    chunk.AppendChunk(expr.right.Accept(this));

                    chunk.AppendInstruction(OpCode.SUB);

                    break;

            }

            return chunk;
        }

        public object? Visit(VariableExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.FETCH,
                operand1 = (string)expr.name.lexeme                
            });

            if (expr.prepostfixop != null)
            {
                if (expr.prepostfixop == TokenType.POSTFIX_INCREMENT)
                {
                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.FETCH,
                        operand1 = (string)expr.name.lexeme
                    });

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.CONST,
                        operand1 = constants.FindIndex(e => e.GetType() == typeof(SmolNumber) && (double)((SmolVariableType)e).GetValue()! == 1.0)
                    });

                    chunk.AppendInstruction(OpCode.ADD);

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.STORE,
                        operand1 = (string)expr.name.lexeme
                    });
                }

                if (expr.prepostfixop == TokenType.POSTFIX_DECREMENT)
                {
                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.FETCH,
                        operand1 = (string)expr.name.lexeme
                    });

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.CONST,
                        operand1 = constants.FindIndex(e => e.GetType() == typeof(SmolNumber) && (double)((SmolVariableType)e).GetValue()! == 1.0)
                    });

                    chunk.AppendInstruction(OpCode.SUB);

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.STORE,
                        operand1 = (string)expr.name.lexeme
                    });
                }

                if (expr.prepostfixop == TokenType.PREFIX_INCREMENT)
                {
                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.CONST,
                        operand1 = constants.FindIndex(e => e.GetType() == typeof(SmolNumber) && (double)((SmolVariableType)e).GetValue()! == 1.0)
                    });

                    chunk.AppendInstruction(OpCode.ADD);

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.STORE,
                        operand1 = (string)expr.name.lexeme
                    });

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.FETCH,
                        operand1 = (string)expr.name.lexeme
                    });
                }

                if (expr.prepostfixop == TokenType.PREFIX_DECREMENT)
                {
                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.CONST,
                        operand1 = constants.FindIndex(e => e.GetType() == typeof(SmolNumber) && (double)((SmolVariableType)e).GetValue()! == 1.0)
                    });

                    chunk.AppendInstruction(OpCode.SUB);

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.STORE,
                        operand1 = (string)expr.name.lexeme
                    });

                    chunk.AppendChunk(new ByteCodeInstruction()
                    {
                        opcode = OpCode.FETCH,
                        operand1 = (string)expr.name.lexeme
                    });
                }
            }

            return chunk;
        }

        public object? Visit(AssignExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(expr.value.Accept(this));

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.STORE,
                operand1 = (string)expr.name.lexeme
            });

            // This is so inefficient

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.FETCH,
                operand1 = (string)expr.name.lexeme
            });

            return chunk;
        }

        public object? Visit(CallExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            // Evalulate the arguments from left to right and pop them on the stack.

            foreach (var arg in expr.args.Reverse())
            {
                chunk.AppendChunk(((Expression)arg!).Accept(this));
            }

            chunk.AppendChunk(expr.callee.Accept(this)); // Load the function name onto the stack

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.CALL,
                operand1 = expr.args.Count,
                operand2 = expr.useObjectRef
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
                    operand1 = (string)stmt.name.lexeme
                });

                chunk.AppendChunk(stmt.initializerExpression.Accept(this));

                chunk.AppendChunk(new ByteCodeInstruction()
                {
                    opcode = OpCode.STORE,
                    operand1 = (string)stmt.name.lexeme
                });

                return chunk;
            }
            else
            {
                return new ByteCodeInstruction()
                {
                    opcode = OpCode.DECLARE,
                    operand1 = (string)stmt.name.lexeme
                };
            }
        }

        public object? Visit(ExpressionStatement stmt)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(stmt.expression.Accept(this));
            chunk.AppendInstruction(OpCode.POP_AND_DISCARD);

            return chunk;
        }

        public object? Visit(BlockStatement stmt)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendInstruction(OpCode.ENTER_SCOPE);

            foreach (var blockStmt in stmt.statements)
            {
                chunk.AppendChunk(blockStmt.Accept(this));
            }

            chunk.AppendInstruction(OpCode.LEAVE_SCOPE);

            return chunk;
        }

        public object? Visit(PrintStatement stmt)
        {
            var chunk = new List<ByteCodeInstruction>();
            chunk.AppendChunk(stmt.expression.Accept(this));
            chunk.AppendInstruction(OpCode.PRINT);

            return chunk;
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
                chunk.AppendChunk(new ByteCodeInstruction()
                {
                    opcode = OpCode.CONST,
                    operand1 = constants.FindIndex(e => e.GetType() == typeof(SmolUndefined))
                });
            }

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.RETURN
            });

            return chunk;
        }

        public object? Visit(IfStatement stmt)
        {
            var chunk = new List<ByteCodeInstruction>();

            int notTrueLabel = reserveLabelId();

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
                int skipElseLabel = reserveLabelId();

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

        public object? Visit(TernaryExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            int notTrueLabel = reserveLabelId();
            int endLabel = reserveLabelId();

            chunk.AppendChunk(expr.evaluationExpression.Accept(this));

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.JMPFALSE,
                operand1 = notTrueLabel
            });

            chunk.AppendChunk(expr.expressionIfTrue.Accept(this));

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.JMP,
                operand1 = endLabel
            });

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.LABEL,
                operand1 = notTrueLabel
            });

            chunk.AppendChunk(expr.expresisonIfFalse.Accept(this));

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.LABEL,
                operand1 = endLabel
            });

            return chunk;
        }


        private struct WhileLoop
        {
            public int startOfLoop;
            public int endOfLoop;
        }

        private Stack<WhileLoop> loopStack = new Stack<WhileLoop>();

        public object? Visit(WhileStatement stmt)
        {
            var chunk = new List<ByteCodeInstruction>();

            int startOfLoop = reserveLabelId();
            int endOfLoop = reserveLabelId();

            loopStack.Push(new WhileLoop() { startOfLoop = startOfLoop, endOfLoop = endOfLoop });

            chunk.AppendInstruction(OpCode.LOOP_START);

            chunk.AppendInstruction(OpCode.LABEL, startOfLoop);

            chunk.AppendChunk(stmt.whileCondition.Accept(this));

            chunk.AppendInstruction(OpCode.JMPFALSE, endOfLoop);

            chunk.AppendChunk(stmt.executeStatement.Accept(this));

            chunk.AppendInstruction(OpCode.JMP, startOfLoop);

            chunk.AppendInstruction(OpCode.LABEL, endOfLoop);

            chunk.AppendInstruction(OpCode.LOOP_END);

            loopStack.Pop();

            return chunk;
        }

        public object? Visit(BreakStatement stmt)
        {
            return new ByteCodeInstruction()
            {
                opcode = OpCode.LOOP_EXIT,
                operand1 = loopStack.Peek().endOfLoop
            };
        }

        public object? Visit(ContinueStatement stmt)
        {
            return new ByteCodeInstruction()
            {
                opcode = OpCode.LOOP_EXIT,
                operand1 = loopStack.Peek().startOfLoop
            };
        }

        public object? Visit(ThrowStatement stmt)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(stmt.expression.Accept(this));
            chunk.AppendInstruction(OpCode.THROW);
            
            return chunk;
        }

        public object? Visit(TryStatement stmt)
        {
            var chunk = new List<ByteCodeInstruction>();

            var exceptionLabel = reserveLabelId();
            var finallyLabel = reserveLabelId();
            var finallyWithExceptionLabel = reserveLabelId();

            // This will create a try 'checkpoint' in the vm. If we hit an exception the
            // vm will rewind the stack back to this instruction and jump to the catch/finally.
            chunk.AppendInstruction(OpCode.TRY, exceptionLabel, false);

            // If an exception happens inside the body, it will rewind the stack to the try that just went on
            // and that tells us where to jump to

            chunk.AppendChunk(this.Visit(stmt.tryBody));

            // If there was no exception, we need to get rid of that try checkpoint that's on the stack, we aren't
            // going back there even if there's an exception in the finally

            chunk.AppendInstruction(OpCode.POP_AND_DISCARD);

            // Now execute the finally

            chunk.AppendInstruction(OpCode.JMP, finallyLabel);

            chunk.AppendInstruction(OpCode.LABEL, exceptionLabel);

            // We're now at the catch part -- even if the user didn't specify one, we'll have a default (of { throw })
            // We now should have the thrown exception on the stack, so if a throw happens inside the catch that will
            // be the thing that's thrown.

            chunk.AppendInstruction(OpCode.TRY, finallyWithExceptionLabel, true); // True means keep the exception at the top of the stack

            if (stmt.catchBody != null)
            {
                if (stmt.exceptionVariableName != null)
                {
                    chunk.AppendInstruction(OpCode.ENTER_SCOPE);

                    // Top of stack will be exception so store it in variable name

                    chunk.AppendInstruction(OpCode.DECLARE, (string)stmt.exceptionVariableName.lexeme);

                    chunk.AppendInstruction(OpCode.STORE, (string)stmt.exceptionVariableName.lexeme);
                }
                else
                {
                    // Top of stack is exception, but no variable defined to hold it so get rid of it
                    chunk.AppendInstruction(OpCode.POP_AND_DISCARD);
                }

                chunk.AppendChunk(this.Visit(stmt.catchBody!)); // Might be a throw inside here...

                if (stmt.exceptionVariableName != null)
                {
                    chunk.AppendInstruction(OpCode.LEAVE_SCOPE);
                }
            }
            else
            {
                // No catch body is replaced by single instruction to rethrow the exception, which is already on the top of the stack

                chunk.AppendInstruction(OpCode.THROW);
            }

            // If we made it here we got through the catch block without a throw, so we're free to execute the regular
            // finally and carry on with execution, exception is fully handled.

            // Top of stack has to the try checkpoint, so get rid of it because we aren't going back there
            chunk.AppendInstruction(OpCode.POP_AND_DISCARD);

            chunk.AppendInstruction(OpCode.JMP, finallyLabel);

            chunk.AppendInstruction(OpCode.LABEL, finallyWithExceptionLabel);

            // If we're here then we had a throw inside the catch, so execute the finally and then throw it again.
            // When we throw this time the try checkpoint has been removed so we'll bubble down the stack to the next
            // try checkpoint (if there is one -- and panic if not)

            if (stmt.finallyBody != null)
            {
                chunk.AppendChunk(this.Visit(stmt.finallyBody));

                // Instruction to check for unthrown exception and throw it
            }

            chunk.AppendInstruction(OpCode.THROW);

            chunk.AppendInstruction(OpCode.LABEL, finallyLabel);

            if (stmt.finallyBody != null)
            {
                chunk.AppendChunk(this.Visit(stmt.finallyBody));

                // Instruction to check for unthrown exception and throw it
            }

            // Hopefully that all works. It's mega dependent on the instructions leaving the stack in a pristine state -- no
            // half finished evaluations or anything. That's definitely going to be a problem.

            return chunk;
        }

        public object? Visit(FunctionStatement stmt)
        {
            var function_index = function_bodies.Count() + 1;
            var function_name = stmt.name?.lexeme! ?? $"$_anon_{function_index}";

            function_table.Add(new SmolFunction(
                global_function_name: function_name,
                code_section: function_index,
                arity: stmt.parameters.Count(),
                param_variable_names: stmt.parameters.Select(p => p.lexeme).ToList()
            ));

            var body = (List<ByteCodeInstruction>)stmt.functionBody.Accept(this)!;

            if (!body.Any() || body.Last().opcode != OpCode.RETURN)
            {
                body.Add(new ByteCodeInstruction()
                {
                    opcode = OpCode.CONST,
                    operand1 = constants.FindIndex(e => e.GetType() == typeof(SmolUndefined))
                });
                body.AppendInstruction(OpCode.RETURN);
            }

            function_bodies.Add(body);

            // We are declaring a function, we don't add anything to the byte stream at the current loc.
            // When we allow functions as expressions and assignments we'll need to do something
            // here, I guess something more like load constant but for functions
            return new ByteCodeInstruction()
            {
                opcode = OpCode.NOP
            };
        }

        public object? Visit(DebuggerStatement stmt)
        {
            return new ByteCodeInstruction()
            {
                opcode = OpCode.DEBUGGER
            };
        }

        public object? Visit(ClassStatement stmt)
        {
            //this.class_table.Add(stmt.className.lexeme, stmt.superclassName?.lexeme);

            foreach (var fn in stmt.functions)
            {
                var function_index = function_bodies.Count() + 1;
                var function_name = $"@{stmt.className.lexeme}.{fn.name!.lexeme}";

                function_table.Add(new SmolFunction(
                    global_function_name: function_name,
                    code_section: function_index,
                    arity: fn.parameters.Count(),
                    param_variable_names: fn.parameters.Select(p => p.lexeme).ToList()
                ));

                var body = (List<ByteCodeInstruction>)fn.functionBody.Accept(this)!;

                if (!body.Any() || body.Last().opcode != OpCode.RETURN)
                {
                    body.Add(new ByteCodeInstruction()
                    {
                        opcode = OpCode.CONST,
                        operand1 = constants.FindIndex(e => e.GetType() == typeof(SmolUndefined))
                    });
                    body.AppendInstruction(OpCode.RETURN);
                }

                function_bodies.Add(body);
            }

            // We are declaring a function, we don't add anything to the byte stream at the current loc.
            // When we allow functions as expressions and assignments we'll need to do something
            // here, I guess something more like load constant but for functions
            return new ByteCodeInstruction()
            {
                opcode = OpCode.NOP
            };
        }

        public object? Visit(NewInstanceExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            var className = (string)(expr.className.lexeme);

            // We need to tell the VM that we want to create an instance of a class.
            // It will need its own environment, and the instance info needs to be on the stack
            // so we can call the ctor, which needs to leave it on the stack afterwards
            // ready for whatever was wanting it in the first place
            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.CREATE_OBJECT,
                operand1 = className
            });

            if (className != "Object")
            {
                foreach (Expression arg in expr.ctorArgs.Reverse())
                {
                    chunk.AppendChunk(arg.Accept(this));
                }

                chunk.AppendInstruction(OpCode.DUPLICATE_VALUE, operand1: expr.ctorArgs.Count); // We need two copies of that ref
            }
            else
            {
                chunk.AppendInstruction(OpCode.DUPLICATE_VALUE, operand1: 0); // We need two copies of that ref
            }


            // Stack now has class instance value

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.FETCH,
                operand1 = $"@{expr.className.lexeme}.constructor",
                operand2 = true
            });

            if (className == "Object")
            {
                foreach (Expression arg in expr.ctorArgs.Reverse())
                {
                    chunk.AppendChunk(arg.Accept(this));
                }
            }

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.CALL,
                operand1 = expr.ctorArgs.Count,
                operand2 = true // means use instance that's on stack -- might not use this in the end because I think we should be able to tell from class name it's an instance call...
            });

            chunk.AppendInstruction(OpCode.POP_AND_DISCARD); // We don't care about the ctor's return value

            return chunk;
        }

        public object? Visit(GetExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(expr.obj.Accept(this));

            chunk.AppendInstruction(OpCode.FETCH,
                operand1: expr.name.lexeme,
                operand2: true);

            // Who knows if this will work... :)

            return chunk;
        }

        public object? Visit(IndexerGetExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(expr.obj.Accept(this));

            chunk.AppendChunk(expr.indexerExpr.Accept(this));

            // Now on the stack we have an expression result which is
            // the index value we want to get, so we have a special
            // way to call fetch that knows to get that value and use
            // it as a property

            chunk.AppendInstruction(OpCode.FETCH,
                operand1: "@IndexerGet",
                operand2: true);

            // Who knows if this will work... :)

            return chunk;
        }

        public object? Visit(SetExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(expr.obj.Accept(this));

            chunk.AppendChunk(expr.value.Accept(this));

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.STORE,
                operand1 = (string)expr.name.lexeme,
                operand2 = true // Means object reference on stack
            });

            // This is so inefficient, but we need to read the saved value back onto the stack

            chunk.AppendChunk(expr.obj.Accept(this));

            chunk.AppendInstruction(OpCode.FETCH, expr.name.lexeme, true);

            return chunk;
        }

        public object? Visit(ObjectInitializerExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendInstruction(OpCode.DUPLICATE_VALUE, operand1: 2);

            chunk.AppendChunk(expr.value.Accept(this));

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.STORE,
                operand1 = expr.name.lexeme,
                operand2 = true // Means object reference on stack
            });

            // We don't reload the value onto the stack for these...

            return chunk;
        }

        public object? Visit(IndexerSetExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(expr.obj.Accept(this));

            chunk.AppendChunk(expr.value.Accept(this));

            chunk.AppendChunk(expr.indexerExpr.Accept(this));

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.STORE,
                operand1 = "@IndexerSet",
                operand2 = true // Means object reference on stack
            });

            // This is so inefficient, but we need to read the saved value back onto the stack

            chunk.AppendChunk(expr.obj.Accept(this));

            // TODO: This won't even work for indexer++ etc.
            chunk.AppendChunk(expr.indexerExpr.Accept(this));

            chunk.AppendInstruction(OpCode.FETCH, "@IndexerGet", true);

            return chunk;
        }

        public object? Visit(FunctionExpression expr)
        {
            var function_index = function_bodies.Count() + 1;
            var function_name = $"$_anon_{function_index}";

            function_table.Add(new SmolFunction(
                global_function_name: function_name,
                code_section: function_index,
                arity: expr.parameters.Count(),
                param_variable_names: expr.parameters.Select(p => p.lexeme).ToList()
            ));

            var body = (List<ByteCodeInstruction>)expr.functionBody.Accept(this)!;

            if (!body.Any() || body.Last().opcode != OpCode.RETURN)
            {
                body.Add(new ByteCodeInstruction()
                {
                    opcode = OpCode.CONST,
                    operand1 = constants.FindIndex(e => e.GetType() == typeof(SmolUndefined))
                });
                body.AppendInstruction(OpCode.RETURN);
            }

            function_bodies.Add(body);

            // We are declaring a function expression, so the reference to the function needs
            // to go on the stack so some other code can grab and use it
            return (new ByteCodeInstruction()
            {
                opcode = OpCode.FETCH,
                operand1 = function_name
            });
        }
    }
}