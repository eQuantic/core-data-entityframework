using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Config;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Linq.Extensions;
using eQuantic.Linq.Sorter;
using eQuantic.Linq.Specification;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.Repository.Read;

public class AsyncQueryableReadRepository<TUnitOfWork, TEntity, TKey> : AsyncReadRepository<TUnitOfWork, TEntity, TKey>, IAsyncQueryableReadRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    private Set<TEntity> _dbset = null;

    public AsyncQueryableReadRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<TEntity>> AllMatchingAsync(ISpecification<TEntity> specification, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration).Where(specification.SatisfiedBy()).ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration).AsQueryable().ToListAsync();
    }
    
    public async Task<IEnumerable<TEntity>> GetAllAsync(ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration).OrderBy(sortingColumns).ToListAsync();
    }

    public async Task<TEntity> GetAsync(TKey id, Action<QueryableConfiguration<TEntity>> configuration)
    {
        var idExpression = GetSet().GetExpression(id);
        return await GetQueryable(configuration).FirstOrDefaultAsync(idExpression);
    }

    public async Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration).Where(filter).ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration).Where(filter).OrderBy(sortColumns).ToListAsync();
    }

    public async Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration).FirstOrDefaultAsync(filter);
    }

    public async Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration).OrderBy(sortingColumns).FirstOrDefaultAsync(filter);
    }

    public async Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration).FirstOrDefaultAsync(specification.SatisfiedBy());
    }

    public async Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration).OrderBy(sortingColumns).FirstOrDefaultAsync(specification.SatisfiedBy());
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int limit, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetPagedAsync((Expression<Func<TEntity, bool>>)null, 1, limit, sortColumns, configuration);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int limit, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetPagedAsync(specification.SatisfiedBy(), 1, limit, sortColumns, configuration);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int limit, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetPagedAsync(filter, 1, limit, sortColumns, configuration);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int pageIndex, int pageCount, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetPagedAsync((Expression<Func<TEntity, bool>>)null, pageIndex, pageCount, sortColumns, configuration);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int pageIndex, int pageCount, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetPagedAsync(specification.SatisfiedBy(), pageIndex, pageCount, sortColumns, configuration);
    }
    
    public async Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int pageIndex, int pageCount, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
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
            return await query.Skip((pageIndex - 1) * pageCount).Take(pageCount).ToListAsync();
        }

        return await query.ToListAsync();
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration).SingleOrDefaultAsync(filter);
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration).OrderBy(sortingColumns).SingleOrDefaultAsync(filter);
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration).SingleOrDefaultAsync(specification.SatisfiedBy());
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, ISorting[] sortingColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration).OrderBy(sortingColumns).SingleOrDefaultAsync(specification.SatisfiedBy());
    }

    private IQueryable<TEntity> GetQueryable(Action<QueryableConfiguration<TEntity>> configuration)
    {
        var config = new QueryableConfiguration<TEntity>();
        configuration.Invoke(config);

        return config.Customize.Invoke(GetSet());
    }
    
    private Set<TEntity> GetSet()
    {
        return _dbset ??= (Set<TEntity>)UnitOfWork.CreateSet<TEntity>();
    }
}