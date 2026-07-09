using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Common;
using UserManagement.Api.Data;
using UserManagement.Api.Mappings;
using UserManagement.Api.Models;
using UserManagement.Api.Constants;
using UserManagement.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure custom Validation Error formatting matching ApiResponse<object>
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        var response = ApiResponse<object>.FailureResponse(Messages.Error.ValidationFailed, errors);
        return new BadRequestObjectResult(response);
    };
});

// Configure services using extensions
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.ConfigureIdentity();
builder.Services.ConfigureJwt(builder.Configuration);
builder.Services.ConfigureSwagger();
builder.Services.ConfigureCustomServices();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

var app = builder.Build();

// Seed initial data if the Users table is empty.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();

    // Apply database migrations automatically (useful during development when tables are added)
    db.Database.Migrate();

    var userManager = services.GetRequiredService<UserManager<User>>();

    if (!userManager.Users.Any())
    {
        var seedUsers = new List<User>
        {
            new User { FirstName = "Ali", LastName = "Khan", Email = "ali.khan@example.com", UserName = "ali.khan@example.com", EmailConfirmed = true },
            new User { FirstName = "Sara", LastName = "Ahmed", Email = "sara.ahmed@example.com", UserName = "sara.ahmed@example.com", EmailConfirmed = true },
            new User { FirstName = "Bilal", LastName = "Hassan", Email = "bilal.hassan@example.com", UserName = "bilal.hassan@example.com", EmailConfirmed = true }
        };

        foreach (var user in seedUsers)
        {
            // Seed users with a default password that satisfies Identity rules
            var result = await userManager.CreateAsync(user, "Password123!");
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                Console.WriteLine($"Failed to seed user {user.Email}: {errors}");
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication(); // Register Authentication middleware before Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();