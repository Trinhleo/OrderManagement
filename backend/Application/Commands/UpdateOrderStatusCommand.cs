using System;

namespace OrderManagement.Application.Commands
{
    public record UpdateOrderStatusCommand(Guid OrderId, string Status);
}
