using System;
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

namespace eQuantic.Core.Data.EntityFramework.MongoDb.Repository;

public class Set<TEntity> : SetBase<TEntity> where TEntity : class, IEntity, new()
{
    public Set(DbContext context) : base(context)
    {
    }

    public override long DeleteMany(Expression<Func<TEntity, bool>> filter)
    {
        throw new NotImplementedException();
    }

    public override Task<long> DeleteManyAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override long UpdateMany(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateExpression)
    {
        throw new NotImplementedException();
    }

    public override Task<long> UpdateManyAsync(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateExpression, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override IQueryable<TEntity> GetQueryable<TConfig>(Action<TConfig> configuration, Func<IQueryable<TEntity>, IQueryable<TEntity>> internalQueryAction)
    {
        if (configuration == null)
        {
            return internalQueryAction.Invoke(this);
        }

        var config = GetConfig(configuration);
        var queryableConfig = config as QueryableConfiguration<TEntity>;
        
        IQueryable<TEntity> query = this;

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