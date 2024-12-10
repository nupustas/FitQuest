using System.Collections.Generic;

namespace FitQuest.Models
{
    public class WorkoutDay
    {
        public string Day { get; set; }
        public List<string> Exercises { get; set; }

        public WorkoutDay(string day)
        {
            Day = day;
            Exercises = new List<string>();
        }
    }
}
