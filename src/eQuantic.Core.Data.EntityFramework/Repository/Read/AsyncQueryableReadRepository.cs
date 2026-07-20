using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.EntityFramework.Repository.Extensions;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Options;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Linq.Specification;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.Repository.Read;

[ExcludeFromCodeCoverage]
public class AsyncQueryableReadRepository<TEntity, TKey> :
    QueryableReadRepository<TEntity, TKey>,
    IAsyncQueryableReadRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    private const string FilterExpressionCannotBeNull = "Filter expression cannot be null";
    private const string SpecificationCannotBeNull = "Specification cannot be null";
    private const string MapCannotBeNull = "Map expression cannot be null";

    public AsyncQueryableReadRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<TEntity> GetAsync(TKey id, QueryOptions<TEntity> options = null,
        CancellationToken cancellationToken = default)
    {
        if (id is null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        if (options == null)
        {
            return await GetSet().FindAsync(id, cancellationToken).ConfigureAwait(false);
        }

        var idExpression = GetSet().GetExpression(id);
        return await GetSet().GetQueryable(options, query => query.Where(idExpression))
            .SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(QueryOptions<TEntity> options = null,
        CancellationToken cancellationToken = default)
    {
        return await GetSet().GetQueryable(options).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter,
        QueryOptions<TEntity> options = null, CancellationToken cancellationToken = default)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter), FilterExpressionCannotBeNull);
        }

        return await GetSet().GetQueryable(options, query => query.Where(filter))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TEntity>> AllMatchingAsync(ISpecification<TEntity> specification,
        QueryOptions<TEntity> options = null, CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification), SpecificationCannotBeNull);
        }

        return await GetSet().GetQueryable(options, query => query.Where(specification.SatisfiedBy()))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<TResult>> GetMappedAsync<TResult>(Expression<Func<TEntity, TResult>> map,
        QueryOptions<TEntity> options = null, CancellationToken cancellationToken = default)
    {
        if (map == null)
        {
            throw new ArgumentNullException(nameof(map), MapCannotBeNull);
        }

        return await GetSet().GetQueryable(options).Select(map)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<TEntity> GetFirstAsync(QueryOptions<TEntity> options, CancellationToken cancellationToken = default)
    {
        return GetSet().GetQueryable(options).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TResult> GetFirstMappedAsync<TResult>(Expression<Func<TEntity, TResult>> map,
        QueryOptions<TEntity> options, CancellationToken cancellationToken = default)
    {
        if (map == null)
        {
            throw new ArgumentNullException(nameof(map), MapCannotBeNull);
        }

        return GetSet().GetQueryable(options).Select(map).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TEntity> GetSingleAsync(QueryOptions<TEntity> options, CancellationToken cancellationToken = default)
    {
        return GetSet().GetQueryable(options).SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<TEntity>> GetPagedAsync(PageRequest page, QueryOptions<TEntity> options = null,
        CancellationToken cancellationToken = default)
    {
        if (page == null)
        {
            throw new ArgumentNullException(nameof(page));
        }

        var query = GetSet().GetQueryable(options);
        var totalCount = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query
            .OrderByPrimaryKeyIfUnordered(GetSet().DbContext)
            .Skip(page.Skip)
            .Take(page.Take)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PagedResult<TEntity>(items, totalCount, page.PageIndex, page.PageSize);
    }

    public async Task<PagedResult<TResult>> GetPagedAsync<TResult>(PageRequest page,
        Expression<Func<TEntity, TResult>> map, QueryOptions<TEntity> options = null,
        CancellationToken cancellationToken = default)
    {
        if (page == null)
        {
            throw new ArgumentNullException(nameof(page));
        }

        if (map == null)
        {
            throw new ArgumentNullException(nameof(map), MapCannotBeNull);
        }

        var query = GetSet().GetQueryable(options);
        var totalCount = await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query
            .OrderByPrimaryKeyIfUnordered(GetSet().DbContext)
            .Skip(page.Skip)
            .Take(page.Take)
            .Select(map)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PagedResult<TResult>(items, totalCount, page.PageIndex, page.PageSize);
    }

    public Task<long> CountAsync(QueryOptions<TEntity> options = null, CancellationToken cancellationToken = default)
    {
        return GetSet().GetQueryable(options).LongCountAsync(cancellationToken);
    }

    public Task<bool> AnyAsync(QueryOptions<TEntity> options = null, CancellationToken cancellationToken = default)
    {
        return GetSet().GetQueryable(options).AnyAsync(cancellationToken);
    }

    public Task<bool> AllAsync(Expression<Func<TEntity, bool>> predicate, QueryOptions<TEntity> options = null,
        CancellationToken cancellationToken = default)
    {
        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate), FilterExpressionCannotBeNull);
        }

        return GetSet().GetQueryable(options).AllAsync(predicate, cancellationToken);
    }

    public Task<int> SumAsync(Expression<Func<TEntity, int>> selector, QueryOptions<TEntity> options = null,
        CancellationToken cancellationToken = default)
    {
        return GetSet().GetQueryable(options).SumAsync(selector, cancellationToken);
    }

    public Task<int?> SumAsync(Expression<Func<TEntity, int?>> selector, QueryOptions<TEntity> options = null,
        CancellationToken cancellationToken = default)
    {
        return GetSet().GetQueryable(options).SumAsync(selector, cancellationToken);
    }

    public Task<long> SumAsync(Expression<Func<TEntity, long>> selector, QueryOptions<TEntity> options = null,
        CancellationToken cancellationToken = default)
    {
        return GetSet().GetQueryable(options).SumAsync(selector, cancellationToken);
    }

    public Task<long?> SumAsync(Expression<Func<TEntity, long?>> selector, QueryOptions<TEntity> options = null,
        CancellationToken cancellationToken = default)
    {
        return GetSet().GetQueryable(options).SumAsync(selector, cancellationToken);
    }

    public Task<double> SumAsync(Expression<Func<TEntity, double>> selector, QueryOptions<TEntity> options = null,
        CancellationToken cancellationToken = default)
    {
        return GetSet().GetQueryable(options).SumAsync(selector, cancellationToken);
    }

    public Task<double?> SumAsync(Expression<Func<TEntity, double?>> selector, QueryOptions<TEntity> options = null,
        CancellationToken cancellationToken = default)
    {
        return GetSet().GetQueryable(options).SumAsync(selector, cancellationToken);
    }

    public Task<float> SumAsync(Expression<Func<TEntity, float>> selector, QueryOptions<TEntity> options = null,
        CancellationToken cancellationToken = default)
    {
        return GetSet().GetQueryable(options).SumAsync(selector, cancellationToken);
    }

    public Task<float?> SumAsync(Expression<Func<TEntity, float?>> selector, QueryOptions<TEntity> options = null,
        CancellationToken cancellationToken = default)
    {
        return GetSet().GetQueryable(options).SumAsync(selector, cancellationToken);
    }

    public Task<decimal> SumAsync(Expression<Func<TEntity, decimal>> selector, QueryOptions<TEntity> options = null,
        CancellationToken cancellationToken = default)
    {
        return GetSet().GetQueryable(options).SumAsync(selector, cancellationToken);
    }

    public Task<decimal?> SumAsync(Expression<Func<TEntity, decimal?>> selector, QueryOptions<TEntity> options = null,
        CancellationToken cancellationToken = default)
    {
        return GetSet().GetQueryable(options).SumAsync(selector, cancellationToken);
    }
}
