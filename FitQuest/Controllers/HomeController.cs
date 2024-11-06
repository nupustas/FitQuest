using FitQuest.Models;
using FitQuest.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FitQuest.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDietPlanService _dietPlanService;

        public HomeController(IDietPlanService dietPlanService)
        {
            _dietPlanService = dietPlanService;
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

        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult Main()
        {
            var username = HttpContext.Session.GetString("Username");

            // Pass the username to the view using ViewBag or a ViewModel
            ViewBag.Username = username; 

            return View();
        }

        [HttpGet]
        public IActionResult Nutrition()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Nutrition(string diet, int calories)
        {
            var dietPlan = await _dietPlanService.GenerateDietPlanAsync(diet, calories);

            // Log the raw API response for debugging
            System.Diagnostics.Debug.WriteLine(dietPlan);

            var viewModel = new DietPlanViewModel
            {
                DietPlan = dietPlan
            };
            return View(viewModel);
        }
    }
}