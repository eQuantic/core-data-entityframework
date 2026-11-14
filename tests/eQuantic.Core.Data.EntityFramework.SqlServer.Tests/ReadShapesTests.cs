using eQuantic.Core.Data.EntityFramework.Repository.Read;
using eQuantic.Core.Data.EntityFramework.SqlServer.Repository;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.Data.EntityFramework.SqlServer.Tests;

/// <summary>
///     The read shapes eQuantic.Core.Data 7.0.0 added to <see cref="QueryOptions{TEntity}" />, honoured by the
///     Entity Framework read repositories as the core-data engines honour them: <c>Skip</c>/<c>Take</c> without a
///     total (and refused where a read does not return rows that way), nulls placed first or last, and shaped
///     collection includes. The same scenarios run against a real PostgreSQL in the PostgreSql tests, which is what
///     proves the translation; these prove the rules on the in-memory provider.
/// </summary>
public class ReadShapesTests
{
    public sealed class Run : IEntity<int>
    {
        public int Id { get; set; }

        public int? Position { get; set; }

        public string? Label { get; set; }

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

    private sealed class RunsContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Run> Runs => Set<Run>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Run>().HasMany(x => x.Steps).WithOne().HasForeignKey(x => x.RunId);
            builder.Entity<Run>().HasOne(x => x.Team).WithMany().HasForeignKey(x => x.TeamId);
        }
    }

    private static AsyncQueryableReadRepository<Run, int> NewRepository(out RunsContext context)
    {
        context = new RunsContext(new DbContextOptionsBuilder()
            .UseInMemoryDatabase($"read-shapes-{Guid.NewGuid():N}").Options);
        return new AsyncQueryableReadRepository<Run, int>(
            new DefaultUnitOfWork(new ServiceCollection().BuildServiceProvider(), context));
    }

    [Test]
    public async Task Skip_and_take_return_the_page_without_a_total()
    {
        var repository = NewRepository(out var context);
        context.Runs.AddRange(Enumerable.Range(1, 25).Select(id => new Run { Id = id }));
        await context.SaveChangesAsync();

        var page = await repository.GetAllAsync(new QueryOptions<Run>().OrderBy(x => x.Id).Skip(10).Take(10));

        Assert.That(page.Select(x => x.Id), Is.EqualTo(Enumerable.Range(11, 10)));
    }

    [Test]
    public async Task Take_composes_with_a_first_and_a_single()
    {
        var repository = NewRepository(out var context);
        context.Runs.AddRange(new Run { Id = 1 }, new Run { Id = 2 }, new Run { Id = 3 });
        await context.SaveChangesAsync();

        var second = await repository.GetFirstAsync(new QueryOptions<Run>().OrderBy(x => x.Id).Skip(1));
        var only = await repository.GetSingleAsync(new QueryOptions<Run>().OrderBy(x => x.Id).Take(1));

        Assert.That(second!.Id, Is.EqualTo(2));
        Assert.That(only!.Id, Is.EqualTo(1), "the window holds one row, so a single finds exactly one");
    }

    [Test]
    public void A_read_over_every_row_refuses_skip_and_take()
    {
        var repository = NewRepository(out _);
        var page = new QueryOptions<Run>().Take(10);

        Assert.Multiple(() =>
        {
            Assert.ThrowsAsync<NotSupportedException>(() => repository.CountAsync(page));
            Assert.ThrowsAsync<NotSupportedException>(() => repository.AnyAsync(page));
            Assert.ThrowsAsync<NotSupportedException>(() => repository.AllAsync(x => x.Id > 0, page));
            Assert.ThrowsAsync<NotSupportedException>(() => repository.SumAsync(x => x.Id, page));
            Assert.ThrowsAsync<NotSupportedException>(() => repository.GetPagedAsync(new PageRequest(1, 10), page));
            Assert.Throws<NotSupportedException>(() => repository.Count(new QueryOptions<Run>().Skip(1)));
            Assert.Throws<NotSupportedException>(() => repository.Sum(x => x.Id, new QueryOptions<Run>().Skip(1)));
            Assert.Throws<NotSupportedException>(() => repository.GetPaged(new PageRequest(1, 10), page));
        });
    }

    [TestCase(false, NullsOrder.First, new[] { 0, 1, 2 })]
    [TestCase(false, NullsOrder.Last, new[] { 1, 2, 0 })]
    [TestCase(true, NullsOrder.First, new[] { 0, 2, 1 })]
    [TestCase(true, NullsOrder.Last, new[] { 2, 1, 0 })]
    public async Task Nulls_go_where_the_sort_places_them(bool descending, NullsOrder nulls, int[] positions)
    {
        var repository = NewRepository(out var context);
        context.Runs.AddRange(
            new Run { Id = 1, Position = 2 },
            new Run { Id = 2, Position = null },
            new Run { Id = 3, Position = 1 });
        await context.SaveChangesAsync();

        var options = descending
            ? new QueryOptions<Run>().OrderByDescending(x => x.Position, nulls)
            : new QueryOptions<Run>().OrderBy(x => x.Position, nulls);
        var runs = await repository.GetAllAsync(options);

        Assert.That(runs.Select(x => x.Position ?? 0), Is.EqualTo(positions));
    }

    [Test]
    public async Task A_secondary_sort_places_its_nulls_within_the_primary()
    {
        var repository = NewRepository(out var context);
        context.Runs.AddRange(
            new Run { Id = 1, Position = 1, Label = "b" },
            new Run { Id = 2, Position = 1, Label = null },
            new Run { Id = 3, Position = 0, Label = "a" });
        await context.SaveChangesAsync();

        var runs = await repository.GetAllAsync(new QueryOptions<Run>()
            .OrderBy(x => x.Position).ThenBy(x => x.Label, NullsOrder.Last));

        Assert.That(runs.Select(x => x.Id), Is.EqualTo(new[] { 3, 1, 2 }));
    }

    [Test]
    public async Task A_shaped_include_loads_the_last_three_newest_first()
    {
        var repository = NewRepository(out var context);
        var start = new DateTime(2026, 11, 14, 12, 0, 0, DateTimeKind.Utc);
        context.Runs.Add(new Run
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
        });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var run = await repository.GetAsync(1, new QueryOptions<Run>()
            .Include(x => x.Steps, steps => steps
                .Where(s => s.Status != "skipped")
                .OrderByDescending(s => s.StartedAt)
                .Take(3)));

        Assert.That(run!.Steps.Select(s => s.Id), Is.EqualTo(new[] { 4, 5, 3 }));
    }

    [Test]
    public async Task A_shaped_include_with_a_skip_and_nulls_last_pages_each_parent()
    {
        var repository = NewRepository(out var context);
        var start = new DateTime(2026, 11, 14, 12, 0, 0, DateTimeKind.Utc);
        context.Runs.AddRange(
            new Run
            {
                Id = 1,
                Steps =
                [
                    new Step { Id = 1, Status = "queued", StartedAt = null },
                    new Step { Id = 2, Status = "success", StartedAt = start },
                    new Step { Id = 3, Status = "success", StartedAt = start.AddMinutes(1) },
                ],
            },
            new Run
            {
                Id = 2,
                Steps =
                [
                    new Step { Id = 4, Status = "success", StartedAt = start.AddMinutes(9) },
                    new Step { Id = 5, Status = "queued", StartedAt = null },
                ],
            });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var runs = await repository.GetAllAsync(new QueryOptions<Run>()
            .OrderBy(x => x.Id)
            .Include(x => x.Steps, steps => steps.OrderBy(s => s.StartedAt, NullsOrder.Last).Skip(1)));

        Assert.That(runs.Select(r => r.Steps.Select(s => s.Id).ToArray()),
            Is.EqualTo(new[] { new[] { 3, 1 }, new[] { 5 } }));
    }

    [TestCase(false, NullsOrder.First, new[] { 2, 3, 1 })]
    [TestCase(false, NullsOrder.Last, new[] { 3, 1, 2 })]
    [TestCase(true, NullsOrder.First, new[] { 2, 1, 3 })]
    [TestCase(true, NullsOrder.Last, new[] { 1, 3, 2 })]
    public async Task A_value_read_through_an_optional_navigation_places_its_nulls(bool descending, NullsOrder nulls,
        int[] ids)
    {
        var repository = NewRepository(out var context);
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
