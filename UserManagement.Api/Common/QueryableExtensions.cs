using System.Linq.Dynamic.Core;

namespace UserManagement.Api.Common
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Search<T>(this IQueryable<T> source, string? searchTerm, params string[] propertyNames)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || propertyNames == null || propertyNames.Length == 0)
                return source;

            var lowerCaseTerm = searchTerm.Trim().ToLower();

            // Build dynamic query string e.g. "FirstName != null and FirstName.ToLower().Contains(@0) or LastName != null and LastName.ToLower().Contains(@0)"
            var filterExpression = string.Join(" or ", propertyNames.Select(p => $"{p} != null and {p}.ToLower().Contains(@0)"));

            return source.Where(filterExpression, lowerCaseTerm);
        }

        public static IQueryable<T> Sort<T>(this IQueryable<T> source, string? orderByQueryString, string defaultSortProperty)
        {
            if (string.IsNullOrWhiteSpace(orderByQueryString))
            {
                return source.OrderBy(defaultSortProperty);
            }

            // OrderBy in System.Linq.Dynamic.Core automatically parses sorting strings (e.g., "firstName desc, lastName")
            return source.OrderBy(orderByQueryString);
        }
    }
}
