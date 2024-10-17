using Microsoft.AspNetCore.Mvc;

namespace FitQuest.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Main()
        {
            return View();
        }
    }
}
