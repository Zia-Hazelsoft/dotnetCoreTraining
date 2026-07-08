
namespace UserManagement.Api.Repositories
{
    public interface IRepositoryWrapper
    {
        IUserRepository User { get; }
        Task SaveAsync();
    }
}