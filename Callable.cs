namespace SmolScript 
{
    public interface ICallable
    {
        object? call(Interpreter interpreter, IList<object?> args); 
    }

    public class Callable : ICallable
    {
        public virtual int arity()
        {
            return 0;
        }

        public virtual object? call(Interpreter interpreter, IList<object?> args)
        {
            return null;
        }
    }


}