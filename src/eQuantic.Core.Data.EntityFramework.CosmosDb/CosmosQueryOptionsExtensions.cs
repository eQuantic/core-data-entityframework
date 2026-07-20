using eQuantic.Core.Data.Repository.Options;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.CosmosDb;

/// <summary>
///     Azure Cosmos DB conveniences for <see cref="QueryOptions{TEntity}" />.
/// </summary>
public static class CosmosQueryOptionsExtensions
{
    /// <summary>
    ///     Restricts the query to a single Cosmos DB logical partition, avoiding a (costly) cross-partition
    ///     scan. Applied as a before-customization so it runs on the root query.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="options">The query options.</param>
    /// <param name="partitionKeyValue">The partition key value.</param>
    /// <returns>The same <see cref="QueryOptions{TEntity}" /> instance for chaining.</returns>
    public static QueryOptions<TEntity> WithPartitionKey<TEntity>(this QueryOptions<TEntity> options,
        string partitionKeyValue)
        where TEntity : class
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return options.WithBeforeCustomization(query => query.WithPartitionKey(partitionKeyValue));
    }

#if NET10_0_OR_GREATER
    /// <summary>
    ///     Restricts the query to a two-level hierarchical Cosmos DB partition key (EF Core 10+).
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="options">The query options.</param>
    /// <param name="partitionKeyValue">The first-level partition key value.</param>
    /// <param name="secondLevelPartitionKeyValue">The second-level partition key value.</param>
    /// <returns>The same <see cref="QueryOptions{TEntity}" /> instance for chaining.</returns>
    public static QueryOptions<TEntity> WithPartitionKey<TEntity>(this QueryOptions<TEntity> options,
        object partitionKeyValue, object secondLevelPartitionKeyValue)
        where TEntity : class
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return options.WithBeforeCustomization(query =>
            query.WithPartitionKey(partitionKeyValue, secondLevelPartitionKeyValue));
    }

    /// <summary>
    ///     Restricts the query to a three-level hierarchical Cosmos DB partition key (EF Core 10+).
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="options">The query options.</param>
    /// <param name="partitionKeyValue">The first-level partition key value.</param>
    /// <param name="secondLevelPartitionKeyValue">The second-level partition key value.</param>
    /// <param name="thirdLevelPartitionKeyValue">The third-level partition key value.</param>
    /// <returns>The same <see cref="QueryOptions{TEntity}" /> instance for chaining.</returns>
    public static QueryOptions<TEntity> WithPartitionKey<TEntity>(this QueryOptions<TEntity> options,
        object partitionKeyValue, object secondLevelPartitionKeyValue, object thirdLevelPartitionKeyValue)
        where TEntity : class
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return options.WithBeforeCustomization(query =>
            query.WithPartitionKey(partitionKeyValue, secondLevelPartitionKeyValue, thirdLevelPartitionKeyValue));
    }
#endif
}
