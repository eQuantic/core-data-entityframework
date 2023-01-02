using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Config;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Linq.Extensions;
using eQuantic.Linq.Sorter;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository.Read;

public class QueryableReadRepository<TUnitOfWork, TEntity, TKey> : ReadRepository<TUnitOfWork, TEntity, TKey>, IQueryableReadRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    private Set<TEntity> _dbset = null;

    public QueryableReadRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification, Action<QueryableConfiguration<TEntity>> configuration)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetQueryable(configuration).Where(specification.SatisfiedBy());
    }

    public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetQueryable(configuration).Where(specification.SatisfiedBy()).OrderBy(sortingColumns);
    }

    public TEntity Get(TKey id, Action<QueryableConfiguration<TEntity>> configuration)
    {
        var idExpression = GetSet().GetExpression(id);
        return GetQueryable(configuration).FirstOrDefault(idExpression);
    }

    public IEnumerable<TEntity> GetAll(Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration);
    }

    public IEnumerable<TEntity> GetAll(ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration).OrderBy(sortingColumns);
    }

    public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration).Where(filter);
    }

    public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration).Where(filter).OrderBy(sortColumns);
    }

    public TEntity GetFirst(Expression<Func<TEntity, bool>> filter, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration).FirstOrDefault(filter);
    }

    public TEntity GetFirst(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration).OrderBy(sortingColumns).FirstOrDefault(filter);
    }

    public TEntity GetFirst(ISpecification<TEntity> specification, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration).FirstOrDefault(specification.SatisfiedBy());
    }

    public TEntity GetFirst(ISpecification<TEntity> specification, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration).OrderBy(sortingColumns).FirstOrDefault(specification.SatisfiedBy());
    }

    public IEnumerable<TEntity> GetPaged(int limit, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetPaged((Expression<Func<TEntity, bool>>)null, 1, limit, sortColumns, configuration);
    }

    public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int limit, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetPaged(specification.SatisfiedBy(), 1, limit, sortColumns, configuration);
    }

    public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int limit, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetPaged(filter, 1, limit, sortColumns, configuration);
    }

    public IEnumerable<TEntity> GetPaged(int pageIndex, int pageCount, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetPaged((Expression<Func<TEntity, bool>>)null, pageIndex, pageCount, sortColumns, configuration);
    }

    public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int pageIndex, int pageCount, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetPaged(specification.SatisfiedBy(), pageIndex, pageCount, sortColumns, configuration);
    }

    public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        var query = GetQueryable(configuration);
        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (sortColumns?.Length > 0)
        {
            query = query.OrderBy(sortColumns);
        }
        if (pageCount > 0)
        {
            return query.Skip((pageIndex - 1) * pageCount).Take(pageCount);
        }

        return query;
    }

    public TEntity GetSingle(Expression<Func<TEntity, bool>> filter, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration).SingleOrDefault(filter);
    }

    public TEntity GetSingle(ISpecification<TEntity> specification, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration).SingleOrDefault(specification.SatisfiedBy());
    }

    public TEntity GetSingle(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration).OrderBy(sortingColumns).SingleOrDefault(filter);
    }

    public TEntity GetSingle(ISpecification<TEntity> specification, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration).OrderBy(sortingColumns).SingleOrDefault(specification.SatisfiedBy());
    }

    private IQueryable<TEntity> GetQueryable(Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetSet().GetQueryable(configuration);
    }
    
    private Set<TEntity> GetSet()
    {
        return _dbset ??= (Set<TEntity>)UnitOfWork.CreateSet<TEntity>();
    }
}