using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class GroqCloudAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly ILogger<GroqCloudAIService> _logger;

    public GroqCloudAIService(HttpClient httpClient, IConfiguration configuration, ILogger<GroqCloudAIService> logger)
{
    _httpClient = httpClient;
    _logger = logger;

    // Load API configuration
    var groqConfig = configuration.GetSection("GroqCloudAI");
    _apiKey = groqConfig["ApiKey"];
    _endpoint = groqConfig["Endpoint"];

    if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_endpoint))
    {
        throw new InvalidOperationException("GroqCloud AI API key or endpoint is not configured.");
    }

    _httpClient.BaseAddress = new Uri(_endpoint);
    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
}


    public async Task<string> GenerateWorkoutPlan(UserData userData)
{
    var prompt = $"Generate a workout plan (do not include leg or cardio days) for a user with the following details:\n" +
                 $"- Age: {userData.Age}\n" +
                 $"- Height: {userData.Height} cm\n" +
                 $"- Weight: {userData.Weight} kg\n" +
                 $"- Goal: {userData.Goals}\n" +
                 $"- Workout Frequency: {userData.WorkoutFrequency} times per week\n" +
                 $"- Gender: {userData.Gender}\n";

    var payload = new
    {
        messages = new[]
        {
            new { role = "user", content = prompt }
        },
        model = "llama3-8b-8192"
    };

    string jsonPayload = JsonSerializer.Serialize(payload);

    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

    try
    {
        var response = await _httpClient.PostAsync("", content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Raw Response Content: {ResponseContent}", responseContent);

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var groqResponse = JsonSerializer.Deserialize<GroqCloudResponse>(responseContent, options);

            // Extract the workout plan content
            var workoutPlan = groqResponse?.Choices?.FirstOrDefault()?.Message?.Content;

            if (!string.IsNullOrEmpty(workoutPlan))
            {
                return workoutPlan;
            }

            return "The AI did not generate a valid workout plan. Please try again.";
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error Response: {ErrorContent}", errorContent);
            throw new Exception($"API error: {response.StatusCode}, {errorContent}");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError("An error occurred: {Message}", ex.Message);
        throw;
    }
}


}

public class GroqCloudResponse
{
    public string Id { get; set; }
    public string Object { get; set; }
    public long Created { get; set; }
    public string Model { get; set; }
    public List<Choice> Choices { get; set; }
    public ResponseUsage Usage { get; set; } // Renamed

    public class Choice
    {
        public int Index { get; set; }
        public Message Message { get; set; }
        public string FinishReason { get; set; }
    }

    public class Message
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    public class ResponseUsage // Renamed from 'Usage'
    {
        public double QueueTime { get; set; }
        public int PromptTokens { get; set; }
        public double PromptTime { get; set; }
        public int CompletionTokens { get; set; }
        public double CompletionTime { get; set; }
        public int TotalTokens { get; set; }
        public double TotalTime { get; set; }
    }
}



public class UserData
{
    [Key]
    public int UserId { get; set; }
    public int Age { get; set; }
    public int Height { get; set; }
    public int Weight { get; set; }
    public string Goals { get; set; }
    public int WorkoutFrequency { get; set; }
    public string Gender { get; set; }
}
