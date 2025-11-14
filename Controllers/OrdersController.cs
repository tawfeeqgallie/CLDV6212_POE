using CLDV6212_POE.Models;
using CLDV6212_POE.Services;
using Microsoft.AspNetCore.Mvc;

namespace CLDV6212_POE.Controllers
{
    public class OrdersController : Controller
    {
        private readonly InMemoryRepository _repo;

        public OrdersController(InMemoryRepository repo)
        {
            _repo = repo;
        }

        private string? GetCustomerId()
        {
            return HttpContext.Session.GetString("UserId");
        }

        // Customer order view
        public IActionResult Index()
        {
            var customerId = GetCustomerId();
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            var cart = _repo.GetCart(customerId);
            return View(cart);
        }

        // Add to cart
        public IActionResult Add(string productId, int quantity, decimal price)
        {
            var customerId = GetCustomerId();
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            _repo.AddToCart(customerId, productId, quantity, price);
            return RedirectToAction("Index");
        }

        // Checkout
        public IActionResult Checkout()
        {
            var customerId = GetCustomerId();
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            _repo.Checkout(customerId);
            return RedirectToAction("Index", "Products");
        }

        // Admin dashboard
        public IActionResult Admin()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
                return Unauthorized();

            var orders = _repo.GetAllOrders();
            return View(orders);
        }

        // Admin processing
        public IActionResult Process(string id)
        {
            _repo.ProcessOrder(id);
            return RedirectToAction("Admin");
        }
    }
}
