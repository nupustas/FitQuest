using Microsoft.AspNetCore.Mvc;
using FitQuest.Models;
using FitQuest.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.AspNetCore.Http;
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
            // Retrieve the UserId from the session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                return userId.Value; // Return the user ID
            }

            throw new Exception("User is not logged in.");
        }

        // Save or update the profile information
        [HttpPost]
        public async Task<IActionResult> SaveProfile(UserData userData)
        {

            
            if (ModelState.IsValid)
            {
                int userId = GetUserId(); 

                // Check if the user already has profile data
                var existingProfile = await _context.UserData.SingleOrDefaultAsync(u => u.UserId == userId);
                if (existingProfile != null)
                {
                    // Update existing data
                    existingProfile.Age = userData.Age;
                    existingProfile.Height = userData.Height;
                    existingProfile.Weight = userData.Weight;
                    existingProfile.Goals = userData.Goals;
                    existingProfile.WorkoutFrequency = userData.WorkoutFrequency;
                    existingProfile.Gender = userData.Gender;

                    // Save changes for the updated profile
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Create new UserData and set UserId
                    var newUserData = new UserData
                    {
                        UserId = userId, // Set UserId for the new entry
                        Age = userData.Age,
                        Height = userData.Height,
                        Weight = userData.Weight,
                        Goals = userData.Goals,
                        WorkoutFrequency = userData.WorkoutFrequency,
                        Gender = userData.Gender
                    };

                    _context.UserData.Add(newUserData); // Add new UserData entry
                    await _context.SaveChangesAsync(); // Save changes to the database
                }

                return RedirectToAction("Profile", "Home");
            }

           

            return RedirectToAction("Profile", "Home");
        }

    

    }
}
