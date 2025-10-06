using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence
{

    public class OrderRepository : IOrderRepository
    {
        private static readonly ConcurrentDictionary<Guid, Order> _store = new();

        public Task<Order?> GetByIdAsync(Guid id)
        {
            _store.TryGetValue(id, out var order);
            return Task.FromResult(order);
        }

        public Task SaveAsync(Order order)
        {
            _store[order.Id] = order;
            return Task.CompletedTask;
        }

        public Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(int page, int pageSize, string? sortBy = null, bool descending = true)
        {
            var allOrders = _store.Values.AsQueryable();

            // Apply sorting first
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var key = sortBy.ToLowerInvariant();
                allOrders = key switch
                {
                    "id" => descending ? allOrders.OrderByDescending(o => o.Id) : allOrders.OrderBy(o => o.Id),
                    "customername" => descending ? allOrders.OrderByDescending(o => o.CustomerName) : allOrders.OrderBy(o => o.CustomerName),
                    "status" => descending ? allOrders.OrderByDescending(o => o.Status) : allOrders.OrderBy(o => o.Status),
                    "createdat" => descending ? allOrders.OrderByDescending(o => o.CreatedAt) : allOrders.OrderBy(o => o.CreatedAt),
                    _ => allOrders.OrderByDescending(o => o.CreatedAt) // Default sort
                };
            }
            else
            {
                // Default sort by CreatedAt descending (newest first)
                allOrders = allOrders.OrderByDescending(o => o.CreatedAt);
            }

            int total = allOrders.Count();
            var paged = allOrders.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Task.FromResult(((IEnumerable<Order>)paged, total));
        }
    }
}