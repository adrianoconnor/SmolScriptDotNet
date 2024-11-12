using System.Text;
using SmolScript.Internals.Ast.Expressions;
using SmolScript.Internals.Ast.Statements;

namespace SmolScript.Internals.Ast
{
    internal class AstDump : IExpressionVisitor, IStatementVisitor
    {
        string _newline = System.Environment.NewLine;
        private int _depth = 0;
        private string Indent
        {
            get { return "".PadLeft(_depth * 2); }
        }

        private void Enter()
        {
            _depth++;
        }

        private void Leave()
        {
            _depth--;
        }

        public string? Print(IList<Statement> stmts)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var stmt in stmts)
            {
                sb.AppendLine(stmt.Accept(this) as string);
            }

            return sb.ToString();
        }

        public object? Visit(BinaryExpression expr)
        {
            var s = $"({expr.BinaryOperator.Lexeme} {expr.LeftExpression.Accept(this)} {expr.RightExpression.Accept(this)})";

            return s;
        }

        public object? Visit(LogicalExpression expr)
        {
            var s = $"({expr.Operator.Lexeme} {expr.LeftExpression.Accept(this)} {expr.RightExpression.Accept(this)})";

            return s;
        }

        public object? Visit(GroupingExpression expr)
        {
            var s = $"(group {expr.GroupedExpression.Accept(this)})";

            return s;
        }

        public object? Visit(LiteralExpression expr)
        {
            return $"{(expr.Value == null ? "nil" : expr.Value.ToString())}";
        }

        public object? Visit(UnaryExpression expr)
        {
            return $"({expr.Operator.Lexeme} {expr.RightExpression.Accept(this)})";
        }

        public object? Visit(VariableExpression expr)
        {
            return $"(var {expr.VariableName})";
        }

        public object? Visit(AssignExpression expr)
        {
            return $"(assign {expr.VariableName.Lexeme} {expr.ValueExpression.Accept(this)})";
        }

        public object? Visit(CallExpression expr)
        {
            return $"(call {expr.Callee.Accept(this)} args[{expr.Arguments.Count}])";
        }

        public object? Visit(VarStatement stmt)
        {
            Enter();

            if (stmt.InitialValueExpression != null)
            {
                var output = $"[declare var {stmt.VariableName.Lexeme} with initial value]";
                output += System.Environment.NewLine;
                output += $"initializer: {stmt.InitialValueExpression.Accept(this)}";
                output += System.Environment.NewLine;
                output += "[/declare var]";


                return output;
            }
            else
            {
                return $"[declare var {stmt.VariableName.Lexeme} (undefined) /]";
            }
        }

        public object? Visit(ExpressionStatement stmt)
        {
            return $"[expr {stmt.Expression.Accept(this)}]";
        }

        public object? Visit(BlockStatement stmt)
        {
            var s = new StringBuilder();

            s.AppendLine("[block begin]");

            foreach (var blockStmt in stmt.Statements)
            {
                s.AppendLine(blockStmt.Accept(this) as string);
            }

            s.AppendLine("[block end]");

            return s.ToString();
        }

        public object? Visit(DebuggerStatement stmt)
        {
            return $"[debugger]]";
        }

        public object? Visit(ReturnStatement stmt)
        {
            return $"[return {stmt.ReturnValueExpression.Accept(this)}]";
        }

        public object? Visit(BreakStatement stmt)
        {
            return $"[break]";
        }

        public object? Visit(ContinueStatement stmt)
        {
            return $"[continue]";
        }

        public object? Visit(IfStatement stmt)
        {
            var s = new StringBuilder();

            s.AppendLine($"[if {stmt.TestExpression.Accept(this)}]");

            s.AppendLine($"[then]");
            s.Append($"{stmt.StatementWhenTrue.Accept(this)}");
            s.AppendLine($"[/then]");

            if (stmt.ElseStatement != null)
            {
                s.AppendLine($"[else]");
                s.Append($"{stmt.ElseStatement!.Accept(this)}");
                s.AppendLine($"[/else]");
            }

            s.AppendLine("[end if]");

            return s.ToString();
        }

        public object? Visit(ThrowStatement stmt)
        {
            if (stmt.ThrownValueExpression != null)
            {
                return $"[throw {stmt.ThrownValueExpression!.Accept(this)}]";
            }
            else
            {
                return "[throw]";
            }
        }

        public object? Visit(TryStatement stmt)
        {
            var s = new StringBuilder();
            /*
                        s.AppendLine($"[if {stmt.testExpression.Accept(this)}]");

                        s.AppendLine($"[then]");
                        s.Append($"{stmt.thenStatement.Accept(this)}");
                        s.AppendLine($"[/then]");

                        if (stmt.elseStatement != null)
                        {
                            s.AppendLine($"[else]");
                            s.Append($"{stmt.elseStatement!.Accept(this)}");
                            s.AppendLine($"[/else]");
                        }

                        s.AppendLine("[end if]");
                            */
            return s.ToString();
        }

        public object? Visit(TernaryExpression expr)
        {
            var s = new StringBuilder();

            s.AppendLine($"[ternary {expr.EvaluationExpression.Accept(this)}]");

            s.AppendLine($"[true]");
            s.Append($"{expr.TrueValue.Accept(this)}");
            s.AppendLine($"[/true]");

            s.AppendLine($"[false]");
            s.Append($"{expr.FalseValue.Accept(this)}");
            s.AppendLine($"[/false]");

            s.AppendLine("[end ternary]");

            return s.ToString();
        }

        public object? Visit(WhileStatement stmt)
        {
            var s = new StringBuilder();

            s.AppendLine($"[while {stmt.WhileCondition.Accept(this)}]");

            s.Append($"{stmt.ExecuteStatement.Accept(this)}");

            s.AppendLine("[end while]");

            return s.ToString();
        }

        public object? Visit(FunctionStatement stmt)
        {
            var s = new StringBuilder();

            s.AppendLine($"[declare function {stmt.FunctionName?.Lexeme ?? ""} ()]");

            s.Append($"{stmt.FunctionBody.Accept(this)}");

            s.AppendLine("[end function declaration]");

            return s.ToString();
        }

        public object? Visit(ClassStatement stmt)
        {
            var s = new StringBuilder();

            s.AppendLine($"[declare class {stmt.ClassName.Lexeme} ()]");

            //s.Append($"{stmt.constructor?.Accept(this) ?? "no ctor"}");

            foreach (var function in stmt.Functions)
            {
                s.Append($"{function.Accept(this)}");
            }

            return s.ToString();
        }

        public object? Visit(NewInstanceExpression expr)
        {
            var s = new StringBuilder();

            s.AppendLine($"[new instance of {expr.ClassName.Lexeme} ()]");

            return s.ToString();
        }
        /*
        public object? Visit(DotExpression expr)
        {
            var s = new StringBuilder();

            s.AppendLine($"[{expr.objectRefExpression.Accept(this)} . {expr.nextExpressionInChain.Accept(this)}]");

            return s.ToString();
        }
        */

        public object? Visit(GetExpression expr)
        {
            var s = new StringBuilder();

            s.AppendLine($"[getter obj={expr.TargetObject.Accept(this)}, property name={expr.AttributeName}]");

            return s.ToString();
        }

        public object? Visit(SetExpression expr)
        {
            var s = new StringBuilder();

            s.AppendLine($"[setter obj={expr.TargetObject.Accept(this)}, property name={expr.AttributeName} value={expr.Value.Accept(this)}]");

            return s.ToString();
        }

        public object? Visit(ObjectInitializerExpression expr)
        {
            var s = new StringBuilder();

            s.AppendLine($"[initializer property name={expr.ObjectName} value={expr.Value.Accept(this)}]");

            return s.ToString();
        }


        public object? Visit(IndexerGetExpression expr)
        {
            var s = new StringBuilder();

            s.AppendLine($"[indexGetter obj={expr.TargetObject.Accept(this)}, indexer Expr={expr.IndexerExpression.Accept(this)}]");

            return s.ToString();
        }

        public object? Visit(IndexerSetExpression expr)
        {
            var s = new StringBuilder();

            s.AppendLine($"[indexSetter obj={expr.TargetObject.Accept(this)}, indexer Expr={expr.IndexerExpression.Accept(this)} value={expr.Value.Accept(this)}]");

            return s.ToString();
        }

        public object? Visit(FunctionExpression expr)
        {
            return null;
        }
    }
}