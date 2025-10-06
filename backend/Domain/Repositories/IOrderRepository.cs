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
        /// Gets a paginated list of orders with sorting.
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortBy">Field to sort by (id, customername, status, createdat)</param>
        /// <param name="descending">Sort in descending order</param>
        /// <returns>Paged list of orders and total count</returns>
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(int page, int pageSize, string? sortBy = null, bool descending = true);
    }
}