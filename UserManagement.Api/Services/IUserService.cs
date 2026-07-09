using UserManagement.Api.Dtos;

namespace UserManagement.Api.Services
{
    public interface IUserService
    {
        Task<PaginatedResponseDto<UserDto>> GetUsersAsync(UserParameters userParameters);

        Task<UserDto?> GetUserByIdAsync(int id);

        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);

        Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto);

        Task<bool> DeleteUserAsync(int id);
    }
}