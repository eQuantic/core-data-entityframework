using System.Linq.Expressions;
using eQuantic.Core.Data.EntityFramework.Repository;
using eQuantic.Core.Data.EntityFramework.Repository.Extensions;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Config;
using eQuantic.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;
using MongoDB.EntityFrameworkCore.Infrastructure;

namespace eQuantic.Core.Data.EntityFramework.MongoDb.Repository;

public class Set<TEntity> : SetBase<TEntity> where TEntity : class, IEntity, new()
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string? _collectionName;
    private readonly string? _databaseName;
    private IMongoDatabase? _mongoDatabase;

    public Set(IServiceProvider serviceProvider, DbContext context) : base(context)
    {
        _serviceProvider = serviceProvider;
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        _collectionName = entityType?.GetCollectionName();

        // Resolve the database name from the DbContext configuration up front (the context is only
        // available here), NOT from the collection name.
        _databaseName = context.GetService<IDbContextOptions>()
            .FindExtension<MongoOptionsExtension>()?.DatabaseName;
    }
    
    public override long DeleteMany(Expression<Func<TEntity, bool>> filter)
    {
        var result = GetCollection().DeleteMany(filter);
        return result.DeletedCount;
    }

    public override async Task<long> DeleteManyAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        var mongoFilter = Builders<TEntity>.Filter.Where(filter);
        var result = await GetCollection().DeleteManyAsync(mongoFilter, cancellationToken);

        return result.DeletedCount;
    }

    public override long UpdateMany(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateExpression)
    {
        var mongoFilter = Builders<TEntity>.Filter.Where(filter);
        var updateDefinition = UpdateDefinitionBuilder.BuildUpdateDefinition(updateExpression);

        if (updateDefinition == null)
            return 0;

        var result = GetCollection().UpdateMany(mongoFilter, updateDefinition);
        return result.ModifiedCount;
    }

    public override async Task<long> UpdateManyAsync(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateExpression, CancellationToken cancellationToken = default)
    {
        var mongoFilter = Builders<TEntity>.Filter.Where(filter);
        var updateDefinition = UpdateDefinitionBuilder.BuildUpdateDefinition(updateExpression);

        if (updateDefinition == null)
            return 0;

        var result = await GetCollection().UpdateManyAsync(mongoFilter, updateDefinition, null, cancellationToken);
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
    
    private IMongoCollection<TEntity> GetCollection()
    {
        return GetDatabase().GetCollection<TEntity>(_collectionName);
    }

    private IMongoDatabase GetDatabase()
    {
        if (_mongoDatabase != null)
        {
            return _mongoDatabase;
        }

        // The database name comes from the DbContext configuration (UseMongoDB(..., databaseName)),
        // NOT from the collection name. Using the collection name here meant every bulk operation ran
        // against the wrong database and silently affected zero documents.
        if (string.IsNullOrEmpty(_databaseName))
        {
            throw new InvalidOperationException(
                $"Could not resolve the MongoDB database name for '{typeof(TEntity).Name}'. " +
                "Ensure the DbContext is configured with UseMongoDB(..., databaseName).");
        }

        var client = _serviceProvider.GetService<IMongoClient>()
            ?? throw new InvalidOperationException(
                "No IMongoClient is registered in the service provider; bulk MongoDB operations " +
                "(DeleteMany/UpdateMany) cannot be executed. Register an IMongoClient in DI.");

        _mongoDatabase = client.GetDatabase(_databaseName);
        return _mongoDatabase;
    }
}