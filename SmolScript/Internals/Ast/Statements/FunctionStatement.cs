using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Statements
{
    internal class FunctionStatement : Statement
    {
        public readonly Token name;
        public readonly IList<Token> parameters;
        public readonly BlockStatement functionBody;

        public FunctionStatement(Token name, IList<Token> parameters, BlockStatement functionBody)
        {
            this.name = name;
            this.parameters = parameters;
            this.functionBody = functionBody;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}