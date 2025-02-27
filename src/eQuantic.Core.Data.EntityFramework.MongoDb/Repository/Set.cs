using System.Linq.Expressions;
using eQuantic.Core.Data.EntityFramework.Repository;
using eQuantic.Core.Data.EntityFramework.Repository.Extensions;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Config;
using eQuantic.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;

namespace eQuantic.Core.Data.EntityFramework.MongoDb.Repository;

public class Set<TEntity> : SetBase<TEntity> where TEntity : class, IEntity, new()
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string? _collectionName;
    private IMongoDatabase? _mongoDatabase;
    
    public Set(IServiceProvider serviceProvider, DbContext context) : base(context)
    {
        _serviceProvider = serviceProvider;
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        _collectionName = entityType?.GetCollectionName();
    }
    
    public override long DeleteMany(Expression<Func<TEntity, bool>> filter)
    {
        var collection = GetCollection();
        if (collection == null) return 0;

        var result = collection.DeleteMany(filter);
        return result.DeletedCount;
    }

    public override async Task<long> DeleteManyAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();
        if (collection == null) return 0;
        
        var mongoFilter = Builders<TEntity>.Filter.Where(filter);
        var result = await collection.DeleteManyAsync(mongoFilter, cancellationToken);
        
        return result.DeletedCount;
    }

    public override long UpdateMany(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateExpression)
    {
        var collection = GetCollection();
        if (collection == null) return 0;

        var mongoFilter = Builders<TEntity>.Filter.Where(filter);
        var updateDefinition = UpdateDefinitionBuilder.BuildUpdateDefinition(updateExpression);

        if (updateDefinition == null)
            return 0;
        
        var result = collection.UpdateMany(mongoFilter, updateDefinition);
        return result.ModifiedCount;
    }

    public override async Task<long> UpdateManyAsync(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateExpression, CancellationToken cancellationToken = default)
    {
        var collection = GetCollection();
        if (collection == null) return 0;

        var mongoFilter = Builders<TEntity>.Filter.Where(filter);
        var updateDefinition = UpdateDefinitionBuilder.BuildUpdateDefinition(updateExpression);

        if (updateDefinition == null)
            return 0;
        
        var result = await collection.UpdateManyAsync(mongoFilter, updateDefinition, null, cancellationToken);
        return result.ModifiedCount;
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
    
    private IMongoCollection<TEntity>? GetCollection()
    {
        _mongoDatabase ??= _serviceProvider.GetService<IMongoClient>()?.GetDatabase(_collectionName);
        return _mongoDatabase?.GetCollection<TEntity>(_collectionName);
    }
}