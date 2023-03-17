namespace SmolScript
{
    public interface ICallable
    {
        public object? call(IList<object?> args);
    }
}