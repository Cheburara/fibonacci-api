using FibonacciApi.Models;
using FibonacciApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FibonacciApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FibonacciController : ControllerBase
    {
        // Service that handles Fibonacci logic
        private readonly IFibonacciService _fibonacciService;
        
        //Constructor injection of the service 
        public FibonacciController(IFibonacciService fibonacciService)
        {
            _fibonacciService = fibonacciService;
        }

        
        [HttpPost]
        public async Task<ActionResult<FibonacciResponse>> GetSubsequence(
            [FromBody] FibonacciRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Introduction service layer
                var response = await _fibonacciService.GenerateFibonacciAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", detail = ex.Message });
            }
        }
    }
}