using UserManagement.Api.Models;

namespace UserManagement.Api.Repositories
{
    public interface IUserRepository : IRepositoryBase<User>
    {
        Task<IEnumerable<User>> GetAllUserAsync(bool trackChanges = false);
        Task<User?> GetUserByIdAsync(int userId, bool trackChanges = true);
    }
}