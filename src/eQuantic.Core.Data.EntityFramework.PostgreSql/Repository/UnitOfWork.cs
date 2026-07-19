using System;
using eQuantic.Core.Data.EntityFramework.Relational.Repository;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.PostgreSql.Repository;

/// <summary>
///     PostgreSQL unit of work. The implementation lives in
///     <see cref="RelationalUnitOfWork{TDbContext}" />; PostgreSQL uses the default ANSI <c>CALL</c>
///     stored-procedure dialect, so no dialect override is required.
/// </summary>
public abstract class UnitOfWork<TDbContext> : RelationalUnitOfWork<TDbContext>
    where TDbContext : DbContext
{
    protected UnitOfWork(IServiceProvider serviceProvider, TDbContext context) : base(serviceProvider, context)
    {
    }
}
