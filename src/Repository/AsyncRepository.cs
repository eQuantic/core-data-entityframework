using System;
using System.Collections.Generic;
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

public class AsyncRepository<TUnitOfWork, TConfig, TEntity, TKey> : Repository<TUnitOfWork, TConfig, TEntity, TKey>,
    IAsyncRepository<TUnitOfWork, TConfig, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
    where TConfig : Configuration<TEntity>
{
    private readonly IAsyncReadRepository<TUnitOfWork, TConfig, TEntity, TKey> _asyncReadRepository;
    private readonly IAsyncWriteRepository<TUnitOfWork, TEntity> _asyncWriteRepository;
    private bool _disposed;

    public AsyncRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
        this._asyncReadRepository = new AsyncReadRepository<TUnitOfWork, TConfig, TEntity, TKey>(unitOfWork);
        this._asyncWriteRepository = new AsyncWriteRepository<TUnitOfWork, TEntity>(unitOfWork);
    }

    public Task AddAsync(TEntity item)
    {
        return this._asyncWriteRepository.AddAsync(item);
    }

    public Task<IEnumerable<TEntity>> AllMatchingAsync(ISpecification<TEntity> specification,
        Action<TConfig> configuration = default, CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.AllMatchingAsync(specification, configuration, cancellationToken);
    }

    public Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.CountAsync(cancellationToken);
    }

    public Task<long> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.CountAsync(specification, cancellationToken);
    }

    public Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.CountAsync(filter, cancellationToken);
    }

    public Task<bool> AllAsync(ISpecification<TEntity> specification, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.AllAsync(specification, configuration, cancellationToken);
    }

    public Task<bool> AllAsync(Expression<Func<TEntity, bool>> filter, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.AllAsync(filter, configuration, cancellationToken);
    }

    public Task<bool> AnyAsync(Action<TConfig> configuration = default, CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.AnyAsync(configuration, cancellationToken);
    }

    public Task<bool> AnyAsync(ISpecification<TEntity> specification, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.AnyAsync(specification, configuration, cancellationToken);
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.AnyAsync(filter, configuration, cancellationToken);
    }

    public Task<long> DeleteManyAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        return this._asyncWriteRepository.DeleteManyAsync(filter, cancellationToken);
    }

    public Task<long> DeleteManyAsync(ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return this._asyncWriteRepository.DeleteManyAsync(specification, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetAllAsync(Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetAllAsync(configuration, cancellationToken);
    }

    public Task<TEntity> GetAsync(TKey id, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetAsync(id, configuration, cancellationToken);
    }

    public Task<IEnumerable<TResult>> GetMappedAsync<TResult>(Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetMappedAsync(filter, map, configuration, cancellationToken);
    }

    public Task<IEnumerable<TResult>> GetMappedAsync<TResult>(ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetMappedAsync(specification, map, configuration, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> filter,
        Action<TConfig> configuration = default, CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetFilteredAsync(filter, configuration, cancellationToken);
    }

    public Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> filter, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetFirstAsync(filter, configuration, cancellationToken);
    }

    public Task<TEntity> GetFirstAsync(ISpecification<TEntity> specification, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetFirstAsync(specification, configuration, cancellationToken);
    }

    public Task<TResult> GetFirstMappedAsync<TResult>(Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TResult>> map, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetFirstMappedAsync(filter, map, configuration, cancellationToken);
    }

    public Task<TResult> GetFirstMappedAsync<TResult>(ISpecification<TEntity> specification,
        Expression<Func<TEntity, TResult>> map, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetFirstMappedAsync(specification, map, configuration, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int limit, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetPagedAsync(limit, configuration, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(ISpecification<TEntity> specification, int limit,
        Action<TConfig> configuration = default, CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetPagedAsync(specification, limit, configuration, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> filter, int limit,
        Action<TConfig> configuration = default, CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetPagedAsync(filter, limit, configuration, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(int pageIndex, int pageSize,
        Action<TConfig> configuration = default, CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetPagedAsync(pageIndex, pageSize, configuration, cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(
        ISpecification<TEntity> specification, int pageIndex, int pageSize,
        Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetPagedAsync(specification, pageIndex, pageSize, configuration,
            cancellationToken);
    }

    public Task<IEnumerable<TEntity>> GetPagedAsync(
        Expression<Func<TEntity, bool>> filter, int pageIndex, int pageSize,
        Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetPagedAsync(filter, pageIndex, pageSize, configuration, cancellationToken);
    }

    public Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> filter, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetSingleAsync(filter, configuration, cancellationToken);
    }

    public Task<TEntity> GetSingleAsync(ISpecification<TEntity> specification, Action<TConfig> configuration = default,
        CancellationToken cancellationToken = default)
    {
        return this._asyncReadRepository.GetSingleAsync(specification, configuration, cancellationToken);
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

public class AsyncRepository<TUnitOfWork, TEntity, TKey> :
    AsyncRepository<TUnitOfWork, Configuration<TEntity>, TEntity, TKey>, IAsyncRepository<TUnitOfWork, TEntity, TKey>
    where TUnitOfWork : IQueryableUnitOfWork
    where TEntity : class, IEntity, new()
{
    public AsyncRepository(TUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}
