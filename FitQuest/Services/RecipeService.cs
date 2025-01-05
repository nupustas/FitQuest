using Dapper;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RecipeService
{
    private readonly string _connectionString;

    public RecipeService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // Save Recipe to Database
public async Task<bool> SaveRecipe(RecipeModel recipe)
{
    const string query = @"
        INSERT INTO recipes (userId, title, ingredients, instructions, url, image)
        VALUES (@UserId, @Title, @Ingredients, @Instructions, @Url, @Image)";

    using var connection = new MySqlConnection(_connectionString);
    var result = await connection.ExecuteAsync(query, recipe);

    return result > 0;
}


    // Get Saved Recipes for a Specific User
    public async Task<IEnumerable<RecipeModel>> GetSavedRecipes(string userId)
    {
        const string query = @"
            SELECT id, userId, title, ingredients, instructions, url, image, created_at 
            FROM recipes 
            WHERE userId = @UserId 
            ORDER BY created_at DESC";

        using var connection = new MySqlConnection(_connectionString);
        return await connection.QueryAsync<RecipeModel>(query, new { UserId = userId });
    }
}
