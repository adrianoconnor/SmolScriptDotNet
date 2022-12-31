using System.Text;

namespace SmolScript
{
    public class AstDebugPrinter : IExpressionVisitor, IStatementVisitor {

        public string? Print(Expression expr) {
            return expr.Accept(this) as string;
        }

        public string? Print(Statement stmt) {
            return stmt.Accept(this) as string;
        }

        public string? Print(IList<Statement> stmts) {
            
            StringBuilder sb = new StringBuilder();

            foreach(var stmt in stmts)
            {
                sb.AppendLine(stmt.Accept(this) as string);
            }

            return sb.ToString();
        }

        public object? Visit(Expression.Binary expr)
        {
            return $"({expr.op.lexeme} {expr.left.Accept(this)} {expr.right.Accept(this)})";
        }

        public object? Visit(Expression.Logical expr)
        {
            return $"({expr.op.lexeme} {expr.left.Accept(this)} {expr.right.Accept(this)})";
        }

        public object? Visit(Expression.Grouping expr)
        {
            return $"(group {expr.expr.Accept(this)})";
        }

        public object? Visit(Expression.Literal expr)
        {
            return $"{(expr.value == null ? "nil" : expr.value.ToString())}";
        }

        public object? Visit(Expression.Unary expr)
        {
            return $"({expr.op.lexeme} {expr.right.Accept(this)})";
        }

        public object? Visit(Expression.Variable expr)
        {
            return $"(var {expr.name})";
        }

        public object? Visit(Expression.Assign expr)
        {
            return $"(assign {expr.name.lexeme} {expr.value.Accept(this)})";
        }

        public object? Visit(Expression.Call expr)
        {
            return $"(call {expr.callee.Accept(this)} {expr.paren.lexeme} args[{expr.args.Count}])";
        }

        public object? Visit(Statement.Var stmt)
        {
            if (stmt.initializerExpression != null)
            {
                return $"[var {stmt.name} = {stmt.initializerExpression.Accept(this)}]";
            }
            else 
            {
                return $"[var {stmt.name} = <<NULL REFERENCE>>]";
            }
        }

        public object? Visit(Statement.Expression stmt)
        {
            return $"[expr {stmt.expression.Accept(this)}]";
        }

        public object? Visit(Statement.Block stmt)
        {
            var s = new StringBuilder();
            
            s.AppendLine("[block begin]");
            
            foreach(var blockStmt in stmt.statements)
            {
                s.AppendLine(blockStmt.Accept(this) as string);
            }

            s.AppendLine("[block end]");

            return s.ToString();
        }

        public object? Visit(Statement.Print stmt)
        {
            return $"[print {stmt.expression.Accept(this)}]";
        }

        public object? Visit(Statement.Return stmt)
        {
            return $"[return {stmt.expression.Accept(this)}]";
        }

        public object? Visit(Statement.Break stmt)
        {
            return $"[break]";
        }

        public object? Visit(Statement.If stmt)
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

        public object? Visit(Statement.While stmt)
        {
            var s = new StringBuilder();
            
            s.AppendLine($"[while {stmt.whileCondition.Accept(this)}]");

            s.Append($"{stmt.executeStatement.Accept(this)}");

            s.AppendLine("[end while]");

            return s.ToString();
        }

        public object? Visit(Statement.Function stmt)
        {
            var s = new StringBuilder();
            
            s.AppendLine($"[declare function {stmt.name?.lexeme ?? ""} ()]");

            s.Append($"{stmt.functionBody.Accept(this)}");

            s.AppendLine("[end function declaration]");

            return s.ToString();
        }

    }
}