using Sieve.Exceptions;
using Sieve.Models;
using Sieve.Services;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using UserManagement.Api.Common;

namespace UserManagement.Api.Extensions
{
    /// <summary>
    /// Extension methods on IQueryable for dynamic querying operations.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Searches specified properties of an queryable source using a case-insensitive term.
        /// </summary>
        public static IQueryable<T> Search<T>(this IQueryable<T> source, string? searchTerm, params string[] propertyNames)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || propertyNames == null || propertyNames.Length == 0)
                return source;

            string lowerCaseTerm = searchTerm.Trim().ToLower();

            // Build dynamic query string e.g. "FirstName != null and FirstName.ToLower().Contains(@0) or..."
            string filterExpression = string.Join(" or ", propertyNames.Select(p => $"{p} != null and {p}.ToLower().Contains(@0)"));

            return source.Where(filterExpression, lowerCaseTerm);
        }

        /// <summary>
        /// Sorts an queryable source dynamically using System.Linq.Dynamic.Core.
        /// </summary>
        public static IQueryable<T> Sort<T>(this IQueryable<T> source, string? orderByQueryString, string defaultSortProperty)
        {
            if (string.IsNullOrWhiteSpace(orderByQueryString))
            {
                return source.OrderBy(defaultSortProperty);
            }

            // OrderBy in System.Linq.Dynamic.Core automatically parses sorting strings (e.g., "firstName desc, lastName")
            return source.OrderBy(orderByQueryString);
        }

        /// <summary>
        /// Filters an queryable source using the third-party Sieve library.
        /// </summary>
        public static IQueryable<T> Filter<T>(this IQueryable<T> source, ISieveProcessor sieveProcessor, string? filterString)
        {
            if (string.IsNullOrWhiteSpace(filterString))
                return source;

            try
            {
                SieveModel sieveModel = new() { Filters = filterString };
                return sieveProcessor.Apply(sieveModel, source, applySorting: false, applyPagination: false);
            }
            catch (SieveException ex)
            {
                throw new ApplicationValidationException("Invalid filter parameters.", [ex.Message]);
            }
        }
    }
}
