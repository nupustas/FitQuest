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
        public string Goals { get; set; }

        [Required]
        public int WorkoutFrequency { get; set; } // Days per week

        [Required]
        public string Gender { get; set; }

        public string? MondayWorkout { get; set; }
        public string? TuesdayWorkout { get; set; }
        public string? WednesdayWorkout { get; set; }
        public string? ThursdayWorkout { get; set; }
        public string? FridayWorkout { get; set; }
        public string? SaturdayWorkout { get; set; }
        public string? SundayWorkout { get; set; }
    }
}
