using FibonacciApi.Services;
using FibonacciApi.Caching;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

// Register services
builder.Services.AddScoped<IFibonacciService, FibonacciService>();
builder.Services.AddScoped<IFibonacciCache, FibonacciMemoryCache>();


var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();