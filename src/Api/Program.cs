using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag.AspNetCore;
using OrderManagement.Application.Handlers;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IOrderRepository, OrderRepository>();
builder.Services.AddTransient<PlaceOrderHandler>();
builder.Services.AddTransient<GetOrderHandler>();

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

app.UseHttpsRedirection();
app.MapControllers();
app.Run();