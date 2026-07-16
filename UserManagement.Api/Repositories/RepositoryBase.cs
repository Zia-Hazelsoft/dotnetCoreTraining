using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;
using System.Threading.Tasks;
using UserManagement.Api.Common;
using UserManagement.Api.Data;

namespace UserManagement.Api.Repositories
{
    /// <summary>
    /// Implements generic database persistence operations and pagination logic backed by EF Core.
    /// </summary>
    /// <typeparam name="T">The database entity type.</typeparam>
    public class RepositoryBase<T>(AppDbContext repositoryContext) : IRepositoryBase<T> where T : class
    {
        /// <summary>
        /// Gets or sets the EF Core database context.
        /// </summary>
        protected AppDbContext RepositoryContext { get; set; } = repositoryContext;

        /// <summary>
        /// Retrieves all records of type T from the database context.
        /// </summary>
        /// <param name="trackChanges">Whether EF Core change tracking is enabled.</param>
        /// <returns>An <see cref="IQueryable{T}"/>.</returns>
        public IQueryable<T> FindAll(bool trackChanges = false) =>
            trackChanges ? RepositoryContext.Set<T>() : RepositoryContext.Set<T>().AsNoTracking();

        /// <summary>
        /// Stages a new entity for creation in the database context.
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        public void Create(T entity) => RepositoryContext.Set<T>().Add(entity);

        /// <summary>
        /// Stages an existing entity for modification in the database context.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        public void Update(T entity) => RepositoryContext.Set<T>().Update(entity);

        /// <summary>
        /// Stages an existing entity for deletion in the database context.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        public void Delete(T entity) => RepositoryContext.Set<T>().Remove(entity);

        /// <summary>
        /// Commits all staged database updates asynchronously.
        /// </summary>
        public async Task SaveAsync() => await RepositoryContext.SaveChangesAsync();

        /// <summary>
        /// Fetches a single entity by its primary key integer identifier.
        /// </summary>
        /// <param name="id">The primary key integer identifier.</param>
        /// <returns>The matching entity if found; null otherwise.</returns>
        public async Task<T?> GetByIdAsync(int id) => await RepositoryContext.Set<T>().FindAsync(id);

        /// <summary>
        /// Retrieves a paginated, filtered, and sorted list of entities.
        /// </summary>
        /// <param name="pageNumber">The index of the page to retrieve (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="filterString">The string containing comma-separated filter clauses.</param>
        /// <param name="orderBy">The sorting order string.</param>
        /// <param name="defaultSortProperty">The fallback sorting property when orderBy is omitted.</param>
        /// <param name="trackChanges">Whether EF Core change tracking is enabled.</param>
        /// <returns>A paginated list wrapper containing metadata and data rows.</returns>
        public async Task<PagedList<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? filterString,
            string? orderBy,
            string defaultSortProperty,
            bool trackChanges = false)
        {
            IQueryable<T> query = FindAll(trackChanges);

            // Dynamic Filter Logic
            if (!string.IsNullOrWhiteSpace(filterString))
            {
                try
                {
                    query = query.Where(filterString);
                }
                catch (ParseException ex)
                {
                    throw new ArgumentException(ex.Message);
                }
            }

            // Dynamic Sort Logic 
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                query = query.OrderBy(defaultSortProperty);
            }
            else
            {
                query = query.OrderBy(orderBy);
            }

            return await PagedList<T>.ToPagedListAsync(query, pageNumber, pageSize);
        }
    }
}