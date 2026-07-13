using System.Linq;
using System.Threading.Tasks;
using UserManagement.Api.Common;

namespace UserManagement.Api.Repositories
{
    /// <summary>
    /// Contract for a generic repository providing basic CRUD, database persistence, 
    /// and advanced dynamic searching, sorting, and pagination logic.
    /// </summary>
    /// <typeparam name="T">The database entity type.</typeparam>
    public interface IRepositoryBase<T>
    {
        /// <summary>
        /// Retrieves all records of type T from the database.
        /// </summary>
        /// <param name="trackChanges">Whether EF Core change tracking is enabled.</param>
        /// <returns>An <see cref="IQueryable{T}"/> to chain further queries.</returns>
        IQueryable<T> FindAll(bool trackChanges = false);

        /// <summary>
        /// Stages a new entity for creation in the database context.
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        void Create(T entity);

        /// <summary>
        /// Stages an existing entity for modification in the database context.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        void Update(T entity);

        /// <summary>
        /// Stages an existing entity for deletion in the database context.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        void Delete(T entity);

        /// <summary>
        /// Commits all staged database updates asynchronously.
        /// </summary>
        Task SaveAsync();

        /// <summary>
        /// Fetches a single entity by its primary key integer identifier.
        /// </summary>
        /// <param name="id">The primary key integer identifier.</param>
        /// <returns>The matching entity if found; null otherwise.</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves a paginated, filtered, searched, and sorted list of entities.
        /// </summary>
        /// <param name="pageNumber">The index of the page to retrieve (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="searchTerm">The search term filter.</param>
        /// <param name="searchFields">The fields to match the search term against.</param>
        /// <param name="filterString">The string containing comma-separated filter clauses.</param>
        /// <param name="orderBy">The sorting order string.</param>
        /// <param name="defaultSortProperty">The fallback sorting property when orderBy is omitted.</param>
        /// <param name="trackChanges">Whether EF Core change tracking is enabled.</param>
        /// <returns>A paginated list wrapper containing metadata and data rows.</returns>
        Task<PagedList<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm,
            string[] searchFields,
            string? filterString,
            string? orderBy,
            string defaultSortProperty,
            bool trackChanges = false);
    }
}
