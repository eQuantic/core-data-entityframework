using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace eQuantic.Core.Data.EntityFramework.Relational.Sql;

/// <summary>
/// The async sql executor interface
/// </summary>
/// <remarks>
///     Rehomed into the Relational provider from the removed <c>eQuantic.Core.Data.Repository.Sql</c>
///     namespace (dropped in eQuantic.Core.Data v5).
/// </remarks>
public interface IAsyncSqlExecutor<out TConfig> where TConfig : SqlConfiguration
{
    /// <summary>
    /// Begins the transaction asynchronous.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the transaction asynchronous.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollbacks the transaction asynchronous.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the raw SQL asynchronous.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">The SQL.</param>
    /// <param name="map">The map.</param>
    /// <param name="config">The configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<IEnumerable<T>> ExecuteRawSqlAsync<T>(string sql, Func<DbDataReader, T> map, Action<TConfig> config = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute async arbitrary command into underlying persistence store
    /// </summary>
    /// <param name="sqlCommand">Command to execute</param>
    /// <param name="config"></param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of affected records</returns>
    Task<int> ExecuteCommandAsync(string sqlCommand, Action<TConfig> config = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute Function Async.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="name"></param>
    /// <param name="config"></param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<TResult> ExecuteFunctionAsync<TResult>(string name, Action<TConfig> config = null, CancellationToken cancellationToken = default)
        where TResult : class;

    /// <summary>
    /// Execute Procedure Async.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="config"></param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<int> ExecuteProcedureAsync(string name, Action<TConfig> config = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Use transaction async.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task UseTransactionAsync(DbTransaction transaction, CancellationToken cancellationToken = default);
}

public interface IAsyncSqlExecutor : IAsyncSqlExecutor<DefaultSqlConfiguration>
{
}
