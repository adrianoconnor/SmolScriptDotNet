namespace SmolScript.StdLib
{
    public class Ticks : SmolScript.ICallable
    {
        public int arity()
        {
            return 0;
        }

        public object? call(Interpreter interpreter, IList<object?> args)
        {
            return (double)System.Environment.TickCount;
        }
    }
}