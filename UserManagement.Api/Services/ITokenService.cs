using UserManagement.Api.Models;

namespace UserManagement.Api.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
