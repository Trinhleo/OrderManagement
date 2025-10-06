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

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(int page, int pageSize, string? sortBy = null, bool descending = true)
        {
            var baseQuery = _db.Orders.AsQueryable();

            // Apply sorting first
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var key = sortBy.ToLowerInvariant();
                baseQuery = key switch
                {
                    "id" => descending ? baseQuery.OrderByDescending(o => o.Id) : baseQuery.OrderBy(o => o.Id),
                    "customername" => descending ? baseQuery.OrderByDescending(o => o.CustomerName) : baseQuery.OrderBy(o => o.CustomerName),
                    "status" => descending ? baseQuery.OrderByDescending(o => o.Status) : baseQuery.OrderBy(o => o.Status),
                    "createdat" => descending ? baseQuery.OrderByDescending(o => o.CreatedAt) : baseQuery.OrderBy(o => o.CreatedAt),
                    _ => baseQuery.OrderByDescending(o => o.CreatedAt) // Default sort
                };
            }
            else
            {
                // Default sort by CreatedAt descending (newest first)
                baseQuery = baseQuery.OrderByDescending(o => o.CreatedAt);
            }

            var total = await baseQuery.CountAsync();
            var orders = await baseQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (orders, total);
        }
    }
}
