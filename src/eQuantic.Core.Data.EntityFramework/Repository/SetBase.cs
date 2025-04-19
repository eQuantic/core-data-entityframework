using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.EntityFramework.Repository.Extensions;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace eQuantic.Core.Data.EntityFramework.Repository;

public abstract class SetBase<TEntity> : Data.Repository.ISet<TEntity> where TEntity : class, IEntity, new()
{
    protected readonly DbContext DbContext;

    protected SetBase(DbContext context)
    {
        DbContext = context ?? throw new ArgumentNullException(nameof(context));
        InternalDbSet = context.Set<TEntity>();
    }

    public Type ElementType => ((IQueryable<TEntity>)InternalDbSet).ElementType;
    public Expression Expression => ((IQueryable<TEntity>)InternalDbSet).Expression;
    public LocalView<TEntity> Local => InternalDbSet.Local;
    public IQueryProvider Provider => ((IQueryable<TEntity>)InternalDbSet).Provider;
    protected DbSet<TEntity> InternalDbSet { get; private set; }

    public virtual void AddRange(IEnumerable<TEntity> entities)
    {
        InternalDbSet.AddRange(entities);
    }

    public virtual void AddRange(params TEntity[] entities)
    {
        InternalDbSet.AddRange(entities);
    }
    
    public virtual Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        return InternalDbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual Task AddRangeAsync(params TEntity[] entities)
    {
        return InternalDbSet.AddRangeAsync(entities);
    }
    
    public virtual void ApplyCurrentValues(TEntity original, TEntity current)
    {
        //if it is not attached, attach original and set current values
        DbContext.Entry<TEntity>(original).CurrentValues.SetValues(current);
    }

    public virtual EntityEntry<TEntity> Attach(TEntity entity)
    {
        return InternalDbSet.Attach(entity);
    }

    public virtual void AttachRange(IEnumerable<TEntity> entities)
    {
        InternalDbSet.AttachRange(entities);
    }

    public virtual void AttachRange(params TEntity[] entities)
    {
        InternalDbSet.AttachRange(entities);
    }

    public virtual void Delete(Expression<Func<TEntity, bool>> filter)
    {
        var entity = InternalDbSet.Single(filter);
        InternalDbSet.Remove(entity);
    }

    public abstract long DeleteMany(Expression<Func<TEntity, bool>> filter);

    public abstract Task<long> DeleteManyAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);

    public override bool Equals(object obj)
    {
        return InternalDbSet.Equals(obj);
    }
    
    public virtual IEnumerable<TEntity> Execute()
    {
        return InternalDbSet.ToList();
    }

    public virtual TEntity Find<TKey>(TKey key)
    {
        return InternalDbSet.Find(key);
    }

    public virtual async Task<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken)
    {
        return await InternalDbSet.FindAsync(keyValues, cancellationToken);
    }

    public virtual async Task<TEntity> FindAsync(params object[] keyValues)
    {
        return await InternalDbSet.FindAsync(keyValues);
    }

    public virtual async Task<TEntity> FindAsync<TKey>(TKey key, CancellationToken cancellationToken = default)
    {
        return await InternalDbSet.FindAsync(new object[] { key }, cancellationToken);
    }

    public virtual IEnumerator<TEntity> GetEnumerator()
    {
        return ((IQueryable<TEntity>)InternalDbSet).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IQueryable<TEntity>)InternalDbSet).GetEnumerator();
    }

    public override int GetHashCode()
    {
        return InternalDbSet.GetHashCode();
    }

    public virtual void Insert(TEntity item)
    {
        InternalDbSet.Add(item);
    }

    public virtual async Task InsertAsync(TEntity item, CancellationToken cancellationToken = default)
    {
        await InternalDbSet.AddAsync(item, cancellationToken);
    }

    public virtual EntityEntry<TEntity> Remove(TEntity entity)
    {
        return InternalDbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        InternalDbSet.RemoveRange(entities);
    }

    public virtual void RemoveRange(params TEntity[] entities)
    {
        InternalDbSet.RemoveRange(entities);
    }

    public virtual void SetModified(TEntity item)
    {
        var entry = this.DbContext.Entry<TEntity>(item);

        if (DbContext.ChangeTracker.AutoDetectChangesEnabled && entry.State != EntityState.Detached)
        {
            return;
        }
        
        //this operation also attach item in object state manager
        entry.State = EntityState.Modified;
    }

    public override string ToString()
    {
        return InternalDbSet.ToString();
    }

    public virtual EntityEntry<TEntity> Update(TEntity entity)
    {
        return InternalDbSet.Update(entity);
    }

    public abstract long UpdateMany(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateExpression);

    public abstract Task<long> UpdateManyAsync(Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TEntity>> updateExpression, CancellationToken cancellationToken = default);

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        InternalDbSet.UpdateRange(entities);
    }

    public virtual void UpdateRange(params TEntity[] entities)
    {
        InternalDbSet.UpdateRange(entities);
    }
    
    internal Expression<Func<TEntity, bool>> GetExpression<TKey>(TKey id)
    {
        return DbContext.GetFindByKeyExpression<TEntity, TKey>(id);
    }
    
    protected static TConfig GetConfig<TConfig>(Action<TConfig> configuration)
        where TConfig : Configuration<TEntity>
    {
        Configuration<TEntity> config;
        if (configuration is Action<QueryableConfiguration<TEntity>>)
        {
            config = new QueryableConfiguration<TEntity>();
        }
        else
        {
            config = new DefaultConfiguration<TEntity>();
        }

        configuration.Invoke((TConfig)config);

        return (TConfig)config;
    }

    public abstract IQueryable<TEntity> GetQueryable<TConfig>(Action<TConfig> configuration,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> internalQueryAction)
        where TConfig : Configuration<TEntity>;
}