using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using eQuantic.Core.Data.EntityFramework.Repository.Extensions;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Linq.Extensions;
using eQuantic.Linq.Sorter;
using eQuantic.Linq.Specification;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.Repository.Read;

public class AsyncRelationalReadRepository<TUnitOfWork, TEntity, TKey> : AsyncReadRepository<TUnitOfWork, TEntity, TKey>, IAsyncRelationalReadRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    private Set<TEntity> _dbset = null;

    public AsyncRelationalReadRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<TEntity>> AllMatchingAsync(ISpecification<TEntity> specification, params string[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).Where(specification.SatisfiedBy()).ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> AllMatchingAsync(ISpecification<TEntity> specification, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).Where(specification.SatisfiedBy()).ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(params string[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).AsQueryable().ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).AsQueryable().ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(ISorting[] sortingColumns, params string[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).ToListAsync();
    }

    public async Task<TEntity> GetAsync(TKey id, params string[] loadProperties)
    {
        var item = await GetSet().FindAsync(id);
        if (item != null)
        {
            await GetSet().LoadPropertiesAsync(item, loadProperties);
        }
        return item;
    }

    public async Task<TEntity> GetAsync(TKey id, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return await GetAsync(id, loadProperties.GetPropertyNames());
    }

    public async Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter, params string[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).Where(filter).ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).Where(filter).ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortColumns, params string[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).Where(filter).OrderBy(sortColumns).ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).Where(filter).OrderBy(sortColumns).ToListAsync();
    }

    public async Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter, params string[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).FirstOrDefaultAsync(filter);
    }

    public async Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).FirstOrDefaultAsync(filter);
    }

    public async Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, params string[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).FirstOrDefaultAsync(filter);
    }

    public async Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).FirstOrDefaultAsync(filter);
    }

    public async Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, params string[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).FirstOrDefaultAsync(specification.SatisfiedBy());
    }

    public async Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).FirstOrDefaultAsync(specification.SatisfiedBy());
    }

    public async Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, ISorting[] sortingColumns, params string[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).FirstOrDefaultAsync(specification.SatisfiedBy());
    }

    public async Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return await GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).FirstOrDefaultAsync(specification.SatisfiedBy());
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int limit, ISorting[] sortColumns, params string[] loadProperties)
    {
        return GetPagedAsync((Expression<Func<TEntity, bool>>)null, 1, limit, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int limit, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetPagedAsync((Expression<Func<TEntity, bool>>)null, 1, limit, sortColumns, loadProperties.GetPropertyNames());
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int limit, ISorting[] sortColumns, params string[] loadProperties)
    {
        return GetPagedAsync(specification.SatisfiedBy(), 1, limit, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int limit, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetPagedAsync(specification.SatisfiedBy(), 1, limit, sortColumns, loadProperties.GetPropertyNames());
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int limit, ISorting[] sortColumns, params string[] loadProperties)
    {
        return GetPagedAsync(filter, 1, limit, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int limit, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetPagedAsync(filter, 1, limit, sortColumns, loadProperties.GetPropertyNames());
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int pageIndex, int pageCount, ISorting[] sortColumns, params string[] loadProperties)
    {
        return GetPagedAsync((Expression<Func<TEntity, bool>>)null, pageIndex, pageCount, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int pageIndex, int pageCount, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetPagedAsync((Expression<Func<TEntity, bool>>)null, pageIndex, pageCount, sortColumns, loadProperties.GetPropertyNames());
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int pageIndex, int pageCount, ISorting[] sortColumns, params string[] loadProperties)
    {
        return GetPagedAsync(specification.SatisfiedBy(), pageIndex, pageCount, sortColumns, loadProperties);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int pageIndex, int pageCount, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetPagedAsync(specification.SatisfiedBy(), pageIndex, pageCount, sortColumns, loadProperties.GetPropertyNames());
    }

    public async Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, ISorting[] sortColumns, params string[] loadProperties)
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
            return await query.Skip((pageIndex - 1) * pageCount).Take(pageCount).ToListAsync();
        }

        return await query.ToListAsync();
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, ISorting[] sortColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetPagedAsync(filter, pageIndex, pageCount, sortColumns, loadProperties.GetPropertyNames());
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, params string[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).SingleOrDefaultAsync(filter);
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).SingleOrDefaultAsync(filter);
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, params string[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).SingleOrDefaultAsync(filter);
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).SingleOrDefaultAsync(filter);
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, params string[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).SingleOrDefaultAsync(specification.SatisfiedBy());
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).SingleOrDefaultAsync(specification.SatisfiedBy());
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, ISorting[] sortingColumns, params string[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).SingleOrDefaultAsync(specification.SatisfiedBy());
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, ISorting[] sortingColumns, params Expression<Func<TEntity, object>>[] loadProperties)
    {
        return GetSet().IncludeMany(loadProperties).OrderBy(sortingColumns).SingleOrDefaultAsync(specification.SatisfiedBy());
    }

    private Set<TEntity> GetSet()
    {
        return _dbset ?? (_dbset = (Set<TEntity>)UnitOfWork.CreateSet<TEntity>());
    }
}