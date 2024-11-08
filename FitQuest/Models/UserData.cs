using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitQuest.Models
{
    public class UserData
    {

        [Key, ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public int Age { get; set; }

        [Required]
        public int Height { get; set; }  // Height in cm

        [Required]
        public int Weight { get; set; }  // Weight in kg

        [Required]
        public string Goals { get; set; }  // e.g., "lose weight", "muscle gain", etc.

        [Required]
        public int WorkoutFrequency { get; set; } // Days per week

        [Required]
        public string Gender { get; set; }
    }
}
