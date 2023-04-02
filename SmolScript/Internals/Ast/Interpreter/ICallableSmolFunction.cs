namespace SmolScript.Internals.Ast.Interpreter
{
    internal interface ICallableSmolFunction
    {
        public object? call(AstInterpreter interpreter, IList<object?> args)
        {
            return null;
        }
    }
}