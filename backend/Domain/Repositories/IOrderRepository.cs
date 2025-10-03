using System;
using System.Threading.Tasks;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id);
        Task SaveAsync(Order order);

        /// <summary>
        /// Gets a paginated list of orders.
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paged list of orders and total count</returns>
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(int page, int pageSize);
    }
}