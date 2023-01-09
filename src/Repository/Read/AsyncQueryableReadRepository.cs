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

public class AsyncQueryableReadRepository<TUnitOfWork, TEntity, TKey> : AsyncReadRepository<TUnitOfWork, TEntity, TKey>,
    IAsyncQueryableReadRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    private Set<TEntity> _dbset = null;

    public AsyncQueryableReadRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<TEntity>> AllMatchingAsync(ISpecification<TEntity> specification,
        Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration, query => 
                query.Where(specification.SatisfiedBy()))
            .ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration, query => query)
            .ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(ISorting[] sortingColumns,
        Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration, query => 
                query.OrderBy(sortingColumns))
            .ToListAsync();
    }

    public async Task<TEntity> GetAsync(TKey id, Action<QueryableConfiguration<TEntity>> configuration)
    {
        var idExpression = GetSet().GetExpression(id);
        return await GetQueryable(configuration, query => 
                query.Where(idExpression))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter,
        Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration, query => 
                query.Where(filter))
            .ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter,
        ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration, query => 
                query
                    .Where(filter)
                    .OrderBy(sortColumns))
            .ToListAsync();
    }

    public async Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter,
        Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration, query => 
                query.Where(filter))
            .FirstOrDefaultAsync();
    }

    public async Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns,
        Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration, query => 
                query
                    .Where(filter)
                    .OrderBy(sortingColumns))
            .FirstOrDefaultAsync();
    }

    public async Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification,
        Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration, query => 
                query.Where(specification.SatisfiedBy()))
            .FirstOrDefaultAsync();
    }

    public async Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, ISorting[] sortingColumns,
        Action<QueryableConfiguration<TEntity>> configuration)
    {
        return await GetQueryable(configuration, query => 
                query
                    .Where(specification.SatisfiedBy())
                    .OrderBy(sortingColumns))
            .FirstOrDefaultAsync();
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int limit, ISorting[] sortColumns,
        Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetPagedAsync((Expression<Func<TEntity, bool>>)null, 1, limit, sortColumns, configuration);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int limit,
        ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetPagedAsync(specification.SatisfiedBy(), 1, limit, sortColumns, configuration);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int limit,
        ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetPagedAsync(filter, 1, limit, sortColumns, configuration);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int pageIndex, int pageCount, ISorting[] sortColumns,
        Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetPagedAsync((Expression<Func<TEntity, bool>>)null, pageIndex, pageCount, sortColumns, configuration);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int pageIndex, int pageCount,
        ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetPagedAsync(specification.SatisfiedBy(), pageIndex, pageCount, sortColumns, configuration);
    }

    public async Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int pageIndex,
        int pageCount, ISorting[] sortColumns, Action<QueryableConfiguration<TEntity>> configuration)
    {
        var query = GetQueryable(configuration, query =>
        {
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (sortColumns?.Length > 0)
            {
                query = query.OrderBy(sortColumns);
            }

            return pageCount > 0 ? query.Skip((pageIndex - 1) * pageCount).Take(pageCount) : query;
        });

        return await query.ToListAsync();
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter,
        Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration, query =>
                query.Where(filter))
            .SingleOrDefaultAsync();
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, ISorting[] sortingColumns,
        Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration, query =>
                query
                    .Where(filter)
                    .OrderBy(sortingColumns))
            .SingleOrDefaultAsync();
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification,
        Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration, query =>
                query.Where(specification.SatisfiedBy()))
            .SingleOrDefaultAsync();
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, ISorting[] sortingColumns,
        Action<QueryableConfiguration<TEntity>> configuration)
    {
        return GetQueryable(configuration, query =>
                query
                    .Where(specification.SatisfiedBy())
                    .OrderBy(sortingColumns))
            .SingleOrDefaultAsync();
    }

    private IQueryable<TEntity> GetQueryable(Action<QueryableConfiguration<TEntity>> configuration,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> internalQueryAction)
    {
        return GetSet().GetQueryable(configuration, internalQueryAction);
    }

    private Set<TEntity> GetSet()
    {
        return _dbset ??= (Set<TEntity>)UnitOfWork.CreateSet<TEntity>();
    }
}