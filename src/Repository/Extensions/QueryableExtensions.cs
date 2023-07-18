using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.Repository.Extensions;

/// <summary>
/// Queryable Extensions
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Includes many.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="query">The query.</param>
    /// <param name="properties">The properties.</param>
    /// <returns></returns>
    public static IQueryable<TEntity> IncludeMany<TEntity>(this IQueryable<TEntity> query, params string[] properties)
        where TEntity : class
    {
        if (properties is { Length: > 0 })
        {
            query = properties.Where(property => !string.IsNullOrEmpty(property))
                .Aggregate(query, (current, property) => current.Include(property));
        }

        return query;
    }

    /// <summary>
    /// Includes many.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="query">The query.</param>
    /// <param name="properties">The properties.</param>
    /// <returns></returns>
    public static IQueryable<TEntity> IncludeMany<TEntity>(this IQueryable<TEntity> query,
        params Expression<Func<TEntity, object>>[] properties)
        where TEntity : class
    {
        if (properties != null && properties.Length > 0)
        {
            query = properties.Where(property => property != null)
                .Aggregate(query, (current, property) => current.Include(property));
        }

        return query;
    }
}
