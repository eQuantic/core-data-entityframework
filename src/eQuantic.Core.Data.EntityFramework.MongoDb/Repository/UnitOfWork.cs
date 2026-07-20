using System;
using eQuantic.Core.Data.EntityFramework.Repository;
using eQuantic.Core.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.MongoDb.Repository;

/// <summary>
///     The MongoDB unit of work. All of the store-agnostic behaviour lives in
///     <see cref="EntityFrameworkUnitOfWork" />; this only supplies the MongoDB <see cref="Set{TEntity}" />.
/// </summary>
public abstract class UnitOfWork(IServiceProvider serviceProvider, DbContext context)
    : EntityFrameworkUnitOfWork(serviceProvider, context)
{
    protected override Data.Repository.ISet<TEntity> CreateSetCore<TEntity>() =>
        new Set<TEntity>(ServiceProvider, Context);
}

public abstract class UnitOfWork<TDbContext>(IServiceProvider serviceProvider, TDbContext context)
    : UnitOfWork(serviceProvider, context)
    where TDbContext : DbContext;
