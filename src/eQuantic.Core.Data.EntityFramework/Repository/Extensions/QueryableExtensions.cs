using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using eQuantic.Core.Data.Repository.Options;
using eQuantic.Linq.Web;
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

    /// <summary>
    ///     Applies the ordered set of <see cref="QuerySort{T}" /> sortings to the query, translating each
    ///     into the matching <see cref="Queryable" /> <c>OrderBy</c>/<c>OrderByDescending</c>/<c>ThenBy</c>/
    ///     <c>ThenByDescending</c> call so the ordering stays server-side (EF translatable).
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="query">The query.</param>
    /// <param name="sortings">The sortings, in application order.</param>
    /// <returns>The ordered query.</returns>
    public static IQueryable<TEntity> ApplySorts<TEntity>(this IQueryable<TEntity> query,
        IReadOnlyList<QuerySort<TEntity>> sortings)
    {
        if (sortings is not { Count: > 0 })
        {
            return query;
        }

        var first = true;
        foreach (var sort in sortings)
        {
            if (sort == null)
            {
                continue;
            }

            var method = first
                ? (sort.Direction == SortDirection.Ascending ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending))
                : (sort.Direction == SortDirection.Ascending ? nameof(Queryable.ThenBy) : nameof(Queryable.ThenByDescending));

            var call = Expression.Call(
                typeof(Queryable),
                method,
                new[] { typeof(TEntity), sort.KeySelector.ReturnType },
                query.Expression,
                Expression.Quote(sort.KeySelector));

            query = query.Provider.CreateQuery<TEntity>(call);
            first = false;
        }

        return query;
    }

    /// <summary>
    ///     Translates a <see cref="QueryOptions{TEntity}" /> into an <see cref="IQueryable{TEntity}" />,
    ///     applying, in order: <see cref="QueryOptions{TEntity}.BeforeCustomization" /> →
    ///     <see cref="QueryOptions{TEntity}.Specification" /> and <see cref="QueryOptions{TEntity}.Filter" />
    ///     → the per-call <paramref name="internalQueryAction" /> → eager
    ///     <see cref="QueryOptions{TEntity}.IncludePaths" /> → <see cref="QueryOptions{TEntity}.Sortings" />
    ///     → <see cref="QueryOptions{TEntity}.AsNoTracking" /> →
    ///     <see cref="QueryOptions{TEntity}.IgnoreQueryFilters" /> → <see cref="QueryOptions{TEntity}.Tag" />
    ///     → <see cref="QueryOptions{TEntity}.AfterCustomization" />.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="source">The source query (typically the entity set).</param>
    /// <param name="options">The query options, or <c>null</c> when no shaping is requested.</param>
    /// <param name="internalQueryAction">
    ///     An optional per-call transformation (e.g. an explicit filter) applied right after the options'
    ///     own filter and before eager loading, sorting and the remaining shaping.
    /// </param>
    /// <returns>The shaped query.</returns>
    internal static IQueryable<TEntity> ApplyOptions<TEntity>(this IQueryable<TEntity> source,
        QueryOptions<TEntity> options,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> internalQueryAction = null)
        where TEntity : class
    {
        var query = source;

        if (options?.BeforeCustomization != null)
        {
            query = options.BeforeCustomization(query);
        }

        if (options?.Specification != null)
        {
            query = query.Where(options.Specification.SatisfiedBy());
        }

        if (options?.Filter != null)
        {
            query = query.Where(options.Filter);
        }

        if (internalQueryAction != null)
        {
            query = internalQueryAction(query);
        }

        if (options == null)
        {
            return query;
        }

        if (options.IncludePaths.Count > 0)
        {
            query = query.IncludeMany(options.IncludePaths.ToArray());
        }

        if (options.Sortings.Count > 0)
        {
            query = query.ApplySorts(options.Sortings);
        }

        if (options.AsNoTracking)
        {
            query = query.AsNoTracking();
        }

        if (options.IgnoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        if (!string.IsNullOrEmpty(options.Tag))
        {
            query = query.TagWith(options.Tag);
        }

        if (options.AfterCustomization != null)
        {
            query = options.AfterCustomization(query);
        }

        return query;
    }
}
