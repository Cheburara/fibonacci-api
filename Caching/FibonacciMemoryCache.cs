using Microsoft.Extensions.Caching.Memory;

namespace FibonacciApi.Caching
{
    public class FibonacciMemoryCache : IFibonacciCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public FibonacciMemoryCache(IConfiguration configuration)
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            int expireSeconds = configuration.GetValue<int>("CacheExpirationSeconds", 300);
            _cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(expireSeconds)
            };
        }

        public bool TryGet(int index, out long value)
        {
            return _memoryCache.TryGetValue(index, out value);
        }

        public void Set(int index, long value)
        {
            _memoryCache.Set(index, value, _cacheOptions);
        }
    }
}