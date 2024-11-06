using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FitQuest.Services
{
    public interface IDietPlanService
    {
        Task<string> GenerateDietPlanAsync(int age, string gender, double weight, double height, string exerciseGoal, int exerciseFrequency);
    }

    public class DietPlanService : IDietPlanService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "14629cbc86de4b4694d7f3c826b9d5a4"; // Replace with your Spoonacular API key

        public DietPlanService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateDietPlanAsync(int age, string gender, double weight, double height, string exerciseGoal, int exerciseFrequency)
        {
            // Calculate BMR (Basal Metabolic Rate) based on user inputs
            double bmr;
            if (gender.ToLower() == "male")
            {
                bmr = 88.362 + (13.397 * weight) + (4.799 * height) - (5.677 * age);
            }
            else
            {
                bmr = 447.593 + (9.247 * weight) + (3.098 * height) - (4.330 * age);
            }

            // Adjust calories based on exercise frequency and goal
            double adjustedCalories = bmr * (1.2 + (exerciseFrequency * 0.1)); // Example adjustment
            if (exerciseGoal.ToLower() == "lose")
            {
                adjustedCalories -= 500;
            }
            else if (exerciseGoal.ToLower() == "gain")
            {
                adjustedCalories += 500;
            }

            var response = await _httpClient.GetAsync($"https://api.spoonacular.com/mealplanner/generate?apiKey={_apiKey}&targetCalories={adjustedCalories}");
            response.EnsureSuccessStatusCode(); // This will throw an exception if the status code is not 2xx

            var responseString = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseString);

            // Format the JSON response into a readable string
            var formattedResponse = FormatDietPlan(json);
            return formattedResponse;
        }

        private string FormatDietPlan(JObject json)
        {
            var result = new System.Text.StringBuilder();

            result.AppendLine("Diet Plan:");

            if (json["week"] != null)
            {
                foreach (var day in json["week"])
                {
                    result.AppendLine($"{day.Path}:");

                    var dayMeals = day.First["meals"];
                    if (dayMeals != null)
                    {
                        foreach (var meal in dayMeals)
                        {
                            result.AppendLine($"- {meal["title"]}");
                            result.AppendLine($"  Ready in {meal["readyInMinutes"]} minutes");
                            result.AppendLine($"  Servings: {meal["servings"]}");
                            result.AppendLine($"  Source URL: {meal["sourceUrl"]}");
                        }
                    }
                    else
                    {
                        result.AppendLine("  No meals found.");
                    }

                    result.AppendLine("  Nutrients:");
                    var dayNutrients = day.First["nutrients"];
                    if (dayNutrients != null)
                    {
                        foreach (var nutrient in dayNutrients.Children<JProperty>())
                        {
                            var nutrientValue = nutrient.Value as JObject;
                            if (nutrientValue != null)
                            {
                                result.AppendLine($"  - {nutrient.Name}: {nutrientValue["amount"]} {nutrientValue["unit"]}");
                            }
                        }
                    }
                    else
                    {
                        result.AppendLine("  No nutrients information found.");
                    }
                }
            }
            else
            {
                result.AppendLine("No weekly meal plan found.");
            }

            return result.ToString();
        }
    }
}