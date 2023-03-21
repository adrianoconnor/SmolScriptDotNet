using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Statements
{
    public class ClassStatement //: Statement
    {
        public readonly Token? className;
        public readonly Token? parentClassName;
        public readonly BlockStatement initializorBody;

        public readonly List<FunctionStatement> functions = new List<FunctionStatement>();

        public ClassStatement(Token? name, BlockStatement initializorBody)
        {
            this.className = name;
            this.parentClassName = name;
            this.initializorBody = initializorBody;
        }

        /*
        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }*/
    }
}