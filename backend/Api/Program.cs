using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag.AspNetCore;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Handlers;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using OrderManagement.Api.Services;
using OrderManagement.Api.Middleware;
using FluentValidation;
using OrderManagement.Application.Commands.Validators;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
});

// Configure JSON options for proper DateTime handling
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    // Ensure DateTime is serialized in UTC ISO format
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Ensure DateTime is serialized in UTC ISO format
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<PlaceOrderCommandValidator>();

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-that-is-at-least-32-characters-long")),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "OrderManagementApi",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "OrderManagementClient",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Add custom services
builder.Services.AddScoped<ITokenService, TokenService>();

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
    config.Description = "API documentation for Order Management system with JWT Authentication.";

    // Add JWT security definition
    config.AddSecurity("JWT", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Description = "Type into the textbox: Bearer {your JWT token}."
    });
});

var app = builder.Build();

// Add exception handling middleware
app.UseMiddleware<ExceptionMiddleware>();

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

// ✅ Enable CORS before authentication
app.UseCors("AllowAngularDev");

// Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Simple health endpoint for readiness checks
app.MapGet("/health", () => Results.Ok("OK"));

app.MapControllers();

Console.WriteLine("[Startup] Application configured. Starting web host...");

app.Run();

// For test project access
public partial class Program { }
