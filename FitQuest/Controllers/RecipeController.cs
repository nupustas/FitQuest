using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class RecipeController : ControllerBase
{
    private readonly SpoonacularService _spoonacularService;
    private readonly RecipeService _recipeService;

    public RecipeController(SpoonacularService spoonacularService, RecipeService recipeService)
    {
        _spoonacularService = spoonacularService;
        _recipeService = recipeService;
    }

    // Search Recipes from Spoonacular
    [HttpGet("search")]
    public async Task<IActionResult> Search(string query)
    {
        var result = await _spoonacularService.SearchRecipes(query);

        if (result == null)
        {
            return BadRequest("Error fetching recipes.");
        }

        return Ok(result);
    }

    // Save Recipe to the database
    [HttpPost("save")]
    public async Task<IActionResult> SaveRecipe([FromBody] RecipeModel recipe)
    {
        // Debug: Print incoming recipe data
        Console.WriteLine($"Received Save Recipe request: Title = {recipe.Title}, SourceUrl = {recipe.SourceUrl}, UserId = {recipe.UserId}");

        // Ensure the recipe contains valid data
        if (string.IsNullOrEmpty(recipe.Title) || string.IsNullOrEmpty(recipe.SourceUrl) || string.IsNullOrEmpty(recipe.UserId))
        {
            Console.WriteLine("Invalid recipe data: Title, SourceUrl, or UserId is missing.");
            return BadRequest("Recipe data is incomplete.");
        }

        var result = await _recipeService.SaveRecipe(recipe);

        if (result)
        {
            // Debug: Success message
            Console.WriteLine($"Successfully saved recipe: {recipe.Title}");
            return Ok(new { message = "Recipe saved successfully" });
        }

        // Debug: Failure message
        Console.WriteLine($"Failed to save recipe: {recipe.Title}");
        return BadRequest("Error saving recipe.");
    }

    // Get Saved Recipes for a User
    [HttpGet("saved")]
    public async Task<IActionResult> GetSavedRecipes(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID is required.");
        }

        var recipes = await _recipeService.GetSavedRecipes(userId);
        if (recipes == null)
        {
            return StatusCode(500, "Failed to fetch saved recipes.");
        }

        return Ok(recipes);
    }

    // Fetch detailed recipe information (including sourceUrl)
    [HttpGet("details/{id}")]
    public async Task<IActionResult> GetRecipeDetails(int id)
    {
        var result = await _spoonacularService.GetRecipeInformation(id);
        
        if (result == null)
        {
            return BadRequest("Error fetching recipe details.");
        }

        return Ok(result); // This should return the full details including sourceUrl
    }
}
