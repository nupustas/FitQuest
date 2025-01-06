using Microsoft.AspNetCore.Mvc;
using FitQuest.Services;
using FitQuest.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FitQuest.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FitQuest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PasswordResetController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<PasswordResetController> _logger;
        private readonly ApplicationDbContext _context;

        public PasswordResetController(IEmailSender emailSender, ILogger<PasswordResetController> logger, ApplicationDbContext context)
        {
            _emailSender = emailSender;
            _logger = logger;
            _context = context;
        }

        // GET: Request Password Reset
        [HttpGet("RequestPasswordReset")]
        public IActionResult RequestPasswordReset()
        {
            return View("~/Views/Home/RequestPasswordReset.cshtml");
        }

        // POST: Handle Password Reset Request
        [HttpPost("RequestPasswordReset")]
        public async Task<IActionResult> RequestPasswordReset([FromForm] PasswordResetRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Home/RequestPasswordReset.cshtml", request);
            }

            _logger.LogInformation("RequestPasswordReset called with email: {Email}", request.Email);

            // Check if the email exists
            var userExists = _context.Users.Any(u => u.Email == request.Email);
            if (!userExists)
            {
                _logger.LogWarning("No user found with email: {Email}", request.Email);
                ViewBag.ErrorMessage = "No user found with the provided email address.";
                return View("~/Views/Home/RequestPasswordReset.cshtml", request);
            }

            // Generate and send token
            var token = GeneratePasswordResetToken(request.Email);
            try
            {
                await SendPasswordResetEmailAsync(request.Email, token);
                _logger.LogInformation("Password reset email sent to {Email}", request.Email);
                return RedirectToAction("ResetPasswordConfirmation", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", request.Email);
                ViewBag.ErrorMessage = "An error occurred while sending the password reset email.";
                return View("~/Views/Home/RequestPasswordReset.cshtml", request);
            }
        }

        // GET: Reset Password
        [HttpGet("ResetPassword")]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required.");
            }

            ViewData["Token"] = token; // Pass the token to the view
            return View("~/Views/Home/ResetPassword.cshtml");
        }

        // POST: Handle Password Reset
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromForm] string token, [FromForm] string newPassword, [FromForm] string confirmPassword)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                return BadRequest(new { error = "Token, new password, and confirm password are required." });
            }

            if (newPassword != confirmPassword)
            {
                return BadRequest(new { error = "Passwords do not match." });
            }

            var passwordResetToken = _context.PasswordResetTokens
                .FirstOrDefault(t => t.Token == token && t.Expiration > DateTime.UtcNow);
            if (passwordResetToken == null)
            {
                return BadRequest(new { error = "Invalid or expired token." });
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == passwordResetToken.Email);
            if (user == null)
            {
                return BadRequest(new { error = "Invalid email address." });
            }

            // Update the user's password
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.PasswordResetTokens.Remove(passwordResetToken); // Remove token after use
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password has been reset successfully." });
        }

        // Helper: Generate password reset token
        private string GeneratePasswordResetToken(string email)
        {
            var token = Guid.NewGuid().ToString();
            SaveTokenToDatabase(email, token, DateTime.UtcNow.AddHours(1));
            return token;
        }

        // Helper: Save token to the database
        private void SaveTokenToDatabase(string email, string token, DateTime expiration)
        {
            var passwordResetToken = new PasswordResetToken
            {
                Email = email,
                Token = token,
                Expiration = expiration
            };
            _context.PasswordResetTokens.Add(passwordResetToken);
            _context.SaveChanges();
        }

        // Helper: Send password reset email
        private async Task SendPasswordResetEmailAsync(string email, string token)
        {
            var resetLink = Url.Action(nameof(ResetPassword), "PasswordReset", new { token }, Request.Scheme);
            var subject = "Password Reset Request";
            var body = $"Please reset your password by clicking <a href=\"{resetLink}\">here</a>.";

            await _emailSender.SendEmailAsync(email, subject, body);
        }
    }
}