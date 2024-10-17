using FitQuest.Models;
using Microsoft.EntityFrameworkCore;

namespace FitQuest.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        // No need for Fluent API in this case
        // We will create a unique constraint in the migration
    }
}
