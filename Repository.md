# Repository walkthrough (Entity Framework Core)

An end-to-end slice built on **eQuantic.Core.Data v5** and this EF Core provider: data entities → a
unit of work → repositories → specifications → a domain service that consumes them. You code against the
provider-agnostic contracts (`IEntity<TKey>`, `IQueryableUnitOfWork`, `QueryOptions<TEntity>`,
`PageRequest`/`PagedResult<T>`); the provider supplies the EF Core engine. The examples use SQL Server —
PostgreSQL and MySQL are identical (swap the provider namespace and `UseSqlServer`), and MongoDB differs
only in registration (see §3).

## 1. Data entities — `IEntity<TKey>`

An entity is a plain class that implements `IEntity<TKey>`. The key is exposed through `GetKey()`/
`SetKey()` (there is no mandated `Id` property, though you will usually have one).

```csharp
using System;
using eQuantic.Core.Data.Repository;

public enum OrderStatus { Pending, Paid, Cancelled }

public class CustomerData : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsVip { get; set; }

    public Guid GetKey() => Id;
    public void SetKey(Guid key) => Id = key;
}

public class OrderData : IEntity<Guid>
{
    public Guid Id { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid CustomerId { get; set; }
    public CustomerData Customer { get; set; } = default!;

    public Guid GetKey() => Id;
    public void SetKey(Guid key) => Id = key;
}
```

## 2. The DbContext

A regular EF Core `DbContext` — the provider works with whatever context you already have.

```csharp
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<OrderData> Orders => Set<OrderData>();
    public DbSet<CustomerData> Customers => Set<CustomerData>();
}
```

## 3. The unit of work

Derive the provider's `UnitOfWork<TDbContext>`. Declaring your own interface (deriving
`IQueryableUnitOfWork`) keeps call sites decoupled from the concrete type.

```csharp
using System;
using eQuantic.Core.Data.EntityFramework.SqlServer.Repository; // provider base
using eQuantic.Core.Data.Repository;

public interface IAppUnitOfWork : IQueryableUnitOfWork
{
}

public class AppUnitOfWork : UnitOfWork<AppDbContext>, IAppUnitOfWork
{
    public AppUnitOfWork(IServiceProvider serviceProvider, AppDbContext context)
        : base(serviceProvider, context)
    {
    }
}
```

Register the context and the repositories. `AddRelationalRepositories` wires the unit of work, the generic
repositories, and — because the relational unit of work implements it — the raw-SQL
`ISqlUnitOfWork`.

```csharp
using eQuantic.Core.Data.EntityFramework.Relational.Repository.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));
services.AddRelationalRepositories<IAppUnitOfWork, AppUnitOfWork>();
```

No custom unit-of-work type? Use the provider's `DefaultUnitOfWork` (over a bare `DbContext`) instead.
MongoDB has no SQL executor, so its unit of work derives
`eQuantic.Core.Data.EntityFramework.MongoDb.Repository.UnitOfWork<TDbContext>` and is registered with the
SQL-agnostic `AddQueryableRepositories<IAppUnitOfWork, AppUnitOfWork>()`.

## 4. Getting a repository

Ask the unit of work for a repository over any `IEntity<TKey>`. The queryable accessors resolve the
generic repositories registered above:

```csharp
// asynchronous read + write:
IAsyncRepository<OrderData, Guid> orders =
    unitOfWork.GetAsyncRepository<OrderData, Guid>();

// synchronous sibling:
IRepository<OrderData, Guid> ordersSync =
    unitOfWork.GetRepository<OrderData, Guid>();
```

All four accessors resolve from the generic registration above — the plain
`GetAsyncRepository`/`GetRepository` shown here and the richer
`GetAsyncQueryableRepository`/`GetQueryableRepository` variants (which add the `IQueryable`/`ISet` surface).
Custom repositories (§8) layer your own named interface on top of the same wiring.

## 5. Reading — one `QueryOptions`

All query shaping — filtering, includes, sorting, tracking — is expressed through a single
`QueryOptions<TEntity>`. Filters read like code and fail at compile time; clauses fold left to right.

```csharp
using eQuantic.Core.Data.Repository.Options;
using eQuantic.Linq.Web; // FilterOperator

var options = new QueryOptions<OrderData>()
    .Where(o => o.Total, FilterOperator.GreaterThanOrEqual, 100m)   // typed member selector
    .And(o => o.Status, FilterOperator.Equal, OrderStatus.Paid)     // (total>=100 AND paid)
    .Or(o => o.Customer.IsVip, FilterOperator.Equal, true)          //   OR the customer is VIP
    .OrderByDescending(o => o.CreatedAt)
    .ThenBy("customer.name")                                        // string path for dynamic columns
    .Include(nameof(OrderData.Customer))                            // eager load
    .NoTracking();
```

Paged reads return a `PagedResult<T>` — the items plus the totals needed to render a pager:

```csharp
using eQuantic.Core.Data.Repository;

PagedResult<OrderData> page = await orders.GetPagedAsync(PageRequest.Of(pageIndex: 1, pageSize: 20), options);

foreach (var order in page.Items) { /* ... */ }
_ = page.TotalCount;                       // total across all pages
_ = page.PageIndex; _ = page.PageSize;     // echoed back
_ = page.PageCount;                        // computed
_ = page.HasPreviousPage; _ = page.HasNextPage;
```

The rest of the read surface follows the same shape — pass a `QueryOptions` (or none):

```csharp
// by key (a non-null options routes through the key predicate + includes; otherwise DbSet.Find):
OrderData? one = await orders.GetAsync(id, new QueryOptions<OrderData>().Include(nameof(OrderData.Customer)));

// by predicate:
IEnumerable<OrderData> big = await orders.GetFilteredAsync(o => o.Total >= 100m, new QueryOptions<OrderData>().NoTracking());

// projection (server-side Select):
IEnumerable<OrderSummary> summaries = await orders.GetMappedAsync(
    o => new OrderSummary(o.Id, o.Total, o.Customer.Name),
    new QueryOptions<OrderData>().Include(nameof(OrderData.Customer)));

// paged projection:
PagedResult<OrderSummary> summaryPage = await orders.GetPagedAsync(
    PageRequest.Of(1, 20),
    o => new OrderSummary(o.Id, o.Total, o.Customer.Name),
    new QueryOptions<OrderData>().Include(nameof(OrderData.Customer)));

// aggregates (Count returns long; Sum has an overload per numeric type):
long paidCount = await orders.CountAsync(new QueryOptions<OrderData>().Where(o => o.Status, FilterOperator.Equal, OrderStatus.Paid));
decimal paidTotal = await orders.SumAsync(o => o.Total, new QueryOptions<OrderData>().Where(o => o.Status, FilterOperator.Equal, OrderStatus.Paid));

public record OrderSummary(Guid Id, decimal Total, string Customer);
```

Every method has a synchronous twin on the queryable repository (`GetPaged`, `Get`, `Count`, `Sum`, …).

## 6. Specifications

Encapsulate a reusable rule as an `ISpecification<T>` (base class `Specification<T>`).

```csharp
using System;
using System.Linq.Expressions;
using eQuantic.Linq.Specification;

public sealed class PaidOrdersSpecification : Specification<OrderData>
{
    public override Expression<Func<OrderData, bool>> SatisfiedBy() => o => o.Status == OrderStatus.Paid;
}
```

Apply it either as the filter inside `QueryOptions`, or directly through `AllMatchingAsync`:

```csharp
var spec = new PaidOrdersSpecification();

// inside QueryOptions (composes with sorting, includes, tracking, ...):
var recentPaid = await orders.GetPagedAsync(
    PageRequest.Of(1, 20),
    new QueryOptions<OrderData>().Where(spec).OrderByDescending(o => o.CreatedAt));

// or directly:
IEnumerable<OrderData> allPaid = await orders.AllMatchingAsync(spec, new QueryOptions<OrderData>().NoTracking());
```

## 7. Writing

Writes are staged on the repository and persisted when the unit of work commits.

```csharp
var order = new OrderData { Id = Guid.NewGuid(), Total = 150m, Status = OrderStatus.Pending, CreatedAt = DateTime.UtcNow };

await orders.AddAsync(order);                       // stage an insert
await orders.AddRangeAsync(new[] { order1, order2 }); // stage several

order.Status = OrderStatus.Paid;
await orders.ModifyAsync(order);                    // mark modified

await orders.RemoveAsync(order);                    // stage a delete

int affected = await unitOfWork.CommitAsync();      // one round-trip, returns affected rows
```

On the relational providers, set-based writes run as a single server-side statement (EF
`ExecuteUpdate`/`ExecuteDelete`) and do not need a commit:

```csharp
long cancelled = await orders.UpdateManyAsync(
    o => o.Status == OrderStatus.Pending,
    o => new OrderData { Status = OrderStatus.Cancelled });

long removed = await orders.DeleteManyAsync(o => o.Total == 0m);
```

`DeleteManyAsync`/`UpdateManyAsync` also accept an `ISpecification<T>` in place of the predicate.

> The document providers implement these two methods differently — **MongoDB** through its native driver,
> **Azure Cosmos DB** by loading the matching documents and modifying them through the context (Cosmos has no
> server-side `ExecuteUpdate`/`ExecuteDelete`). The contract is identical; only the execution differs.

## 8. Custom repositories

Need repository-specific methods, or the plain `IRepository`/`IAsyncRepository` shape resolved by
`GetRepository`/`GetAsyncRepository`? Declare an interface and derive the generic base:

```csharp
using eQuantic.Core.Data.EntityFramework.Repository;
using eQuantic.Core.Data.Repository;

public interface IOrderRepository : IRepository<OrderData, Guid>, IAsyncRepository<OrderData, Guid>
{
}

public class OrderRepository : AsyncQueryableRepository<OrderData, Guid>, IOrderRepository
{
    public OrderRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}
```

Register by scanning the assembly, then resolve through the unit of work (or inject `IOrderRepository`
directly):

```csharp
using eQuantic.Core.Data.EntityFramework.Repository.Extensions;

services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));
services.AddCustomRepositories<AppUnitOfWork>(o => o
    .AddLifetime(ServiceLifetime.Scoped)
    .FromAssembly(typeof(OrderRepository).Assembly));

// later:
IAsyncRepository<OrderData, Guid> repo = unitOfWork.GetAsyncRepository<OrderData, Guid>();
IRepository<OrderData, Guid> repoSync = unitOfWork.GetRepository<OrderData, Guid>();
```

## 9. A domain service

Putting it together — a service consumes the unit of work and its repositories, keeping EF Core out of the
domain layer.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Options;
using eQuantic.Linq.Web;

public class OrderService
{
    private readonly IAppUnitOfWork _unitOfWork;

    public OrderService(IAppUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public Task<OrderData?> GetAsync(Guid id, CancellationToken ct = default) =>
        _unitOfWork.GetAsyncRepository<OrderData, Guid>()
            .GetAsync(id, new QueryOptions<OrderData>().Include(nameof(OrderData.Customer)), ct);

    public async Task<Guid> CreateAsync(OrderData order, CancellationToken ct = default)
    {
        var repo = _unitOfWork.GetAsyncRepository<OrderData, Guid>();
        await repo.AddAsync(order, ct);
        await _unitOfWork.CommitAsync(ct);
        return order.Id;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var repo = _unitOfWork.GetAsyncRepository<OrderData, Guid>();
        var order = await repo.GetAsync(id, cancellationToken: ct);
        if (order is null) return false;

        await repo.RemoveAsync(order);
        return await _unitOfWork.CommitAsync(ct) > 0;
    }

    public Task<PagedResult<OrderData>> FindAsync(decimal minTotal, int pageIndex, int pageSize, CancellationToken ct = default) =>
        _unitOfWork.GetAsyncRepository<OrderData, Guid>()
            .GetPagedAsync(
                PageRequest.Of(pageIndex, pageSize),
                new QueryOptions<OrderData>()
                    .Where(o => o.Total, FilterOperator.GreaterThanOrEqual, minTotal)
                    .OrderByDescending(o => o.CreatedAt)
                    .Include(nameof(OrderData.Customer))
                    .NoTracking(),
                ct);

    public Task<decimal> RevenueAsync(CancellationToken ct = default) =>
        _unitOfWork.GetAsyncRepository<OrderData, Guid>()
            .SumAsync(o => o.Total, new QueryOptions<OrderData>().Where(o => o.Status, FilterOperator.Equal, OrderStatus.Paid), ct);
}
```
