using System.Threading.Tasks;
using UserManagement.Api.Dtos;

namespace UserManagement.Api.Services.AuthService
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task ConfirmRegistrationAsync(ConfirmRegistrationDto confirmRegistrationDto);
    }
}
