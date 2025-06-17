using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.EntityFramework.PostgreSql.Repository.Extensions;
using eQuantic.Core.Data.Repository.Config;
using eQuantic.Core.Data.Repository.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace eQuantic.Core.Data.EntityFramework.PostgreSql.Repository;

/// <summary>
///     The sql executor class
/// </summary>
/// <seealso cref="ISqlExecutor" />
/// <seealso cref="IAsyncSqlExecutor" />
/// <seealso cref="IDisposable" />
[ExcludeFromCodeCoverage]
public abstract class SqlExecutor : ISqlExecutor, IAsyncSqlExecutor, IDisposable
{
    /// <summary>
    ///     The context
    /// </summary>
    protected readonly DbContext Context;

    /// <summary>
    ///     The disposed
    /// </summary>
    protected bool Disposed;

    /// <summary>
    ///     The transaction
    /// </summary>
    protected IDbContextTransaction Transaction;
    
    /// <summary>
    ///     Initializes a new instance of the class
    /// </summary>
    /// <param name="context">The context</param>
    protected SqlExecutor(DbContext context)
    {
        Context = context;
    }

    /// <summary>
    ///     Begins the transaction using the specified cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        Transaction?.Dispose();
        Transaction = await Context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    ///     Commits the transaction using the specified cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Transaction != null)
        {
            await Transaction.CommitAsync(cancellationToken);
        }
    }
    
    /// <summary>
    ///     Rollbacks the transaction using the specified cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Transaction != null)
        {
            await Transaction.RollbackAsync(cancellationToken);
        }
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<T>> ExecuteRawSqlAsync<T>(string sql, Func<DbDataReader, T> map,
        Action<DefaultSqlConfiguration> config = null, CancellationToken cancellationToken = default)
    {
        var configuration = GetConfig(config);
        
        await using var command = Context.Database.GetDbConnection().CreateCommand();
        SetCommand(sql, command, configuration);

        await Context.Database.OpenConnectionAsync(cancellationToken);
        await using var result = await command.ExecuteReaderAsync(cancellationToken);

        var items = new List<T>();
        while (await result.ReadAsync(cancellationToken))
        {
            items.Add(map(result));
        }

        return items;
    }

    /// <summary>
    /// Executes the non query using the specified sql command
    /// </summary>
    /// <param name="sqlCommand">The sql command</param>
    /// <param name="config">The configuration.</param>
    /// <returns>The int</returns>
    public int ExecuteNonQuery(string sqlCommand,  Action<DefaultSqlConfiguration> config = null)
    {
        var configuration = GetConfig(config);
        using var command = Context.Database.GetDbConnection().CreateCommand();
        SetCommand(sqlCommand, command, configuration);

        Context.Database.OpenConnection();
        return command.ExecuteNonQuery();
    }

    /// <summary>
    /// Executes the function using the specified name
    /// </summary>
    /// <typeparam name="TResult">The result</typeparam>
    /// <param name="name">The name</param>
    /// <param name="config">The configuration.</param>
    /// <returns>The result</returns>
    public TResult ExecuteFunction<TResult>(string name, Action<DefaultSqlConfiguration> config = null) where TResult : class
    {
        var configuration = GetConfig(config);
        var sql = ParseSql(GetQueryFunction(name, configuration), configuration);
        return Context.Set<TResult>()
            .FromSqlRaw(sql)
            .FirstOrDefault();
    }

    /// <summary>
    /// Executes the command using the specified command timeout
    /// </summary>
    /// <param name="sqlCommand">The sql command</param>
    /// <param name="config">The configuration.</param>
    /// <returns>The int</returns>
    public int ExecuteCommand(string sqlCommand, Action<DefaultSqlConfiguration> config = null)
    {
        var configuration = GetConfig(config);
        
        using var command = Context.Database.GetDbConnection().CreateCommand();
        SetCommand(sqlCommand, command, configuration);

        Context.Database.OpenConnection();
        var result = command.ExecuteScalar();

        return result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }
    
    /// <summary>
    ///     Executes the command using the specified command timeout
    /// </summary>
    /// <param name="sqlCommand">The sql command</param>
    /// <param name="config"></param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the int</returns>
    public async Task<int> ExecuteCommandAsync(string sqlCommand,
        Action<DefaultSqlConfiguration> config = null, CancellationToken cancellationToken = default)
    {
        var configuration = GetConfig(config);
        
        await using var command = Context.Database.GetDbConnection().CreateCommand();
        SetCommand(sqlCommand, command, configuration);

        await Context.Database.OpenConnectionAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }

    /// <summary>
    ///     Executes the function using the specified name
    /// </summary>
    /// <typeparam name="TResult">The result</typeparam>
    /// <param name="name">The name</param>
    /// <param name="config"></param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the result</returns>
    public Task<TResult> ExecuteFunctionAsync<TResult>(string name, 
        Action<DefaultSqlConfiguration> config = null,
        CancellationToken cancellationToken = default) where TResult : class
    {
        var configuration = GetConfig(config);
        var sql = ParseSql(GetQueryFunction(name, configuration), configuration);
        return Context.Set<TResult>().FromSqlRaw(sql)
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    /// <summary>
    /// Executes the procedure using the specified name
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="config">The configuration</param>
    /// <returns>The int</returns>
    public int ExecuteProcedure(string name, Action<DefaultSqlConfiguration> config = null)
    {
        var configuration = GetConfig(config);
        return ExecuteCommand(GetQueryProcedure(name, configuration) + ";");
    }
    
    /// <summary>
    /// Executes the query using the specified sql query
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    /// <param name="sqlQuery">The sql query</param>
    /// <param name="config">The configuration.</param>
    /// <returns>An enumerable of t entity</returns>
    public IEnumerable<TEntity> ExecuteQuery<TEntity>(string sqlQuery, Action<DefaultSqlConfiguration> config = null) where TEntity : class
    {
        var configuration = GetConfig(config);
        var sql = ParseSql(sqlQuery, configuration);
        return Context.Set<TEntity>().FromSqlRaw(sql, configuration.Parameters.Select(p => p.Value));
    }
    
    /// <summary>
    ///     Executes the transaction using the specified operation
    /// </summary>
    /// <param name="operation">The operation</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void ExecuteTransaction(Action<ISqlUnitOfWork> operation)
    {
        if (operation == null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        var strategy = Context.Database.CreateExecutionStrategy();

        strategy.Execute(() => { operation.Invoke((ISqlUnitOfWork)this); });
    }

    /// <summary>
    ///     Executes the procedure using the specified name
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="config"></param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the int</returns>
    public Task<int> ExecuteProcedureAsync(string name, Action<DefaultSqlConfiguration> config = null,
        CancellationToken cancellationToken = default)
    {
        var configuration = GetConfig(config);
        return ExecuteCommandAsync(GetQueryProcedure(name, configuration) + ";", config, cancellationToken);
    }

    /// <inheritdoc />
    public Task ExecuteTransactionAsync(Func<ISqlUnitOfWork, Task> operation,
        CancellationToken cancellationToken = default)
    {
        if (operation == null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        var strategy = Context.Database.CreateExecutionStrategy();

        return strategy.ExecuteAsync(() => operation.Invoke((ISqlUnitOfWork)this));
    }

    /// <summary>
    ///     Begins the transaction
    /// </summary>
    public void BeginTransaction()
    {
        Transaction?.Dispose();
        Transaction = Context.Database.BeginTransaction();
    }

    /// <summary>
    ///     Commits the transaction
    /// </summary>
    public virtual void CommitTransaction()
    {
        Transaction?.Commit();
    }

    /// <summary>
    /// Executes the raw sql using the specified sql
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="sql">The sql</param>
    /// <param name="map">The map</param>
    /// <param name="config">The configuration.</param>
    /// <returns>The items</returns>
    public IEnumerable<T> ExecuteRawSql<T>(string sql, Func<DbDataReader, T> map,
        Action<DefaultSqlConfiguration> config = null)
    {
        var configuration = GetConfig(config);
        
        using var command = Context.Database.GetDbConnection().CreateCommand();
        SetCommand(sql, command, configuration);

        Context.Database.OpenConnection();
        using var result = command.ExecuteReader();

        var items = new List<T>();
        while (result.Read())
        {
            items.Add(map(result));
        }

        return items;
    }
    
    /// <summary>
    ///     Gets the transaction
    /// </summary>
    /// <returns>The db transaction</returns>
    public DbTransaction GetTransaction()
    {
        return Transaction?.GetDbTransaction();
    }

    /// <summary>
    ///     Rollbacks the transaction
    /// </summary>
    public void RollbackTransaction()
    {
        Transaction?.Rollback();
    }

    /// <summary>
    ///     Uses the transaction using the specified transaction
    /// </summary>
    /// <param name="transaction">The transaction</param>
    public void UseTransaction(DbTransaction transaction)
    {
        Context.Database.UseTransaction(transaction);
    }
    
    /// <summary>
    ///     Uses the transaction using the specified transaction
    /// </summary>
    /// <param name="transaction">The transaction</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public Task UseTransactionAsync(DbTransaction transaction, CancellationToken cancellationToken = default)
    {
        return Context.Database.UseTransactionAsync(transaction, cancellationToken);
    }
    
    /// <summary>
    ///     Gets the query function using the specified name
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="config">The configuration.</param>
    /// <returns>The string</returns>
    private static string GetQueryFunction(string name, SqlConfiguration config)
    {
        return $"SELECT {name}({GetQueryParameters(config.Parameters.ToArray())} )";
    }
    
    /// <summary>
    ///     Gets the query procedure using the specified name
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="config">The configuration.</param>
    /// <returns>The string</returns>
    private static string GetQueryProcedure(string name, SqlConfiguration config)
    {
        return $"EXEC {name}{GetQueryParameters(config.Parameters.ToArray())}";
    }
    
    /// <summary>
    ///     Gets the query parameters using the specified parameters
    /// </summary>
    /// <param name="parameters">The parameters</param>
    /// <returns>The string</returns>
    private static string GetQueryParameters(params ParamValue[] parameters)
    {
        var cmdBuilder = new StringBuilder();
        if (parameters is not { Length: > 0 })
        {
            return cmdBuilder.ToString();
        }

        for (var i = 0; i < parameters.Length; i++)
        {
            if (i > 0)
            {
                cmdBuilder.Append(',');
            }

            if (parameters[i].Value == null)
            {
                cmdBuilder.Append(" NULL");
            }
            else
            {
                var fmt = " {0}";
                if (parameters[i].Value is Guid or string or DateTime)
                {
                    fmt = " '{0}'";
                }

                cmdBuilder.Append(string.Format(fmt, parameters[i].Value));
            }
        }

        return cmdBuilder.ToString();
    }

    private static string ParseSql(string sql, SqlConfiguration config)
    {
        return !string.IsNullOrEmpty(config.Tag) ? GetQueryWithTag(sql, config.Tag) : sql;
    }
    /// <summary>
    ///     Sets the command using the specified command timeout
    /// </summary>
    /// <param name="sqlCommand">The sql command</param>
    /// <param name="command">The command</param>
    /// <param name="config">The configuration.</param>
    private void SetCommand(string sqlCommand, DbCommand command, SqlConfiguration config)
    {
        if (Transaction != null)
        {
            command.Transaction = Transaction.GetDbTransaction();
        }

        command.CommandText = ParseSql(sqlCommand, config);
        command.CommandType = CommandType.Text;
        command.CommandTimeout = config.GetCommandTimeout(Context);

        if (config.Parameters == null)
        {
            return;
        }

        var i = 0;
        foreach (var t in config.Parameters)
        {
            var parameterName = string.IsNullOrEmpty(t.Name) ? $"Param{i}" : t.Name;
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = t.Value;
            command.Parameters.Add(parameter);
            i++;
        }
    }

    /// <summary>
    /// Gets the query with tag using the specified query
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="tag">The tag</param>
    /// <returns>The string</returns>
    private static string GetQueryWithTag(string query, string tag)
    {
        var queryBuilder = new StringBuilder();
        queryBuilder.AppendLine($"--{tag}");
        queryBuilder.AppendLine();
        queryBuilder.Append(query);
        return queryBuilder.ToString();
    }
    
    private static SqlConfiguration GetConfig(Action<DefaultSqlConfiguration> config = null)
    {
        var configuration = new DefaultSqlConfiguration();
        config?.Invoke(configuration);

        return configuration;
    }
    
    /// <summary>
    ///     Disposes this instance
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    ///     Disposes the disposing
    /// </summary>
    /// <param name="disposing">The disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (Disposed)
        {
            return;
        }

        if (disposing)
        {
            Transaction?.Dispose();
            Context?.Dispose();
        }

        Disposed = true;
    }
}
