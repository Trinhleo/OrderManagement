using System.Linq;
using System.Threading.Tasks;
using OrderManagement.Domain.Repositories;
using OrderManagement.Application.Queries;

namespace OrderManagement.Application.Handlers
{
    public class ListOrdersHandler
    {
        private readonly IOrderRepository _repo;
        public ListOrdersHandler(IOrderRepository repo) => _repo = repo;

        public async Task<PagedOrdersResult> Handle(ListOrdersQuery query)
        {
            // Pass sorting parameters to repository for proper sorting before pagination
            var (orders, total) = await _repo.GetPagedAsync(query.Page, query.PageSize, query.SortBy, query.Desc);

            var orderDtos = orders.Select(o => new
            {
                o.Id,
                o.CustomerName,
                o.Status,
                o.CreatedAt
            });

            return new PagedOrdersResult
            {
                Orders = orderDtos,
                TotalCount = total
            };
        }
    }
}
