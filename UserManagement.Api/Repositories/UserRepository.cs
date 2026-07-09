using Microsoft.EntityFrameworkCore;
using UserManagement.Api.Common;
using UserManagement.Api.Data;
using UserManagement.Api.Dtos;
using UserManagement.Api.Models;

namespace UserManagement.Api.Repositories
{
    public class UserRepository : RepositoryBase<User>, IUserRepository
    {
        public UserRepository(AppDbContext repositoryContext) : base(repositoryContext)
        {
        }

        public async Task<PagedList<User>> GetUsersAsync(UserParameters userParameters, bool trackChanges = false)
        {
            var usersQuery = FindAll(trackChanges);

            // 1. Searching & Filtering
            if (!string.IsNullOrWhiteSpace(userParameters.SearchTerm))
            {
                var lowerCaseTerm = userParameters.SearchTerm.Trim().ToLower();
                usersQuery = usersQuery.Where(u =>
                    (u.FirstName != null && u.FirstName.ToLower().Contains(lowerCaseTerm)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(lowerCaseTerm)) ||
                    (u.Email != null && u.Email.ToLower().Contains(lowerCaseTerm)));
            }

            // 2. Sorting
            usersQuery = ApplySort(usersQuery, userParameters.OrderBy);

            // 3. Pagination
            return await PagedList<User>.ToPagedListAsync(
                usersQuery,
                userParameters.PageNumber,
                userParameters.PageSize);
        }

        private static IQueryable<User> ApplySort(IQueryable<User> users, string? orderByQueryString)
        {
            if (string.IsNullOrWhiteSpace(orderByQueryString))
            {
                return users.OrderBy(u => u.LastName); // Default sorting
            }

            var orderParams = orderByQueryString.Trim().Split(',');
            var sorted = false;

            foreach (var param in orderParams)
            {
                if (string.IsNullOrWhiteSpace(param))
                    continue;

                var propertyName = param.Trim().Split(" ")[0];
                var direction = param.EndsWith(" desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

                // Map sort strings to strongly-typed LINQ OrderBy/ThenBy expressions
                if (propertyName.Equals("firstName", StringComparison.OrdinalIgnoreCase))
                {
                    users = !sorted
                        ? (direction == "desc" ? users.OrderByDescending(u => u.FirstName) : users.OrderBy(u => u.FirstName))
                        : (direction == "desc" ? ((IOrderedQueryable<User>)users).ThenByDescending(u => u.FirstName) : ((IOrderedQueryable<User>)users).ThenBy(u => u.FirstName));
                }
                else if (propertyName.Equals("lastName", StringComparison.OrdinalIgnoreCase))
                {
                    users = !sorted
                        ? (direction == "desc" ? users.OrderByDescending(u => u.LastName) : users.OrderBy(u => u.LastName))
                        : (direction == "desc" ? ((IOrderedQueryable<User>)users).ThenByDescending(u => u.LastName) : ((IOrderedQueryable<User>)users).ThenBy(u => u.LastName));
                }
                else if (propertyName.Equals("email", StringComparison.OrdinalIgnoreCase))
                {
                    users = !sorted
                        ? (direction == "desc" ? users.OrderByDescending(u => u.Email) : users.OrderBy(u => u.Email))
                        : (direction == "desc" ? ((IOrderedQueryable<User>)users).ThenByDescending(u => u.Email) : ((IOrderedQueryable<User>)users).ThenBy(u => u.Email));
                }
                else if (propertyName.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    users = !sorted
                        ? (direction == "desc" ? users.OrderByDescending(u => u.Id) : users.OrderBy(u => u.Id))
                        : (direction == "desc" ? ((IOrderedQueryable<User>)users).ThenByDescending(u => u.Id) : ((IOrderedQueryable<User>)users).ThenBy(u => u.Id));
                }
                sorted = true;
            }

            return users;
        }

        public async Task<User?> GetUserByIdAsync(int userId, bool trackChanges = true)
        {
            return await FindByCondition(user => user.Id == userId, trackChanges)
                .FirstOrDefaultAsync();
        }
    }
}