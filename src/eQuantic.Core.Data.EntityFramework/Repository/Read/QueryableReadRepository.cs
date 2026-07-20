using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using eQuantic.Core.Data.EntityFramework.Repository.Extensions;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Options;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository.Read;

[ExcludeFromCodeCoverage]
public class QueryableReadRepository<TEntity, TKey> :
    IQueryableReadRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    internal SetBase<TEntity> _dbSet;
    private bool _disposed;

    private const string FilterExpressionCannotBeNull = "Filter expression cannot be null";
    private const string SpecificationCannotBeNull = "Specification cannot be null";

    /// <summary>
    /// Creates a new instance of the read repository
    /// </summary>
    /// <param name="unitOfWork">Associated Unit Of Work</param>
    public QueryableReadRepository(IQueryableUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// The associated queryable unit of work.
    /// </summary>
    public IQueryableUnitOfWork UnitOfWork { get; private set; }

    public TEntity Get(TKey id, QueryOptions<TEntity> options = null)
    {
        if (id is null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        if (options == null)
        {
            return GetSet().Find(id);
        }

        var idExpression = GetSet().GetExpression(id);
        return GetSet().GetQueryable(options, query => query.Where(idExpression)).SingleOrDefault();
    }

    public IEnumerable<TEntity> GetAll(QueryOptions<TEntity> options = null)
    {
        return GetSet().GetQueryable(options);
    }

    public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, QueryOptions<TEntity> options = null)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter), FilterExpressionCannotBeNull);
        }

        return GetSet().GetQueryable(options, query => query.Where(filter));
    }

    public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification, QueryOptions<TEntity> options = null)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification), SpecificationCannotBeNull);
        }

        return GetSet().GetQueryable(options, query => query.Where(specification.SatisfiedBy()));
    }

    public IEnumerable<TResult> GetMapped<TResult>(Expression<Func<TEntity, TResult>> map, QueryOptions<TEntity> options = null)
    {
        if (map == null)
        {
            throw new ArgumentNullException(nameof(map));
        }

        return GetSet().GetQueryable(options).Select(map);
    }

    public TEntity GetFirst(QueryOptions<TEntity> options)
    {
        return GetSet().GetQueryable(options).FirstOrDefault();
    }

    public TResult GetFirstMapped<TResult>(Expression<Func<TEntity, TResult>> map, QueryOptions<TEntity> options)
    {
        if (map == null)
        {
            throw new ArgumentNullException(nameof(map));
        }

        return GetSet().GetQueryable(options).Select(map).FirstOrDefault();
    }

    public TEntity GetSingle(QueryOptions<TEntity> options)
    {
        return GetSet().GetQueryable(options).SingleOrDefault();
    }

    public PagedResult<TEntity> GetPaged(PageRequest page, QueryOptions<TEntity> options = null)
    {
        if (page == null)
        {
            throw new ArgumentNullException(nameof(page));
        }

        var query = GetSet().GetQueryable(options);
        var totalCount = query.LongCount();
        var items = query
            .OrderByPrimaryKeyIfUnordered(GetSet().DbContext)
            .Skip(page.Skip)
            .Take(page.Take)
            .ToList();

        return new PagedResult<TEntity>(items, totalCount, page.PageIndex, page.PageSize);
    }

    public PagedResult<TResult> GetPaged<TResult>(PageRequest page, Expression<Func<TEntity, TResult>> map,
        QueryOptions<TEntity> options = null)
    {
        if (page == null)
        {
            throw new ArgumentNullException(nameof(page));
        }

        if (map == null)
        {
            throw new ArgumentNullException(nameof(map));
        }

        var query = GetSet().GetQueryable(options);
        var totalCount = query.LongCount();
        var items = query
            .OrderByPrimaryKeyIfUnordered(GetSet().DbContext)
            .Skip(page.Skip)
            .Take(page.Take)
            .Select(map)
            .ToList();

        return new PagedResult<TResult>(items, totalCount, page.PageIndex, page.PageSize);
    }

    public long Count(QueryOptions<TEntity> options = null)
    {
        return GetSet().GetQueryable(options).LongCount();
    }

    public bool Any(QueryOptions<TEntity> options = null)
    {
        return GetSet().GetQueryable(options).Any();
    }

    public bool All(Expression<Func<TEntity, bool>> predicate, QueryOptions<TEntity> options = null)
    {
        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate), FilterExpressionCannotBeNull);
        }

        return GetSet().GetQueryable(options).All(predicate);
    }

    public int Sum(Expression<Func<TEntity, int>> selector, QueryOptions<TEntity> options = null)
    {
        return GetSet().GetQueryable(options).Sum(selector);
    }

    public int? Sum(Expression<Func<TEntity, int?>> selector, QueryOptions<TEntity> options = null)
    {
        return GetSet().GetQueryable(options).Sum(selector);
    }

    public long Sum(Expression<Func<TEntity, long>> selector, QueryOptions<TEntity> options = null)
    {
        return GetSet().GetQueryable(options).Sum(selector);
    }

    public long? Sum(Expression<Func<TEntity, long?>> selector, QueryOptions<TEntity> options = null)
    {
        return GetSet().GetQueryable(options).Sum(selector);
    }

    public double Sum(Expression<Func<TEntity, double>> selector, QueryOptions<TEntity> options = null)
    {
        return GetSet().GetQueryable(options).Sum(selector);
    }

    public double? Sum(Expression<Func<TEntity, double?>> selector, QueryOptions<TEntity> options = null)
    {
        return GetSet().GetQueryable(options).Sum(selector);
    }

    public float Sum(Expression<Func<TEntity, float>> selector, QueryOptions<TEntity> options = null)
    {
        return GetSet().GetQueryable(options).Sum(selector);
    }

    public float? Sum(Expression<Func<TEntity, float?>> selector, QueryOptions<TEntity> options = null)
    {
        return GetSet().GetQueryable(options).Sum(selector);
    }

    public decimal Sum(Expression<Func<TEntity, decimal>> selector, QueryOptions<TEntity> options = null)
    {
        return GetSet().GetQueryable(options).Sum(selector);
    }

    public decimal? Sum(Expression<Func<TEntity, decimal?>> selector, QueryOptions<TEntity> options = null)
    {
        return GetSet().GetQueryable(options).Sum(selector);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        // The UnitOfWork is injected, not created here, so its creator (the DI container or the caller)
        // owns its lifetime. Disposing it here would tear down the shared DbContext out from under the
        // other repositories in the same scope.
        _disposed = true;
    }

    internal virtual SetBase<TEntity> GetSet()
    {
        return _dbSet ??= (SetBase<TEntity>)UnitOfWork.CreateSet<TEntity>();
    }
}
