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

        // Endpoint to request password reset
        [HttpGet("RequestPasswordReset")]
        public IActionResult RequestPasswordReset()
        {
            return View("~/Views/Home/RequestPasswordReset.cshtml");
        }

        [HttpPost("RequestPasswordReset")]
        public async Task<IActionResult> RequestPasswordReset([FromForm] PasswordResetRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Home/RequestPasswordReset.cshtml", request);
            }

            _logger.LogInformation("RequestPasswordReset called with email: {Email}", request.Email);

            // Check if the email exists in the database
            var userExists = _context.Users.Any(u => u.Email == request.Email);
            if (!userExists)
            {
                _logger.LogWarning("No user found with email: {Email}", request.Email);
                ViewBag.ErrorMessage = "No user found with the provided email address.";
                return View("~/Views/Home/RequestPasswordReset.cshtml", request);
            }

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

        // Method to generate password reset token
        private string GeneratePasswordResetToken(string email)
        {
            var token = Guid.NewGuid().ToString();
            // Save the token and email to the database with an expiration time
            SaveTokenToDatabase(email, token, DateTime.UtcNow.AddHours(1));
            return token;
        }

        // Method to save token to the database
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

        // Method to send password reset email
        private async Task SendPasswordResetEmailAsync(string email, string token)
        {
            var resetLink = Url.Action(nameof(PasswordResetController.ResetPassword), "PasswordReset", new { token }, Request.Scheme);
            var subject = "Password Reset Request";
            var body = $"Please reset your password by clicking <a href=\"{resetLink}\">here</a>.";

            // Use your email sender API to send the email
            await _emailSender.SendEmailAsync(email, subject, body);
        }

        // Endpoint to reset password
        [HttpGet("ResetPassword")]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("A token is required for password reset.");
            }

            var model = new ResetPasswordModel { Token = token };
            return View("~/Views/Home/ResetPassword.cshtml", model);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Home/ResetPassword.cshtml", model);
            }

            var passwordResetToken = _context.PasswordResetTokens.FirstOrDefault(t => t.Token == model.Token && t.Expiration > DateTime.UtcNow);
            if (passwordResetToken == null)
            {
                return BadRequest("Invalid or expired token.");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == passwordResetToken.Email);
            if (user == null)
            {
                return BadRequest("Invalid email address.");
            }

            // Reset the password and hash it
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.PasswordResetTokens.Remove(passwordResetToken); // Remove the token after use
            await _context.SaveChangesAsync();

            return Ok("Password has been reset successfully.");
        }
    }
}