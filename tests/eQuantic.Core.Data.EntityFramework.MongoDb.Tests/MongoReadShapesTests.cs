using eQuantic.Core.Data.EntityFramework.MongoDb.Repository;
using eQuantic.Core.Data.EntityFramework.Repository.Read;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Options;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using Testcontainers.MongoDb;

namespace eQuantic.Core.Data.EntityFramework.MongoDb.Tests;

/// <summary>
///     The read shapes of eQuantic.Core.Data 7.0.0 against a real MongoDB, through the MongoDB Entity Framework
///     provider. MongoDB sorts nulls as the smallest value (first ascending, last descending): those placements are a
///     plain sort, and the two others need the leading "is null" key, which only a real server proves translates.
/// </summary>
[TestFixture]
public sealed class MongoReadShapesTests
{
    private MongoDbContainer _mongo = null!;
    private bool _available;

    public sealed class Run : IEntity<int>
    {
        public int Id { get; set; }

        public int? Position { get; set; }

        public int GetKey() => Id;

        public void SetKey(int key) => Id = key;
    }

    private sealed class RunsContext(DbContextOptions<RunsContext> options) : DbContext(options)
    {
        public DbSet<Run> Runs => Set<Run>();

        protected override void OnModelCreating(ModelBuilder builder) =>
            builder.Entity<Run>().ToCollection("runs");
    }

    private sealed class NoServices : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    [OneTimeSetUp]
    public async Task StartAsync()
    {
        // A replica set: the MongoDB provider writes SaveChanges in a transaction, which a standalone server refuses.
        _mongo = new MongoDbBuilder("mongo:7").WithReplicaSet().Build();
        try
        {
            await _mongo.StartAsync();
            _available = true;
        }
        catch (Exception)
        {
            _available = false;
        }
    }

    [OneTimeTearDown]
    public async Task StopAsync()
    {
        if (_available)
        {
            await _mongo.DisposeAsync();
        }
    }

    private async Task<(RunsContext Context, AsyncQueryableReadRepository<Run, int> Repository)> NewAsync(params Run[] seed)
    {
        if (!_available)
        {
            Assert.Ignore("MongoDB test container is unavailable (Docker required).");
        }

        var context = new RunsContext(new DbContextOptionsBuilder<RunsContext>()
            .UseMongoDB(_mongo.GetConnectionString(), $"shapes_{Guid.NewGuid():N}").Options);
        context.Runs.AddRange(seed);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        return (context, new AsyncQueryableReadRepository<Run, int>(new DefaultUnitOfWork(new NoServices(), context)));
    }

    [TestCase(false, NullsOrder.First, new[] { 0, 1, 2 })]
    [TestCase(false, NullsOrder.Last, new[] { 1, 2, 0 })]
    [TestCase(true, NullsOrder.First, new[] { 0, 2, 1 })]
    [TestCase(true, NullsOrder.Last, new[] { 2, 1, 0 })]
    public async Task Nulls_go_where_the_sort_places_them(bool descending, NullsOrder nulls, int[] positions)
    {
        var (context, repository) = await NewAsync(
            new Run { Id = 1, Position = 2 },
            new Run { Id = 2, Position = null },
            new Run { Id = 3, Position = 1 });
        await using var _ = context;

        var options = descending
            ? new QueryOptions<Run>().OrderByDescending(x => x.Position, nulls)
            : new QueryOptions<Run>().OrderBy(x => x.Position, nulls);
        var runs = await repository.GetAllAsync(options);

        Assert.That(runs.Select(x => x.Position ?? 0), Is.EqualTo(positions));
    }

    [Test]
    public async Task Skip_and_take_return_rows_eleven_to_twenty()
    {
        var (context, repository) = await NewAsync(Enumerable.Range(1, 25).Select(id => new Run { Id = id }).ToArray());
        await using var _ = context;

        var page = await repository.GetAllAsync(new QueryOptions<Run>().OrderBy(x => x.Id).Skip(10).Take(10));

        Assert.That(page.Select(x => x.Id), Is.EqualTo(Enumerable.Range(11, 10)));
    }
}
