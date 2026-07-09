using UserManagement.Api.Models;
using UserManagement.Api.Dtos;
using UserManagement.Api.Common;

namespace UserManagement.Api.Repositories
{
    public interface IUserRepository : IRepositoryBase<User>
    {
        Task<PagedList<User>> GetUsersAsync(UserParameters userParameters, bool trackChanges = false);
        Task<User?> GetUserByIdAsync(int userId, bool trackChanges = true);
    }
}