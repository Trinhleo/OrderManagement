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
            var (orders, total) = await _repo.GetPagedAsync(query.Page, query.PageSize);

            // Always include paging but apply sorting inside page (current repository returns page already)
            var ordered = orders.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                var key = query.SortBy.ToLowerInvariant();
                ordered = key switch
                {
                    "customername" => (query.Desc ? ordered.OrderByDescending(o => o.CustomerName) : ordered.OrderBy(o => o.CustomerName)),
                    "status" => (query.Desc ? ordered.OrderByDescending(o => o.Status) : ordered.OrderBy(o => o.Status)),
                    "createdat" => (query.Desc ? ordered.OrderByDescending(o => o.CreatedAt) : ordered.OrderBy(o => o.CreatedAt)),
                    _ => ordered
                };
            }

            var orderDtos = ordered.Select(o => new
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
