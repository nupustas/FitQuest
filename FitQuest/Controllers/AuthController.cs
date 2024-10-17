using FitQuest.Data;
using FitQuest.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
            // Check if passwords match
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                return View("../Home/Register");
            }

            // Check if the username already exists
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == username);
            if (existingUser != null)
            {
                ViewBag.Error = "Username is taken, please choose a different one";
                return View("../Home/Register");
            }

            // Add the new user
            var user = new User { Username = username, Password = password };
            _context.Users.Add(user);

            try
            {
                _context.SaveChanges();  // Save to the database
            }
            catch (DbUpdateException ex)
            {
                // Handle any database exceptions (e.g., unique constraint violation)
                ViewBag.Error = "An error occurred while saving the user. Please try again later.";
                return View("../Home/Register");
            }

            return RedirectToAction("Login", "Home");
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Find the user by username and password
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
