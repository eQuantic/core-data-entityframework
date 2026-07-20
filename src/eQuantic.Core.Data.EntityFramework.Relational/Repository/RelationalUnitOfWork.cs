using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.EntityFramework.Relational.Sql;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.Data.EntityFramework.Relational.Repository;

/// <summary>
///     The shared non-generic relational unit of work. Providers derive their own thin
///     <c>UnitOfWork&lt;TDbContext&gt;</c> from <see cref="RelationalUnitOfWork{TDbContext}" />.
/// </summary>
public abstract class RelationalUnitOfWork : RelationalSqlExecutor
{
    protected static int IsMigrating;

    protected RelationalUnitOfWork(DbContext context) : base(context)
    {
    }

    internal DbContext GetDbContext() => Context;
}

public abstract class RelationalUnitOfWork<TDbContext> : RelationalUnitOfWork, ISqlUnitOfWork
    where TDbContext : DbContext
{
    private readonly TDbContext _context;
    private readonly IServiceProvider _serviceProvider;

    protected RelationalUnitOfWork(IServiceProvider serviceProvider, TDbContext context) : base(context)
    {
        _serviceProvider = serviceProvider;
        _context = context;
    }

    public int Commit()
    {
        return _context.SaveChanges();
    }

    public int CommitAndRefreshChanges()
    {
        var changes = 0;
        var saveFailed = false;

        do
        {
            try
            {
                changes = _context.SaveChanges();

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
                changes = await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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
        return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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

    public Data.Repository.ISet<TEntity> CreateSet<TEntity>() where TEntity : class, IEntity =>
        InternalCreateSet<TEntity>();

    public void ApplyCurrentValues<TEntity>(TEntity original, TEntity current) where TEntity : class, IEntity
    {
        ((RelationalSet<TEntity>)InternalCreateSet<TEntity>()).ApplyCurrentValues(original, current);
    }

    public void Attach<TEntity>(TEntity item) where TEntity : class, IEntity
    {
        ((RelationalSet<TEntity>)InternalCreateSet<TEntity>()).Attach(item);
    }

    public IEnumerable<string> GetPendingMigrations()
    {
        return _context.Database.GetPendingMigrations();
    }

    public void LoadProperty<TEntity, TComplexProperty>(TEntity item, Expression<Func<TEntity, TComplexProperty>> selector)
        where TEntity : class, IEntity
        where TComplexProperty : class
    {
        ((RelationalSet<TEntity>)InternalCreateSet<TEntity>()).LoadProperty(item, selector);
    }

    public void LoadProperty<TEntity>(TEntity item, string propertyName)
        where TEntity : class, IEntity
    {
        ((RelationalSet<TEntity>)InternalCreateSet<TEntity>()).LoadProperty(item, propertyName);
    }

    public Task LoadPropertyAsync<TEntity, TComplexProperty>(TEntity item, Expression<Func<TEntity, TComplexProperty>> selector, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
        where TComplexProperty : class
    {
        return ((RelationalSet<TEntity>)InternalCreateSet<TEntity>()).LoadPropertyAsync(item, selector, cancellationToken);
    }

    public Task LoadPropertyAsync<TEntity>(TEntity item, string propertyName, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
    {
        return ((RelationalSet<TEntity>)InternalCreateSet<TEntity>()).LoadPropertyAsync(item, propertyName, cancellationToken);
    }

    public void LoadCollection<TEntity, TElement>(TEntity item,
        Expression<Func<TEntity, IEnumerable<TElement>>> navigationProperty,
        Expression<Func<TElement, bool>> filter = null) where TEntity : class where TElement : class
    {
        if (filter != null)
        {
            _context.Entry<TEntity>(item).Collection(navigationProperty).Query().Where(filter).Load();
        }
        else
        {
            _context.Entry<TEntity>(item).Collection(navigationProperty).Load();
        }
    }

    public async Task LoadCollectionAsync<TEntity, TElement>(TEntity item,
        Expression<Func<TEntity, IEnumerable<TElement>>> navigationProperty,
        Expression<Func<TElement, bool>> filter = null) where TEntity : class where TElement : class
    {
        if (filter != null)
        {
            await _context.Entry<TEntity>(item).Collection(navigationProperty).Query().Where(filter).LoadAsync().ConfigureAwait(false);
        }
        else
        {
            await _context.Entry<TEntity>(item).Collection(navigationProperty).LoadAsync().ConfigureAwait(false);
        }
    }

    public void Reload<TEntity>(TEntity item) where TEntity : class
    {
        var entry = _context.Entry(item);
        entry.CurrentValues.SetValues(entry.OriginalValues);
        entry.Reload();
    }

    public void RollbackChanges()
    {
        // set all entities in change tracker
        // as 'unchanged state'
        _context?.ChangeTracker.Entries()
            .ToList()
            .ForEach(entry => entry.State = EntityState.Unchanged);
    }

    public void SetModified<TEntity>(TEntity item) where TEntity : class
    {
        //this operation also attach item in object state manager
        _context.Entry<TEntity>(item).State = EntityState.Modified;
    }

    public void UpdateDatabase()
    {
        if (0 != Interlocked.Exchange(ref IsMigrating, 1))
        {
            return;
        }

        try
        {
            _context.Database.Migrate();
        }
        finally
        {
            Interlocked.Exchange(ref IsMigrating, 0);
        }
    }

    public virtual SaveOptions GetSaveOptions()
    {
        return new SaveOptions();
    }

    public virtual IRepository<TEntity, TKey> GetRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>
    {
        return _serviceProvider.GetRequiredService<IRepository<TEntity, TKey>>();
    }

    public IAsyncRepository<TEntity, TKey> GetAsyncRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>
    {
        return _serviceProvider.GetRequiredService<IAsyncRepository<TEntity, TKey>>();
    }

    public IQueryableRepository<TEntity, TKey> GetQueryableRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>
    {
        return _serviceProvider.GetRequiredService<IQueryableRepository<TEntity, TKey>>();
    }

    public IAsyncQueryableRepository<TEntity, TKey> GetAsyncQueryableRepository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>
    {
        return _serviceProvider.GetRequiredService<IAsyncQueryableRepository<TEntity, TKey>>();
    }

    private Data.Repository.ISet<TEntity> InternalCreateSet<TEntity>() where TEntity : class, IEntity =>
        new RelationalSet<TEntity>(_context);
}
