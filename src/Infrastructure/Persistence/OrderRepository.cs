using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence
{
    // Simple in-memory repository for demo
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
    }
}