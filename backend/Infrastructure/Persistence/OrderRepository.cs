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

        public Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(int page, int pageSize)
        {
            var allOrders = _store.Values.OrderByDescending(o => o.CreatedAt).ToList();
            int total = allOrders.Count;
            var paged = allOrders.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Task.FromResult(((IEnumerable<Order>)paged, total));
        }
    }
}