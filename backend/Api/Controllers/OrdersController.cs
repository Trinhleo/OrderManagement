using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using OrderManagement.Application.Commands;
using OrderManagement.Application.Queries;
using OrderManagement.Application.Handlers;
using FluentValidation;

namespace OrderManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all endpoints
    public class OrdersController : ControllerBase
    {
        private readonly PlaceOrderHandler _placeOrder;
        private readonly GetOrderHandler _getOrder;
        private readonly ListOrdersHandler _listOrders;
        private readonly UpdateOrderStatusHandler _updateStatus;
        private readonly IValidator<PlaceOrderCommand> _placeOrderValidator;
        private readonly IValidator<UpdateOrderStatusCommand> _updateStatusValidator;

        public OrdersController(
            PlaceOrderHandler placeOrder,
            GetOrderHandler getOrder,
            ListOrdersHandler listOrders,
            UpdateOrderStatusHandler updateStatus,
            IValidator<PlaceOrderCommand> placeOrderValidator,
            IValidator<UpdateOrderStatusCommand> updateStatusValidator)
        {
            _placeOrder = placeOrder;
            _getOrder = getOrder;
            _listOrders = listOrders;
            _updateStatus = updateStatus;
            _placeOrderValidator = placeOrderValidator;
            _updateStatusValidator = updateStatusValidator;
        }
        /// <summary>
        /// Gets a paginated list of orders.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sortBy = null, [FromQuery] bool desc = true)
        {
            var result = await _listOrders.Handle(new ListOrdersQuery(page, pageSize, sortBy, desc));
            // Ensure the response is { orders: [...], totalCount: n }
            return Ok(new { orders = result.Orders, totalCount = result.TotalCount });
        }

        /// <summary>
        /// Creates a new order
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderCommand cmd)
        {
            // Validate the command
            var validationResult = await _placeOrderValidator.ValidateAsync(cmd);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

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

        public class UpdateStatusRequest { public string Status { get; set; } = string.Empty; }

        /// <summary>
        /// Updates the status of an order
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Manager")] // Only Admin and Manager can update status
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest req)
        {
            var command = new UpdateOrderStatusCommand(id, req.Status);

            // Validate the command
            var validationResult = await _updateStatusValidator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var ok = await _updateStatus.Handle(command);
            if (!ok) throw new KeyNotFoundException($"Order with ID {id} not found");

            return NoContent();
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