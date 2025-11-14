using CLDV6212_POE.Data;
using CLDV6212_POE.Models;
using CLDV6212_POE.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLDV6212_POE.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if email already exists
            var existingUser = await _db.Customers
                .FirstOrDefaultAsync(c => c.Email == model.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError("", "An account with that email already exists.");
                return View(model);
            }

            // Create new customer
            var newCustomer = new Customer
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                Password = model.Password,  // Plain text (POE OK)
                Role = "Customer"
            };

            _db.Customers.Add(newCustomer);
            await _db.SaveChangesAsync();

            // Auto-login after registration
            HttpContext.Session.SetString("UserId", newCustomer.Id);
            HttpContext.Session.SetString("UserName", newCustomer.FirstName);
            HttpContext.Session.SetString("UserRole", newCustomer.Role);

            return RedirectToAction("Index", "Home");
        }

        // Show the login form
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // Handle login POST
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Find user in SQL DB by Email + Password
            var user = await _db.Customers
                .FirstOrDefaultAsync(c => c.Email == model.Email && c.Password == model.Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // Store basic user info in Session
            HttpContext.Session.SetString("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FirstName);
            HttpContext.Session.SetString("UserRole", user.Role);

            await HttpContext.Session.CommitAsync();

            // Admin -> admin orders, Customer -> home
            if (user.Role == "Admin")
                return RedirectToAction("AdminOrders", "Cart");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
