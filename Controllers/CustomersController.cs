using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CLDV6212_POE.Models;
using CLDV6212_POE.Services;
using System.Linq;
using System;

namespace CLDV6212_POE.Controllers
{
    public class CustomersController : Controller
    {
        private readonly FunctionsClient _functions;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(FunctionsClient functions, ILogger<CustomersController> logger)
        {
            _functions = functions;
            _logger = logger;
        }

        // GET: /Customers
        public IActionResult Index()
        {
            try
            {
                var customers = _functions.GetCustomers(); // calls GET /api/customers
                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch customers from Function.");
                ViewBag.Error = "Could not load customers.";
                return View(Enumerable.Empty<Customer>());
            }
        }

        // GET: /Customers/Create
        [HttpGet]
        public IActionResult Create() => View();

        // POST: /Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Customer model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                _functions.SaveCustomer(new
                {
                    model.FirstName,
                    model.LastName,
                    model.Email
                });

                return RedirectToAction(nameof(Index)); // reloads from table via Function
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save customer via Function.");
                ModelState.AddModelError(string.Empty, "Could not save customer. Please try again.");
                return View(model);
            }
        }
    }
}
