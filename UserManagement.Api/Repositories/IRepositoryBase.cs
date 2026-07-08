using System.Linq.Expressions;

namespace UserManagement.Api.Repositories
{
    public interface IRepositoryBase<T>
    {
        IQueryable<T> FindAll(bool trackChanges = false);
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = true);
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}