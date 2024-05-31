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

namespace eQuantic.Core.Data.EntityFramework.MongoDb.Repository;

public abstract class UnitOfWork : IQueryableUnitOfWork
{
    protected readonly IServiceProvider ServiceProvider;
    /// <summary>
    ///     The context
    /// </summary>
    protected readonly DbContext Context;
    
    /// <summary>
    ///     The disposed
    /// </summary>
    protected bool Disposed;

    /// <summary>
    ///     Initializes a new instance of the class
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="context">The context</param>
    protected UnitOfWork(IServiceProvider serviceProvider, DbContext context)
    {
        ServiceProvider = serviceProvider;
        Context = context;
    }
    
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
                changes = await Context.SaveChangesAsync(cancellationToken);

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
        return await Context.SaveChangesAsync(cancellationToken);
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

    public void ApplyCurrentValues<TEntity>(TEntity original, TEntity current) where TEntity : class, IEntity, new()
    {
        ((Set<TEntity>)InternalCreateSet<TEntity>()).ApplyCurrentValues(original, current);
    }

    public void Attach<TEntity>(TEntity item) where TEntity : class, IEntity, new()
    {
        ((Set<TEntity>)InternalCreateSet<TEntity>()).Attach(item);
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
            await Context.Entry<TEntity>(item).Collection(navigationProperty).Query().Where(filter).LoadAsync();
        }
        else
        {
            await Context.Entry<TEntity>(item).Collection(navigationProperty).LoadAsync();
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
        // set all entities in change tracker
        // as 'unchanged state'
        Context?.ChangeTracker.Entries()
            .ToList()
            .ForEach(entry => entry.State = EntityState.Unchanged);
    }

    public void SetModified<TEntity>(TEntity item) where TEntity : class
    {
        //this operation also attach item in object state manager
        Context.Entry<TEntity>(item).State = EntityState.Modified;
    }

    public virtual SaveOptions GetSaveOptions()
    {
        return new SaveOptions();
    }
    
    public virtual IRepository<TUnitOfWork, TEntity, TKey> GetRepository<TUnitOfWork, TEntity, TKey>()
        where TEntity : class, IEntity, new() where TUnitOfWork : IUnitOfWork
    {
        var repo = ServiceProvider.GetRequiredService<IRepository<TUnitOfWork, TEntity, TKey>>();
        return repo;
    }

    public IAsyncRepository<TUnitOfWork, TEntity, TKey> GetAsyncRepository<TUnitOfWork, TEntity, TKey>()
        where TEntity : class, IEntity, new() 
        where TUnitOfWork : IUnitOfWork
    {
        return ServiceProvider.GetRequiredService<IAsyncRepository<TUnitOfWork, TEntity, TKey>>();
    }

    public Data.Repository.ISet<TEntity> CreateSet<TEntity>() where TEntity : class, IEntity, new() => InternalCreateSet<TEntity>();

    public IQueryableRepository<TUnitOfWork, TEntity, TKey> GetQueryableRepository<TUnitOfWork, TEntity, TKey>()
        where TEntity : class, IEntity, new() 
        where TUnitOfWork : IQueryableUnitOfWork
    {
        return ServiceProvider.GetRequiredService<IQueryableRepository<TUnitOfWork, TEntity, TKey>>();
    }

    public IAsyncQueryableRepository<TUnitOfWork, TEntity, TKey> GetAsyncQueryableRepository<TUnitOfWork, TEntity, TKey>()
        where TEntity : class, IEntity, new() 
        where TUnitOfWork : IQueryableUnitOfWork
    {
        return ServiceProvider.GetRequiredService<IAsyncQueryableRepository<TUnitOfWork, TEntity, TKey>>();
    }
    
    internal DbContext GetDbContext() => Context;
    
    internal Data.Repository.ISet<TEntity> InternalCreateSet<TEntity>() where TEntity : class, IEntity, new() =>
        new Set<TEntity>(Context);
    
    /// <summary>
    ///     Disposes this instance
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    ///     Disposes the disposing
    /// </summary>
    /// <param name="disposing">The disposing</param>
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

public abstract class UnitOfWork<TDbContext>(IServiceProvider serviceProvider, TDbContext context)
    : UnitOfWork(serviceProvider, context)
    where TDbContext : DbContext;