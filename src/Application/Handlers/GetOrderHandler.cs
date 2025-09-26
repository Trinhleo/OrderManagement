using System.Threading.Tasks;
using OrderManagement.Application.Queries;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Handlers
{
    public class GetOrderHandler
    {
        private readonly IOrderRepository _repo;

        public GetOrderHandler(IOrderRepository repo) => _repo = repo;

        public async Task<Order?> Handle(GetOrderQuery query)
            => await _repo.GetByIdAsync(query.OrderId);
    }
}