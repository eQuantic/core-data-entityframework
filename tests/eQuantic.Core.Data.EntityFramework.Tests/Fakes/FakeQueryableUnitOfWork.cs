using System;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Options;

namespace eQuantic.Core.Data.EntityFramework.Tests.Fakes;

/// <summary>
///     A non-relational unit of work: it implements <see cref="IQueryableUnitOfWork" /> but NOT
///     <see cref="eQuantic.Core.Data.Repository.Sql.ISqlUnitOfWork" /> — mirroring the MongoDb provider.
///     Members throw because the DI-registration tests only inspect the service collection; the fake is
///     never instantiated or resolved.
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

    public IRepository<TUnitOfWork, TEntity, TKey> GetRepository<TUnitOfWork, TEntity, TKey>()
        where TEntity : class, IEntity, new()
        where TUnitOfWork : IUnitOfWork
        => throw new NotSupportedException();

    public IAsyncRepository<TUnitOfWork, TEntity, TKey> GetAsyncRepository<TUnitOfWork, TEntity, TKey>()
        where TEntity : class, IEntity, new()
        where TUnitOfWork : IUnitOfWork
        => throw new NotSupportedException();

    public eQuantic.Core.Data.Repository.ISet<TEntity> CreateSet<TEntity>() where TEntity : class, IEntity, new()
        => throw new NotSupportedException();

    public IQueryableRepository<TUnitOfWork, TEntity, TKey> GetQueryableRepository<TUnitOfWork, TEntity, TKey>()
        where TEntity : class, IEntity, new()
        where TUnitOfWork : IQueryableUnitOfWork
        => throw new NotSupportedException();

    public IAsyncQueryableRepository<TUnitOfWork, TEntity, TKey> GetAsyncQueryableRepository<TUnitOfWork, TEntity, TKey>()
        where TEntity : class, IEntity, new()
        where TUnitOfWork : IQueryableUnitOfWork
        => throw new NotSupportedException();
}
