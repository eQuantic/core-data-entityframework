using eQuantic.Core.Data.EntityFramework.CosmosDb.Repository;
using eQuantic.Core.Data.EntityFramework.Repository.Read;
using eQuantic.Core.Data.Repository.Options;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.CosmosDb.Tests;

/// <summary>
///     Cosmos DB orders by document paths only and sorts nulls as the smallest value, so a sort whose nulls
///     placement is the one it gives by itself is sent as a plain <c>ORDER BY</c>, and the other placement is
///     refused before any request, rather than sent as an expression the service rejects (or, worse, ignored).
///     The context points at an endpoint nothing listens on: neither case may reach it.
/// </summary>
public class CosmosNullsOrderTests
{
    private static TestContext NewContext() =>
        new(new DbContextOptionsBuilder<TestContext>()
            .UseCosmos("https://localhost:1/", Convert.ToBase64String(new byte[64]), "nulls-order")
            .Options);

    [TestCase(false, NullsOrder.First)]
    [TestCase(true, NullsOrder.Last)]
    public void The_placement_cosmos_gives_by_itself_is_a_plain_order_by(bool descending, NullsOrder nulls)
    {
        using var context = NewContext();
        var options = descending
            ? new QueryOptions<TestDoc>().OrderByDescending(x => x.Status, nulls)
            : new QueryOptions<TestDoc>().OrderBy(x => x.Status, nulls);

        var sql = new Set<TestDoc>(context).GetQueryable(options).ToQueryString();

        Assert.That(sql, Does.Contain($"ORDER BY c[\"Status\"]{(descending ? " DESC" : "")}"));
        Assert.That(sql, Does.Not.Contain("?"), "no conditional key: Cosmos DB orders by paths only");
    }

    [TestCase(false, NullsOrder.Last)]
    [TestCase(true, NullsOrder.First)]
    public void The_other_placement_is_refused_before_any_request(bool descending, NullsOrder nulls)
    {
        using var context = NewContext();
        var repository = new AsyncQueryableReadRepository<TestDoc, int>(
            new DefaultUnitOfWork(new NoServices(), context));
        var options = descending
            ? new QueryOptions<TestDoc>().OrderByDescending(x => x.Status, nulls)
            : new QueryOptions<TestDoc>().OrderBy(x => x.Status, nulls);

        var refusal = Assert.ThrowsAsync<NotSupportedException>(() => repository.GetAllAsync(options));

        Assert.That(refusal!.Message, Does.Contain("Cosmos DB orders by document paths only"));
    }

    [Test]
    public void A_key_that_cannot_hold_null_needs_no_placement()
    {
        using var context = NewContext();

        var sql = new Set<TestDoc>(context)
            .GetQueryable(new QueryOptions<TestDoc>().OrderBy(x => x.Count, NullsOrder.Last))
            .ToQueryString();

        Assert.That(sql, Does.Contain("ORDER BY c[\"Count\"]"));
    }

    private sealed class NoServices : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
