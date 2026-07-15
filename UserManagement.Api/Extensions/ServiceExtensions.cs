using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using UserManagement.Api.Data;
using UserManagement.Api.Models;
using UserManagement.Api.Repositories;
using UserManagement.Api.Services.AuthService;
using UserManagement.Api.Services.AuthService.Implementation;
using UserManagement.Api.Services.EmailService;
using UserManagement.Api.Services.EmailService.Implementation;
using UserManagement.Api.Services.TokenService;
using UserManagement.Api.Services.TokenService.Implementation;
using UserManagement.Api.Services.UserService;
using UserManagement.Api.Services.UserService.Implementation;
using Sieve.Models;
using Sieve.Services;
using UserManagement.Api.Services;

namespace UserManagement.Api.Extensions
{
    public static class ServiceExtensions
    {
        // 1. Configure SQL Database Context
        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        }

        // 2. Configure ASP.NET Core Identity
        public static void ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole<int>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        }

        // 3. Configure JWT Authentication
        public static void ConfigureJwt(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection jwtSettings = configuration.GetSection("Jwt");
            byte[] key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Secret Key is not configured."));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        // 4. Configure Swagger Generation
        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "User Management API", Version = "v1" });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter your token below without the 'Bearer ' prefix.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer", document),
                        new List<string>()
                    }
                });
            });
        }

        // 5. Configure Repositories and Domain Services (Dependency Injection)
        public static void ConfigureCustomServices(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryBase<User>, RepositoryBase<User>>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();
            services.Configure<SieveOptions>(options =>
            {
                options.ThrowExceptions = true;
            });
        }
    }
}
