using FitQuest.Controllers;
using FitQuest.Data;
using FitQuest.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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
            return View("~/Views/Home/Fitness.cshtml");
        }

        // Retrieve the user's data from the database
        var userData = await _dbContext.UserData.FirstOrDefaultAsync(u => u.UserId == userId.Value);

        if (userData == null)
        {
            ViewBag.Error = "User data not found. Enter your data in the profile page.";
            return View("~/Views/Home/Fitness.cshtml");
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
            return View("~/Views/Home/Fitness.cshtml");
        }

        // Pass the generated workout plan to the view
        ViewBag.WorkoutPlan = workoutPlan;
        return View("~/Views/Home/Fitness.cshtml", workoutPlan);

    }
}
