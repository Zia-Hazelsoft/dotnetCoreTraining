# User Management API

ASP.NET Core Web API for managing users with JWT authentication, ASP.NET Core Identity, Entity Framework Core, AutoMapper, and Swagger.

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core with SQL Server
- ASP.NET Core Identity
- JWT Bearer authentication
- AutoMapper
- Swagger / OpenAPI

## Project Structure

- `UserManagement.slnx` - solution file
- `UserManagement.Api/` - API project
- `UserManagement.Api/Controllers/` - auth and user endpoints
- `UserManagement.Api/Services/` - business logic and token generation
- `UserManagement.Api/Data/` - database context
- `UserManagement.Api/Migrations/` - EF Core migrations

## Prerequisites

- .NET 10 SDK
- SQL Server or LocalDB

## Configuration

The API uses the following settings in `UserManagement.Api/appsettings.json`:

- `ConnectionStrings:DefaultConnection` - SQL Server connection string
- `Jwt:Key` - signing key for tokens
- `Jwt:Issuer` - token issuer
- `Jwt:Audience` - token audience
- `Jwt:ExpiryInMinutes` - token lifetime

Update these values before deploying to production.

## Run Locally

From the solution root:

```bash
dotnet restore
dotnet build
dotnet run --project UserManagement.Api
```

If you are using Visual Studio or VS Code, you can also launch the `UserManagement.Api` project directly.

## Database

The application applies Entity Framework Core migrations on startup and seeds a few sample users when the database is empty.

Default seeded users use the password `Password123!`.

## API Endpoints

### Authentication

- `POST /api/v1/auth/login` - authenticate a user and receive a JWT

### Users

All user endpoints require authentication.

- `GET /api/v1/users` - list users
- `GET /api/v1/users/{id}` - get a user by id
- `POST /api/v1/users` - create a user
- `PUT /api/v1/users/{id}` - update a user
- `DELETE /api/v1/users/{id}` - delete a user

## Swagger

When the app is running in development mode, Swagger UI is available at the default Swagger endpoint and supports JWT bearer authorization.

## Notes

- Validation errors are returned in a consistent `ApiResponse` format.
- The API uses custom exception handling middleware for unhandled errors.
- Change the JWT signing key before using the app in production.
