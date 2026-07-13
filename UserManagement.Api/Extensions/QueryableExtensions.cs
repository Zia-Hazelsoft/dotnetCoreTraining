using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text.RegularExpressions;
using UserManagement.Api.Common;

namespace UserManagement.Api.Extensions
{
    /// <summary>
    /// Extension methods on IQueryable for dynamic querying operations.
    /// </summary>
    public static class QueryableExtensions
    {
        private static readonly Regex FilterRegex = new(
            @"^([a-zA-Z0-9_]+)(==|!=|>=|<=|>|<|@=)(.*)$",
            RegexOptions.Compiled);

        private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> PropertyCache = new();

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

            // Parse and validate property names in orderby clause
            string[] orderClauses = orderByQueryString.Split(',', StringSplitOptions.RemoveEmptyEntries);
            List<string> validationErrors = [];

            foreach (string clause in orderClauses)
            {
                string[] parts = clause.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    string propertyName = parts[0];

                    // Look up property on entity using cache
                    PropertyInfo? prop = PropertyCache.GetOrAdd((typeof(T), propertyName), key =>
                        key.Item1.GetProperty(key.Item2, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance));

                    if (prop == null)
                    {
                        validationErrors.Add($"Property '{propertyName}' does not exist on entity '{typeof(T).Name}' for sorting. Matches are case-insensitive.");
                    }
                }
            }

            if (validationErrors.Count > 0)
            {
                throw new ApplicationValidationException("Invalid sorting parameters.", validationErrors);
            }

            // OrderBy in System.Linq.Dynamic.Core automatically parses sorting strings (e.g., "firstName desc, lastName")
            return source.OrderBy(orderByQueryString);
        }

        /// <summary>
        /// Filters an queryable source using dynamic comma-separated filter clauses (e.g. "Email==test@example.com,Id>5").
        /// </summary>
        /// <typeparam name="T">The type of entity.</typeparam>
        /// <param name="source">The source queryable.</param>
        /// <param name="filterString">The string containing comparison clauses.</param>
        /// <returns>The filtered queryable source.</returns>
        public static IQueryable<T> Filter<T>(this IQueryable<T> source, string? filterString)
        {
            if (string.IsNullOrWhiteSpace(filterString))
                return source;

            // Split by comma to support multiple filters e.g. "Email==test@example.com,Id>5"
            string[] filterClauses = filterString.Split(',', StringSplitOptions.RemoveEmptyEntries);

            IQueryable<T> query = source;
            List<string> validationErrors = [];

            foreach (string clause in filterClauses)
            {
                string trimmedClause = clause.Trim();
                Match match = FilterRegex.Match(trimmedClause);
                if (!match.Success)
                {
                    validationErrors.Add($"Invalid filter format for clause '{trimmedClause}'. Expected format: 'PropertyOperatorValue' (e.g. 'Email==value'). Supported operators: ==, !=, >=, <=, >, <, @=");
                    continue;
                }

                string propertyName = match.Groups[1].Value;
                string op = match.Groups[2].Value;
                string valueStr = match.Groups[3].Value.Trim().Trim('"'); // Trim quotes if present

                // Look up property info using type-safe concurrent cache
                PropertyInfo? prop = PropertyCache.GetOrAdd((typeof(T), propertyName), key =>
                    key.Item1.GetProperty(key.Item2, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance));

                if (prop == null)
                {
                    validationErrors.Add($"Property '{propertyName}' does not exist on entity '{typeof(T).Name}'. Matches are case-insensitive.");
                    continue;
                }

                Type targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                try
                {
                    // Convert string value to the appropriate concrete type
                    object? convertedValue;

                    if (targetType == typeof(bool))
                    {
                        // Support 1/0 as boolean representation in requests
                        if (valueStr == "1") convertedValue = true;
                        else if (valueStr == "0") convertedValue = false;
                        else convertedValue = bool.Parse(valueStr);
                    }
                    else if (targetType == typeof(DateTime))
                    {
                        convertedValue = DateTime.Parse(valueStr);
                    }
                    else if (targetType == typeof(DateOnly))
                    {
                        convertedValue = DateOnly.Parse(valueStr);
                    }
                    else if (targetType == typeof(TimeOnly))
                    {
                        convertedValue = TimeOnly.Parse(valueStr);
                    }
                    else if (targetType.IsEnum)
                    {
                        convertedValue = Enum.Parse(targetType, valueStr, true);
                    }
                    else if (targetType == typeof(Guid))
                    {
                        convertedValue = Guid.Parse(valueStr);
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(valueStr, targetType);
                    }

                    // Build LINQ Dynamic string predicate.
                    // Parameter placeholder is always @0 because each Where call operates on its own argument list.
                    string predicate = op switch
                    {
                        "==" => $"{prop.Name} == @0",
                        "!=" => $"{prop.Name} != @0",
                        ">" => $"{prop.Name} > @0",
                        "<" => $"{prop.Name} < @0",
                        ">=" => $"{prop.Name} >= @0",
                        "<=" => $"{prop.Name} <= @0",
                        "@=" => targetType == typeof(string)
                            ? $"{prop.Name} != null && {prop.Name}.ToLower().Contains(@0)"
                            : $"{prop.Name} == @0",
                        _ => throw new InvalidOperationException($"Operator '{op}' is not supported.")
                    };

                    if (op == "@=" && targetType == typeof(string) && convertedValue != null)
                    {
                        convertedValue = ((string)convertedValue).ToLower();
                    }

                    if (convertedValue != null)
                    {
                        query = query.Where(predicate, convertedValue);
                    }
                }
                catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is OverflowException || ex is ArgumentException)
                {
                    validationErrors.Add($"Value '{valueStr}' is not valid for property '{propertyName}' of type '{targetType.Name}'.");
                    continue;
                }
            }

            if (validationErrors.Count > 0)
            {
                throw new ApplicationValidationException("Invalid filter parameters.", validationErrors);
            }

            return query;
        }
       
    }
}
