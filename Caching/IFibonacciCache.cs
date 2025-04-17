namespace FibonacciApi.Caching
{
    public interface IFibonacciCache
    {
        bool TryGet(int index, out long value);
        void Set(int index, long value);
    }
}
