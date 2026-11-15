using eQuantic.Core.Data.EntityFramework.PostgreSql.Repository;
using eQuantic.Core.Data.EntityFramework.Repository.Read;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Options;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace eQuantic.Core.Data.EntityFramework.PostgreSql.Tests;

/// <summary>
///     The read shapes of eQuantic.Core.Data 7.0.0 against a real PostgreSQL: what proves that the nulls key, the
///     filtered include and the row window translate to SQL, rather than pass on a provider that evaluates in
///     memory. PostgreSQL puts nulls LAST when ascending, unlike SQL Server, MySQL and the document stores, so
///     <see cref="NullsOrder.First" /> ascending is a placement it would not give by itself.
/// </summary>
[TestFixture]
public sealed class ReadShapesTests
{
    private PostgreSqlContainer _postgres = null!;
    private bool _available;

    public sealed class Run : IEntity<int>
    {
        public int Id { get; set; }

        public int? Position { get; set; }

        public List<Step> Steps { get; set; } = [];

        public int? TeamId { get; set; }

        public Team? Team { get; set; }

        public int GetKey() => Id;

        public void SetKey(int key) => Id = key;
    }

    public sealed class Team
    {
        public int Id { get; set; }

        public int Rank { get; set; }
    }

    public sealed class Step
    {
        public int Id { get; set; }

        public int RunId { get; set; }

        public string Status { get; set; } = "";

        public DateTime? StartedAt { get; set; }
    }

    private sealed class RunsContext(DbContextOptions<RunsContext> options) : DbContext(options)
    {
        public DbSet<Run> Runs => Set<Run>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Run>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<Step>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<Run>().HasMany(x => x.Steps).WithOne().HasForeignKey(x => x.RunId);
            builder.Entity<Run>().HasOne(x => x.Team).WithMany().HasForeignKey(x => x.TeamId);
            builder.Entity<Team>().Property(x => x.Id).ValueGeneratedNever();
        }
    }

    private sealed class NoServices : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    [OneTimeSetUp]
    public async Task StartAsync()
    {
        _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();
        try
        {
            await _postgres.StartAsync();
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
            await _postgres.DisposeAsync();
        }
    }

    private async Task<(RunsContext Context, AsyncQueryableReadRepository<Run, int> Repository)> NewAsync()
    {
        if (!_available)
        {
            Assert.Ignore("PostgreSQL test container is unavailable (Docker required).");
        }

        var builder = new Npgsql.NpgsqlConnectionStringBuilder(_postgres.GetConnectionString())
        {
            Database = $"shapes_{Guid.NewGuid():N}",
        };
        var context = new RunsContext(new DbContextOptionsBuilder<RunsContext>().UseNpgsql(builder.ConnectionString).Options);
        await context.Database.EnsureCreatedAsync();
        return (context, new AsyncQueryableReadRepository<Run, int>(new DefaultUnitOfWork(new NoServices(), context)));
    }

    [TestCase(false, NullsOrder.First, new[] { 0, 1, 2 })]
    [TestCase(false, NullsOrder.Last, new[] { 1, 2, 0 })]
    [TestCase(true, NullsOrder.First, new[] { 0, 2, 1 })]
    [TestCase(true, NullsOrder.Last, new[] { 2, 1, 0 })]
    public async Task Nulls_go_where_the_sort_places_them(bool descending, NullsOrder nulls, int[] positions)
    {
        var (context, repository) = await NewAsync();
        await using var _ = context;
        context.Runs.AddRange(
            new Run { Id = 1, Position = 2 },
            new Run { Id = 2, Position = null },
            new Run { Id = 3, Position = 1 });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var options = descending
            ? new QueryOptions<Run>().OrderByDescending(x => x.Position, nulls)
            : new QueryOptions<Run>().OrderBy(x => x.Position, nulls);
        var runs = await repository.GetAllAsync(options);

        Assert.That(runs.Select(x => x.Position ?? 0), Is.EqualTo(positions));
    }

    [Test]
    public async Task Skip_and_take_return_rows_eleven_to_twenty()
    {
        var (context, repository) = await NewAsync();
        await using var _ = context;
        context.Runs.AddRange(Enumerable.Range(1, 25).Select(id => new Run { Id = id }));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var page = await repository.GetAllAsync(new QueryOptions<Run>().OrderBy(x => x.Id).Skip(10).Take(10));

        Assert.That(page.Select(x => x.Id), Is.EqualTo(Enumerable.Range(11, 10)));
    }

    [Test]
    public async Task A_shaped_include_loads_the_last_three_newest_first_for_each_run()
    {
        var (context, repository) = await NewAsync();
        await using var _ = context;
        var start = new DateTime(2026, 11, 14, 12, 0, 0, DateTimeKind.Utc);
        context.Runs.AddRange(
            new Run
            {
                Id = 1,
                Steps =
                [
                    new Step { Id = 1, Status = "success", StartedAt = start },
                    new Step { Id = 2, Status = "skipped", StartedAt = start.AddMinutes(5) },
                    new Step { Id = 3, Status = "success", StartedAt = start.AddMinutes(1) },
                    new Step { Id = 4, Status = "failure", StartedAt = start.AddMinutes(3) },
                    new Step { Id = 5, Status = "success", StartedAt = start.AddMinutes(2) },
                ],
            },
            new Run
            {
                Id = 2,
                Steps =
                [
                    new Step { Id = 6, Status = "queued", StartedAt = null },
                    new Step { Id = 7, Status = "success", StartedAt = start },
                ],
            });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var runs = await repository.GetAllAsync(new QueryOptions<Run>()
            .OrderBy(x => x.Id)
            .Include(x => x.Steps, steps => steps
                .Where(s => s.Status != "skipped")
                .OrderByDescending(s => s.StartedAt, NullsOrder.Last)
                .Take(3)));

        Assert.That(runs.Select(r => r.Steps.Select(s => s.Id).ToArray()),
            Is.EqualTo(new[] { new[] { 4, 5, 3 }, new[] { 7, 6 } }),
            "PostgreSQL puts nulls first when descending, so the queued step comes last only through the placement");
    }

    [TestCase(false, NullsOrder.First, new[] { 2, 3, 1 })]
    [TestCase(false, NullsOrder.Last, new[] { 3, 1, 2 })]
    [TestCase(true, NullsOrder.First, new[] { 2, 1, 3 })]
    [TestCase(true, NullsOrder.Last, new[] { 1, 3, 2 })]
    public async Task A_value_read_through_an_optional_navigation_places_its_nulls(bool descending, NullsOrder nulls,
        int[] ids)
    {
        var (context, repository) = await NewAsync();
        await using var _ = context;
        var first = new Team { Id = 1, Rank = 1 };
        var second = new Team { Id = 2, Rank = 2 };
        context.Runs.AddRange(
            new Run { Id = 1, Team = second },
            new Run { Id = 2 },
            new Run { Id = 3, Team = first });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Rank is an int, but the run without a team reads it as NULL through the join.
        var options = descending
            ? new QueryOptions<Run>().OrderByDescending(x => x.Team!.Rank, nulls)
            : new QueryOptions<Run>().OrderBy(x => x.Team!.Rank, nulls);
        var runs = await repository.GetAllAsync(options);

        Assert.That(runs.Select(x => x.Id), Is.EqualTo(ids));
    }
}
