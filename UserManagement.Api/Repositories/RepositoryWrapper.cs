using UserManagement.Api.Data;

namespace UserManagement.Api.Repositories
{
    public class RepositoryWrapper(AppDbContext repositoryContext) : IRepositoryWrapper
    {
        private readonly AppDbContext _repoContext = repositoryContext;
        private IUserRepository? _user;

        public IUserRepository User => _user ??= new UserRepository(_repoContext);

        public async Task SaveAsync()
        {
            await _repoContext.SaveChangesAsync();
        }
    }
}