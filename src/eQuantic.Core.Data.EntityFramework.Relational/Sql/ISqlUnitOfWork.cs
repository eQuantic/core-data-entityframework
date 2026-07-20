using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.Repository;

namespace eQuantic.Core.Data.EntityFramework.Relational.Sql;

/// <summary>
/// The UnitOfWork contract for the EF relational implementation.
/// <remarks>
/// This contract extends IQueryableUnitOfWork with relational/SQL specific operations.
/// Rehomed into the Relational provider from the removed <c>eQuantic.Core.Data.Repository.Sql</c>
/// namespace (dropped in eQuantic.Core.Data v5). The <c>new()</c> entity constraint that v4 required
/// has been removed to match the v5 <see cref="IEntity" /> surface.
/// </remarks>
/// </summary>
public interface ISqlUnitOfWork : IQueryableUnitOfWork, ISqlExecutor, IAsyncSqlExecutor
{
    /// <summary>
    /// Apply current values in <paramref name="original"/>
    /// </summary>
    /// <typeparam name="TEntity">The type of entity</typeparam>
    /// <param name="original">The original entity</param>
    /// <param name="current">The current entity</param>
    void ApplyCurrentValues<TEntity>(TEntity original, TEntity current) where TEntity : class, IEntity;

    /// <summary>
    /// Attach this item into "ObjectStateManager"
    /// </summary>
    /// <typeparam name="TEntity">The type of entity</typeparam>
    /// <param name="item">The item </param>
    void Attach<TEntity>(TEntity item) where TEntity : class, IEntity;

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> GetPendingMigrations();

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="item"></param>
    /// <param name="navigationProperty"></param>
    /// <param name="filter"></param>
    void LoadCollection<TEntity, TElement>(TEntity item,
        Expression<Func<TEntity, IEnumerable<TElement>>> navigationProperty,
        Expression<Func<TElement, bool>> filter = null)
        where TEntity : class
        where TElement : class;

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="item"></param>
    /// <param name="navigationProperty"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task LoadCollectionAsync<TEntity, TElement>(TEntity item,
        Expression<Func<TEntity, IEnumerable<TElement>>> navigationProperty,
        Expression<Func<TElement, bool>> filter = null) where TEntity : class where TElement : class;

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TComplexProperty"></typeparam>
    /// <param name="item"></param>
    /// <param name="selector"></param>
    void LoadProperty<TEntity, TComplexProperty>(TEntity item, Expression<Func<TEntity, TComplexProperty>> selector)
        where TEntity : class, IEntity
        where TComplexProperty : class;

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="item"></param>
    /// <param name="propertyName"></param>
    void LoadProperty<TEntity>(TEntity item, string propertyName)
        where TEntity : class, IEntity;

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TComplexProperty"></typeparam>
    /// <param name="item"></param>
    /// <param name="selector"></param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns></returns>
    Task LoadPropertyAsync<TEntity, TComplexProperty>(TEntity item,
        Expression<Func<TEntity, TComplexProperty>> selector, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity
        where TComplexProperty : class;

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="item"></param>
    /// <param name="propertyName"></param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task LoadPropertyAsync<TEntity>(TEntity item, string propertyName, CancellationToken cancellationToken = default)
        where TEntity : class, IEntity;

    /// <summary>
    /// Reload this item ignoring cache
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="item"></param>
    void Reload<TEntity>(TEntity item) where TEntity : class;

    /// <summary>
    /// Set object as modified
    /// </summary>
    /// <typeparam name="TEntity">The type of entity</typeparam>
    /// <param name="item">The entity item to set as modifed</param>
    void SetModified<TEntity>(TEntity item) where TEntity : class;

    /// <summary>
    ///
    /// </summary>
    void UpdateDatabase();
}
