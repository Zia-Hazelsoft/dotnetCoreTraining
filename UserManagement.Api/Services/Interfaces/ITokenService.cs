using UserManagement.Api.Models;

namespace UserManagement.Api.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
