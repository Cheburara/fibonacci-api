using FibonacciApi.Models;
namespace FibonacciApi.Services;

public interface IFibonacciService
{
    Task<FibonacciResponse> GenerateFibonacciAsync(
        FibonacciRequest request,
        CancellationToken cancellationToken);
}