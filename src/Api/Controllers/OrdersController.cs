using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using OrderManagement.Application.Commands;
using OrderManagement.Application.Queries;
using OrderManagement.Application.Handlers;

namespace OrderManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly PlaceOrderHandler _placeOrder;
        private readonly GetOrderHandler _getOrder;

        public OrdersController(PlaceOrderHandler placeOrder, GetOrderHandler getOrder)
        {
            _placeOrder = placeOrder;
            _getOrder = getOrder;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderCommand cmd)
        {
            var id = await _placeOrder.Handle(cmd);
            return Ok(new { OrderId = id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _getOrder.Handle(new GetOrderQuery(id));
            return order == null ? NotFound() : Ok(order);
        }
    }
}