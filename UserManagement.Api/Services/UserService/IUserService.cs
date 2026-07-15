using UserManagement.Api.Dtos;

namespace UserManagement.Api.Services.UserService
{
    public interface IUserService
    {
        Task<PaginatedResponseDto<UserDto>> GetUsersAsync(UserParameters userParameters);

        Task<UserDto?> GetUserByIdAsync(int id);

        Task<CreateUserResponseDto> CreateUserAsync(CreateUserDto createUserDto);

        Task<bool> UpdateUserAsync(int id, UpdateUserDto updateUserDto);

        Task<bool> DeleteUserAsync(int id);
    }
}