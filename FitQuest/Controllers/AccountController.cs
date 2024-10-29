using FitQuest.Data;
using FitQuest.Models;
using FitQuest.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitQuest.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailSender _emailSender;

        public AccountController(ApplicationDbContext context, ILogger<AccountController> logger, IEmailSender emailSender)
        {
            _context = context;
            _logger = logger;
            _emailSender = emailSender;
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                return View("~/Views/Home/Login.cshtml");
            }

            // Check if the username or email already exists
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                ViewBag.Error = "Username already exists";
                return View("~/Views/Home/Login.cshtml");
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "Email already exists";
                return View("~/Views/Home/Login.cshtml");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Username = username, Email = email, Password = hashedPassword };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Store email in session
            HttpContext.Session.SetString("UserEmail", email);

            // Send confirmation email
            var token = "dummy-token"; // Generate a token in a real application
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, token }, Request.Scheme);
            await _emailSender.SendEmailAsync(email, "Confirm your email", $"Please confirm your email by clicking <a href=\"{confirmationLink}\">here</a>.");

            return RedirectToAction("RegistrationConfirmation");
        }

        [HttpGet]
        public IActionResult RegistrationConfirmation()
        {
            return View("~/Views/Home/RegistrationConfirmation.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(int userId, string token)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.EmailConfirmed = true;
            await _context.SaveChangesAsync();

            return Ok("Email confirmed successfully!");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View("~/Views/Home/Login.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string usernameOrEmail, string password, string returnUrl = null)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                HttpContext.Session.SetString("Username", user.Username);
                if (!user.EmailConfirmed)
                {
                    ViewBag.Error = "Please confirm your email before logging in.";
                    return View("~/Views/Home/Login.cshtml");
                }

                // Sign in the user
                HttpContext.Session.SetString("UserId", user.Id.ToString());

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                    
                }
                else
                {
                    return RedirectToAction("Main", "Home");
                }
            }

            ViewBag.Error = "Invalid login credentials";
            return View("~/Views/Home/Login.cshtml");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            // Sign out the user
            HttpContext.Session.Remove("UserId");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ResendConfirmation()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound();
            }

            // Send confirmation email
            var token = "dummy-token"; // Generate a token in a real application
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { userId = user.Id, token }, Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email, "Confirm your email", $"Please confirm your email by clicking <a href=\"{confirmationLink}\">here</a>.");

            ViewBag.Message = "Confirmation email resent. Please check your email.";
            return View("~/Views/Home/RegistrationConfirmation.cshtml");
        }
    }
}