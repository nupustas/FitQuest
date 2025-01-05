using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class SpoonacularService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.spoonacular.com";
    private readonly string _apiKey;

    public SpoonacularService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Spoonacular:ApiKey"];
    }

    // Search for recipes by query
    public async Task<string> SearchRecipes(string query)
    {
        var url = $"{BaseUrl}/recipes/complexSearch?query={query}&apiKey={_apiKey}";
        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }

        return null;
    }

    // Get detailed information for a specific recipe
    public async Task<string> GetRecipeInformation(int recipeId)
    {
        var url = $"{BaseUrl}/recipes/{recipeId}/information?apiKey={_apiKey}";
        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }

        return null;
    }
}
