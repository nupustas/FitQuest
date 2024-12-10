using FitQuest.Models;
using FitQuest.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using FitQuest.Data;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq; // For handling JSON-like structures

namespace FitQuest.Controllers
{
    public class WorkoutController : Controller
    {
        private readonly GroqCloudAIService _groqCloudAIService;
        private readonly ApplicationDbContext _dbContext;

        public WorkoutController(GroqCloudAIService groqCloudAIService, ApplicationDbContext dbContext)
        {
            _groqCloudAIService = groqCloudAIService;
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateWorkoutPlan()
        {
            // Retrieve the user ID from the session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                ViewBag.Error = "User not logged in. Please log in to generate a workout plan.";
                return View("~/Views/Home/Fitness.cshtml"); // Render Fitness view directly
            }

            // Retrieve the user's data from the database
            var userData = await _dbContext.UserData.FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (userData == null)
            {
                ViewBag.Error = "User data not found. Enter your data in the profile page.";
                return View("~/Views/Home/Fitness.cshtml"); // Render Fitness view directly
            }

            // Check if workout data is already in the database and not empty
            if (!string.IsNullOrEmpty(userData.MondayWorkout) || 
                !string.IsNullOrEmpty(userData.TuesdayWorkout) ||
                !string.IsNullOrEmpty(userData.WednesdayWorkout) ||
                !string.IsNullOrEmpty(userData.ThursdayWorkout) ||
                !string.IsNullOrEmpty(userData.FridayWorkout) ||
                !string.IsNullOrEmpty(userData.SaturdayWorkout) ||
                !string.IsNullOrEmpty(userData.SundayWorkout))
            {
                // If workout data exists, display it
                ViewBag.MondayWorkout = userData.MondayWorkout;
                ViewBag.TuesdayWorkout = userData.TuesdayWorkout;
                ViewBag.WednesdayWorkout = userData.WednesdayWorkout;
                ViewBag.ThursdayWorkout = userData.ThursdayWorkout;
                ViewBag.FridayWorkout = userData.FridayWorkout;
                ViewBag.SaturdayWorkout = userData.SaturdayWorkout;
                ViewBag.SundayWorkout = userData.SundayWorkout;

                return View("~/Views/Home/Fitness.cshtml"); // Render Fitness view directly
            }

            // Generate workout plan using GroqCloud AI
            string workoutPlan;
            try
            {
                workoutPlan = await _groqCloudAIService.GenerateWorkoutPlan(userData);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"An error occurred while generating the workout plan: {ex.Message}";
                return View("~/Views/Home/Fitness.cshtml"); // Render Fitness view directly
            }

            // Regular expression to match "Day X: ...", where X is the day number
            var regex = new Regex(@"(Day \d+:)(.*?)((?=Day \d+:)|$)", RegexOptions.Singleline);

            // Match all the days in the workout plan
            var matches = regex.Matches(workoutPlan);

            // Loop through the matches and assign each to the appropriate day
            for (int i = 0; i < matches.Count; i++)
            {
                var dayWorkout = matches[i].Groups[2].Value.Trim();

                // If the day contains "Rest Day" in the structured format, handle it
                if (dayWorkout.Contains("Rest Day"))
                {
                    // Attempt to parse the JSON-like structure
                    try
                    {
                        var jsonObj = JObject.Parse(dayWorkout); // Parse the string to a JObject

                        // Check if it's a rest day and adjust display
                        if (jsonObj["Exercises"] != null && jsonObj["Exercises"].ToString().Contains("Rest Day"))
                        {
                            dayWorkout = "Rest Day";
                        }
                    }
                    catch (Exception)
                    {
                        // If parsing fails, assume it is not in JSON format and fallback to the default
                        dayWorkout = "Rest Day";
                    }
                }
                else
                {
                    if (dayWorkout.Contains("**Additional Tips:**"))
                    {
                        dayWorkout = dayWorkout.Split("**Additional Tips:**")[0].Trim();
                    }
                    if (dayWorkout.Contains("**Additional Tips**"))
                    {
                        dayWorkout = dayWorkout.Split("**Additional Tips**")[0].Trim();
                    }
                }

                // Assign the workout text to the corresponding day
                switch (i)
                {
                    case 0:
                        userData.MondayWorkout = dayWorkout;
                        break;
                    case 1:
                        userData.TuesdayWorkout = dayWorkout;
                        break;
                    case 2:
                        userData.WednesdayWorkout = dayWorkout;
                        break;
                    case 3:
                        userData.ThursdayWorkout = dayWorkout;
                        break;
                    case 4:
                        userData.FridayWorkout = dayWorkout;
                        break;
                    case 5:
                        userData.SaturdayWorkout = dayWorkout;
                        break;
                    case 6:
                        userData.SundayWorkout = dayWorkout;
                        break;
                }
            }

            // Save the updated user data to the database
            _dbContext.UserData.Update(userData);
            await _dbContext.SaveChangesAsync();

            // Pass the workout data for each day to the view
            ViewBag.MondayWorkout = userData.MondayWorkout;
            ViewBag.TuesdayWorkout = userData.TuesdayWorkout;
            ViewBag.WednesdayWorkout = userData.WednesdayWorkout;
            ViewBag.ThursdayWorkout = userData.ThursdayWorkout;
            ViewBag.FridayWorkout = userData.FridayWorkout;
            ViewBag.SaturdayWorkout = userData.SaturdayWorkout;
            ViewBag.SundayWorkout = userData.SundayWorkout;

            // Return the Fitness view with data
            return View("~/Views/Home/Fitness.cshtml"); // Render Fitness view directly
        }
    }
}
