using System.Threading.Tasks;
using OrderManagement.Application.Commands;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Handlers
{
    public class UpdateOrderStatusHandler
    {
        private readonly IOrderRepository _repo;
        public UpdateOrderStatusHandler(IOrderRepository repo) => _repo = repo;

        public async Task<bool> Handle(UpdateOrderStatusCommand command)
        {
            var order = await _repo.GetByIdAsync(command.OrderId);
            if (order == null) return false;
            var status = command.Status?.Trim();
            if (string.IsNullOrWhiteSpace(status)) return false;
            // order has private setter; use reflection or better extend domain model (simplest: reflection here)
            var prop = order.GetType().GetProperty("Status");
            prop?.SetValue(order, status);
            await _repo.SaveAsync(order);
            return true;
        }
    }
}
