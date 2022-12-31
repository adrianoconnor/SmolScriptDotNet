namespace SmolScript 
{
    public interface ICallable
    {
        object? call(Interpreter interpreter, IList<object?> args); 
    }
}