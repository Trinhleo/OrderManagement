namespace OrderManagement.Application.Commands
{
    public record PlaceOrderLine(string Product, int Quantity, decimal Price, string Currency);
}
