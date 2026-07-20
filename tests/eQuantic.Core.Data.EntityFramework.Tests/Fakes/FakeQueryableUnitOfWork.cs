using System;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Options;

namespace eQuantic.Core.Data.EntityFramework.Tests.Fakes;

/// <summary>
///     A non-relational unit of work: it implements <see cref="IQueryableUnitOfWork" /> but not the
///     relational SQL unit-of-work contract — mirroring the MongoDb provider. Members throw because the
///     DI-registration tests only inspect the service collection; the fake is never instantiated or
///     resolved.
/// </summary>
internal sealed class FakeQueryableUnitOfWork : IQueryableUnitOfWork
{
    public int DisposeCount { get; private set; }

    public void Dispose() => DisposeCount++;

    public int Commit() => throw new NotSupportedException();
    public int Commit(Action<SaveOptions> options) => throw new NotSupportedException();
    public int CommitAndRefreshChanges() => throw new NotSupportedException();
    public int CommitAndRefreshChanges(Action<SaveOptions> options) => throw new NotSupportedException();

    public Task<int> CommitAndRefreshChangesAsync(CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<int> CommitAndRefreshChangesAsync(Action<SaveOptions> options, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<int> CommitAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<int> CommitAsync(Action<SaveOptions> options, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public void RollbackChanges() => throw new NotSupportedException();

    public IRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>
        => throw new NotSupportedException();

    public IAsyncRepository<TEntity, TKey> GetAsyncRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>
        => throw new NotSupportedException();

    public eQuantic.Core.Data.Repository.ISet<TEntity> CreateSet<TEntity>() where TEntity : class, IEntity
        => throw new NotSupportedException();

    public IQueryableRepository<TEntity, TKey> GetQueryableRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>
        => throw new NotSupportedException();

    public IAsyncQueryableRepository<TEntity, TKey> GetAsyncQueryableRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>
        => throw new NotSupportedException();
}
