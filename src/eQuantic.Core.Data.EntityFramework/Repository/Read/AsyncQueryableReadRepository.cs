using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

[ExcludeFromCodeCoverage]
public class AsyncQueryableReadRepository<TUnitOfWork, TEntity, TKey> :
    QueryableReadRepository<TUnitOfWork, TEntity, TKey>,
    IAsyncQueryableReadRepository<TUnitOfWork, TEntity, TKey>,
    IAsyncReadRepository<TUnitOfWork, QueryableConfiguration<TEntity>, TEntity, TKey>
    where TUnitOfWork : class, IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    private SetBase<TEntity> _dbSet;

    public AsyncQueryableReadRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task<IEnumerable<TEntity>> AllMatchingAsync(
        ISpecification<TEntity> specification,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return AllMatchingAsync(specification, configuration, CancellationToken.None);
    }
    
    public async Task<IEnumerable<TEntity>> AllMatchingAsync(
        ISpecification<TEntity> specification,
        Action<QueryableConfiguration<TEntity>> configuration, 
        CancellationToken cancellationToken)
    {
        return await GetQueryable(configuration, query =>
                query
                    .Where(specification.SatisfiedBy()))
            .ToListAsync(cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> AllMatchingAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken)
    {
        return AllMatchingAsync(specification, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }

    public Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return GetSet().LongCountAsync(cancellationToken);
    }

    public Task<long> CountAsync(
        ISpecification<TEntity> specification, 
        CancellationToken cancellationToken = default)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return this.CountAsync(specification.SatisfiedBy(), cancellationToken);
    }

    public Task<long> CountAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        return GetSet().LongCountAsync(filter, cancellationToken);
    }

    public Task<bool> AllAsync(
        ISpecification<TEntity> specification,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return AllAsync(specification, configuration, CancellationToken.None);
    }

    public Task<bool> AllAsync(
        ISpecification<TEntity> specification, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return AllAsync(specification.SatisfiedBy(), configuration, cancellationToken);
    }

    public Task<bool> AllAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken)
    {
        return AllAsync(specification, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }
    
    public Task<bool> AllAsync(
        Expression<Func<TEntity, bool>> filter, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return AllAsync(filter, configuration, CancellationToken.None);
    }
    
    public Task<bool> AllAsync(
        Expression<Func<TEntity, bool>> filter, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return GetQueryable(configuration, e => e).AllAsync(filter, cancellationToken);
    }
    
    public Task<bool> AllAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
    {
        return AllAsync(filter, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }

    public Task<bool> AnyAsync(
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return AnyAsync(configuration, CancellationToken.None);
    }
    
    public Task<bool> AnyAsync(
        Action<QueryableConfiguration<TEntity>> configuration, 
        CancellationToken cancellationToken)
    {
        return GetQueryable(configuration, e => e).AnyAsync(cancellationToken);
    }
    
    public Task<bool> AnyAsync(
        CancellationToken cancellationToken)
    {
        return AnyAsync((Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }

    public Task<bool> AnyAsync(
        ISpecification<TEntity> specification, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return AnyAsync(specification, configuration, CancellationToken.None);
    }
    
    public Task<bool> AnyAsync(
        ISpecification<TEntity> specification, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return AnyAsync(specification.SatisfiedBy(), configuration, cancellationToken);
    }
    
    public Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken)
    {
        return AnyAsync(specification, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }

    public Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> filter, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return AnyAsync(filter, configuration, CancellationToken.None);
    }
    
    public Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> filter, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return GetQueryable(configuration, query => query.Where(filter)).AnyAsync(cancellationToken);
    }
    
    public Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        return AnyAsync(filter, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetAllAsync(
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetAllAsync(configuration, CancellationToken.None);
    }
    
    public async Task<IEnumerable<TEntity>> GetAllAsync(
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return await GetQueryable(configuration, query => query.Where(_ => true))
            .ToListAsync(cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return GetAllAsync((Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }

    public Task<TEntity> GetAsync(
        TKey id, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetAsync(id, configuration, CancellationToken.None);
    }

    public Task<TEntity> GetAsync(
        TKey id, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        if (Equals(id, default(TKey)))
        {
            throw new ArgumentNullException(nameof(id));
        }

        return GetInternalAsync(id, configuration, cancellationToken);
    }
    
    public Task<TEntity> GetAsync(
        TKey id,
        CancellationToken cancellationToken)
    {
        return GetAsync(id, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }
    
    public Task<IEnumerable<TResult>> GetMappedAsync<TResult>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetMappedAsync(filter, map, configuration, CancellationToken.None);
    }
    
    public async Task<IEnumerable<TResult>> GetMappedAsync<TResult>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return await GetQueryable(configuration, query => query.Where(filter))
            .Select(map)
            .ToListAsync(cancellationToken);
    }
    
    public Task<IEnumerable<TResult>> GetMappedAsync<TResult>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map,
        CancellationToken cancellationToken)
    {
        return GetMappedAsync(filter, map, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }

    public Task<IEnumerable<TResult>> GetMappedAsync<TResult>(
        ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetMappedAsync(specification, map, configuration, CancellationToken.None);
    }
    
    public Task<IEnumerable<TResult>> GetMappedAsync<TResult>(
        ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetMappedAsync(specification.SatisfiedBy(), map, configuration, cancellationToken);
    }
    
    public Task<IEnumerable<TResult>> GetMappedAsync<TResult>(
        ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map,
        CancellationToken cancellationToken = default)
    {
        return GetMappedAsync(specification, map, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetFilteredAsync(
        Expression<Func<TEntity, bool>> filter,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetFilteredAsync(filter, configuration, CancellationToken.None);
    }
    
    public async Task<IEnumerable<TEntity>> GetFilteredAsync(
        Expression<Func<TEntity, bool>> filter,
        Action<QueryableConfiguration<TEntity>> configuration, 
        CancellationToken cancellationToken)
    {
        return await GetQueryable(configuration, query => query.Where(filter))
            .ToListAsync(cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetFilteredAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
    {
        return GetFilteredAsync(filter, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }

    public Task<TEntity> GetFirstAsync(
        Expression<Func<TEntity, bool>> filter,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetFirstAsync(filter, configuration, CancellationToken.None);
    }
    
    public async Task<TEntity> GetFirstAsync(
        Expression<Func<TEntity, bool>> filter,
        Action<QueryableConfiguration<TEntity>> configuration, 
        CancellationToken cancellationToken)
    {
        return await GetQueryable(configuration, query => query.Where(filter))
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    public Task<TEntity> GetFirstAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
    {
        return GetFirstAsync(filter, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }

    public Task<TEntity> GetFirstAsync(
        ISpecification<TEntity> specification, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetFirstAsync(specification, configuration, CancellationToken.None);
    }

    public Task<TEntity> GetFirstAsync(
        ISpecification<TEntity> specification, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetFirstAsync(specification.SatisfiedBy(), configuration, cancellationToken);
    }
    
    public Task<TEntity> GetFirstAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken)
    {
        return GetFirstAsync(specification, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }
    
    public Task<TResult> GetFirstMappedAsync<TResult>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetFirstMappedAsync(filter, map, configuration, CancellationToken.None);
    }
    
    public Task<TResult> GetFirstMappedAsync<TResult>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter), "Filter expression cannot be null");
        }
        
        return GetQueryable(configuration, query => query.Where(filter))
            .Select(map)
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    public Task<TResult> GetFirstMappedAsync<TResult>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map,
        CancellationToken cancellationToken)
    {
        return GetFirstMappedAsync(filter, map, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }

    public Task<TResult> GetFirstMappedAsync<TResult>(
        ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetFirstMappedAsync(specification, map, configuration, CancellationToken.None);
    }
    
    public Task<TResult> GetFirstMappedAsync<TResult>(
        ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetFirstMappedAsync(specification.SatisfiedBy(), map, configuration, cancellationToken);
    }
    
    public Task<TResult> GetFirstMappedAsync<TResult>(
        ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map,
        CancellationToken cancellationToken = default)
    {
        return GetFirstMappedAsync(specification, map, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(
        int limit, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetPagedAsync(limit, configuration, CancellationToken.None);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        int limit, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return GetPagedAsync((Expression<Func<TEntity, bool>>)null, 1, limit, configuration, cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        return GetPagedAsync(limit, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(
        ISpecification<TEntity> specification, 
        int limit,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetPagedAsync(specification, limit, configuration, CancellationToken.None);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        ISpecification<TEntity> specification, 
        int limit,
        Action<QueryableConfiguration<TEntity>> configuration, 
        CancellationToken cancellationToken)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetPagedAsync(specification.SatisfiedBy(), 1, limit, configuration, cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        ISpecification<TEntity> specification, 
        int limit,
        CancellationToken cancellationToken)
    {
        return GetPagedAsync(specification, limit, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter, 
        int limit,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetPagedAsync(filter, limit, configuration, CancellationToken.None);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter, 
        int limit,
        Action<QueryableConfiguration<TEntity>> configuration, 
        CancellationToken cancellationToken)
    {
        return GetPagedAsync(filter, 1, limit, configuration, cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter, 
        int limit,
        CancellationToken cancellationToken)
    {
        return GetPagedAsync(filter, limit, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(
        int pageIndex, 
        int pageSize,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetPagedAsync(pageIndex, pageSize, configuration,
            CancellationToken.None);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        int pageIndex, 
        int pageSize,
        Action<QueryableConfiguration<TEntity>> configuration, 
        CancellationToken cancellationToken)
    {
        return GetPagedAsync((Expression<Func<TEntity, bool>>)null, pageIndex, pageSize, configuration,
            cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        int pageIndex, 
        int pageSize,
        CancellationToken cancellationToken)
    {
        return GetPagedAsync(pageIndex, pageSize, (Action<QueryableConfiguration<TEntity>>)null,
            cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(
        ISpecification<TEntity> specification, 
        int pageIndex, 
        int pageSize,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetPagedAsync(specification, pageIndex, pageSize, configuration, CancellationToken.None);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(
        ISpecification<TEntity> specification, 
        int pageIndex, 
        int pageSize,
        Action<QueryableConfiguration<TEntity>> configuration, 
        CancellationToken cancellationToken)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetPagedAsync(specification.SatisfiedBy(), pageIndex, pageSize, configuration, cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        ISpecification<TEntity> specification, 
        int pageIndex, 
        int pageSize,
        CancellationToken cancellationToken)
    {
        return GetPagedAsync(specification, pageIndex, pageSize, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter, 
        int pageIndex,
        int pageSize, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetPagedAsync(filter, pageIndex, pageSize, configuration, CancellationToken.None);
    }
    
    public async Task<IEnumerable<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter, 
        int pageIndex,
        int pageSize, 
        Action<QueryableConfiguration<TEntity>> configuration, 
        CancellationToken cancellationToken)
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

    public Task<IEnumerable<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter, 
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return GetPagedAsync(filter, pageIndex, pageSize, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }
    
    public Task<TEntity> GetSingleAsync(
        Expression<Func<TEntity, bool>> filter,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetSingleAsync(filter, configuration, CancellationToken.None);
    }
    
    public async Task<TEntity> GetSingleAsync(
        Expression<Func<TEntity, bool>> filter,
        Action<QueryableConfiguration<TEntity>> configuration, 
        CancellationToken cancellationToken)
    {
        return await GetQueryable(configuration, query => query.Where(filter))
            .SingleOrDefaultAsync(cancellationToken);
    }
    
    public Task<TEntity> GetSingleAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
    {
        return GetSingleAsync(filter, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }

    public Task<TEntity> GetSingleAsync(
        ISpecification<TEntity> specification, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return GetSingleAsync(specification, configuration, CancellationToken.None);
    }
    
    public Task<TEntity> GetSingleAsync(
        ISpecification<TEntity> specification, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        if (specification == null)
        {
            throw new ArgumentNullException(nameof(specification));
        }

        return GetSingleAsync(specification.SatisfiedBy(), configuration, cancellationToken);
    }
    
    public Task<TEntity> GetSingleAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken)
    {
        return GetSingleAsync(specification, (Action<QueryableConfiguration<TEntity>>)null, cancellationToken);
    }

    private async Task<TEntity> GetInternalAsync(TKey id, Action<QueryableConfiguration<TEntity>> configuration = default,
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
