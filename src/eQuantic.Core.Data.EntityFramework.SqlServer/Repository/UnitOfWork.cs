using System;
using System.Linq;
using eQuantic.Core.Data.EntityFramework.Relational.Repository;
using eQuantic.Core.Data.EntityFramework.Relational.Sql;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.SqlServer.Repository;

/// <summary>
///     SQL Server unit of work. The implementation lives in
///     <see cref="RelationalUnitOfWork{TDbContext}" />; this type only supplies the SQL Server dialect
///     (stored procedures are invoked with <c>EXEC</c> rather than the ANSI <c>CALL</c>).
/// </summary>
public abstract class UnitOfWork<TDbContext> : RelationalUnitOfWork<TDbContext>
    where TDbContext : DbContext
{
    protected UnitOfWork(IServiceProvider serviceProvider, TDbContext context) : base(serviceProvider, context)
    {
    }

    internal override string BuildProcedureSql(string name, SqlConfiguration config)
    {
        return $"EXEC {name}{GetNamedPlaceholders(config.Parameters.ToArray())}";
    }
}
