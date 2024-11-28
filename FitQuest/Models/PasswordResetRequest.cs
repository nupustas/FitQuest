using System.ComponentModel.DataAnnotations;

namespace FitQuest.Models
{
    public class PasswordResetRequest
    {
        [Required(ErrorMessage = "The Email field is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
    }
}