using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Models;

namespace UserManagement.Api.Data
{
    // Represents the database session and provides access to the Users table.
    // EF Core uses this to translate LINQ queries into SQL.
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Keep the user table name as "Users"
            builder.Entity<User>().ToTable("Users");
        }
    }
}