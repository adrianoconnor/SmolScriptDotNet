using SmolScript;

namespace SmolScript.StdLib
{
    public class Ticks : Callable, ICallable
    {
        public override int arity()
        {
            return 0;
        }

        public override object? call(Interpreter interpreter, IList<object?> args)
        {
            return (double)System.Environment.TickCount;
        }
    }
}