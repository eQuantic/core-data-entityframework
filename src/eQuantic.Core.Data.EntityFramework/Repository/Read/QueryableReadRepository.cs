using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Config;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository.Read;

[ExcludeFromCodeCoverage]
public class QueryableReadRepository<TUnitOfWork, TEntity, TKey> :
    IQueryableReadRepository<TUnitOfWork, TEntity, TKey>,
    IReadRepository<TUnitOfWork, QueryableConfiguration<TEntity>, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    private SetBase<TEntity> _dbSet;
    private bool _disposed;
    private const string SpecificationCannotBeNull = "Specification cannot be null";
    private const string FilterExpressionCannotBeNull = "Filter expression cannot be null";
    /// <summary>
    /// Creates a new instance of the read repository
    /// </summary>
    /// <param name="unitOfWork">Associated Unit Of Work</param>
    public QueryableReadRepository(TUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// <see cref="IReadRepository{TUnitOfWork, TEntity, TKey}"/>
    /// </summary>
    public TUnitOfWork UnitOfWork { get; private set; }

    public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification), SpecificationCannotBeNull);
        }

        return GetQueryable(configuration, query => query.Where(specification.SatisfiedBy()));
    }

    public long Count()
    {
        return GetSet().LongCount();
    }

    public long Count(ISpecification<TEntity> specification)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification), SpecificationCannotBeNull);
        }

        return this.Count(specification.SatisfiedBy());
    }

    public long Count(Expression<Func<TEntity, bool>> filter)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter), FilterExpressionCannotBeNull);
        }

        return GetSet().LongCount(filter);
    }

    public bool All(ISpecification<TEntity> specification, Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification), SpecificationCannotBeNull);
        }

        return this.All(specification.SatisfiedBy());
    }

    public bool All(Expression<Func<TEntity, bool>> filter, Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter), FilterExpressionCannotBeNull);
        }

        return GetQueryable(configuration, _ => _).All(filter);
    }

    public bool Any(Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetQueryable(configuration, _ => _).Any();
    }

    public bool Any(ISpecification<TEntity> specification, Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification), SpecificationCannotBeNull);
        }

        return this.Any(specification.SatisfiedBy());
    }

    public bool Any(Expression<Func<TEntity, bool>> filter, Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter), FilterExpressionCannotBeNull);
        }

        return GetQueryable(configuration, query => query.Where(filter)).Any();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public TEntity Get(TKey id, Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        if (configuration == null)
        {
            return GetSet().Find(id);
        }

        var idExpression = GetSet().GetExpression(id);
        return GetQueryable(configuration, query => query.Where(idExpression))
            .SingleOrDefault();
    }

    public IEnumerable<TEntity> GetAll(Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetQueryable(configuration, query => query);
    }

    public IEnumerable<TResult> GetMapped<TResult>(Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map, Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetQueryable(configuration, query => query.Where(filter)).Select(map);
    }

    public IEnumerable<TResult> GetMapped<TResult>(ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map, Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification), SpecificationCannotBeNull);
        }

        return this.GetMapped(specification.SatisfiedBy(), map, configuration);
    }

    public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter), FilterExpressionCannotBeNull);
        }

        return GetQueryable(configuration, query => query.Where(filter));
    }

    public TEntity GetFirst(Expression<Func<TEntity, bool>> filter, Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter), FilterExpressionCannotBeNull);
        }

        return GetQueryable(configuration, query => query.Where(filter)).FirstOrDefault();
    }

    public TEntity GetFirst(ISpecification<TEntity> specification, Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification),SpecificationCannotBeNull);
        }

        return GetQueryable(configuration, query => query.Where(specification.SatisfiedBy())).FirstOrDefault();
    }

    public TResult GetFirstMapped<TResult>(Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map, Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter), FilterExpressionCannotBeNull);
        }
        return GetQueryable(configuration, query => query.Where(filter))
            .Select(map)
            .FirstOrDefault();
    }

    public TResult GetFirstMapped<TResult>(ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map, Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification), SpecificationCannotBeNull);
        }

        return this.GetFirstMapped(specification.SatisfiedBy(), map, configuration);
    }

    public IEnumerable<TEntity> GetPaged(int limit, Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetPaged((Expression<Func<TEntity, bool>>)null, 1, limit, configuration);
    }

    public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int limit,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification), SpecificationCannotBeNull);
        }

        return GetPaged(specification.SatisfiedBy(), 1, limit, configuration);
    }

    public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int limit,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetPaged(filter, 1, limit, configuration);
    }

    public IEnumerable<TEntity> GetPaged(int pageIndex, int pageSize, Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetPaged((Expression<Func<TEntity, bool>>)null, pageIndex, pageSize, configuration);
    }

    public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int pageIndex, int pageSize,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification), SpecificationCannotBeNull);
        }

        return GetPaged(specification.SatisfiedBy(), pageIndex, pageSize, configuration);
    }

    public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageSize,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        var query = GetQueryable(configuration, internalQuery =>
        {
            if (filter != null)
            {
                internalQuery = internalQuery.Where(filter);
            }

            return internalQuery;
        });
        return pageSize > 0 ? query.Skip((pageIndex - 1) * pageSize).Take(pageSize) : query;
    }

    public TEntity GetSingle(Expression<Func<TEntity, bool>> filter, Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter), FilterExpressionCannotBeNull);
        }

        return GetQueryable(configuration, query => query.Where(filter))
            .SingleOrDefault();
    }

    public TEntity GetSingle(ISpecification<TEntity> specification, Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification), SpecificationCannotBeNull);
        }

        return GetQueryable(configuration, query =>
                query
                    .Where(specification.SatisfiedBy()))
            .SingleOrDefault();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            UnitOfWork?.Dispose();
        }

        _disposed = true;
    }

    private IQueryable<TEntity> GetQueryable(Action<QueryableConfiguration<TEntity>> configuration,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> internalQueryAction)
    {
        return GetSet().GetQueryable(configuration, internalQueryAction);
    }

    private SetBase<TEntity> GetSet()
    {
        return _dbSet ??= (SetBase<TEntity>)UnitOfWork.CreateSet<TEntity>();
    }
}
