using FibonacciApi.Models;
using FibonacciApi.Services;
using FibonacciApi.Caching;
using Xunit;
using Moq;
using System.Threading;

namespace FibonacciApi.Tests;

public class FibonacciServiceTests
{
    private readonly FibonacciService _service;
    private readonly Mock<IFibonacciCache> _mockCache;

    public FibonacciServiceTests()
    {
        _mockCache = new Mock<IFibonacciCache>();
        _service = new FibonacciService(_mockCache.Object);
    }

    [Fact]
    public async Task ReturnsCorrectSequence()
    {
        var request = new FibonacciRequest
        {
            StartIndex = 0,
            EndIndex = 5,
            TimeoutMs = 5000,
            MaxMemoryMb = 100,
            UseCache = false
        };

        var result = await _service.GenerateFibonacciAsync(request, CancellationToken.None);

        Assert.Equal(new List<long> { 0, 1, 1, 2, 3, 5 }, result.Sequence);
        Assert.False(result.TimedOut);
        Assert.False(result.MemoryLimitReached);
    }

    [Fact]
    public async Task TimeoutIsRespected()
    {
        var request = new FibonacciRequest
        {
            StartIndex = 0,
            EndIndex = 3,
            TimeoutMs = 200, 
            MaxMemoryMb = 100,
            UseCache = false
        };

        using var timeoutCts = new CancellationTokenSource(request.TimeoutMs);
        var result = await _service.GenerateFibonacciAsync(request, timeoutCts.Token);

        Assert.True(result.TimedOut);
        Assert.InRange(result.Sequence.Count, 0, 2);
    }

    [Fact]
    public async Task MemoryLimitIsRespected()
    {
        var request = new FibonacciRequest
        {
            StartIndex = 0,
            EndIndex = 1000,
            TimeoutMs = 10000,
            MaxMemoryMb = 1, 
            UseCache = false
        };

        var result = await _service.GenerateFibonacciAsync(request, CancellationToken.None);

        Assert.True(result.MemoryLimitReached);
        Assert.True(result.Sequence.Count >= 0);
    }

    [Fact]
    public async Task UsesCacheIfEnabled()
    {
        var request = new FibonacciRequest
        {
            StartIndex = 5,
            EndIndex = 5,
            TimeoutMs = 1000,
            MaxMemoryMb = 100,
            UseCache = true
        };

        _mockCache.Setup(c => c.TryGet(5, out It.Ref<long>.IsAny)).Returns((int key, out long val) =>
        {
            val = 5;
            return true;
        });

        var result = await _service.GenerateFibonacciAsync(request, CancellationToken.None);

        Assert.Single(result.Sequence);
        Assert.Equal(5, result.Sequence[0]);
        _mockCache.Verify(c => c.TryGet(5, out It.Ref<long>.IsAny), Times.Once);
    }

    [Fact]
    public async Task ThrowsArgumentException_WhenStartIndexGreaterThanEndIndex()
    {
        var request = new FibonacciRequest
        {
            StartIndex = 5,
            EndIndex = 2,
            TimeoutMs = 1000,
            MaxMemoryMb = 100,
            UseCache = false
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GenerateFibonacciAsync(request, CancellationToken.None));

        Assert.Equal("Invalid index range.", exception.Message);
    }
}
