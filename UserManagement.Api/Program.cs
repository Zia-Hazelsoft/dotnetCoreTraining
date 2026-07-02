using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Data;
using UserManagement.Api.Mappings;
using UserManagement.Api.Services;
using UserManagement.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// Database context — registered as Scoped by default (one instance per request).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency injection for services
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Seed initial data if the Users table is empty.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new User { FirstName = "Ali", LastName = "Khan", Email = "ali.khan@example.com" },
            new User { FirstName = "Sara", LastName = "Ahmed", Email = "sara.ahmed@example.com" },
            new User { FirstName = "Bilal", LastName = "Hassan", Email = "bilal.hassan@example.com" }
        );
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();