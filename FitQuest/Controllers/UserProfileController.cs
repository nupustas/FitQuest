using Microsoft.AspNetCore.Mvc;
using FitQuest.Models;
using FitQuest.Data;
using Microsoft.EntityFrameworkCore; // Import this for session handling

namespace FitQuest.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Retrieve the UserId from the session
        private int GetUserId()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                return userId.Value; // Return the user ID
            }

            throw new Exception("User is not logged in.");
        }

        // Display the current profile 
        
        public async Task<IActionResult> Profile()
        {
            var userId = GetUserId();

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

        // Save or update the profile information (POST request)
        [HttpPost]
public async Task<IActionResult> SaveProfile(UserData userData)
{
    if (ModelState.IsValid)
    {
        int userId = GetUserId(); // Ensure this gets the correct user ID

        // Check if user data already exists
        var existingProfile = await _context.UserData.SingleOrDefaultAsync(u => u.UserId == userId);

        if (existingProfile != null)
        {
            // Update existing profile
            existingProfile.Age = userData.Age;
            existingProfile.Height = userData.Height;
            existingProfile.Weight = userData.Weight;
            existingProfile.Goals = userData.Goals;
            existingProfile.WorkoutFrequency = userData.WorkoutFrequency;
            existingProfile.Gender = userData.Gender;

            _context.UserData.Update(existingProfile);
        }
        else
        {
            // Insert new profile
            userData.UserId = userId;
            _context.UserData.Add(userData);
        }

        // Save changes
        await _context.SaveChangesAsync();

        // Redirect to the profile view
        return RedirectToAction("Profile", "Home");
    }

            // Return to the profile view with errors if validation fails
            return View("Profile", userData);
        }


    }
}
