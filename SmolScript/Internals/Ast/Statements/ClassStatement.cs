using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Statements
{
    internal class ClassStatement : Statement
    {
        public readonly Token ClassName;
        public readonly Token? SuperClassName;
        public readonly List<FunctionStatement> Functions;

        public ClassStatement(Token className, Token? superClassName, List<FunctionStatement> functions)
        {
            this.ClassName = className;
            this.SuperClassName = superClassName;
            this.Functions = functions;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}