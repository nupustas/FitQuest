using FitQuest.Data;
using FitQuest.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net; // Add BCrypt.Net for hashing

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
        public IActionResult Register(string username, string email, string password, string confirmPassword)
        {
            // Check if passwords match
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                return View("../Home/Register");
            }

            // Check if the username or email already exists
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == username);
            var existingEmail = _context.Users.FirstOrDefault(u => u.Email == email);
            
            if (existingUser != null)
            {
                ViewBag.Error = "Username is taken";
                return View("../Home/Register");
            }

            if (existingEmail != null)
            {
                ViewBag.Error = "Email is already registered";
                return View("../Home/Register");
            }

            // Hash the password using BCrypt
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            // Add the new user with the hashed password
            var user = new User { Username = username, Email = email, Password = hashedPassword };
            _context.Users.Add(user);

            try
            {
                _context.SaveChanges();  // Save to the database
           
                HttpContext.Session.SetString("Username", user.Username); // Set the username in the session
            }
            catch (DbUpdateException)
            {
                ViewBag.Error = "An error occurred while saving the user. Please try again later.";
                return View("../Home/Register");
            }

            return RedirectToAction("Main", "Home");
        }

        [HttpPost]
        public IActionResult Login(string usernameOrEmail, string password)
        {
            // Find the user by username or email
            var user = _context.Users.FirstOrDefault(u => 
                u.Username == usernameOrEmail || u.Email == usernameOrEmail);

            if (user != null)
            {
                // Verify the password using BCrypt
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
                
                if (isPasswordValid)
                {
                    HttpContext.Session.SetString("Username", user.Username);
                    return RedirectToAction("Main", "Home");
                }
            }

            ViewBag.Error = "Invalid login credentials";
            return View("../Home/Login");
        }
    }
}
