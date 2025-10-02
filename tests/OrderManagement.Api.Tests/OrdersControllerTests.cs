using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
using OrderManagement.Application.Commands;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Api.Tests
{
    // DTO to map API response
    public class PlaceOrderResponse
    {
        public Guid OrderId { get; set; }
    }

    public class OrdersControllerTests
        : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly OrderDbContext _dbContext;

        public OrdersControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _scope = factory.Services.CreateScope();
            _dbContext = _scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        }

        public async Task InitializeAsync()
        {
            await _dbContext.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            // ✅ Only clean up UT_ data
            await _dbContext.Database.ExecuteSqlRawAsync(
                "DELETE FROM [OrderLines] WHERE Product LIKE 'UT_%';"
            );
            await _dbContext.Database.ExecuteSqlRawAsync(
                "DELETE FROM [Orders] WHERE CustomerName LIKE 'UT_%';"
            );
        }

        [Fact]
        public async Task PlaceOrder_ReturnsOrderId()
        {
            // Arrange
            var command = new PlaceOrderCommand("UT_Product1", 2, 10.5m, "USD", "UT_Customer1");

            // Act
            var response = await _client.PostAsJsonAsync("/api/orders", command);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PlaceOrderResponse>();

            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result!.OrderId); // Ensure OrderId is returned
        }

        [Fact]
        public async Task GetOrder_ReturnsOrder()
        {
            // Arrange
            var command = new PlaceOrderCommand("UT_Product2", 1, 5.0m, "USD", "UT_Customer2");
            var postResponse = await _client.PostAsJsonAsync("/api/orders", command);
            postResponse.EnsureSuccessStatusCode();

            var postResult = await postResponse.Content.ReadFromJsonAsync<PlaceOrderResponse>();
            Assert.NotNull(postResult);
            var orderId = postResult!.OrderId;

            // Act
            var getResponse = await _client.GetAsync($"/api/orders/{orderId}");
            getResponse.EnsureSuccessStatusCode();

            var orderJson = await getResponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("UT_Product2", orderJson);
        }

        [Fact]
        public async Task ListOrders_ReturnsPagedOrders()
        {
            var response = await _client.GetAsync("/api/orders?page=1&pageSize=5");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("orders", content, StringComparison.OrdinalIgnoreCase);
        }
    }
}
