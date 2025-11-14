using CLDV6212_POE.Models;
using System.Collections.Generic;
using System.Linq;

namespace CLDV6212_POE.Services
{
    public class InMemoryRepository
    {
        // In-memory product list (if needed)
        public List<Product> Products { get; set; } = new List<Product>();

        // In-memory order list
        public List<Order> Orders { get; set; } = new List<Order>();

        // Add product to user's cart
        public void AddToCart(string customerId, string productId, int quantity, decimal unitPrice)
        {
            // Find customer's active cart
            var cart = Orders.FirstOrDefault(o => o.CustomerId == customerId && o.Status == "Cart");

            if (cart == null)
            {
                cart = new Order
                {
                    CustomerId = customerId,
                    Status = "Cart",
                };
                Orders.Add(cart);
            }

            // Check if product is already in cart
            var line = cart.Lines.FirstOrDefault(l => l.ProductId == productId);
            if (line == null)
            {
                cart.Lines.Add(new OrderLine
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = unitPrice
                });
            }
            else
            {
                line.Quantity += quantity;
            }
        }

        // Get a customer's cart
        public Order? GetCart(string customerId)
        {
            return Orders.FirstOrDefault(o => o.CustomerId == customerId && o.Status == "Cart");
        }

        // Checkout: mark cart as pending
        public void Checkout(string customerId)
        {
            var cart = GetCart(customerId);
            if (cart != null)
            {
                cart.Status = "Pending";
            }
        }

        // Admin: get all non-cart orders
        public List<Order> GetAllOrders()
        {
            return Orders.Where(o => o.Status != "Cart").ToList();
        }

        // Admin: mark as processed
        public void ProcessOrder(string id)
        {
            if (!int.TryParse(id, out int orderId))
                return; // or throw error

            var order = Orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
                order.Status = "Processed";
        }
    }
}
