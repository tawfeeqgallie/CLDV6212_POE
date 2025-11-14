using System;
using System.Collections.Generic;

namespace CLDV6212_POE.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string CustomerId { get; set; } = string.Empty;
        public Customer? Customer { get; set; }

        public string Status { get; set; } = "Cart";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<OrderLine> Lines { get; set; } = new List<OrderLine>();
    }

    public class OrderLine
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public string ProductId { get; set; } = string.Empty;
        public Product? Product { get; set; }

        public string ProductName => Product?.Name ?? "(Unknown)";

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal Total => Quantity * UnitPrice;
    }
}
