using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        return query.ApplySorts(sortings, null, NullsPlacement.AnyExpression);
    }

    /// <summary>
    ///     Applies the sortings, each placing its nulls where <paramref name="nullsOf" /> says (a leading
    ///     "is null" key, which every relational store translates), and refusing a placement the store cannot
    ///     order by.
    /// </summary>
    internal static IQueryable<TEntity> ApplySorts<TEntity>(this IQueryable<TEntity> query,
        IReadOnlyList<QuerySort<TEntity>> sortings, Func<QuerySort<TEntity>, NullsOrder> nullsOf,
        NullsPlacement placement)
    {
        if (sortings is not { Count: > 0 })
        {
            return query;
        }

        var first = true;
        var expression = query.Expression;
        foreach (var sort in sortings)
        {
            if (sort == null)
            {
                continue;
            }

            expression = Order(expression, true, typeof(TEntity), sort.KeySelector,
                sort.Direction == SortDirection.Descending, nullsOf?.Invoke(sort) ?? NullsOrder.Default, placement,
                ref first);
        }

        return query.Provider.CreateQuery<TEntity>(expression);
    }

    /// <summary>
    ///     Refuses <c>Skip</c>/<c>Take</c> on a read that works over every matching row (a count, a sum, an
    ///     <c>All</c>) or that pages by itself: silently counting a page, or paging twice, would answer another
    ///     question. The same rule, in the same words, as the eQuantic.Core.Data engines.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="options">The read's options.</param>
    /// <param name="read">The read, as the message names it (<c>CountAsync</c>).</param>
    internal static void RefuseRowWindow<TEntity>(QueryOptions<TEntity> options, string read)
        where TEntity : class
    {
        if (options is { SkipCount: not null } or { TakeCount: not null })
        {
            throw new NotSupportedException(
                $"Skip and Take shape the rows a read returns, and {read} does not return them that way; drop them " +
                "from the options (GetPagedAsync reads a page with its total).");
        }
    }

    /// <summary>
    ///     Eagerly loads a shaped collection include through EF Core's filtered include
    ///     (<c>Include(x =&gt; x.Steps.Where(…).OrderBy(…).Skip(…).Take(…))</c>): each parent gets exactly the
    ///     related entities the shape keeps, in its order.
    /// </summary>
    internal static IQueryable<TEntity> IncludeShaped<TEntity>(this IQueryable<TEntity> query, IncludeShape shape,
        NullsPlacement placement)
        where TEntity : class
    {
        var navigation = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             .FirstOrDefault(property => property.Name == shape.Path)
                         ?? throw new InvalidOperationException(
                             $"Cannot include '{shape.Path}': '{typeof(TEntity).Name}' has no such navigation property.");
        var element = shape.ElementType;
        if (!typeof(IEnumerable<>).MakeGenericType(element).IsAssignableFrom(navigation.PropertyType))
        {
            throw new InvalidOperationException(
                $"Cannot shape the include '{shape.Path}': a filter, an order or a limit applies to a collection, and it is a reference.");
        }

        var parameter = Expression.Parameter(typeof(TEntity), "x");
        Expression body = Expression.Property(parameter, navigation);
        if (shape.Filter is not null)
        {
            body = Expression.Call(typeof(Enumerable), nameof(Enumerable.Where), new[] { element }, body, shape.Filter);
        }

        var first = true;
        foreach (var sort in shape.Sortings)
        {
            body = Order(body, false, element, sort.KeySelector, sort.Descending, sort.Nulls, placement, ref first);
        }

        if (shape.SkipCount is > 0)
        {
            body = Expression.Call(typeof(Enumerable), nameof(Enumerable.Skip), new[] { element }, body,
                Expression.Constant(shape.SkipCount.Value));
        }

        if (shape.TakeCount is { } take)
        {
            body = Expression.Call(typeof(Enumerable), nameof(Enumerable.Take), new[] { element }, body,
                Expression.Constant(take));
        }

        var include = Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(TEntity), body.Type), body, parameter);
        return (IQueryable<TEntity>)IncludeMethod.MakeGenericMethod(typeof(TEntity), body.Type)
            .Invoke(null, new object[] { query, include });
    }

    private static readonly MethodInfo IncludeMethod = typeof(EntityFrameworkQueryableExtensions)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Single(method => method.Name == nameof(EntityFrameworkQueryableExtensions.Include)
                          && method.GetGenericArguments().Length == 2);

    /// <summary>
    ///     Appends one sort to a query (<see cref="Queryable" />) or to an include's collection
    ///     (<see cref="Enumerable" />), preceded, when nulls have a placement the store does not give by itself,
    ///     by a key that is 0 for the rows that go first and 1 for the others.
    /// </summary>
    private static Expression Order(Expression source, bool queryable, Type element, LambdaExpression key,
        bool descending, NullsOrder nulls, NullsPlacement placement, ref bool first)
    {
        var methods = queryable ? typeof(Queryable) : typeof(Enumerable);
        Expression Argument(LambdaExpression lambda) => queryable ? Expression.Quote(lambda) : lambda;

        var nullsKey = NullsKey(key, descending, nulls, placement);
        if (nullsKey is not null)
        {
            source = Expression.Call(methods, first ? nameof(Queryable.OrderBy) : nameof(Queryable.ThenBy),
                new[] { element, typeof(int) }, source, Argument(nullsKey));
            first = false;
        }

        var method = first
            ? (descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy))
            : (descending ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy));
        first = false;
        return Expression.Call(methods, method, new[] { element, key.ReturnType }, source, Argument(key));
    }

    /// <summary>
    ///     The key that places nulls, or <c>null</c> when none is needed: no placement asked for, a key that cannot
    ///     hold null, or a placement the store gives by itself.
    /// </summary>
    private static LambdaExpression NullsKey(LambdaExpression key, bool descending, NullsOrder nulls,
        NullsPlacement placement)
    {
        if (nulls == NullsOrder.Default)
        {
            return null;
        }

        var body = key.Body;
        while (body is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } convert)
        {
            body = convert.Operand;
        }

        if (body.Type.IsValueType && Nullable.GetUnderlyingType(body.Type) is null)
        {
            return null;
        }

        if (placement != NullsPlacement.AnyExpression && nulls == (descending ? NullsOrder.Last : NullsOrder.First))
        {
            return null;
        }

        if (placement == NullsPlacement.SmallestByPathOnly)
        {
            throw new NotSupportedException(
                $"Cosmos DB orders by document paths only and places nulls first when ascending and last when " +
                $"descending; NullsOrder.{nulls} on a {(descending ? "descending" : "ascending")} sort needs an " +
                "expression it cannot order by. Sort the other way, or drop the placement.");
        }

        var first = nulls == NullsOrder.First;
        var isNull = Expression.Equal(body, Expression.Constant(null, body.Type));
        return Expression.Lambda(
            Expression.Condition(isNull, Expression.Constant(first ? 0 : 1), Expression.Constant(first ? 1 : 0)),
            key.Parameters);
    }

    /// <summary>
    ///     Translates a <see cref="QueryOptions{TEntity}" /> into an <see cref="IQueryable{TEntity}" />,
    ///     applying, in order: <see cref="QueryOptions{TEntity}.BeforeCustomization" /> →
    ///     <see cref="QueryOptions{TEntity}.Specification" /> and <see cref="QueryOptions{TEntity}.Filter" />
    ///     → the per-call <paramref name="internalQueryAction" /> → eager
    ///     <see cref="QueryOptions{TEntity}.IncludePaths" /> (shaped ones through a filtered include) →
    ///     <see cref="QueryOptions{TEntity}.Sortings" /> (with their nulls placement) →
    ///     <see cref="QueryOptions{TEntity}.SkipCount" />/<see cref="QueryOptions{TEntity}.TakeCount" />
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
    /// <param name="placement">How the store can place nulls in an order.</param>
    /// <returns>The shaped query.</returns>
    internal static IQueryable<TEntity> ApplyOptions<TEntity>(this IQueryable<TEntity> source,
        QueryOptions<TEntity> options,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> internalQueryAction = null,
        NullsPlacement placement = NullsPlacement.AnyExpression)
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
            // A shaped path is loaded by its filtered include; a longer path through it ("Steps.Logs") keeps its
            // string include, and EF Core applies the shape to the shared navigation.
            query = query.IncludeMany(options.IncludePaths
                .Where(path => !options.IncludeShapes.ContainsKey(path)).ToArray());
        }

        foreach (var shape in options.IncludeShapes.Values)
        {
            query = query.IncludeShaped(shape, placement);
        }

        if (options.Sortings.Count > 0)
        {
            query = query.ApplySorts(options.Sortings, options.NullsOrderOf, placement);
        }

        if (options.SkipCount is > 0)
        {
            query = query.Skip(options.SkipCount.Value);
        }

        if (options.TakeCount is { } take)
        {
            query = query.Take(take);
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

/// <summary>How a store lets a sort place its nulls.</summary>
internal enum NullsPlacement
{
    /// <summary>The store orders by any expression (every relational store): a leading "is null" key places them.</summary>
    AnyExpression,

    /// <summary>
    ///     The store sorts nulls as the smallest value (first ascending, last descending) and orders by expressions
    ///     too (MongoDB): the other placements take the leading key.
    /// </summary>
    Smallest,

    /// <summary>The store sorts nulls as the smallest value and orders by document paths only (Cosmos DB).</summary>
    SmallestByPathOnly,
}
