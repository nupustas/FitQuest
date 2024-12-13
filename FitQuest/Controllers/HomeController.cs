using FitQuest.Data;
using FitQuest.Models;
using FitQuest.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FitQuest.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDietPlanService _dietPlanService;
        private readonly ApplicationDbContext _context;

        public HomeController(IDietPlanService dietPlanService, ApplicationDbContext context)
        {
            _dietPlanService = dietPlanService;
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Logout()
        {
            // Clear the session data
            HttpContext.Session.Clear();

            // Redirect to the login page
            return RedirectToAction("Index");
        }

        public IActionResult Fitness()
        {
            return View();
        }

        public IActionResult Progress()
        {
            return View();
        }
        
        public IActionResult Recipes()
        {
            return View();
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            // Fetch the user's existing profile from the database
            var userData = await _context.UserData.FirstOrDefaultAsync(u => u.UserId == userId);
            Console.WriteLine($"Fetched UserData: Age={userData?.Age}, Height={userData?.Height}, Weight={userData?.Weight}, Goals={userData?.Goals}, Frequency={userData?.WorkoutFrequency}, Gender={userData?.Gender}");

            if (userData == null)
            {
                // If no profile exists, create an empty model or show a message
                userData = new UserData();
            }

            // Return the profile data to the view
            return View(userData);
        }

        [HttpGet]
        public IActionResult Nutrition()
        {
            HttpContext.Session.GetString("Username");
            return View();
        }

        [HttpPost]
public async Task<IActionResult> GenerateNutritionPlan()
{
    // Retrieve UserId from session as an integer
    var userId = HttpContext.Session.GetInt32("UserId");

    // Check if the userId is null or not set in session
    if (!userId.HasValue)
    {
        // If the UserId is not available (null), redirect to login page
        return RedirectToAction("Login");
    }

    // Fetch user data from the database using the UserId
    var userData = await _context.UserData.FirstOrDefaultAsync(u => u.UserId == userId.Value);

    // Check if user data exists
    if (userData == null)
    {
        // Pass a message to the view if user data is not found
        ViewBag.ErrorMessage = "Please update your profile information to generate a nutrition plan.";
        return View("Nutrition");
    }

    // Generate the diet plan using the user data
    var dietPlan = await _dietPlanService.GenerateDietPlanAsync(
        userData.Age,
        userData.Gender,
        userData.Weight,
        userData.Height,
        userData.Goals,
        userData.WorkoutFrequency
    );

    // Create the ViewModel to pass to the view
    var viewModel = new DietPlanViewModel
    {
        DietPlan = dietPlan,
        Age = userData.Age,
        Gender = userData.Gender,
        Weight = userData.Weight,
        Height = userData.Height,
        ExerciseGoal = userData.Goals,
        ExerciseFrequency = userData.WorkoutFrequency
    };

    // Return the generated diet plan to the view
    return View("Nutrition", viewModel);
    }

    [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}