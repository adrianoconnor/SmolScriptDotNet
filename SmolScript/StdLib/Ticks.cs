using SmolScript.Internals;

namespace SmolScript.StdLib
{
    public class Ticks : ICallable
    {
        public object? call(IList<object?> args)
        {
            return (double)System.Environment.TickCount;
        }
    }
}