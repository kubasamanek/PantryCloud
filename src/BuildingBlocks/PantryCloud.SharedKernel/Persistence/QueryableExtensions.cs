using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace PantryCloud.SharedKernel.Persistence;

/// <summary>
/// Extension methods for IQueryable to provide common query patterns.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Applies case-insensitive contains filter to a string property.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="queryable">The queryable to filter.</param>
    /// <param name="propertySelector">Expression to select the string property.</param>
    /// <param name="searchTerm">The search term to look for.</param>
    /// <returns>The filtered queryable.</returns>
    public static IQueryable<T> WhereContainsCaseInsensitive<T>(
        this IQueryable<T> queryable,
        Expression<Func<T, string>> propertySelector,
        string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return queryable;

        var searchTermLower = searchTerm.ToLower();
        var parameter = propertySelector.Parameters[0];
        var property = propertySelector.Body;

        var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
        var propertyToLower = Expression.Call(property, toLowerMethod);

        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
        var searchTermConstant = Expression.Constant(searchTermLower);
        var containsExpression = Expression.Call(propertyToLower, containsMethod, searchTermConstant);

        var lambda = Expression.Lambda<Func<T, bool>>(containsExpression, parameter);

        return queryable.Where(lambda);
    }
}
