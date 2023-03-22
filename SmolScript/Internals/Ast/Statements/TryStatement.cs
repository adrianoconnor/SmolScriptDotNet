namespace SmolScript.Internals.Ast.Statements;

public class TryStatement : Statement
{
    public readonly BlockStatement tryBody;
    public readonly Token? exceptionVariableName;
    public readonly BlockStatement? catchBody;
    public readonly BlockStatement? finallyBody;

    public TryStatement(BlockStatement tryBody, Token? exceptionVariableName, BlockStatement? catchBody, BlockStatement? finallyBody)
    {
        this.tryBody = tryBody;
        this.exceptionVariableName = exceptionVariableName;
        this.catchBody = catchBody;
        this.finallyBody = finallyBody;
    }

    public override object? Accept(IStatementVisitor visitor)
    {
        return visitor.Visit(this);
    }
}
