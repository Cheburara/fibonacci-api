namespace FibonacciApi.Models;

public class FibonacciResponse
{
    public List<long> Sequence { get; set; } = new();

    public bool TimedOut { get; set; }

    public bool MemoryLimitReached { get; set; }
}