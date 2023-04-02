namespace SmolScript.Internals.Ast.Statements;

internal class DebuggerStatement : Statement
{
    public DebuggerStatement()
    {
    }

    public override object? Accept(IStatementVisitor visitor)
    {
        return visitor.Visit(this);
    }
}
