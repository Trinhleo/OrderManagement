using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag.AspNetCore;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Handlers;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
});

builder.Services.AddControllers();

// Allow using in-memory database for test/smoke scenarios
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase") ||
                  (System.Environment.GetEnvironmentVariable("USE_INMEMORY_DB") == "1");

if (useInMemory)
{
    Console.WriteLine("[Startup] Using InMemory database provider.");
    builder.Services.AddDbContext<OrderDbContext>(options =>
        options.UseInMemoryDatabase("OrdersDb"));
}
else
{
    Console.WriteLine("[Startup] Using SQL Server database provider.");
    builder.Services.AddDbContext<OrderDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Respect explicit URLs coming from smoke script
var explicitUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (!string.IsNullOrWhiteSpace(explicitUrls))
{
    Console.WriteLine($"[Startup] Binding to URLs: {explicitUrls}");
    builder.WebHost.UseUrls(explicitUrls.Split(';', StringSplitOptions.RemoveEmptyEntries));
}

builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
builder.Services.AddTransient<PlaceOrderHandler>();
builder.Services.AddTransient<GetOrderHandler>();
builder.Services.AddTransient<ListOrdersHandler>();
builder.Services.AddTransient<UpdateOrderStatusHandler>();

// ✅ Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add NSwag OpenAPI/Swagger support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Order Management API";
    config.Version = "v1";
    config.Description = "API documentation for Order Management system.";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi(); // Serve OpenAPI/Swagger docs
    app.UseSwaggerUi(settings =>
    {
        settings.Path = "/swagger";
        settings.DocumentTitle = "Order Management Swagger UI";
        settings.DocExpansion = "list"; // Expand all endpoints by default
    });
}

// Only enforce HTTPS when not running in the lightweight in-memory smoke environment
if (!useInMemory)
{
    app.UseHttpsRedirection();
}
else
{
    Console.WriteLine("[Startup] HTTPS redirection disabled for in-memory mode.");
}

// ✅ Enable CORS before controllers
app.UseCors("AllowAngularDev");

// Simple health endpoint for readiness checks
app.MapGet("/health", () => Results.Ok("OK"));

app.MapControllers();

Console.WriteLine("[Startup] Application configured. Starting web host...");

app.Run();

// For test project access
public partial class Program { }
