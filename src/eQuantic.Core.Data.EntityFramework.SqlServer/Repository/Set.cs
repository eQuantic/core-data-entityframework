using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.EntityFramework.Repository;
using eQuantic.Core.Data.EntityFramework.Repository.Extensions;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Config;
using eQuantic.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
#if NET6_0 || NETSTANDARD2_1
using Z.EntityFramework.Plus;
#endif

namespace eQuantic.Core.Data.EntityFramework.SqlServer.Repository;

public class Set<TEntity> : SetBase<TEntity> where TEntity : class, IEntity, new()
{
    public Set(DbContext context) : base(context)
    {
    }

    public override long DeleteMany(Expression<Func<TEntity, bool>> filter)
    {
#if NET6_0 || NETSTANDARD2_1
        return InternalDbSet.Where(filter).Delete();
#else
        return InternalDbSet.Where(filter).ExecuteDelete();
#endif
    }

    public override async Task<long> DeleteManyAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken = default)
    {
#if NET6_0 || NETSTANDARD2_1
        return await InternalDbSet.Where(filter).DeleteAsync(cancellationToken);
#else
        return await InternalDbSet.Where(filter).ExecuteDeleteAsync(cancellationToken);
#endif
    }

    public void LoadCollection<TChildEntity, TComplexProperty>(TChildEntity item,
        Expression<Func<TChildEntity, IEnumerable<TComplexProperty>>> selector)
        where TChildEntity : class
        where TComplexProperty : class
    {
        DbContext.Entry(item).Collection(selector).Load();
    }

    public void LoadCollection<TChildEntity>(TChildEntity item, string propertyName)
        where TChildEntity : class
    {
        DbContext.Entry(item).Collection(propertyName).Load();
    }

    public async Task LoadCollectionAsync<TChildEntity, TComplexProperty>(TChildEntity item,
        Expression<Func<TChildEntity, IEnumerable<TComplexProperty>>> selector)
        where TChildEntity : class where TComplexProperty : class
    {
        await DbContext.Entry(item).Collection(selector).LoadAsync();
    }

    public async Task LoadCollectionAsync<TChildEntity>(TChildEntity item, string propertyName)
        where TChildEntity : class
    {
        await DbContext.Entry(item).Collection(propertyName).LoadAsync();
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
        DbContext.Entry(item).Reference(selector).Load();
    }

    public void LoadProperty<TChildEntity>(TChildEntity item, string propertyName)
        where TChildEntity : class
    {
        if (typeof(IEnumerable).IsAssignableFrom(typeof(TChildEntity).GetProperty(propertyName)!.PropertyType))
        {
            DbContext.Entry(item).Collection(propertyName).Load();
        }
        else
        {
            DbContext.Entry(item).Reference(propertyName).Load();
        }
    }

    public async Task LoadPropertyAsync<TChildEntity, TComplexProperty>(TChildEntity item,
        Expression<Func<TChildEntity, TComplexProperty>> selector, CancellationToken cancellationToken = default)
        where TChildEntity : class where TComplexProperty : class
    {
        await DbContext.Entry(item).Reference(selector).LoadAsync(cancellationToken);
    }

    public async Task LoadPropertyAsync<TChildEntity>(TChildEntity item, string propertyName,
        CancellationToken cancellationToken = default)
        where TChildEntity : class
    {
        if (typeof(IEnumerable).IsAssignableFrom(typeof(TChildEntity).GetProperty(propertyName)!.PropertyType))
        {
            await DbContext.Entry(item).Collection(propertyName).LoadAsync(cancellationToken);
        }
        else
        {
            await DbContext.Entry(item).Reference(propertyName).LoadAsync(cancellationToken);
        }
    }

    public override long UpdateMany(Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TEntity>> updateExpression)
    {
#if NET6_0 || NETSTANDARD2_1
        return InternalDbSet.Where(filter).Update(updateExpression);
#else
        var convertedExpression = ExpressionConverter<TEntity>.ConvertExpression(updateExpression);
        return InternalDbSet.Where(filter).ExecuteUpdate(convertedExpression);
#endif
    }

    public override async Task<long> UpdateManyAsync(Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TEntity>> updateExpression, CancellationToken cancellationToken = default)
    {
#if NET6_0 || NETSTANDARD2_1
        return await InternalDbSet.Where(filter).UpdateAsync(updateExpression, cancellationToken);
#else
        var convertedExpression = ExpressionConverter<TEntity>.ConvertExpression(updateExpression);
        return await InternalDbSet.Where(filter).ExecuteUpdateAsync(convertedExpression, cancellationToken);
#endif
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
        return DbContext.GetFindByKeyExpression<TEntity, TKey>(id);
    }

    public override IQueryable<TEntity> GetQueryable<TConfig>(Action<TConfig> configuration,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> internalQueryAction)
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
