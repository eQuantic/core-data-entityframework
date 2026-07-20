using System;
using eQuantic.Core.Data.EntityFramework.Repository;
using eQuantic.Core.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.CosmosDb.Repository;

/// <summary>
///     The Azure Cosmos DB unit of work. All of the store-agnostic behaviour lives in
///     <see cref="EntityFrameworkUnitOfWork" />; this only supplies the Cosmos <see cref="Set{TEntity}" />.
/// </summary>
public abstract class UnitOfWork(IServiceProvider serviceProvider, DbContext context)
    : EntityFrameworkUnitOfWork(serviceProvider, context)
{
    protected override Data.Repository.ISet<TEntity> CreateSetCore<TEntity>() =>
        new Set<TEntity>(Context);
}

public abstract class UnitOfWork<TDbContext>(IServiceProvider serviceProvider, TDbContext context)
    : UnitOfWork(serviceProvider, context)
    where TDbContext : DbContext;
