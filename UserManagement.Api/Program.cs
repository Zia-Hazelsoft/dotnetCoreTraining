using UserManagement.Api.Mappings;
using UserManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AutoMapper: register mapping profile explicitly.
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// Dependency injection: whenever IUserService is requested, provide UserService.
// Scoped = one instance per HTTP request.
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

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