using System.Text;

using SmolScript.Internals.Ast.Expressions;
using SmolScript.Internals.Ast.Statements;
using SmolScript.Internals.SmolStackTypes;
using SmolScript.Internals.SmolVariableTypes;

namespace SmolScript.Internals
{
    internal class Compiler : IExpressionVisitor, IStatementVisitor
    {
        private int _nextLabel = 1;

        private int ReserveLabelId()
        {
            return _nextLabel++;
        }

        private List<SmolFunction> _functionTable = new List<SmolFunction>();
        private List<List<ByteCodeInstruction>> _functionBodies = new List<List<ByteCodeInstruction>>();

        private List<SmolVariableType> _constants = new List<SmolVariableType>()
        {
            // There's an edge case that isn't handled by the method that adds constants,
            // becuase there's no natural analog for Undefined in the .net type system.
            // To make it simpler we pre-propulate the constant and then the parser/compiler
            // will just use it without any bother.
            new SmolUndefined()
        };

        private int ConstantIndexForValue(object constantLiteralValue)
        {
            var constantAsSmolType = SmolVariableType.Create(constantLiteralValue);

            // We used to have some real weirdness here comparing the value of our internal types which
            // I think is related to the way that we override equals on the SmolVariableType base class.
            // This approach looks a bit verbose, but it works fine and there's not really much need
            // to change it now.
            var existingConstantIndex = _constants.FindIndex(c => c.GetType() == constantAsSmolType.GetType() && (((SmolVariableType)c).GetValue()?.Equals(constantAsSmolType.GetValue()) ?? false)!);

            if (existingConstantIndex == -1)
            {
                _constants.Add(constantAsSmolType);

                return _constants.Count - 1; // It has to be the last item in the collection
            }
            else
            {
                return existingConstantIndex;
            }
        }

        private int ConstantIndexForUndefined()
        {
            return 0;
        }

        
        internal static SmolProgram Compile(string source)
        {
            var compiler = new Compiler();

            return compiler._Compile(source);
        }

        private SmolProgram _Compile(string source)
        {
            var scanner = new Scanner(source);
            var scanResult = scanner.ScanTokens();
            var parser = new Parser(scanResult);
            var statements = parser.Parse();

            // Creating the main chunk will populate the constants and build the function bodies too

            var mainChunk = new List<ByteCodeInstruction>();

            foreach (var stmt in statements)
            {
                var stmtChunk = new List<ByteCodeInstruction>();
                stmtChunk.AppendChunk(stmt.Accept(this));

                stmtChunk[0].IsStatementStartpoint = true;

                mainChunk.AppendChunk(stmtChunk);
            }

            mainChunk.AppendInstruction(OpCode.EOF);
         
            mainChunk[mainChunk.Count - 1].IsStatementStartpoint = true;

            var codeSections = new List<List<ByteCodeInstruction>>
            {
                mainChunk
            };

            codeSections.AddRange(_functionBodies);

            var prog = new SmolProgram()
            {
                Constants = _constants,
                CodeSections = codeSections,
                FunctionTable = _functionTable,
                Tokens = scanResult,
                Source = source
            };
            
            prog.BuildJumpTable();

            return prog;
        }

        public object? Visit(BinaryExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(expr.LeftExpression.Accept(this));
            chunk.AppendChunk(expr.RightExpression.Accept(this));

            switch (expr.BinaryOperator.Type)
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

            var shortcutLabel = ReserveLabelId();
            var testCompleteLabel = ReserveLabelId();

            switch (expr.Operator.Type)
            {
                case TokenType.LOGICAL_AND:

                    chunk.AppendChunk(expr.LeftExpression.Accept(this));
                    chunk.AppendInstruction(OpCode.JMPFALSE, operand1: shortcutLabel);
                    chunk.AppendChunk(expr.RightExpression.Accept(this));
                    chunk.AppendInstruction(OpCode.JMP, operand1: testCompleteLabel);
                    chunk.AppendInstruction(OpCode.LABEL, operand1: shortcutLabel);

                    // We arrived at this point from the shortcut, which had to be FALSE, and that Jump-not-true
                    // instruction popped the false result from the stack, so we need to put it back. I think a
                    // specific test instruction would make this nicer, but for now we can live with a few extra steps...

                    chunk.AppendInstruction(OpCode.CONST, operand1: ConstantIndexForValue(false));
                    chunk.AppendInstruction(OpCode.LABEL, operand1: testCompleteLabel);

                    break;

                case TokenType.LOGICAL_OR:

                    chunk.AppendChunk(expr.LeftExpression.Accept(this));
                    chunk.AppendInstruction(OpCode.JMPTRUE, shortcutLabel);
                    chunk.AppendChunk(expr.RightExpression.Accept(this));
                    chunk.AppendInstruction(OpCode.JMP, operand1: testCompleteLabel);
                    chunk.AppendInstruction(OpCode.LABEL, operand1: shortcutLabel);
                    chunk.AppendInstruction(OpCode.CONST, operand1: ConstantIndexForValue(true));
                    chunk.AppendInstruction(OpCode.LABEL, operand1: testCompleteLabel);

                    break;
            }

            return chunk;
        }

        public object? Visit(GroupingExpression expr)
        {
            return expr.GroupedExpression.Accept(this);
        }

        public object? Visit(LiteralExpression expr)
        {
            // Literal is always a constant value. The helper method looks to see if we already
            // have that constant (and if not adds it) and returns the index.

            return new ByteCodeInstruction()
            {
                opcode = OpCode.CONST,
                operand1 = ConstantIndexForValue(expr.Value)
            };
        }

        public object? Visit(UnaryExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            switch (expr.Operator.Type)
            {
                case TokenType.NOT:
                    {
                        chunk.AppendChunk(expr.RightExpression.Accept(this));

                        int isTrueLabel = ReserveLabelId();
                        int endLabel = ReserveLabelId();

                        chunk.AppendInstruction(OpCode.JMPTRUE, operand1: isTrueLabel);

                        // If we're here it was false, so now it's true
                        chunk.AppendInstruction(OpCode.CONST, operand1: ConstantIndexForValue(true));
                        chunk.AppendInstruction(OpCode.JMP, operand1: endLabel);
                        chunk.AppendInstruction(OpCode.LABEL, operand1: isTrueLabel);

                        // If we're here it was true, so now it's false
                        chunk.AppendInstruction(OpCode.CONST, operand1: ConstantIndexForValue(false));
                        chunk.AppendInstruction(OpCode.LABEL, operand1: endLabel);

                        break;
                    }

                case TokenType.MINUS:

                    // This block looks to see if the minus sign is followed by a literal number. If it is,
                    // we can create a constant for the negative number and load that instead of the more
                    // generalised unary operator behaviour, which negates whatever expression might come 
                    // after it in normal cirumstances.
                    if (expr.RightExpression.GetType() == typeof(LiteralExpression))
                    {
                        var l = (LiteralExpression)expr.RightExpression;

                        if (l.Value.GetType() == typeof(SmolNumber))
                        {
                            var n = (SmolNumber)l.Value;
                            chunk.AppendInstruction(OpCode.CONST, operand1: ConstantIndexForValue(0-n.NumberValue));

                            break;
                        }
                    }
                    
                    chunk.AppendInstruction(OpCode.CONST, operand1: ConstantIndexForValue(0.0));
                    chunk.AppendChunk(expr.RightExpression.Accept(this));
                    chunk.AppendInstruction(OpCode.SUB);

                    break;

            }

            return chunk;
        }

        public object? Visit(VariableExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendInstruction(OpCode.FETCH, operand1: expr.VariableName.Lexeme);

            if (expr.prepostfixop != null)
            {
                if (expr.prepostfixop == TokenType.POSTFIX_INCREMENT)
                {
                    chunk.AppendInstruction(OpCode.FETCH, operand1: expr.VariableName.Lexeme);
                    chunk.AppendInstruction(OpCode.CONST, operand1: ConstantIndexForValue(1.0));
                    chunk.AppendInstruction(OpCode.ADD);
                    chunk.AppendInstruction(OpCode.STORE, operand1: expr.VariableName.Lexeme);
                }

                if (expr.prepostfixop == TokenType.POSTFIX_DECREMENT)
                {
                    chunk.AppendInstruction(OpCode.FETCH, operand1: expr.VariableName.Lexeme);
                    chunk.AppendInstruction(OpCode.CONST, operand1: ConstantIndexForValue(1.0));
                    chunk.AppendInstruction(OpCode.SUB);
                    chunk.AppendInstruction(OpCode.STORE, operand1: expr.VariableName.Lexeme);
                }

                if (expr.prepostfixop == TokenType.PREFIX_INCREMENT)
                {
                    chunk.AppendInstruction(OpCode.CONST, operand1: ConstantIndexForValue(1.0));
                    chunk.AppendInstruction(OpCode.ADD);
                    chunk.AppendInstruction(OpCode.STORE, operand1: expr.VariableName.Lexeme);
                    chunk.AppendInstruction(OpCode.FETCH, operand1: expr.VariableName.Lexeme);
                }

                if (expr.prepostfixop == TokenType.PREFIX_DECREMENT)
                {
                    chunk.AppendInstruction(OpCode.CONST, operand1: ConstantIndexForValue(1.0));
                    chunk.AppendInstruction(OpCode.SUB);
                    chunk.AppendInstruction(OpCode.STORE, operand1: expr.VariableName.Lexeme);
                    chunk.AppendInstruction(OpCode.FETCH, operand1: expr.VariableName.Lexeme);
                }
            }

            return chunk;
        }

        public object? Visit(AssignExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(expr.ValueExpression.Accept(this));

            chunk.AppendInstruction(OpCode.STORE, operand1: expr.VariableName.Lexeme);

            // This is so inefficient

            chunk.AppendInstruction(OpCode.FETCH, operand1: expr.VariableName.Lexeme);

            return chunk;
        }

        public object? Visit(CallExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            // Evalulate the arguments from left to right and pop them on the stack.

            foreach (var arg in expr.Arguments.Reverse())
            {
                chunk.AppendChunk(((Expression)arg!).Accept(this));
            }

            chunk.AppendChunk(expr.Callee.Accept(this)); // Load the function name onto the stack
            chunk.AppendInstruction(OpCode.CALL, operand1: expr.Arguments.Count, operand2: expr.UseFetchedObjectEnvironment);

            return chunk;
        }

        public object? Visit(VarStatement stmt)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendInstruction(OpCode.DECLARE, operand1: stmt.name.Lexeme);

            if (stmt.initializerExpression != null)
            {                
                chunk.AppendChunk(stmt.initializerExpression.Accept(this));
                chunk.AppendInstruction(OpCode.STORE, operand1: stmt.name.Lexeme);
            }

            chunk.MapTokens(stmt.firstTokenIndex, stmt.lastTokenIndex);

            return chunk;
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
                var c = blockStmt.Accept(this);
                var x = chunk.First();
                x.IsStatementStartpoint = true;
                chunk[0] = x;
                chunk.AppendChunk(c);
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
                chunk.AppendInstruction(OpCode.CONST, ConstantIndexForUndefined());
            }

            chunk.AppendInstruction(OpCode.RETURN);

            return chunk;
        }

        public object? Visit(IfStatement stmt)
        {
            var chunk = new List<ByteCodeInstruction>();

            int notTrueLabel = ReserveLabelId();

            chunk.AppendChunk(stmt.testExpression.Accept(this));
            chunk.AppendInstruction(OpCode.JMPFALSE, notTrueLabel);
            chunk.AppendChunk(stmt.thenStatement.Accept(this));

            if (stmt.elseStatement == null)
            {
                chunk.AppendInstruction(OpCode.LABEL, notTrueLabel);
            }
            else
            {
                int skipElseLabel = ReserveLabelId();

                chunk.AppendInstruction(OpCode.JMP, skipElseLabel);
                chunk.AppendInstruction(OpCode.LABEL, notTrueLabel);
                chunk.AppendChunk(stmt.elseStatement!.Accept(this));
                chunk.AppendInstruction(OpCode.LABEL, skipElseLabel);
            }

            return chunk;
        }

        public object? Visit(TernaryExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            var notTrueLabel = ReserveLabelId();
            var endLabel = ReserveLabelId();

            chunk.AppendChunk(expr.EvaluationExpression.Accept(this));
            chunk.AppendInstruction(OpCode.JMPFALSE, notTrueLabel);
            chunk.AppendChunk(expr.TrueValue.Accept(this));
            chunk.AppendInstruction(OpCode.JMP, endLabel);
            chunk.AppendInstruction(OpCode.LABEL, notTrueLabel);
            chunk.AppendChunk(expr.FalseValue.Accept(this));
            chunk.AppendInstruction(OpCode.LABEL, endLabel);

            return chunk;
        }


        private struct WhileLoop
        {
            public int StartOfLoop;
            public int EndOfLoop;
        }

        private Stack<WhileLoop> _loopStack = new Stack<WhileLoop>();

        public object? Visit(WhileStatement stmt)
        {
            var chunk = new List<ByteCodeInstruction>();

            var startOfLoop = ReserveLabelId();
            var endOfLoop = ReserveLabelId();

            _loopStack.Push(new WhileLoop() { StartOfLoop = startOfLoop, EndOfLoop = endOfLoop });

            chunk.AppendInstruction(OpCode.LOOP_START);
            chunk.AppendInstruction(OpCode.LABEL, startOfLoop);
            chunk.AppendChunk(stmt.whileCondition.Accept(this));
            chunk.AppendInstruction(OpCode.JMPFALSE, endOfLoop);
            chunk.AppendChunk(stmt.executeStatement.Accept(this));
            chunk.AppendInstruction(OpCode.JMP, startOfLoop);
            chunk.AppendInstruction(OpCode.LABEL, endOfLoop);
            chunk.AppendInstruction(OpCode.LOOP_END);

            _loopStack.Pop();

            return chunk;
        }

        public object? Visit(BreakStatement stmt)
        {
            return new ByteCodeInstruction()
            {
                opcode = OpCode.LOOP_EXIT,
                operand1 = _loopStack.Peek().EndOfLoop
            };
        }

        public object? Visit(ContinueStatement stmt)
        {
            return new ByteCodeInstruction()
            {
                opcode = OpCode.LOOP_EXIT,
                operand1 = _loopStack.Peek().StartOfLoop
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

            var exceptionLabel = ReserveLabelId();
            var finallyLabel = ReserveLabelId();
            var finallyWithExceptionLabel = ReserveLabelId();

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

            chunk.AppendInstruction(OpCode.JMP, operand1: finallyLabel);
            chunk.AppendInstruction(OpCode.LABEL, operand1: exceptionLabel);

            // We're now at the catch part -- even if the user didn't specify one, we'll have a default (of { throw })
            // We now should have the thrown exception on the stack, so if a throw happens inside the catch that will
            // be the thing that's thrown.

            chunk.AppendInstruction(OpCode.TRY, operand1: finallyWithExceptionLabel, operand2: true); // True means keep the exception at the top of the stack

            if (stmt.catchBody != null)
            {
                if (stmt.exceptionVariableName != null)
                {
                    chunk.AppendInstruction(OpCode.ENTER_SCOPE);

                    // Top of stack will be exception so store it in variable name

                    chunk.AppendInstruction(OpCode.DECLARE, operand1: stmt.exceptionVariableName.Lexeme);
                    chunk.AppendInstruction(OpCode.STORE, operand1: stmt.exceptionVariableName.Lexeme);
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
            var functionIndex = _functionBodies.Count() + 1;
            var functionName = stmt.name?.Lexeme! ?? $"$_anon_{functionIndex}";

            _functionTable.Add(new SmolFunction(
                globalFunctionName: functionName,
                codeSection: functionIndex,
                arity: stmt.parameters.Count(),
                paramVariableNames: stmt.parameters.Select(p => p.Lexeme).ToList()
            ));

            // Reserve the function body so if we're 
            _functionBodies.Add(new List<ByteCodeInstruction>());
            
            var body = (List<ByteCodeInstruction>)stmt.functionBody.Accept(this)!;

            if (!body.Any() || body.Last().opcode != OpCode.RETURN)
            {
                body.Add(new ByteCodeInstruction()
                {
                    opcode = OpCode.CONST,
                    operand1 = ConstantIndexForUndefined()
                });
                body.AppendInstruction(OpCode.RETURN);
            }

            _functionBodies[functionIndex-1] = body;

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
                var functionIndex = _functionBodies.Count() + 1;
                var functionName = $"@{stmt.className.Lexeme}.{fn.name!.Lexeme}";

                _functionTable.Add(new SmolFunction(
                    globalFunctionName: functionName,
                    codeSection: functionIndex,
                    arity: fn.parameters.Count(),
                    paramVariableNames: fn.parameters.Select(p => p.Lexeme).ToList()
                ));

                var body = (List<ByteCodeInstruction>)fn.functionBody.Accept(this)!;

                if (!body.Any() || body.Last().opcode != OpCode.RETURN)
                {
                    body.AppendInstruction(OpCode.CONST, operand1: ConstantIndexForUndefined());
                    body.AppendInstruction(OpCode.RETURN);
                }

                _functionBodies.Add(body);
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

            var className = expr.ClassName.Lexeme;

            // We need to tell the VM that we want to create an instance of a class.
            // It will need its own environment, and the instance info needs to be on the stack
            // so we can call the ctor, which needs to leave it on the stack afterwards
            // ready for whatever was wanting it in the first place
            chunk.AppendInstruction(OpCode.CREATE_OBJECT, operand1: className);

            if (className != "Object")
            {
                foreach (Expression arg in expr.ConstructorArgs.Reverse())
                {
                    chunk.AppendChunk(arg.Accept(this));
                }

                chunk.AppendInstruction(OpCode.DUPLICATE_VALUE, operand1: expr.ConstructorArgs.Count); // We need two copies of that ref
            }
            else
            {
                chunk.AppendInstruction(OpCode.DUPLICATE_VALUE, operand1: 0); // We need two copies of that ref
            }


            // Stack now has class instance value

            chunk.AppendChunk(new ByteCodeInstruction()
            {
                opcode = OpCode.FETCH,
                operand1 = $"@{expr.ClassName.Lexeme}.constructor",
                operand2 = true
            });

            if (className == "Object")
            {
                foreach (Expression arg in expr.ConstructorArgs.Reverse())
                {
                    chunk.AppendChunk(arg.Accept(this));
                }
            }

            chunk.AppendInstruction(OpCode.CALL,
                operand1: expr.ConstructorArgs.Count,
                operand2: true // means use instance that's on stack -- might not use this in the end because I think we should be able to tell from class name it's an instance call...
            );

            chunk.AppendInstruction(OpCode.POP_AND_DISCARD); // We don't care about the ctor's return value

            return chunk;
        }

        public object? Visit(GetExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(expr.TargetObject.Accept(this));
            chunk.AppendInstruction(OpCode.FETCH, operand1: expr.AttributeName.Lexeme, operand2: true);

            // Who knows if this will work... :)

            return chunk;
        }

        public object? Visit(IndexerGetExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(expr.TargetObject.Accept(this));
            chunk.AppendChunk(expr.IndexerExpression.Accept(this));

            // Now on the stack we have an expression result which is
            // the index value we want to get, so we have a special
            // way to call fetch that knows to get that value and use
            // it as a property

            chunk.AppendInstruction(OpCode.FETCH, operand1: "@IndexerGet", operand2: true);

            // Who knows if this will work... :)

            return chunk;
        }

        public object? Visit(SetExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(expr.TargetObject.Accept(this));

            chunk.AppendChunk(expr.Value.Accept(this));

            chunk.AppendInstruction(OpCode.STORE, operand1: expr.AttributeName.Lexeme, operand2: true); // true means object reference on stack

            // This is so inefficient, but we need to read the saved value back onto the stack

            chunk.AppendChunk(expr.TargetObject.Accept(this));

            chunk.AppendInstruction(OpCode.FETCH, operand1: expr.AttributeName.Lexeme, operand2: true);

            return chunk;
        }

        public object? Visit(ObjectInitializerExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendInstruction(OpCode.DUPLICATE_VALUE, operand1: 2);
            chunk.AppendChunk(expr.Value.Accept(this));
            chunk.AppendInstruction(OpCode.STORE, expr.ObjectName.Lexeme, true); // true means object reference on stack

            // We don't reload the value onto the stack for these...

            return chunk;
        }

        public object? Visit(IndexerSetExpression expr)
        {
            var chunk = new List<ByteCodeInstruction>();

            chunk.AppendChunk(expr.TargetObject.Accept(this));
            chunk.AppendChunk(expr.Value.Accept(this));
            chunk.AppendChunk(expr.IndexerExpression.Accept(this));
            chunk.AppendInstruction(OpCode.STORE, "@IndexerSet", true); // true means object reference on stack

            // This is so inefficient, but we need to read the saved value back onto the stack

            chunk.AppendChunk(expr.TargetObject.Accept(this));

            // TODO: This won't even work for indexer++ etc.
            chunk.AppendChunk(expr.IndexerExpression.Accept(this));
            chunk.AppendInstruction(OpCode.FETCH, "@IndexerGet", true);

            return chunk;
        }

        public object? Visit(FunctionExpression expr)
        {
            var functionIndex = _functionBodies.Count() + 1;
            var functionName = $"$_anon_{functionIndex}";

            _functionTable.Add(new SmolFunction(
                globalFunctionName: functionName,
                codeSection: functionIndex,
                arity: expr.Parameters.Count(),
                paramVariableNames: expr.Parameters.Select(p => p.Lexeme).ToList()
            ));

            var body = (List<ByteCodeInstruction>)expr.FunctionBody.Accept(this)!;

            if (!body.Any() || body.Last().opcode != OpCode.RETURN)
            {
                body.AppendInstruction(OpCode.CONST, operand1: ConstantIndexForUndefined());
                body.AppendInstruction(OpCode.RETURN);
            }

            _functionBodies.Add(body);

            // We are declaring a function expression, so the reference to the function needs
            // to go on the stack so some other code can grab and use it
            return (new ByteCodeInstruction()
            {
                opcode = OpCode.FETCH,
                operand1 = functionName
            });
        }
    }
}