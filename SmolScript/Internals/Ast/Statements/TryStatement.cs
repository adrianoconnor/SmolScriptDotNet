namespace SmolScript.Internals.Ast.Statements;

internal class TryStatement : Statement
{
    public readonly BlockStatement TryBlock;
    public readonly Token? CatchBlockErrorVariableName;
    public readonly BlockStatement? CatchBlock;
    public readonly BlockStatement? FinallyBlock;

    public TryStatement(BlockStatement tryBlock, Token? catchBlockErrorVariableName, BlockStatement? catchBlock, BlockStatement? finallyBlock)
    {
        this.TryBlock = tryBlock;
        this.CatchBlockErrorVariableName = catchBlockErrorVariableName;
        this.CatchBlock = catchBlock;
        this.FinallyBlock = finallyBlock;
    }

    public override object? Accept(IStatementVisitor visitor)
    {
        return visitor.Visit(this);
    }
}
