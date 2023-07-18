using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Config;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Linq.Specification;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.Repository.Read;

public class AsyncReadRepository<TUnitOfWork, TConfig, TEntity, TKey> :
    ReadRepository<TUnitOfWork, TConfig, TEntity, TKey>, IAsyncReadRepository<TUnitOfWork, TConfig, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
    where TConfig : Configuration<TEntity>
{
    private Set<TEntity> _dbSet;

    public AsyncReadRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<IEnumerable<TEntity>> AllMatchingAsync(ISpecification<TEntity> specification,
        Action<TConfig> configuration = default, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(configuration, query =>
                query
                    .Where(specification.SatisfiedBy()))
            .ToListAsync(cancellationToken);
    }

    public Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return GetSet().LongCountAsync(cancellationToken);
    }

    public Task<long> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return this.CountAsync(specification.SatisfiedBy(), cancellationToken);
    }

    public Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        return GetSet().LongCountAsync(filter, cancellationToken);
    }

    public Task<bool> AllAsync(ISpecification<TEntity> specification, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return this.AllAsync(specification.SatisfiedBy(), configuration, cancellationToken);
    }

    public Task<bool> AllAsync(Expression<Func<TEntity, bool>> filter, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(configuration, _ => _).AllAsync(filter, cancellationToken);
    }

    public Task<bool> AnyAsync(Action<TConfig> configuration = default, CancellationToken cancellationToken = default)
    {
        return GetQueryable(configuration, _ => _).AnyAsync(cancellationToken);
    }

    public Task<bool> AnyAsync(ISpecification<TEntity> specification, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return this.AnyAsync(specification.SatisfiedBy(), configuration, cancellationToken);
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return GetQueryable(configuration, query => query.Where(filter)).AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return await GetQueryable(configuration, query => query.Where(_ => true))
            .ToListAsync(cancellationToken);
    }

    public Task<TEntity> GetAsync(TKey id, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return GetInternalAsync(id, configuration, cancellationToken);
    }

    public async Task<IEnumerable<TResult>> GetMappedAsync<TResult>(Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return await GetQueryable(configuration, query => query.Where(filter))
            .Select(map)
            .ToListAsync(cancellationToken);
    }

    public Task<IEnumerable<TResult>> GetMappedAsync<TResult>(ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return this.GetMappedAsync(specification.SatisfiedBy(), map, configuration, cancellationToken);
    }

    private async Task<TEntity> GetInternalAsync(TKey id, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            return await GetSet().FindAsync(id, cancellationToken);
        }

        var idExpression = GetSet().GetExpression(id);
        return await GetQueryable(configuration, query => query.Where(idExpression))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter,
        Action<TConfig> configuration = default, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(configuration, query => query.Where(filter))
            .ToListAsync(cancellationToken);
    }

    public async Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter,
        Action<TConfig> configuration = default, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(configuration, query => query.Where(filter))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetFirstAsync(specification.SatisfiedBy(), configuration, cancellationToken);
    }

    public Task<TResult> GetFirstMappedAsync<TResult>(Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter), "Filter expression cannot be null");
        }
        
        return GetQueryable(configuration, query => query.Where(filter))
            .Select(map)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TResult> GetFirstMappedAsync<TResult>(ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return this.GetFirstMappedAsync(specification.SatisfiedBy(), map, configuration, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int limit, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return GetPagedAsync((Expression<Func<TEntity, bool>>)null, 1, limit, configuration, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int limit,
        Action<TConfig> configuration = default, CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetPagedAsync(specification.SatisfiedBy(), 1, limit, configuration, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int limit,
        Action<TConfig> configuration = default, CancellationToken cancellationToken = default)
    {
        return GetPagedAsync(filter, 1, limit, configuration, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int pageIndex, int pageSize,
        Action<TConfig> configuration = default, CancellationToken cancellationToken = default)
    {
        return GetPagedAsync((Expression<Func<TEntity, bool>>)null, pageIndex, pageSize, configuration,
            cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int pageIndex, int pageSize,
        Action<TConfig> configuration = default, CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetPagedAsync(specification.SatisfiedBy(), pageIndex, pageSize, configuration, cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int pageIndex,
        int pageSize, Action<TConfig> configuration = default, CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(configuration, internalQuery =>
        {
            if (filter != null)
            {
                internalQuery = internalQuery.Where(filter);
            }

            return internalQuery;
        });


        if (pageSize > 0)
        {
            return await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter,
        Action<TConfig> configuration = default, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(configuration, query => query.Where(filter))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetSingleAsync(specification.SatisfiedBy(), configuration, cancellationToken);
    }

    private IQueryable<TEntity> GetQueryable(Action<TConfig> configuration,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> internalQueryAction)
    {
        return GetSet().GetQueryable(configuration, internalQueryAction);
    }

    private Set<TEntity> GetSet()
    {
        return _dbSet ??= (Set<TEntity>)UnitOfWork.CreateSet<TEntity>();
    }
}
