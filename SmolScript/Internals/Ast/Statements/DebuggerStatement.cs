namespace SmolScript.Internals.Ast.Statements;

public class DebuggerStatement : Statement
{
    public DebuggerStatement()
    {
    }

    public override object? Accept(IStatementVisitor visitor)
    {
        return visitor.Visit(this);
    }
}
