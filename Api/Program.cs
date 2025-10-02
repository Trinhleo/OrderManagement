using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag.AspNetCore;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Handlers;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Use connection string from appsettings.json
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
builder.Services.AddTransient<PlaceOrderHandler>();
builder.Services.AddTransient<GetOrderHandler>();
builder.Services.AddTransient<ListOrdersHandler>();

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

app.UseHttpsRedirection();

// ✅ Enable CORS before controllers
app.UseCors("AllowAngularDev");

app.MapControllers();

app.Run();

// For test project access
public partial class Program { }
