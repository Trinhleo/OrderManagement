using System;
using System.Collections.Generic;
using OrderManagement.Domain.ValueObjects;

namespace OrderManagement.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        private readonly List<OrderLine> _lines = new();
        public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public string CustomerName { get; private set; }
        public string Status { get; private set; }

        public Order(string customerName, string status)
        {
            CustomerName = customerName;
            Status = status;
        }

        // For EF
        private Order() { }

        public void AddLine(string product, int quantity, Money price)
        {
            if (quantity <= 0) throw new InvalidOperationException("Quantity must be positive");
            _lines.Add(new OrderLine(product, quantity, price));
        }
    }

    public class OrderLine
    {
        public int Id { get; private set; } // EF Core auto increment within order
        public Guid OrderId { get; private set; } // Needed for composite key & FK
        public string Product { get; private set; }
        public int Quantity { get; private set; }
        public Money Price { get; private set; }

        public OrderLine(string product, int quantity, Money price)
        {
            Product = product;
            Quantity = quantity;
            Price = price;
        }

        // For EF
        private OrderLine() { }
    }
}