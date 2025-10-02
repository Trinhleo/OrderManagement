using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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
        private readonly ListOrdersHandler _listOrders;

        public OrdersController(PlaceOrderHandler placeOrder, GetOrderHandler getOrder, ListOrdersHandler listOrders)
        {
            _placeOrder = placeOrder;
            _getOrder = getOrder;
            _listOrders = listOrders;
        }
        /// <summary>
        /// Gets a paginated list of orders.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _listOrders.Handle(new ListOrdersQuery(page, pageSize));
            // Ensure the response is { orders: [...], totalCount: n }
            return Ok(new { orders = result.Orders, totalCount = result.TotalCount });
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
            if (order == null) return NotFound();

            var dto = new OrderDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                Lines = order.Lines.Select(l => new OrderLineDto
                {
                    Id = l.Id,
                    Product = l.Product,
                    Quantity = l.Quantity,
                    Amount = l.Price.Amount,
                    Currency = l.Price.Currency
                }).ToList()
            };
            return Ok(dto);
        }
    }

    public class OrderDto
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderLineDto> Lines { get; set; }
    }

    public class OrderLineDto
    {
        public int Id { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}