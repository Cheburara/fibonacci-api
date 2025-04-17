using FibonacciApi.Caching;
using FibonacciApi.Models;

namespace FibonacciApi.Services
{
    public class FibonacciService : IFibonacciService
    {
        private readonly IFibonacciCache _cache;

        public FibonacciService(IFibonacciCache cache)
        {
            _cache = cache;
        }

        public async Task<FibonacciResponse> GenerateFibonacciAsync(
            FibonacciRequest request,
            CancellationToken cancellationToken)
        {
            var response = new FibonacciResponse();

            // Validate the request range
            if (request.EndIndex < request.StartIndex || request.StartIndex < 0)
                throw new ArgumentException("Invalid index range.");

            // Store results with index to preserve order
            var results = new List<(int Index, long Value)>();
            var tasks = new List<Task>();

            // Start processing each Fibonacci index in parallel
            for (int i = request.StartIndex; i <= request.EndIndex; i++)
            {
                int currentIndex = i;

                var task = Task.Run(async () =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    long result;

                    // Use cache 
                    if (request.UseCache && _cache.TryGet(currentIndex, out result))
                    {
                        lock (results)
                        {
                            results.Add((currentIndex, result));
                        }
                        return;
                    }

                    // Compute Fibonacci if not in cache
                    result = ComputeFibonacci(currentIndex);

                    await Task.Delay(500, cancellationToken); // Simulate CPU work


                    // Store computed value in cache
                    if (request.UseCache)
                    {
                        _cache.Set(currentIndex, result);
                    }

                    // Store result with its original index
                    lock (results)
                    {
                        results.Add((currentIndex, result));
                    }
                }, cancellationToken);

                tasks.Add(task);
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                response.TimedOut = true;
            }

            // Sort by index to ensure correct sequence order from smallest to largest
            response.Sequence = results
                .OrderBy(r => r.Index)
                .Select(r => r.Value)
                .ToList();

            return response;
        }
        
        private long ComputeFibonacci(int n)
        {
            if (n == 0) return 0;
            if (n == 1) return 1;

            long a = 0, b = 1, result = 0;

            for (int i = 2; i <= n; i++)
            {
                result = a + b;
                a = b;
                b = result;
            }

            return result;
        }
    }
}
