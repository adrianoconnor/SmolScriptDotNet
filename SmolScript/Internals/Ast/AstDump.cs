using System.Text;
using SmolScript.Internals.Ast.Expressions;
using SmolScript.Internals.Ast.Statements;

namespace SmolScript.Internals.Ast
{
    internal class AstDump : IExpressionVisitor, IStatementVisitor
    {
        string newline = System.Environment.NewLine;
        private int depth = 0;
        private string indent
        {
            get { return "".PadLeft(depth * 2); }
        }

        private void enter()
        {
            depth++;
        }

        private void leave()
        {
            depth--;
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
            var s = $"({expr.op.lexeme} {expr.left.Accept(this)} {expr.right.Accept(this)})";

            return s;
        }

        public object? Visit(LogicalExpression expr)
        {
            var s =  $"({expr.op.lexeme} {expr.left.Accept(this)} {expr.right.Accept(this)})";

            return s;
        }

        public object? Visit(GroupingExpression expr)
        {
            var s = $"(group {expr.expr.Accept(this)})";

            return s;
        }

        public object? Visit(LiteralExpression expr)
        {
            return $"{(expr.value == null ? "nil" : expr.value.ToString())}";
        }

        public object? Visit(UnaryExpression expr)
        {
            return $"({expr.op.lexeme} {expr.right.Accept(this)})";
        }

        public object? Visit(VariableExpression expr)
        {
            return $"(var {expr.name})";
        }

        public object? Visit(AssignExpression expr)
        {
            return $"(assign {expr.name.lexeme} {expr.value.Accept(this)})";
        }

        public object? Visit(CallExpression expr)
        {
            return $"(call {expr.callee.Accept(this)} {expr.paren.lexeme} args[{expr.args.Count}])";
        }

        public object? Visit(VarStatement stmt)
        {
            enter();

            if (stmt.initializerExpression != null)
            {
                var output = $"[declare var {stmt.name.lexeme} with initial value]";
                output += System.Environment.NewLine;
                output += $"initializer: {stmt.initializerExpression.Accept(this)}";
                output += System.Environment.NewLine;
                output += "[/declare var]";


                return output;
            }
            else
            {
                return $"[declare var {stmt.name.lexeme} (undefined) /]";
            }
        }

        public object? Visit(ExpressionStatement stmt)
        {
            return $"[expr {stmt.expression.Accept(this)}]";
        }

        public object? Visit(BlockStatement stmt)
        {
            var s = new StringBuilder();

            s.AppendLine("[block begin]");

            foreach (var blockStmt in stmt.statements)
            {
                s.AppendLine(blockStmt.Accept(this) as string);
            }

            s.AppendLine("[block end]");

            return s.ToString();
        }

        public object? Visit(PrintStatement stmt)
        {
            return $"[print {stmt.expression.Accept(this)}]";
        }

        public object? Visit(DebuggerStatement stmt)
        {
            return $"[debugger]]";
        }

        public object? Visit(ReturnStatement stmt)
        {
            return $"[return {stmt.expression.Accept(this)}]";
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

            return s.ToString();
        }

        public object? Visit(ThrowStatement stmt)
        {
            if (stmt.expression != null)
            {
                return $"[throw {stmt.expression!.Accept(this)}]";
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

            s.AppendLine($"[ternary {expr.evaluationExpression.Accept(this)}]");

            s.AppendLine($"[true]");
            s.Append($"{expr.expressionIfTrue.Accept(this)}");
            s.AppendLine($"[/true]");

            s.AppendLine($"[false]");
            s.Append($"{expr.expresisonIfFalse.Accept(this)}");
            s.AppendLine($"[/false]");

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
            var s = new StringBuilder();

            s.AppendLine($"[declare function {stmt.name?.lexeme ?? ""} ()]");

            s.Append($"{stmt.functionBody.Accept(this)}");

            s.AppendLine("[end function declaration]");

            return s.ToString();
        }

        public object? Visit(ClassStatement stmt)
        {
            var s = new StringBuilder();

            s.AppendLine($"[declare class {stmt.className.lexeme} ()]");

            //s.Append($"{stmt.constructor?.Accept(this) ?? "no ctor"}");

            foreach (var function in stmt.functions)
            {
                s.Append($"{function.Accept(this)}");
            }

            return s.ToString();
        }

        public object? Visit(NewInstanceExpression expr)
        {
            var s = new StringBuilder();

            s.AppendLine($"[new instance of {expr.className.lexeme} ()]");

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

            s.AppendLine($"[getter obj={expr.obj.Accept(this)}, property name={expr.name}]");

            return s.ToString();
        }

        public object? Visit(SetExpression expr)
        {
            var s = new StringBuilder();

            s.AppendLine($"[getter obj={expr.obj.Accept(this)}, property name={expr.name} value ={expr.value.Accept(this)}]");
            
            return s.ToString();
        }

        public object? Visit(FunctionExpression expr)
        {
            return null;
        }
    }
}