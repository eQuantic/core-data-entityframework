using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Linq.Extensions;
using eQuantic.Linq.Sorter;
using eQuantic.Linq.Specification;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.Repository.Read;

public class AsyncReadRepository<TUnitOfWork, TEntity, TKey> : ReadRepository<TUnitOfWork, TEntity, TKey>, IAsyncReadRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    private Set<TEntity> _dbset = null;

    public AsyncReadRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<TEntity>> AllMatchingAsync(ISpecification<TEntity> specification)
    {
        return await GetSet().Where(specification.SatisfiedBy()).ToListAsync();
    }

    public Task<long> CountAsync()
    {
        return GetSet().LongCountAsync();
    }

    public Task<long> CountAsync(ISpecification<TEntity> specification)
    {
        return this.CountAsync(specification.SatisfiedBy());
    }

    public Task<long> CountAsync(Expression<Func<TEntity, bool>> filter)
    {
        return GetSet().LongCountAsync(filter);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(params ISorting[] sortingColumns)
    {
        if (sortingColumns?.Length > 0)
        {
            return await GetSet().OrderBy(sortingColumns).ToListAsync();
        }

        return await GetSet().ToListAsync();
    }

    public Task<TEntity> GetAsync(TKey id)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return GetSet().FindAsync(id);
    }

    public async Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter, params ISorting[] sortColumns)
    {
        IQueryable<TEntity> query = GetSet().Where(filter);
        if (sortColumns?.Length > 0)
        {
            query = query.OrderBy(sortColumns);
        }
        return await query.ToListAsync();
    }

    public Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter, params ISorting[] sortingColumns)
    {
        if (sortingColumns?.Length > 0)
        {
            return GetSet().OrderBy(sortingColumns).FirstOrDefaultAsync(filter);
        }
        return GetSet().FirstOrDefaultAsync(filter);
    }

    public Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, params ISorting[] sortingColumns)
    {
        if (sortingColumns?.Length > 0)
        {
            return GetSet().OrderBy(sortingColumns).FirstOrDefaultAsync(specification.SatisfiedBy());
        }
        return GetSet().FirstOrDefaultAsync(specification.SatisfiedBy());
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int limit, params ISorting[] sortColumns)
    {
        return GetPagedAsync((Expression<Func<TEntity, bool>>)null, 1, limit, sortColumns);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int limit, params ISorting[] sortColumns)
    {
        return GetPagedAsync(specification.SatisfiedBy(), 1, limit, sortColumns);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int limit, params ISorting[] sortColumns)
    {
        return GetPagedAsync(filter, 1, limit, sortColumns);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int pageIndex, int pageCount, params ISorting[] sortColumns)
    {
        return GetPagedAsync((Expression<Func<TEntity, bool>>)null, pageIndex, pageCount, sortColumns);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int pageIndex, int pageCount, params ISorting[] sortColumns)
    {
        return GetPagedAsync(specification.SatisfiedBy(), pageIndex, pageCount, sortColumns);
    }

    public async Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, params ISorting[] sortColumns)
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
            return await query.Skip((pageIndex - 1) * pageCount).Take(pageCount).ToListAsync();
        }

        return await query.ToListAsync();
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, params ISorting[] sortingColumns)
    {
        if (sortingColumns?.Length > 0)
        {
            return GetSet().OrderBy(sortingColumns).SingleOrDefaultAsync(filter);
        }
        return GetSet().SingleOrDefaultAsync(filter);
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, params ISorting[] sortingColumns)
    {
        if (sortingColumns?.Length > 0)
        {
            return GetSet().OrderBy(sortingColumns).SingleOrDefaultAsync(specification.SatisfiedBy());
        }
        return GetSet().SingleOrDefaultAsync(specification.SatisfiedBy());
    }

    private Set<TEntity> GetSet()
    {
        return _dbset ?? (_dbset = (Set<TEntity>)UnitOfWork.CreateSet<TEntity>());
    }
}