using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.EntityFramework.Repository.Read;
using eQuantic.Core.Data.EntityFramework.Repository.Write;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Config;
using eQuantic.Core.Data.Repository.Read;
using eQuantic.Core.Data.Repository.Write;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository;

[ExcludeFromCodeCoverage]
public class AsyncQueryableRepository<TUnitOfWork, TEntity, TKey> :
    QueryableRepository<TUnitOfWork, TEntity, TKey>,
    IAsyncRepository<TUnitOfWork, QueryableConfiguration<TEntity>, TEntity, TKey>,
    IAsyncQueryableRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : class, IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    private readonly IAsyncQueryableReadRepository<TUnitOfWork, TEntity, TKey> _asyncReadRepository;
    private readonly IAsyncWriteRepository<TUnitOfWork, TEntity> _asyncWriteRepository;
    private bool _disposed;

    public AsyncQueryableRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
        this._asyncReadRepository = new AsyncQueryableReadRepository<TUnitOfWork, TEntity, TKey>(unitOfWork);
        this._asyncWriteRepository = new AsyncWriteRepository<TUnitOfWork, TEntity>(unitOfWork);
    }

    public Task AddAsync(TEntity item)
    {
        return this._asyncWriteRepository.AddAsync(item);
    }

    public Task<IEnumerable<TEntity>> AllMatchingAsync(
        ISpecification<TEntity> specification,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.AllMatchingAsync(specification, configuration);
    }

    public Task<IEnumerable<TEntity>> AllMatchingAsync(
        ISpecification<TEntity> specification,
        Action<QueryableConfiguration<TEntity>> configuration, 
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.AllMatchingAsync(specification, configuration, cancellationToken);
    }
    public Task<IEnumerable<TEntity>> AllMatchingAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.AllMatchingAsync(specification, cancellationToken);
    }
    
    public Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.CountAsync(cancellationToken);
    }

    public Task<long> CountAsync(
        ISpecification<TEntity> specification, 
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.CountAsync(specification, cancellationToken);
    }

    public Task<long> CountAsync(
        Expression<Func<TEntity, bool>> filter, 
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.CountAsync(filter, cancellationToken);
    }

    public Task<bool> AllAsync(
        ISpecification<TEntity> specification, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.AllAsync(specification, configuration);
    }
    
    public Task<bool> AllAsync(
        ISpecification<TEntity> specification, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.AllAsync(specification, configuration, cancellationToken);
    }
    
    public Task<bool> AllAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.AllAsync(specification, cancellationToken);
    }

    public Task<bool> AllAsync(
        Expression<Func<TEntity, bool>> filter, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.AllAsync(filter, configuration);
    }
    
    public Task<bool> AllAsync(
        Expression<Func<TEntity, bool>> filter, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.AllAsync(filter, configuration, cancellationToken);
    }
    
    public Task<bool> AllAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.AllAsync(filter, cancellationToken);
    }

    public Task<bool> AnyAsync(
        Action<QueryableConfiguration<TEntity>> configuration = default, 
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.AnyAsync(configuration, cancellationToken);
    }

    public Task<bool> AnyAsync(
        ISpecification<TEntity> specification, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.AnyAsync(specification, configuration);
    }
    
    public Task<bool> AnyAsync(
        ISpecification<TEntity> specification, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.AnyAsync(specification, configuration, cancellationToken);
    }
    
    public Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.AnyAsync(specification, cancellationToken);
    }

    public Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> filter, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.AnyAsync(filter, configuration);
    }
    
    public Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> filter, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.AnyAsync(filter, configuration, cancellationToken);
    }
    
    public Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.AnyAsync(filter, cancellationToken);
    }

    public Task<long> DeleteManyAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        return this._asyncWriteRepository.DeleteManyAsync(filter, cancellationToken);
    }

    public Task<long> DeleteManyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return this._asyncWriteRepository.DeleteManyAsync(specification, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetAllAsync(
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetAllAsync(configuration);
    }

    public Task<IEnumerable<TEntity>> GetAllAsync(
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetAllAsync(configuration, cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetAllAsync(cancellationToken);
    }
    
    public Task<TEntity> GetAsync(
        TKey id, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetAsync(id, configuration);
    }

    public Task<TEntity> GetAsync(
        TKey id, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetAsync(id, configuration, cancellationToken);
    }
    
    public Task<TEntity> GetAsync(
        TKey id,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetAsync(id, cancellationToken);
    }
    
    public Task<IEnumerable<TResult>> GetMappedAsync<TResult>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetMappedAsync(filter, map, configuration);
    }
    
    public Task<IEnumerable<TResult>> GetMappedAsync<TResult>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetMappedAsync(filter, map, configuration, cancellationToken);
    }
    
    public Task<IEnumerable<TResult>> GetMappedAsync<TResult>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetMappedAsync(filter, map, cancellationToken);
    }

    public Task<IEnumerable<TResult>> GetMappedAsync<TResult>(
        ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetMappedAsync(specification, map, configuration);
    }
    
    public Task<IEnumerable<TResult>> GetMappedAsync<TResult>(
        ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetMappedAsync(specification, map, configuration, cancellationToken);
    }
    
    public Task<IEnumerable<TResult>> GetMappedAsync<TResult>(
        ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetMappedAsync(specification, map, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetFilteredAsync(
        Expression<Func<TEntity, bool>> filter,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetFilteredAsync(filter, configuration);
    }
    
    public Task<IEnumerable<TEntity>> GetFilteredAsync(
        Expression<Func<TEntity, bool>> filter,
        Action<QueryableConfiguration<TEntity>> configuration, 
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetFilteredAsync(filter, configuration, cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetFilteredAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetFilteredAsync(filter, cancellationToken);
    }

    public Task<TEntity> GetFirstAsync(
        Expression<Func<TEntity, bool>> filter, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetFirstAsync(filter, configuration);
    }

    public Task<TEntity> GetFirstAsync(
        Expression<Func<TEntity, bool>> filter, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetFirstAsync(filter, configuration, cancellationToken);
    }
    
    public Task<TEntity> GetFirstAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetFirstAsync(filter, cancellationToken);
    }
    
    public Task<TEntity> GetFirstAsync(
        ISpecification<TEntity> specification, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetFirstAsync(specification, configuration);
    }

    public Task<TEntity> GetFirstAsync(
        ISpecification<TEntity> specification, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetFirstAsync(specification, configuration, cancellationToken);
    }
    
    public Task<TEntity> GetFirstAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetFirstAsync(specification, cancellationToken);
    }
    
    public Task<TResult> GetFirstMappedAsync<TResult>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetFirstMappedAsync(filter, map, configuration);
    }
    
    public Task<TResult> GetFirstMappedAsync<TResult>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetFirstMappedAsync(filter, map, configuration, cancellationToken);
    }
    
    public Task<TResult> GetFirstMappedAsync<TResult>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetFirstMappedAsync(filter, map, cancellationToken);
    }

    public Task<TResult> GetFirstMappedAsync<TResult>(
        ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetFirstMappedAsync(specification, map, configuration);
    }
    
    public Task<TResult> GetFirstMappedAsync<TResult>(
        ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetFirstMappedAsync(specification, map, configuration, cancellationToken);
    }
    
    public Task<TResult> GetFirstMappedAsync<TResult>(
        ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetFirstMappedAsync(specification, map, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(
        int limit, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetPagedAsync(limit, configuration);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(
        int limit, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetPagedAsync(limit, configuration, cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetPagedAsync(limit, cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        ISpecification<TEntity> specification, 
        int limit,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetPagedAsync(specification, limit, configuration);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        ISpecification<TEntity> specification, 
        int limit,
        Action<QueryableConfiguration<TEntity>> configuration, 
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetPagedAsync(specification, limit, configuration, cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        ISpecification<TEntity> specification, 
        int limit,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetPagedAsync(specification, limit, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter, 
        int limit,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetPagedAsync(filter, limit, configuration);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter, 
        int limit,
        Action<QueryableConfiguration<TEntity>> configuration, 
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetPagedAsync(filter, limit, configuration, cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter, 
        int limit,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetPagedAsync(filter, limit, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(
        int pageIndex, 
        int pageSize,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetPagedAsync(pageIndex, pageSize, configuration);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        int pageIndex, 
        int pageSize,
        Action<QueryableConfiguration<TEntity>> configuration, 
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetPagedAsync(pageIndex, pageSize, configuration, cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        int pageIndex, 
        int pageSize,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetPagedAsync(pageIndex, pageSize, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(
        ISpecification<TEntity> specification, 
        int pageIndex, 
        int pageSize,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetPagedAsync(specification, pageIndex, pageSize, configuration);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(
        ISpecification<TEntity> specification, 
        int pageIndex, 
        int pageSize,
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetPagedAsync(specification, pageIndex, pageSize, configuration,
            cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        ISpecification<TEntity> specification, 
        int pageIndex, 
        int pageSize,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetPagedAsync(specification, pageIndex, pageSize,
            cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter, 
        int pageIndex, 
        int pageSize,
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetPagedAsync(filter, pageIndex, pageSize, configuration);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter, 
        int pageIndex, 
        int pageSize,
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetPagedAsync(filter, pageIndex, pageSize, configuration, cancellationToken);
    }
    
    public Task<IEnumerable<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter, 
        int pageIndex, 
        int pageSize,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetPagedAsync(filter, pageIndex, pageSize, cancellationToken);
    }

    public Task<TEntity> GetSingleAsync(
        Expression<Func<TEntity, bool>> filter, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetSingleAsync(filter, configuration);
    }
    
    public Task<TEntity> GetSingleAsync(
        Expression<Func<TEntity, bool>> filter, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetSingleAsync(filter, configuration, cancellationToken);
    }
    
    public Task<TEntity> GetSingleAsync(
        Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetSingleAsync(filter, cancellationToken);
    }

    public Task<TEntity> GetSingleAsync(
        ISpecification<TEntity> specification, 
        Action<QueryableConfiguration<TEntity>> configuration = default)
    {
        return this._asyncReadRepository.GetSingleAsync(specification, configuration);
    }
    
    public Task<TEntity> GetSingleAsync(
        ISpecification<TEntity> specification, 
        Action<QueryableConfiguration<TEntity>> configuration,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetSingleAsync(specification, configuration, cancellationToken);
    }
    
    public Task<TEntity> GetSingleAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken)
    {
        return this._asyncReadRepository.GetSingleAsync(specification, cancellationToken);
    }

    public Task MergeAsync(TEntity persisted, TEntity current)
    {
        return this._asyncWriteRepository.MergeAsync(persisted, current);
    }

    public Task ModifyAsync(TEntity item)
    {
        return this._asyncWriteRepository.ModifyAsync(item);
    }

    public Task RemoveAsync(TEntity item)
    {
        return this._asyncWriteRepository.RemoveAsync(item);
    }

    public Task<long> UpdateManyAsync(Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TEntity>> updateFactory, CancellationToken cancellationToken = default)
    {
        return this._asyncWriteRepository.UpdateManyAsync(filter, updateFactory, cancellationToken);
    }

    public Task<long> UpdateManyAsync(ISpecification<TEntity> specification,
        Expression<Func<TEntity, TEntity>> updateFactory, CancellationToken cancellationToken = default)
    {
        return this._asyncWriteRepository.UpdateManyAsync(specification, updateFactory, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            this._asyncReadRepository?.Dispose();
            this._asyncWriteRepository?.Dispose();
            UnitOfWork?.Dispose();
        }

        _disposed = true;
    }
}
