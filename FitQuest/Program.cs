using FitQuest.Data;
using FitQuest.Models;
using FitQuest.Services;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var connection = new MySqlConnectionStringBuilder(connectionString)
    {
        SslMode = MySqlSslMode.None,
        SslCa = "certs/server-ca.pem",
        SslCert = "certs/client-cert.pem",
        SslKey = "certs/client-key.pem"
    };
    options.UseMySql(connection.ConnectionString, new MySqlServerVersion(new Version(8, 0, 21)));
});

// Test the database connection
TestDatabaseConnection(builder.Configuration.GetConnectionString("DefaultConnection"));

void TestDatabaseConnection(string connectionString)
{
    try
    {
        var connection = new MySqlConnectionStringBuilder(connectionString)
        {
            SslMode = MySqlSslMode.Required,
            SslCa = "certs/server-ca.pem",
            SslCert = "certs/client-cert.pem",
            SslKey = "certs/client-key.pem"
        };
        using var conn = new MySqlConnection(connection.ConnectionString);
        conn.Open();
        Console.WriteLine("Database connection successful.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database connection failed: {ex.Message}");
    }
}

// Configure GroqCloud AI settings and register the GroqCloudAIService
builder.Services.Configure<GroqCloudAIConfig>(builder.Configuration.GetSection("GroqCloudAI"));
builder.Services.AddHttpClient<GroqCloudAIService>(); // Register GroqCloudAIService with HttpClient



// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict; // Optional: Adds stricter cookie policy
});

builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddHttpClient<IDietPlanService, DietPlanService>(); // Register DietPlanService
builder.Services.AddTransient<SpoonacularService>();
builder.Services.AddTransient<RecipeService>();


builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ReturnUrlParameter = "returnUrl";
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.Redirect("/Home");
        return Task.CompletedTask;
    };
});

// Configure CORS to allow all origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Apply the CORS policy
app.UseCors("AllowAllOrigins");

app.UseSession();

// Comment this out if not using authentication
// app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();