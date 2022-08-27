using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using eQuantic.Core.Data.EntityFramework.Repository.Extensions;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Linq.Extensions;
using eQuantic.Linq.Sorter;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository.Read;

public class RelationalReadRepository<TUnitOfWork, TEntity, TKey> : ReadRepository<TUnitOfWork, TEntity, TKey>, IRelationalReadRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    private Set<TEntity> _dbset = null;

    public RelationalReadRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification, params string[] loadProperties)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetSet().IncludeMany(loadProperties).Where(specification.SatisfiedBy());
    }

    public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetSet().IncludeMany(loadProperties).Where(specification.SatisfiedBy());
    }

    public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification, ISorting[] sortingColumns, params string[] loadProperties)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetSet().IncludeMany(loadProperties).Where(specification.SatisfiedBy()).OrderBy(sortingColumns);
    }

    public IEnumerable<TEntity> AllMatching(ISpecification<TEntity> specification, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetSet().IncludeMany(loadProperties).Where(specification.SatisfiedBy()).OrderBy(sortingColumns);
    }

    public TEntity Get(TKey id, params string[] loadProperties)
    {
        var item = GetSet().Find(id);
        if (item != null)
        {
            GetSet().LoadProperties(item, loadProperties);
        }
        return item;
    }

    public TEntity Get(TKey id, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return Get(id, loadProperties.GetPropertyNames());
    }

    public IEnumerable<TEntity> GetAll(params string[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties);
    }

    public IEnumerable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties);
    }

    public IEnumerable<TEntity> GetAll(ISorting[] sortingColumns, params string[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns);
    }

    public IEnumerable<TEntity> GetAll(ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns);
    }

    public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, params string[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).Where(filter);
    }

    public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).Where(filter);
    }

    public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, ISorting[] sortColumns, params string[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).Where(filter).OrderBy(sortColumns);
    }

    public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).Where(filter).OrderBy(sortColumns);
    }

    public TEntity GetFirst(Expression<Func<TEntity, bool>> filter, params string[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).FirstOrDefault(filter);
    }

    public TEntity GetFirst(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).FirstOrDefault(filter);
    }

    public TEntity GetFirst(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, params string[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).FirstOrDefault(filter);
    }

    public TEntity GetFirst(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).FirstOrDefault(filter);
    }

    public TEntity GetFirst(ISpecification<TEntity> specification, params string[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).FirstOrDefault(specification.SatisfiedBy());
    }

    public TEntity GetFirst(ISpecification<TEntity> specification, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).FirstOrDefault(specification.SatisfiedBy());
    }

    public TEntity GetFirst(ISpecification<TEntity> specification, ISorting[] sortingColumns, params string[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).FirstOrDefault(specification.SatisfiedBy());
    }

    public TEntity GetFirst(ISpecification<TEntity> specification, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).FirstOrDefault(specification.SatisfiedBy());
    }

    public IEnumerable<TEntity> GetPaged(int limit, ISorting[] sortColumns, params string[] loadProperties)
    {
        return GetPaged((Expression<Func<TEntity, bool>>)null, 1, limit, sortColumns, loadProperties);
    }

    public IEnumerable<TEntity> GetPaged(int limit, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetPaged((Expression<Func<TEntity, bool>>)null, 1, limit, sortColumns, loadProperties.GetPropertyNames());
    }

    public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int limit, ISorting[] sortColumns, params string[] loadProperties)
    {
        return GetPaged(specification.SatisfiedBy(), 1, limit, sortColumns, loadProperties);
    }

    public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int limit, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetPaged(specification.SatisfiedBy(), 1, limit, sortColumns, loadProperties.GetPropertyNames());
    }

    public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int limit, ISorting[] sortColumns, params string[] loadProperties)
    {
        return GetPaged(filter, 1, limit, sortColumns, loadProperties);
    }

    public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int limit, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetPaged(filter, 1, limit, sortColumns, loadProperties.GetPropertyNames());
    }

    public IEnumerable<TEntity> GetPaged(int pageIndex, int pageCount, ISorting[] sortColumns, params string[] loadProperties)
    {
        return GetPaged((Expression<Func<TEntity, bool>>)null, pageIndex, pageCount, sortColumns, loadProperties);
    }

    public IEnumerable<TEntity> GetPaged(int pageIndex, int pageCount, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetPaged((Expression<Func<TEntity, bool>>)null, pageIndex, pageCount, sortColumns, loadProperties.GetPropertyNames());
    }

    public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int pageIndex, int pageCount, ISorting[] sortColumns, params string[] loadProperties)
    {
        return GetPaged(specification.SatisfiedBy(), pageIndex, pageCount, sortColumns, loadProperties);
    }

    public IEnumerable<TEntity> GetPaged(ISpecification<TEntity> specification, int pageIndex, int pageCount, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetPaged(specification.SatisfiedBy(), pageIndex, pageCount, sortColumns, loadProperties.GetPropertyNames());
    }

    public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, ISorting[] sortColumns, params string[] loadProperties)
    {
        IQueryable<TEntity> query = GetSet().IncludeMany(loadProperties);
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

    public IEnumerable<TEntity> GetPaged(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetPaged(filter, pageIndex, pageCount, sortColumns, loadProperties.GetPropertyNames());
    }

    public TEntity GetSingle(Expression<Func<TEntity, bool>> filter, params string[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).SingleOrDefault(filter);
    }

    public TEntity GetSingle(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).SingleOrDefault(filter);
    }

    public TEntity GetSingle(ISpecification<TEntity> specification, params string[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).SingleOrDefault(specification.SatisfiedBy());
    }

    public TEntity GetSingle(ISpecification<TEntity> specification, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).SingleOrDefault(specification.SatisfiedBy());
    }

    public TEntity GetSingle(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, params string[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).SingleOrDefault(filter);
    }

    public TEntity GetSingle(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).SingleOrDefault(filter);
    }

    public TEntity GetSingle(ISpecification<TEntity> specification, ISorting[] sortingColumns, params string[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).SingleOrDefault(specification.SatisfiedBy());
    }

    public TEntity GetSingle(ISpecification<TEntity> specification, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).SingleOrDefault(specification.SatisfiedBy());
    }

    private Set<TEntity> GetSet()
    {
        return _dbset ?? (_dbset = (Set<TEntity>)UnitOfWork.CreateSet<TEntity>());
    }
}