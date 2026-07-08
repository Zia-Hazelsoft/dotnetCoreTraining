using UserManagement.Api.Models;
using UserManagement.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Api.Repositories
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(AppDbContext repositoryContext) : base(repositoryContext)
        {
        }

        public async Task<IEnumerable<User>> GetAllUserAsync(bool trackChanges = false)
        {
            return await FindAll(trackChanges).ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int userId, bool trackChanges = true)
        {
            return await FindByCondition(user => user.Id == userId, trackChanges)
                .FirstOrDefaultAsync();
        }
    }
}