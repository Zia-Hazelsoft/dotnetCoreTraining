using UserManagement.Api.Dtos;

namespace UserManagement.Api.Services
{
    /// <summary>
    /// Defines the business operations available for User entities.
    /// The controller depends on this interface, not the concrete
    /// implementation, so the data source can change later without
    /// touching the controller (e.g. moving to EF Core in Week 3).
    /// </summary>
    public interface IUserService
    {
        IEnumerable<UserDto> GetAllUsers();

        UserDto? GetUserById(int id);

        UserDto CreateUser(CreateUserDto createUserDto);

        bool UpdateUser(int id, UpdateUserDto updateUserDto);

        bool DeleteUser(int id);
    }
}