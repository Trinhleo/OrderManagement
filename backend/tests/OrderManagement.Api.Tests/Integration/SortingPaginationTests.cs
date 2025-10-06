using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Persistence;
using Xunit;

namespace OrderManagement.Api.Tests.Integration
{
    /// <summary>
    /// Integration tests to verify sorting + pagination correctness
    /// </summary>
    public class SortingPaginationTests : IAsyncLifetime
    {
        private OrderDbContext _context;
        private EfOrderRepository _repository;
        private List<Order> _testOrders;

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<OrderDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new OrderDbContext(options);
            _repository = new EfOrderRepository(_context);

            // Create test data with known sorting characteristics
            _testOrders = new List<Order>
            {
                CreateOrder("Customer_A", "Pending", DateTime.UtcNow.AddDays(-3)),
                CreateOrder("Customer_C", "Shipped", DateTime.UtcNow.AddDays(-2)),
                CreateOrder("Customer_B", "Completed", DateTime.UtcNow.AddDays(-1)),
                CreateOrder("Customer_D", "Pending", DateTime.UtcNow),
                CreateOrder("Customer_A", "Completed", DateTime.UtcNow.AddHours(-12)),
                CreateOrder("Customer_E", "Cancelled", DateTime.UtcNow.AddDays(-5)),
                CreateOrder("Customer_F", "Processing", DateTime.UtcNow.AddHours(-6)),
                CreateOrder("Customer_B", "Shipped", DateTime.UtcNow.AddHours(-3))
            };

            foreach (var order in _testOrders)
            {
                await _repository.SaveAsync(order);
            }
        }

        public Task DisposeAsync()
        {
            _context?.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task GetPagedAsync_WithoutSorting_ReturnsDefaultSortByCreatedAtDesc()
        {
            // Act
            var (orders, total) = await _repository.GetPagedAsync(1, 3);

            // Assert
            Assert.Equal(_testOrders.Count, total);
            Assert.Equal(3, orders.Count());
            
            // Verify default sort (newest first)
            var ordersList = orders.ToList();
            for (int i = 0; i < ordersList.Count - 1; i++)
            {
                Assert.True(ordersList[i].CreatedAt >= ordersList[i + 1].CreatedAt,
                    $"Order at index {i} should have CreatedAt >= order at index {i+1}");
            }
        }

        [Fact]
        public async Task GetPagedAsync_SortByCustomerNameAsc_ReturnsSortedCorrectly()
        {
            // Act
            var (orders, total) = await _repository.GetPagedAsync(1, 5, "customername", false);

            // Assert
            Assert.Equal(_testOrders.Count, total);
            var ordersList = orders.ToList();
            
            // Verify ascending sort by CustomerName
            for (int i = 0; i < ordersList.Count - 1; i++)
            {
                var comparison = string.Compare(ordersList[i].CustomerName, ordersList[i + 1].CustomerName, StringComparison.Ordinal);
                Assert.True(comparison <= 0,
                    $"CustomerName at index {i} ({ordersList[i].CustomerName}) should be <= {ordersList[i + 1].CustomerName}");
            }
        }

        [Fact]
        public async Task GetPagedAsync_SortByCustomerNameDesc_ReturnsSortedCorrectly()
        {
            // Act
            var (orders, total) = await _repository.GetPagedAsync(1, 5, "customername", true);

            // Assert
            Assert.Equal(_testOrders.Count, total);
            var ordersList = orders.ToList();
            
            // Verify descending sort
            for (int i = 0; i < ordersList.Count - 1; i++)
            {
                var comparison = string.Compare(ordersList[i].CustomerName, ordersList[i + 1].CustomerName, StringComparison.Ordinal);
                Assert.True(comparison >= 0,
                    $"CustomerName at index {i} ({ordersList[i].CustomerName}) should be >= {ordersList[i + 1].CustomerName}");
            }
        }

        [Fact]
        public async Task GetPagedAsync_SortByStatus_WorksWithPagination()
        {
            // Act - Get page 1
            var (page1, total1) = await _repository.GetPagedAsync(1, 3, "status", false);
            
            // Act - Get page 2
            var (page2, total2) = await _repository.GetPagedAsync(2, 3, "status", false);

            // Assert
            Assert.Equal(total1, total2); // Total should be same
            Assert.Equal(_testOrders.Count, total1);
            
            var allOrders = page1.Concat(page2).ToList();
            
            // Verify no duplicates between pages
            var distinctIds = allOrders.Select(o => o.Id).Distinct().Count();
            Assert.Equal(6, distinctIds); // 3 from page1 + 3 from page2

            // Verify sorting is maintained across pages
            for (int i = 0; i < allOrders.Count - 1; i++)
            {
                var comparison = string.Compare(allOrders[i].Status, allOrders[i + 1].Status, StringComparison.Ordinal);
                Assert.True(comparison <= 0,
                    $"Status at index {i} ({allOrders[i].Status}) should be <= {allOrders[i + 1].Status}");
            }
        }

        [Fact]
        public async Task GetPagedAsync_MultiplePages_NoDataLoss()
        {
            // Act - Get all data in pages of 2
            var allPagesData = new List<Order>();
            int pageSize = 2;
            int totalPages = (int)Math.Ceiling(_testOrders.Count / (double)pageSize);

            for (int page = 1; page <= totalPages; page++)
            {
                var (orders, _) = await _repository.GetPagedAsync(page, pageSize, "customername", false);
                allPagesData.AddRange(orders);
            }

            // Assert
            Assert.Equal(_testOrders.Count, allPagesData.Count);
            
            // Verify all unique orders retrieved
            var uniqueIds = allPagesData.Select(o => o.Id).Distinct().Count();
            Assert.Equal(_testOrders.Count, uniqueIds);

            // Verify global sorting
            for (int i = 0; i < allPagesData.Count - 1; i++)
            {
                var comparison = string.Compare(allPagesData[i].CustomerName, allPagesData[i + 1].CustomerName, StringComparison.Ordinal);
                Assert.True(comparison <= 0,
                    $"CustomerName at index {i} should be <= index {i + 1}");
            }
        }

        [Fact]
        public async Task GetPagedAsync_SortByCreatedAt_PaginationDoesNotAffectOrder()
        {
            // Arrange - Sort all orders by CreatedAt desc
            var expectedOrder = _testOrders.OrderByDescending(o => o.CreatedAt).ToList();

            // Act - Get in pages
            var (page1, _) = await _repository.GetPagedAsync(1, 3, "createdat", true);
            var (page2, _) = await _repository.GetPagedAsync(2, 3, "createdat", true);
            var (page3, _) = await _repository.GetPagedAsync(3, 3, "createdat", true);

            // Assert
            var combined = page1.Concat(page2).Concat(page3).ToList();
            
            for (int i = 0; i < combined.Count && i < expectedOrder.Count; i++)
            {
                Assert.Equal(expectedOrder[i].Id, combined[i].Id);
            }
        }

        [Theory]
        [InlineData(1, 5, "customername", false)]
        [InlineData(2, 3, "status", true)]
        [InlineData(1, 10, "createdat", false)]
        [InlineData(3, 2, "customername", true)]
        public async Task GetPagedAsync_VariousConfigurations_TotalCountAlwaysCorrect(
            int page, int pageSize, string sortBy, bool desc)
        {
            // Act
            var (orders, total) = await _repository.GetPagedAsync(page, pageSize, sortBy, desc);

            // Assert
            Assert.Equal(_testOrders.Count, total);
            Assert.True(orders.Count() <= pageSize);
        }

        private Order CreateOrder(string customerName, string status, DateTime createdAt)
        {
            var order = new Order(customerName, status);
            
            // Use reflection to set CreatedAt for testing (since it's private setter)
            var createdAtProp = typeof(Order).GetProperty("CreatedAt");
            createdAtProp?.SetValue(order, createdAt);

            order.AddLine("Product1", 1, new Money(10m, "USD"));
            
            return order;
        }
    }
}
