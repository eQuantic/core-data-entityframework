using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.EntityFramework.Repository.Read;
using eQuantic.Core.Data.EntityFramework.Repository.Write;
using eQuantic.Core.Data.Repository;
using eQuantic.Linq.Specification;

namespace eQuantic.Core.Data.EntityFramework.Repository;

[ExcludeFromCodeCoverage]
public class AsyncQueryableRepository<TEntity, TKey> :
    AsyncQueryableReadRepository<TEntity, TKey>,
    IAsyncQueryableRepository<TEntity, TKey>,
    IQueryableRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    private readonly AsyncWriteRepository<TEntity> _asyncWriteRepository;

    public AsyncQueryableRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
    {
        _asyncWriteRepository = new AsyncWriteRepository<TEntity>(unitOfWork);
    }

    // -------------------------------------------------------------- synchronous write (IWriteRepository)

    public void Add(TEntity item)
    {
        _asyncWriteRepository.Add(item);
    }

    public void AddRange(IEnumerable<TEntity> items)
    {
        _asyncWriteRepository.AddRange(items);
    }

    public long DeleteMany(Expression<Func<TEntity, bool>> filter)
    {
        return _asyncWriteRepository.DeleteMany(filter);
    }

    public long DeleteMany(ISpecification<TEntity> specification)
    {
        return _asyncWriteRepository.DeleteMany(specification);
    }

    public void Merge(TEntity persisted, TEntity current)
    {
        _asyncWriteRepository.Merge(persisted, current);
    }

    public void Modify(TEntity item)
    {
        _asyncWriteRepository.Modify(item);
    }

    public void Remove(TEntity item)
    {
        _asyncWriteRepository.Remove(item);
    }

    public void TrackItem(TEntity item)
    {
        _asyncWriteRepository.TrackItem(item);
    }

    public long UpdateMany(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        return _asyncWriteRepository.UpdateMany(filter, updateFactory);
    }

    public long UpdateMany(ISpecification<TEntity> specification, Expression<Func<TEntity, TEntity>> updateFactory)
    {
        return _asyncWriteRepository.UpdateMany(specification, updateFactory);
    }

    // -------------------------------------------------------------- asynchronous write (IAsyncWriteRepository)

    public Task AddAsync(TEntity item, CancellationToken cancellationToken = default)
    {
        return _asyncWriteRepository.AddAsync(item, cancellationToken);
    }

    public Task AddRangeAsync(IEnumerable<TEntity> items, CancellationToken cancellationToken = default)
    {
        return _asyncWriteRepository.AddRangeAsync(items, cancellationToken);
    }

    public Task<long> DeleteManyAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        return _asyncWriteRepository.DeleteManyAsync(filter, cancellationToken);
    }

    public Task<long> DeleteManyAsync(ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return _asyncWriteRepository.DeleteManyAsync(specification, cancellationToken);
    }

    public Task MergeAsync(TEntity persisted, TEntity current)
    {
        return _asyncWriteRepository.MergeAsync(persisted, current);
    }

    public Task ModifyAsync(TEntity item)
    {
        return _asyncWriteRepository.ModifyAsync(item);
    }

    public Task RemoveAsync(TEntity item)
    {
        return _asyncWriteRepository.RemoveAsync(item);
    }

    public Task<long> UpdateManyAsync(Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TEntity>> updateFactory, CancellationToken cancellationToken = default)
    {
        return _asyncWriteRepository.UpdateManyAsync(filter, updateFactory, cancellationToken);
    }

    public Task<long> UpdateManyAsync(ISpecification<TEntity> specification,
        Expression<Func<TEntity, TEntity>> updateFactory, CancellationToken cancellationToken = default)
    {
        return _asyncWriteRepository.UpdateManyAsync(specification, updateFactory, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _asyncWriteRepository?.Dispose();
        }

        base.Dispose(disposing);
    }
}
