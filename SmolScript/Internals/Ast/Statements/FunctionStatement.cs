using SmolScript.Internals;

namespace SmolScript.Internals.Ast.Statements
{
    internal class FunctionStatement : Statement
    {
        public readonly Token FunctionName;
        public readonly IList<Token> ParameterList;
        public readonly BlockStatement FunctionBody;

        public FunctionStatement(Token functionName, IList<Token> parameterList, BlockStatement functionBody)
        {
            this.FunctionName = functionName;
            this.ParameterList = parameterList;
            this.FunctionBody = functionBody;
        }

        public override object? Accept(IStatementVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}