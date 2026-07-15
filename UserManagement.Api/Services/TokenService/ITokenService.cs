using UserManagement.Api.Models;

namespace UserManagement.Api.Services.TokenService
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
