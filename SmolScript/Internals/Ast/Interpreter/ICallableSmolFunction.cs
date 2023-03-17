namespace SmolScript.Internals.Ast.Interpreter
{
    public interface ICallableSmolFunction
    {
        public object? call(AstInterpreter interpreter, IList<object?> args)
        {
            return null;
        }
    }
}