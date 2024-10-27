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

         public IActionResult Fitness()
        {
            return View();
        }

        public IActionResult Nutrition()
        {
            return View();
        }

         public IActionResult Progress()
        {
            return View();
        }
        
        public IActionResult Recipes()
        {
            return View();
        }
        public IActionResult Main()
        {
             var username = HttpContext.Session.GetString("Username");

            // Pass the username to the view using ViewBag or a ViewModel
            ViewBag.Username = username; 

            return View();
        }
    }
}
