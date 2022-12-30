namespace SmolScript 
{
    public interface ICallable
    {
        int arity();

        object? call(Interpreter interpreter, IList<object?> args); 
    }
}