using System;
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
            var customerName = cmd.CustomerName ?? "Unknown";
            var status = "New";
            var order = new Order(customerName, status);
            order.AddLine(cmd.Product, cmd.Quantity, new Money(cmd.Price, cmd.Currency));
            await _repo.SaveAsync(order);
            return order.Id;
        }
    }
}