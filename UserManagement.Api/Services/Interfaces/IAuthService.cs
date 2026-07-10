using System.Threading.Tasks;
using UserManagement.Api.Dtos;

namespace UserManagement.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task ConfirmRegistrationAsync(ConfirmRegistrationDto confirmRegistrationDto);
    }
}
