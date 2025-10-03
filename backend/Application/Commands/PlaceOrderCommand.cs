namespace OrderManagement.Application.Commands
{
    public record PlaceOrderCommand(string CustomerName, IEnumerable<PlaceOrderLine> Lines);
}