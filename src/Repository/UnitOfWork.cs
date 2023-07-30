using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.EntityFramework.Repository.Sql;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Options;
using eQuantic.Core.Data.Repository.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.Data.EntityFramework.Repository;

public abstract class UnitOfWork : SqlExecutor
{
    protected static int IsMigrating;

    protected UnitOfWork(DbContext context) : base(context)
    {
    }
}

public abstract class UnitOfWork<TUnitOfWork, TDbContext> : UnitOfWork<TDbContext>, ISqlUnitOfWork<TUnitOfWork>
    where TDbContext : DbContext 
    where TUnitOfWork : ISqlUnitOfWork
{
    private readonly IServiceProvider _serviceProvider;
    
    protected UnitOfWork(IServiceProvider serviceProvider, TDbContext context) : base(context)
    {
        _serviceProvider = serviceProvider;
    }
    
    public virtual IRepository<TUnitOfWork, TEntity, TKey> GetRepository<TEntity, TKey>()
        where TEntity : class, IEntity, new()
    {
        return _serviceProvider.GetRequiredService<Repository<TUnitOfWork, TEntity, TKey>>();
    }

    public IAsyncRepository<TUnitOfWork, TEntity, TKey> GetAsyncRepository<TEntity, TKey>()
        where TEntity : class, IEntity, new()
    {
        return _serviceProvider.GetRequiredService<AsyncRepository<TUnitOfWork, TEntity, TKey>>();
    }

    public IQueryableRepository<TUnitOfWork, TEntity, TKey> GetQueryableRepository<TEntity, TKey>()
        where TEntity : class, IEntity, new()
    {
        return _serviceProvider.GetRequiredService<QueryableRepository<TUnitOfWork, TEntity, TKey>>();
    }

    public IAsyncQueryableRepository<TUnitOfWork, TEntity, TKey> GetAsyncQueryableRepository<TEntity, TKey>()
        where TEntity : class, IEntity, new()
    {
        return _serviceProvider.GetRequiredService<AsyncQueryableRepository<TUnitOfWork, TEntity, TKey>>();
    }
}

public abstract class UnitOfWork<TDbContext> : UnitOfWork, ISqlUnitOfWork 
    where TDbContext : DbContext
{
    private readonly TDbContext _context;
    

    protected UnitOfWork(TDbContext context) : base(context)
    {
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
                changes = await _context.SaveChangesAsync(cancellationToken);

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
        return await _context.SaveChangesAsync(cancellationToken);
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

    Data.Repository.ISet<TEntity> IQueryableUnitOfWork.CreateSet<TEntity>() => InternalCreateSet<TEntity>();

    public void ApplyCurrentValues<TEntity>(TEntity original, TEntity current) where TEntity : class, IEntity, new()
    {
        ((Set<TEntity>)InternalCreateSet<TEntity>()).ApplyCurrentValues(original, current);
    }

    public void Attach<TEntity>(TEntity item) where TEntity : class, IEntity, new()
    {
        ((Set<TEntity>)InternalCreateSet<TEntity>()).Attach(item);
    }

    public IEnumerable<string> GetPendingMigrations()
    {
        return _context.Database.GetPendingMigrations();
    }

    public void LoadProperty<TEntity, TComplexProperty>(TEntity item, Expression<Func<TEntity, TComplexProperty>> selector) 
        where TEntity : class, IEntity, new()
        where TComplexProperty : class
    {
        ((Set<TEntity>)InternalCreateSet<TEntity>()).LoadProperty(item, selector);
    }

    public void LoadProperty<TEntity>(TEntity item, string propertyName) 
        where TEntity : class, IEntity, new()
    {
        ((Set<TEntity>)InternalCreateSet<TEntity>()).LoadProperty(item, propertyName);
    }

    public Task LoadPropertyAsync<TEntity, TComplexProperty>(TEntity item, Expression<Func<TEntity, TComplexProperty>> selector, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity, new() 
        where TComplexProperty : class
    {
        return ((Set<TEntity>)InternalCreateSet<TEntity>()).LoadPropertyAsync(item, selector, cancellationToken);
    }

    public Task LoadPropertyAsync<TEntity>(TEntity item, string propertyName, CancellationToken cancellationToken = default) 
        where TEntity : class, IEntity, new() 
    {
        return ((Set<TEntity>)InternalCreateSet<TEntity>()).LoadPropertyAsync(item, propertyName, cancellationToken);
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
            await _context.Entry<TEntity>(item).Collection(navigationProperty).Query().Where(filter).LoadAsync();
        }
        else
        {
            await _context.Entry<TEntity>(item).Collection(navigationProperty).LoadAsync();
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

    public virtual Data.Repository.ISet<TEntity> CreateSet<TEntity>() where TEntity : class, IEntity, new() =>
        InternalCreateSet<TEntity>();

    public virtual SaveOptions GetSaveOptions()
    {
        return new SaveOptions();
    }

    private Data.Repository.ISet<TEntity> InternalCreateSet<TEntity>() where TEntity : class, IEntity, new() =>
        new Set<TEntity>(_context);
}
