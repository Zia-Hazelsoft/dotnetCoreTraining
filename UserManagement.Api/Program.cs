using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Common;
using UserManagement.Api.Data;
using UserManagement.Api.Mappings;
using UserManagement.Api.Models;
using UserManagement.Api.Configuration;
using UserManagement.Api.Constants;
using UserManagement.Api.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Strongly-typed options registration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure custom Validation Error formatting matching ApiResponse<object>
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        List<string> errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        ApiResponse<object> response = ApiResponse<object>.FailureResponse(Messages.Error.ValidationFailed, errors);
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

WebApplication app = builder.Build();

// Migrate database on startup.
using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;
    AppDbContext db = services.GetRequiredService<AppDbContext>();

    // Apply database migrations automatically (useful during development when tables are added)
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Register Authentication middleware before Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();