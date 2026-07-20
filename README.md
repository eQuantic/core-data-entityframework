# eQuantic.Core.Data.EntityFramework

**The Entity Framework Core implementation of [eQuantic.Core.Data](https://github.com/eQuantic/core-data).**
You code against the provider-agnostic `IRepository<TEntity, TKey>` / `IUnitOfWork` contracts; this package
supplies the EF Core engine for **SQL Server, PostgreSQL, MySQL and MongoDB**.

```csharp
// A repository over any IEntity<TKey>, obtained from your DbContext-backed unit of work:
var repo = unitOfWork.GetAsyncRepository<OrderData, Guid>();

// Query typed and fluent — one QueryOptions, no magic strings:
var page = await repo.GetPagedAsync(
    PageRequest.Of(pageIndex: 1, pageSize: 20),
    new QueryOptions<OrderData>()
        .Where(o => o.Total, FilterOperator.GreaterThan, 100m)
        .And(o => o.Customer.Name, FilterOperator.Contains, term)
        .OrderByDescending(o => o.CreatedAt)
        .Include(nameof(OrderData.Customer))
        .NoTracking());

// page is a PagedResult<OrderData>: Items + TotalCount + PageIndex/PageSize/PageCount + Has*Page
```

## What this package gives you

`eQuantic.Core.Data` defines the **contracts** — `IRepository`, `IUnitOfWork`, `QueryOptions`,
`PageRequest`, `PagedResult`, specifications — with the persistence engine kept out of the type signatures
(`IRepository<TEntity, TKey>`, not `IRepository<TUnitOfWork, TEntity, TKey>`).

This package is the **Entity Framework Core implementation** of those contracts. It translates a single
`QueryOptions<TEntity>` into an EF `IQueryable` — applying, in order, custom *before* hooks, the
specification and predicate filter, eager `Include`s, sortings (server-side / EF-translatable),
`AsNoTracking`, `IgnoreQueryFilters`, query tags and custom *after* hooks — and returns `PagedResult<T>`
from paged reads. The relational providers also carry a parameterized raw-SQL executor (functions and
stored procedures, always via `DbParameter`).

## How you query

`QueryOptions<TEntity>` mirrors the eQuantic.Linq query builders, so filters read like code and fail at
compile time — not at runtime:

```csharp
new QueryOptions<OrderData>()
    .Where(o => o.Total, FilterOperator.GreaterThanOrEqual, 100m)   // typed member selector
    .And(o => o.Status, FilterOperator.Equal, OrderStatus.Paid)     // clauses fold left to right:
    .Or(o => o.Customer.IsVip, FilterOperator.Equal, true)          //   (total>=100 AND paid) OR vip
    .OrderByDescending(o => o.CreatedAt)
    .ThenBy("customer.name")                                        // string path for dynamic columns
    .Include(nameof(OrderData.Customer))
    .NoTracking();
```

You reach for whichever filter form fits — member selector, `string` path, `ISpecification<T>`,
`Expression<Func<T,bool>>`, a serialized `ExpressionModel<T>`, or an `eQuantic.Linq.Web` query string — all
end up as one predicate the provider translates. The full query-string grammar is documented in the
[eQuantic.Linq reference](https://github.com/eQuantic/core-linq/blob/main/docs/query-string-syntax.md).

## Providers

| Package | Database |
|---|---|
| `eQuantic.Core.Data.EntityFramework.SqlServer` | SQL Server |
| `eQuantic.Core.Data.EntityFramework.PostgreSql` | PostgreSQL |
| `eQuantic.Core.Data.EntityFramework.MySql` | MySQL (Pomelo) |
| `eQuantic.Core.Data.EntityFramework.MongoDb` | MongoDB (EF Core provider) |

The three relational providers share `eQuantic.Core.Data.EntityFramework.Relational`; every provider builds
on the base `eQuantic.Core.Data.EntityFramework`. Register your `DbContext`-backed unit of work and the
open-generic repositories through `AddRelationalRepositories<TUnitOfWorkInterface, TUnitOfWorkImpl>()` — the
full wiring is in the [walkthrough](Repository.md).

## Versioning — pick the package major that matches your runtime

This library targets **.NET 8** and **.NET 10**, and each runtime is published as its **own package major**
so the EF Core lines never mix:

| Your app | Install | Targets |
|---|---|---|
| .NET 8 | **8.x** (e.g. `8.2.0`) | `net8.0`, EF Core 8 |
| .NET 10 | **10.x** (e.g. `10.1.0`) | `net10.0`, EF Core 10 |

> The shared multi-framework assemblies (the referenceable base `eQuantic.Core.Data.EntityFramework` and
> `eQuantic.Core.Data.EntityFramework.Relational`) stay in the **4.x** line on purpose — a neutral lane that
> must not be read as a .NET version. You normally consume only the provider package for your runtime
> (8.x / 10.x), which pulls the right shared assemblies transitively.

## Install

```bash
# .NET 8 app + SQL Server
dotnet add package eQuantic.Core.Data.EntityFramework.SqlServer --version 8.*

# .NET 10 app + PostgreSQL
dotnet add package eQuantic.Core.Data.EntityFramework.PostgreSql --version 10.*
```

Swap the suffix for `PostgreSql`, `MySql` or `MongoDb` as needed.

## Learn more

- [Repository Pattern walkthrough](Repository.md) — data entities, unit of work, repository and
  specifications, end to end.
- [eQuantic.Core.Data](https://github.com/eQuantic/core-data) — the contracts and the `QueryOptions` /
  `PagedResult` / `PageRequest` query surface, backed by the
  [eQuantic.Linq](https://github.com/eQuantic/core-linq) query engine.

MIT © eQuantic Tech
