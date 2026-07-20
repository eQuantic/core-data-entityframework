using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace eQuantic.Core.Data.EntityFramework.Relational.Sql;

/// <summary>
/// Base contract for support 'dialect specific queries'.
/// </summary>
/// <remarks>
///     Rehomed into the Relational provider from the removed <c>eQuantic.Core.Data.Repository.Sql</c>
///     namespace (dropped in eQuantic.Core.Data v5).
/// </remarks>
public interface ISqlExecutor<out TConfig> where TConfig : SqlConfiguration
{
    /// <summary>
    /// Begins the transaction.
    /// </summary>
    void BeginTransaction();

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    void CommitTransaction();

    /// <summary>
    /// Executes the raw SQL.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql">The SQL.</param>
    /// <param name="map">The map.</param>
    /// <param name="config">The configuration.</param>
    /// <returns></returns>
    IEnumerable<T> ExecuteRawSql<T>(string sql, Func<DbDataReader, T> map, Action<TConfig> config = null);

    /// <summary>
    /// Execute arbitrary command into underlying persistence store
    /// </summary>
    /// <param name="sqlCommand">
    /// Command to execute
    /// <example>
    /// SELECT idCustomer,Name FROM dbo.[Customers] WHERE idCustomer > {0}
    /// </example>
    ///</param>
    /// <param name="config">The configuration a vector of parameters values</param>
    /// <returns>The number of affected records</returns>
    int ExecuteCommand(string sqlCommand, Action<TConfig> config = null);

    /// <summary>
    /// Executes a SQL statement using the specified sql command
    /// </summary>
    /// <param name="sqlCommand">The sql command</param>
    /// <param name="config"></param>
    /// <returns>The number of rows affected.</returns>
    public int ExecuteNonQuery(string sqlCommand, Action<TConfig> config = null);

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="name"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    TResult ExecuteFunction<TResult>(string name, Action<TConfig> config = null) where TResult : class;

    /// <summary>
    ///
    /// </summary>
    /// <param name="name"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    int ExecuteProcedure(string name, Action<TConfig> config = null);

    /// <summary>
    /// Execute specific query with underlying persistence store
    /// </summary>
    /// <typeparam name="TEntity">Entity type to map query results</typeparam>
    /// <param name="sqlQuery">
    /// Dialect Query
    /// <example>
    /// SELECT idCustomer,Name FROM dbo.[Customers] WHERE idCustomer > {0}
    /// </example>
    /// </param>
    /// <param name="config">The configuration with a vector of parameters values</param>
    /// <returns>
    /// Enumerable results
    /// </returns>
    IEnumerable<TEntity> ExecuteQuery<TEntity>(string sqlQuery, Action<TConfig> config = null) where TEntity : class;

    /// <summary>
    /// Executes the transaction.
    /// </summary>
    /// <param name="operation">The operation.</param>
    void ExecuteTransaction(Action<ISqlUnitOfWork> operation);

    /// <summary>
    /// Executes the transaction async.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ExecuteTransactionAsync(Func<ISqlUnitOfWork, Task> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transaction.
    /// </summary>
    /// <returns></returns>
    DbTransaction GetTransaction();

    /// <summary>
    /// Rollbacks the transaction.
    /// </summary>
    void RollbackTransaction();

    /// <summary>
    /// Use transaction.
    /// </summary>
    /// <param name="transaction"></param>
    void UseTransaction(DbTransaction transaction);
}

public interface ISqlExecutor : ISqlExecutor<DefaultSqlConfiguration>
{
}
