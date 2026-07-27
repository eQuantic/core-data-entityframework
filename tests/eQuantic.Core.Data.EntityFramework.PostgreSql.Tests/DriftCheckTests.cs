using eQuantic.Core.Data.EntityFramework.Relational.Evolution;
using eQuantic.Core.Data.Evolution;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace eQuantic.Core.Data.EntityFramework.PostgreSql.Tests;

/// <summary>
///     Comparing an Entity Framework model against the database it actually runs on.
///     <para>
///         This is the question Entity Framework does not answer for itself: its history table records which
///         migrations ran, and <c>has-pending-model-changes</c> compares the model to the migrations. Neither looks
///         at the schema. So the tests that matter here are run against a real PostgreSQL, and each one checks the
///         report by changing the database out from under the model — not by inspecting a query.
///     </para>
///     <para>
///         The first is the one that decides whether the feature is worth having: a schema Entity Framework itself
///         created must produce <b>no findings at all</b>. A check that reports spelling differences on a healthy
///         database is a green light nobody reads.
///     </para>
/// </summary>
[TestFixture]
public sealed class DriftCheckTests
{
    private PostgreSqlContainer _postgres = null!;
    private bool _available;

    public sealed class Order
    {
        public Guid Id { get; set; }

        public string Reference { get; set; } = "";

        public string? Notes { get; set; }

        public decimal Total { get; set; }

        public DateTime PlacedAt { get; set; }
    }

    private sealed class ShopContext(DbContextOptions<ShopContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var order = builder.Entity<Order>();
            order.ToTable("orders");
            order.HasKey(x => x.Id);
            order.Property(x => x.Reference).HasMaxLength(50).IsRequired();
            order.Property(x => x.Total).HasPrecision(18, 2);
        }
    }

    [OneTimeSetUp]
    public async Task StartAsync()
    {
        _postgres = new PostgreSqlBuilder().WithImage("postgres:16-alpine").Build();
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

    private async Task<ShopContext> NewSchemaAsync()
    {
        if (!_available)
        {
            Assert.Ignore("PostgreSQL test container is unavailable (Docker required).");
        }

        // A database of its own per test: dropping a shared one fails while a pooled connection still holds it,
        // and tests that alter the schema out from under the model must not see each other's damage.
        var builder = new Npgsql.NpgsqlConnectionStringBuilder(_postgres.GetConnectionString())
        {
            Database = $"drift_{Guid.NewGuid():N}",
        };

        var context = new ShopContext(new DbContextOptionsBuilder<ShopContext>()
            .UseNpgsql(builder.ConnectionString).Options);

        // Entity Framework creates it from its own model — the honest starting point for a check that claims a
        // healthy database is silent.
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    private static async Task<DriftReport> CheckAsync(ShopContext context)
    {
        var source = new EntityFrameworkDatabaseSnapshotSource(context);
        return DriftComparer.Compare(source.Expect(), await source.ObserveAsync());
    }

    private static Task AlterAsync(ShopContext context, string sql) =>
        context.Database.ExecuteSqlRawAsync(sql);

    [Test]
    public async Task A_schema_entity_framework_created_reports_nothing()
    {
        await using var context = await NewSchemaAsync();

        var report = await CheckAsync(context);

        Assert.That(report.Findings, Is.Empty,
            "every type the model declares must match the one PostgreSQL reports, or the check cries wolf");
        Assert.That(report.IsClean, Is.True);
    }

    [Test]
    public async Task A_column_dropped_by_hand_is_found()
    {
        await using var context = await NewSchemaAsync();
        await AlterAsync(context, "ALTER TABLE orders DROP COLUMN \"Notes\"");

        var finding = (await CheckAsync(context)).Findings.Single();

        Assert.That(finding.Kind, Is.EqualTo(DriftKind.MissingField));
        Assert.That(finding.Field, Is.EqualTo("Notes"));
        Assert.That(finding.Breaks, Is.True, "the application reads that column on every query");
    }

    [Test]
    public async Task A_column_narrowed_by_hand_is_found()
    {
        await using var context = await NewSchemaAsync();
        await AlterAsync(context, "ALTER TABLE orders ALTER COLUMN \"Reference\" TYPE varchar(10)");

        var finding = (await CheckAsync(context)).Findings.Single();

        Assert.That(finding.Kind, Is.EqualTo(DriftKind.TypeDiffers));
        Assert.That(finding.Expected, Is.EqualTo("character varying(50)"));
        Assert.That(finding.Found, Is.EqualTo("character varying(10)"));
    }

    [Test]
    public async Task A_required_column_made_optional_by_hand_is_found()
    {
        await using var context = await NewSchemaAsync();
        await AlterAsync(context, "ALTER TABLE orders ALTER COLUMN \"Reference\" DROP NOT NULL");

        var finding = (await CheckAsync(context)).Findings.Single();

        // Unlike the native engine, Entity Framework does write NOT NULL for a required property — so both
        // directions are comparable here, and a constraint dropped by hand is a real finding.
        Assert.That(finding.Kind, Is.EqualTo(DriftKind.NullabilityDiffers));
        Assert.That(finding.Expected, Is.EqualTo("not null"));
        Assert.That(finding.Found, Is.EqualTo("null allowed"));
    }

    [Test]
    public async Task A_table_dropped_by_hand_is_found()
    {
        await using var context = await NewSchemaAsync();
        await AlterAsync(context, "DROP TABLE orders");

        Assert.That((await CheckAsync(context)).Findings.Single().Kind,
            Is.EqualTo(DriftKind.MissingCollection));
    }

    [Test]
    public async Task A_column_nobody_mapped_is_reported_without_being_treated_as_a_fault()
    {
        await using var context = await NewSchemaAsync();
        await AlterAsync(context, "ALTER TABLE orders ADD COLUMN legacy_note text");

        var report = await CheckAsync(context);

        Assert.That(report.Findings.Single().Kind, Is.EqualTo(DriftKind.UnexpectedField));
        Assert.That(report.Breaks, Is.False,
            "a database gets shared; another application's column is not this one's problem");
    }

    [Test]
    public async Task A_table_nobody_mapped_is_not_read_at_all()
    {
        await using var context = await NewSchemaAsync();
        await AlterAsync(context, "CREATE TABLE somebody_elses (id integer)");

        Assert.That((await CheckAsync(context)).Findings, Is.Empty,
            "reading every table in a shared database would bury the findings that matter");
    }
}
