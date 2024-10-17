using FitQuest.Data;
using FitQuest.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace FitQuest.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Register(string username, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                return View("../Home/Register");
            }

            var user = new User { Username = username, Password = password };
            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login", "Home");
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                return RedirectToAction("Main", "Home");
            }
            ViewBag.Error = "Invalid login credentials";
            return View("../Home/Login");
        }
    }
}
