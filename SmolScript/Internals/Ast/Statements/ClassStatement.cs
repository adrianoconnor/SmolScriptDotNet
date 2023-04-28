using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Statements
{
    internal class ClassStatement : Statement
    {
        public readonly Token className;
        public readonly Token? superclassName;
        public readonly List<FunctionStatement> functions;

        public ClassStatement(Token className, Token? superclassName, List<FunctionStatement> functions)
        {
            this.className = className;
            this.superclassName = superclassName;
            this.functions = functions;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}