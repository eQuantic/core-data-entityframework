using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.Data.EntityFramework.Repository;

/// <summary>
///     The shared, provider-agnostic <see cref="IQueryableUnitOfWork" /> implementation for the document
///     (non-relational) EF Core providers. It carries every unit-of-work member that does not depend on the
///     store; each provider supplies only its own entity set by overriding <see cref="CreateSetCore{TEntity}" />.
///     The relational providers keep their own unit of work (it also is a raw-SQL executor).
/// </summary>
public abstract class EntityFrameworkUnitOfWork : IQueryableUnitOfWork
{
    /// <summary>The service provider used to resolve repositories.</summary>
    protected readonly IServiceProvider ServiceProvider;

    /// <summary>The underlying <see cref="DbContext" />.</summary>
    protected readonly DbContext Context;

    /// <summary>Whether this instance has been disposed.</summary>
    protected bool Disposed;

    /// <summary>Initializes a new instance of the <see cref="EntityFrameworkUnitOfWork" /> class.</summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="context">The context.</param>
    protected EntityFrameworkUnitOfWork(IServiceProvider serviceProvider, DbContext context)
    {
        ServiceProvider = serviceProvider;
        Context = context;
    }

    /// <summary>Creates the provider-specific entity set for <typeparamref name="TEntity" />.</summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <returns>The provider's <see cref="Data.Repository.ISet{TEntity}" /> (a <see cref="SetBase{TEntity}" />).</returns>
    protected abstract Data.Repository.ISet<TEntity> CreateSetCore<TEntity>() where TEntity : class, IEntity;

    public int Commit()
    {
        return Context.SaveChanges();
    }

    public int CommitAndRefreshChanges()
    {
        var changes = 0;
        var saveFailed = false;

        do
        {
            try
            {
                changes = Context.SaveChanges();

                saveFailed = false;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                saveFailed = true;

                ex.Entries.ToList()
                    .ForEach(entry => entry.OriginalValues.SetValues(entry.GetDatabaseValues()));
            }
        } while (saveFailed);

        return changes;
    }

    public async Task<int> CommitAndRefreshChangesAsync(CancellationToken cancellationToken = default)
    {
        var changes = 0;
        var saveFailed = false;

        do
        {
            try
            {
                changes = await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                saveFailed = false;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                saveFailed = true;

                ex.Entries.ToList()
                    .ForEach(entry => entry.OriginalValues.SetValues(entry.GetDatabaseValues()));
            }
        } while (saveFailed);

        return changes;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await Context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public int Commit(Action<SaveOptions> options)
    {
        return Commit();
    }

    public int CommitAndRefreshChanges(Action<SaveOptions> options)
    {
        return CommitAndRefreshChanges();
    }

    public Task<int> CommitAndRefreshChangesAsync(Action<SaveOptions> options, CancellationToken cancellationToken = default)
    {
        return CommitAndRefreshChangesAsync(cancellationToken);
    }

    public Task<int> CommitAsync(Action<SaveOptions> options, CancellationToken cancellationToken = default)
    {
        return CommitAsync(cancellationToken);
    }

    public void ApplyCurrentValues<TEntity>(TEntity original, TEntity current) where TEntity : class, IEntity
    {
        ((SetBase<TEntity>)CreateSetCore<TEntity>()).ApplyCurrentValues(original, current);
    }

    public void Attach<TEntity>(TEntity item) where TEntity : class, IEntity
    {
        ((SetBase<TEntity>)CreateSetCore<TEntity>()).Attach(item);
    }

    public void LoadCollection<TEntity, TElement>(TEntity item,
        Expression<Func<TEntity, IEnumerable<TElement>>> navigationProperty,
        Expression<Func<TElement, bool>> filter = null) where TEntity : class where TElement : class
    {
        if (filter != null)
        {
            Context.Entry<TEntity>(item).Collection(navigationProperty).Query().Where(filter).Load();
        }
        else
        {
            Context.Entry<TEntity>(item).Collection(navigationProperty).Load();
        }
    }

    public async Task LoadCollectionAsync<TEntity, TElement>(TEntity item,
        Expression<Func<TEntity, IEnumerable<TElement>>> navigationProperty,
        Expression<Func<TElement, bool>> filter = null) where TEntity : class where TElement : class
    {
        if (filter != null)
        {
            await Context.Entry<TEntity>(item).Collection(navigationProperty).Query().Where(filter).LoadAsync().ConfigureAwait(false);
        }
        else
        {
            await Context.Entry<TEntity>(item).Collection(navigationProperty).LoadAsync().ConfigureAwait(false);
        }
    }

    public void Reload<TEntity>(TEntity item) where TEntity : class
    {
        var entry = Context.Entry(item);
        entry.CurrentValues.SetValues(entry.OriginalValues);
        entry.Reload();
    }

    public void RollbackChanges()
    {
        // set all entities in change tracker as 'unchanged state'
        Context?.ChangeTracker.Entries()
            .ToList()
            .ForEach(entry => entry.State = EntityState.Unchanged);
    }

    public void SetModified<TEntity>(TEntity item) where TEntity : class
    {
        // this operation also attaches item in the object state manager
        Context.Entry<TEntity>(item).State = EntityState.Modified;
    }

    public virtual SaveOptions GetSaveOptions()
    {
        return new SaveOptions();
    }

    public virtual IRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>
    {
        return ServiceProvider.GetRequiredService<IRepository<TEntity, TKey>>();
    }

    public IAsyncRepository<TEntity, TKey> GetAsyncRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>
    {
        return ServiceProvider.GetRequiredService<IAsyncRepository<TEntity, TKey>>();
    }

    public Data.Repository.ISet<TEntity> CreateSet<TEntity>() where TEntity : class, IEntity => CreateSetCore<TEntity>();

    public IQueryableRepository<TEntity, TKey> GetQueryableRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>
    {
        return ServiceProvider.GetRequiredService<IQueryableRepository<TEntity, TKey>>();
    }

    public IAsyncQueryableRepository<TEntity, TKey> GetAsyncQueryableRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>
    {
        return ServiceProvider.GetRequiredService<IAsyncQueryableRepository<TEntity, TKey>>();
    }

    internal DbContext GetDbContext() => Context;

    /// <summary>Disposes this instance.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Disposes the managed resources.</summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (Disposed)
        {
            return;
        }

        if (disposing)
        {
            Context?.Dispose();
        }

        Disposed = true;
    }
}

/// <summary>
///     The strongly-typed <see cref="EntityFrameworkUnitOfWork" /> that a document provider's unit of work
///     derives from over its concrete <typeparamref name="TDbContext" />.
/// </summary>
/// <typeparam name="TDbContext">The context type.</typeparam>
public abstract class EntityFrameworkUnitOfWork<TDbContext>(IServiceProvider serviceProvider, TDbContext context)
    : EntityFrameworkUnitOfWork(serviceProvider, context)
    where TDbContext : DbContext;
