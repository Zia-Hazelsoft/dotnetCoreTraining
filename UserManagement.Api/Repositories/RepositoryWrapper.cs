using UserManagement.Api.Data;

namespace UserManagement.Api.Repositories
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private readonly AppDbContext _repoContext;

        // Store the UserRepository object
        private IUserRepository? _user;

        // Constructor
        public RepositoryWrapper(AppDbContext repositoryContext)
        {
            _repoContext = repositoryContext;
        }

        // Property to get the UserRepository
        public IUserRepository User
        {
            get
            {
                // Create the repository only if it doesn't already exist
                if (_user == null)
                {
                    _user = new UserRepository(_repoContext);
                }

                return _user;
            }
        }

        // Save all database changes
        public async Task SaveAsync()
        {
            await _repoContext.SaveChangesAsync();
        }
    }
}