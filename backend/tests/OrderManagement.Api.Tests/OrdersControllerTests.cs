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
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace OrderManagement.Api.Tests
{
    public class PlaceOrderResponse { public Guid OrderId { get; set; } }

    public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly OrderDbContext _dbContext;

        static OrdersControllerTests()
        {
            // Force API to use in-memory provider during tests
            Environment.SetEnvironmentVariable("USE_INMEMORY_DB", "1");
        }

        public OrdersControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
            _scope = factory.Services.CreateScope();
            _dbContext = _scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        }

        public async Task InitializeAsync() => await _dbContext.Database.EnsureCreatedAsync();
        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task PlaceOrder_ReturnsOrderId()
        {
            var command = new PlaceOrderCommand("UT_Customer1", new List<PlaceOrderLine>{ new("UT_Product1",2,10.5m,"USD")});
            var response = await _client.PostAsJsonAsync("/api/orders", command);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PlaceOrderResponse>();
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result!.OrderId);
        }

        [Fact]
        public async Task GetOrder_ReturnsOrder()
        {
            var command = new PlaceOrderCommand("UT_Customer2", new List<PlaceOrderLine>{ new("UT_Product2",1,5.0m,"USD")});
            var postResponse = await _client.PostAsJsonAsync("/api/orders", command);
            postResponse.EnsureSuccessStatusCode();
            var postResult = await postResponse.Content.ReadFromJsonAsync<PlaceOrderResponse>();
            Assert.NotNull(postResult);
            var orderId = postResult!.OrderId;
            var getResponse = await _client.GetAsync($"/api/orders/{orderId}");
            getResponse.EnsureSuccessStatusCode();
            var orderJson = await getResponse.Content.ReadAsStringAsync();
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

        [Fact]
        public async Task UpdateStatus_UpdatesOrder()
        {
            var command = new PlaceOrderCommand("UT_CustomerStatus", new List<PlaceOrderLine>{ new("UT_ProductS",1,2m,"USD")});
            var post = await _client.PostAsJsonAsync("/api/orders", command);
            post.EnsureSuccessStatusCode();
            var created = await post.Content.ReadFromJsonAsync<PlaceOrderResponse>();
            Assert.NotNull(created);
            var id = created!.OrderId;
            var put = await _client.PutAsJsonAsync($"/api/orders/{id}/status", new { status = "Completed" });
            put.EnsureSuccessStatusCode();
            var get = await _client.GetAsync($"/api/orders/{id}");
            get.EnsureSuccessStatusCode();
            var json = await get.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            Assert.Equal("Completed", doc.RootElement.GetProperty("status").GetString());
        }

        [Fact]
        public async Task ListOrders_SortsByCustomerNameAscending()
        {
            var cmdB = new PlaceOrderCommand("UT_SORT_Beta", new List<PlaceOrderLine>{ new("UT_SORT_P1",1,1m,"USD")});
            var cmdA = new PlaceOrderCommand("UT_SORT_Alpha", new List<PlaceOrderLine>{ new("UT_SORT_P2",1,1m,"USD")});
            await _client.PostAsJsonAsync("/api/orders", cmdB);
            await _client.PostAsJsonAsync("/api/orders", cmdA);

            var resp = await _client.GetAsync("/api/orders?page=1&pageSize=50&sortBy=customerName&desc=false");
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var subset = doc.RootElement.GetProperty("orders").EnumerateArray()
                .Select(e => e.GetProperty("customerName").GetString())
                .Where(n => n != null && n.StartsWith("UT_SORT_"))
                .ToList();
            var expected = subset.OrderBy(n => n).ToList();
            Assert.Equal(expected, subset);
            Assert.Contains("UT_SORT_Alpha", subset);
            Assert.Contains("UT_SORT_Beta", subset);
        }
    }
}
