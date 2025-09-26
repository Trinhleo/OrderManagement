namespace OrderManagement.Application.Commands
{
    public record PlaceOrderCommand(string Product, int Quantity, decimal Price, string Currency);
}