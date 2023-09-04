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
using eQuantic.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Z.EntityFramework.Plus;

namespace eQuantic.Core.Data.EntityFramework.Repository;

public class Set<TEntity> : Data.Repository.ISet<TEntity> where TEntity : class, IEntity, new()
{
    private readonly DbContext _dbContext;

    public Set(DbContext context)
    {
        this._dbContext = context ?? throw new ArgumentNullException(nameof(context));
        InternalDbSet = context.Set<TEntity>();
    }

    public Type ElementType => ((IQueryable<TEntity>)InternalDbSet).ElementType;
    public Expression Expression => ((IQueryable<TEntity>)InternalDbSet).Expression;
    public LocalView<TEntity> Local => InternalDbSet.Local;
    public IQueryProvider Provider => ((IQueryable<TEntity>)InternalDbSet).Provider;
    protected DbSet<TEntity> InternalDbSet { get; private set; }

    public virtual EntityEntry<TEntity> Add(TEntity entity)
    {
        return InternalDbSet.Add(entity);
    }

    public virtual async Task<EntityEntry<TEntity>> AddAsync(TEntity entity,
        CancellationToken cancellationToken = default)
    {
        return await InternalDbSet.AddAsync(entity, cancellationToken);
    }

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
        _dbContext.Entry<TEntity>(original).CurrentValues.SetValues(current);
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

    public virtual long DeleteMany(Expression<Func<TEntity, bool>> filter)
    {
        return InternalDbSet.Where(filter).Delete();
    }

    public virtual async Task<long> DeleteManyAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        return await InternalDbSet.Where(filter).DeleteAsync(cancellationToken);
    }

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

    public void LoadCollection<TChildEntity, TComplexProperty>(TChildEntity item,
        Expression<Func<TChildEntity, IEnumerable<TComplexProperty>>> selector)
        where TChildEntity : class
        where TComplexProperty : class
    {
        _dbContext.Entry(item).Collection(selector).Load();
    }

    public void LoadCollection<TChildEntity>(TChildEntity item, string propertyName)
        where TChildEntity : class
    {
        _dbContext.Entry(item).Collection(propertyName).Load();
    }

    public async Task LoadCollectionAsync<TChildEntity, TComplexProperty>(TChildEntity item,
        Expression<Func<TChildEntity, IEnumerable<TComplexProperty>>> selector)
        where TChildEntity : class where TComplexProperty : class
    {
        await _dbContext.Entry(item).Collection(selector).LoadAsync();
    }

    public async Task LoadCollectionAsync<TChildEntity>(TChildEntity item, string propertyName)
        where TChildEntity : class
    {
        await _dbContext.Entry(item).Collection(propertyName).LoadAsync();
    }

    public void LoadProperties(TEntity entity, params string[] properties)
    {
        if (properties is not { Length: > 0 })
        {
            return;
        }

        foreach (var property in properties)
        {
            if (string.IsNullOrEmpty(property))
            {
                continue;
            }

            var props = property.Split('.');

            if (props.Length == 1)
            {
                LoadProperty(entity, property);
            }
            else
            {
                LoadCascade(props, entity);
            }
        }
    }

    public async Task LoadPropertiesAsync(TEntity entity, params string[] properties)
    {
        if (properties is { Length: > 0 })
        {
            foreach (var property in properties)
            {
                if (string.IsNullOrEmpty(property))
                {
                    continue;
                }

                var props = property.Split('.');

                if (props.Length == 1)
                {
                    await LoadPropertyAsync(entity, property);
                }
                else
                {
                    await LoadCascadeAsync(props, entity);
                }
            }
        }
    }

    public void LoadProperty<TChildEntity, TComplexProperty>(TChildEntity item,
        Expression<Func<TChildEntity, TComplexProperty>> selector)
        where TChildEntity : class
        where TComplexProperty : class
    {
        _dbContext.Entry(item).Reference(selector).Load();
    }

    public void LoadProperty<TChildEntity>(TChildEntity item, string propertyName)
        where TChildEntity : class
    {
        if (typeof(IEnumerable).IsAssignableFrom(typeof(TChildEntity).GetProperty(propertyName)!.PropertyType))
        {
            _dbContext.Entry(item).Collection(propertyName).Load();
        }
        else
        {
            _dbContext.Entry(item).Reference(propertyName).Load();
        }
    }

    public async Task LoadPropertyAsync<TChildEntity, TComplexProperty>(TChildEntity item,
        Expression<Func<TChildEntity, TComplexProperty>> selector, CancellationToken cancellationToken = default)
        where TChildEntity : class where TComplexProperty : class
    {
        await _dbContext.Entry(item).Reference(selector).LoadAsync(cancellationToken);
    }

    public async Task LoadPropertyAsync<TChildEntity>(TChildEntity item, string propertyName,
        CancellationToken cancellationToken = default)
        where TChildEntity : class
    {
        if (typeof(IEnumerable).IsAssignableFrom(typeof(TChildEntity).GetProperty(propertyName)!.PropertyType))
        {
            await _dbContext.Entry(item).Collection(propertyName).LoadAsync(cancellationToken);
        }
        else
        {
            await _dbContext.Entry(item).Reference(propertyName).LoadAsync(cancellationToken);
        }
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
        //this operation also attach item in object state manager
        this._dbContext.Entry<TEntity>(item).State = EntityState.Modified;
    }

    public override string ToString()
    {
        return InternalDbSet.ToString();
    }

    public virtual EntityEntry<TEntity> Update(TEntity entity)
    {
        return InternalDbSet.Update(entity);
    }

    public virtual long UpdateMany(Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TEntity>> updateExpression)
    {
        return InternalDbSet.Where(filter).Update(updateExpression);
    }

    public virtual async Task<long> UpdateManyAsync(Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TEntity>> updateExpression, CancellationToken cancellationToken = default)
    {
        return await InternalDbSet.Where(filter).UpdateAsync(updateExpression, cancellationToken);
    }

    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        InternalDbSet.UpdateRange(entities);
    }

    public virtual void UpdateRange(params TEntity[] entities)
    {
        InternalDbSet.UpdateRange(entities);
    }

    private void LoadCascade(string[] props, object obj, int index = 0)
    {
        if (obj == null)
        {
            return;
        }

        var prop = obj.GetType().GetProperty(props[index]);
        var nextObj = prop?.GetValue(obj);
        if (nextObj == null)
        {
            LoadProperty(obj, props[index]);
            nextObj = prop?.GetValue(obj);
        }

        if (props.Length > index + 1)
        {
            LoadCascade(props, nextObj, index + 1);
        }
    }

    private async Task LoadCascadeAsync(string[] props, object obj, int index = 0)
    {
        if (obj == null)
        {
            return;
        }

        var prop = obj.GetType().GetProperty(props[index]);
        var nextObj = prop?.GetValue(obj);
        if (nextObj == null)
        {
            await LoadPropertyAsync(obj, props[index]);
            nextObj = prop?.GetValue(obj);
        }

        if (props.Length > index + 1)
        {
            await LoadCascadeAsync(props, nextObj, index + 1);
        }
    }

    internal Expression<Func<TEntity, bool>> GetExpression<TKey>(TKey id)
    {
        return _dbContext.GetFindByKeyExpression<TEntity, TKey>(id);
    }

    private static TConfig GetConfig<TConfig>(Action<TConfig> configuration)
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

    internal IQueryable<TEntity> GetQueryable<TConfig>(Action<TConfig> configuration,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> internalQueryAction)
        where TConfig : Configuration<TEntity>
    {
        if (configuration == null)
        {
            return internalQueryAction.Invoke(this);
        }

        var config = GetConfig(configuration);
        var queryableConfig = config as QueryableConfiguration<TEntity>;
        
        var query = string.IsNullOrEmpty(queryableConfig?.SqlRaw) ? this : InternalDbSet.FromSqlRaw(queryableConfig.SqlRaw);

        if (config.HasNoTracking)
        {
            query = query.AsNoTracking();
        }

        if (config.Properties?.Any() == true)
        {
            query = query.IncludeMany(config.Properties.ToArray());
        }

        if (queryableConfig?.IgnoreQueryFilters == true)
        {
            query = query.IgnoreQueryFilters();
        }
        
        if (!string.IsNullOrEmpty(config.Tag))
        {
            query = query.TagWith(config.Tag);
        }

        if (queryableConfig != null)
        {
            query = queryableConfig.BeforeCustomization.Invoke(query);
        }

        query = internalQueryAction.Invoke(query);

        if (config.SortingColumns.Any())
        {
            query = query.OrderBy(config.SortingColumns.ToArray());
        }

        if (queryableConfig != null)
        {
            query = queryableConfig.AfterCustomization.Invoke(query);
        }

        return query;
    }
}
