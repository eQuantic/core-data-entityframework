using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Linq.Extensions;
using eQuantic.Linq.Sorter;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository.Read;

public class ReadRepository<TUnitOfWork, TEntity, TKey> : IReadRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    private Set<TEntity> _dbset = null;
    private bool disposed = false;

    /// <summary>
    /// Creates a new instance of the read repository
    /// </summary>
    /// <param name="unitOfWork">Associated Unit Of Work</param>
    public ReadRepository(TUnitOfWork unitOfWork)
    {
        if (unitOfWork == null)
        {
            throw new ArgumentNullException(nameof(unitOfWork));
        }

        UnitOfWork = unitOfWork;
    }

    /// <summary>
    /// <see cref="IReadRepository{TUnitOfWork, TEntity, TKey}"/>
    /// </summary>
    public TUnitOfWork UnitOfWork { get; private set; }

    public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification, params ISorting[] sortingColumns)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetSet().Where(specification.SatisfiedBy()).OrderBy(sortingColumns);
    }

    public long Count()
    {
        return GetSet().LongCount();
    }

    public long Count(ISpecification<TEntity> specification)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return this.Count(specification.SatisfiedBy());
    }

    public long Count(Expression<Func<TEntity, bool>> filter)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }
        return GetSet().LongCount(filter);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public TEntity Get(TKey id)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return GetSet().Find(id);
    }

    public IEnumerable<TEntity> GetAll(params ISorting[] sortingColumns)
    {
        if (sortingColumns?.Length > 0)
        {
            return GetSet().OrderBy(sortingColumns);
        }

        return GetSet().AsQueryable();
    }

    public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter)
    {
        return GetFiltered(filter, null);
    }

    public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, params ISorting[] sortColumns)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter), "Filter expression cannot be null");
        }

        var query = GetSet().Where(filter);
        if (sortColumns?.Length > 0)
        {
            query = query.OrderBy(sortColumns);
        }
        return query;
    }

    public TEntity GetFirst(Expression<Func<TEntity, bool>> filter, params ISorting[] sortColumns)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter), "Filter expression cannot be null");
        }

        return GetSet().OrderBy(sortColumns).FirstOrDefault(filter);
    }

    public TEntity GetFirst(ISpecification<TEntity> specification, params ISorting[] sortColumns)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification), "Specification cannot be null");
        }

        return GetSet().OrderBy(sortColumns).FirstOrDefault(specification.SatisfiedBy());
    }

    public IEnumerable<TEntity> GetPaged(int limit, params ISorting[] sortColumns)
    {
        return GetPaged((Expression<Func<TEntity, bool>>)null, 1, limit, sortColumns);
    }

    public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int limit, params ISorting[] sortColumns)
    {
        return GetPaged(specification.SatisfiedBy(), 1, limit, sortColumns);
    }

    public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int limit, params ISorting[] sortColumns)
    {
        return GetPaged(filter, 1, limit, sortColumns);
    }

    public IEnumerable<TEntity> GetPaged(int pageIndex, int pageCount, params ISorting[] sortColumns)
    {
        return GetPaged((Expression<Func<TEntity, bool>>)null, pageIndex, pageCount, sortColumns);
    }

    public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int pageIndex, int pageCount, params ISorting[] sortColumns)
    {
        return GetPaged(specification.SatisfiedBy(), pageIndex, pageCount, sortColumns);
    }

    public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, params ISorting[] sortColumns)
    {
        IQueryable<TEntity> query = GetSet();
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

    public TEntity GetSingle(Expression<Func<TEntity, bool>> filter, params ISorting[] sortColumns)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter), "Filter expression cannot be null");
        }

        return GetSet().SingleOrDefault(filter);
    }

    public TEntity GetSingle(ISpecification<TEntity> specification, params ISorting[] sortColumns)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification), "Specification cannot be null");
        }

        return GetSet().SingleOrDefault(specification.SatisfiedBy());
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            UnitOfWork?.Dispose();
        }

        disposed = true;
    }

    private Set<TEntity> GetSet()
    {
        return _dbset ?? (_dbset = (Set<TEntity>)UnitOfWork.CreateSet<TEntity>());
    }
}