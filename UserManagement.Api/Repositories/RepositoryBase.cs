using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UserManagement.Api.Data;

namespace UserManagement.Api.Repositories
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected AppDbContext RepositoryContext { get; set; }

        protected RepositoryBase(AppDbContext repositoryContext)
        {
            this.RepositoryContext = repositoryContext;
        }

        public IQueryable<T> FindAll(bool trackChanges = false)
        {
            return trackChanges 
                ? this.RepositoryContext.Set<T>() 
                : this.RepositoryContext.Set<T>().AsNoTracking();
        }

        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges = true)
        {
            return trackChanges 
                ? this.RepositoryContext.Set<T>().Where(expression) 
                : this.RepositoryContext.Set<T>().Where(expression).AsNoTracking();
        }

        public void Create(T entity)
        {
            this.RepositoryContext.Set<T>().Add(entity);
        }

        public void Update(T entity)
        {
            this.RepositoryContext.Set<T>().Update(entity);
        }

        public void Delete(T entity)
        {
            this.RepositoryContext.Set<T>().Remove(entity);
        }
    }
}