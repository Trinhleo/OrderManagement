using System;
using System.Linq;
using System.Threading.Tasks;
using OrderManagement.Application.Commands;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Application.Handlers
{
    public class PlaceOrderHandler
    {
        private readonly IOrderRepository _repo;

        public PlaceOrderHandler(IOrderRepository repo) => _repo = repo;

        public async Task<Guid> Handle(PlaceOrderCommand cmd)
        {
            var customerName = string.IsNullOrWhiteSpace(cmd.CustomerName) ? "Unknown" : cmd.CustomerName;
            var order = new Order(customerName, "New");
            foreach (var line in cmd.Lines)
            {
                order.AddLine(line.Product, line.Quantity, new Money(line.Price, line.Currency));
            }
            await _repo.SaveAsync(order);
            return order.Id;
        }
    }
}