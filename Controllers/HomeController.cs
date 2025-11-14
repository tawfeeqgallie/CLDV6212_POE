using Microsoft.AspNetCore.Mvc;

namespace CLDV6212_POE.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
