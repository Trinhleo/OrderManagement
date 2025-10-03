using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence
{
    public class EfOrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _db;
        public EfOrderRepository(OrderDbContext db) => _db = db;

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            return await _db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task SaveAsync(Order order)
        {
            if (_db.Orders.Any(o => o.Id == order.Id))
                _db.Orders.Update(order);
            else
                await _db.Orders.AddAsync(order);
            await _db.SaveChangesAsync();
        }

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(int page, int pageSize)
        {
            var baseQuery = _db.Orders.AsQueryable();
            var total = await baseQuery.CountAsync();
            // Remove default ordering, let application layer handle ordering after paging for deterministic subset testing
            var orders = await baseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (orders, total);
        }
    }
}
