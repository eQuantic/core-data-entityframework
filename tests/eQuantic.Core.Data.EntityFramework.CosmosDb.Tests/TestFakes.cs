using eQuantic.Core.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.CosmosDb.Tests;

/// <summary>Minimal <see cref="IEntity{TKey}" /> used to drive the Cosmos set/expression tests.</summary>
public sealed class TestDoc : IEntity<int>
{
    public int Id { get; set; }
    public string? Status { get; set; }
    public int Count { get; set; }

    public int GetKey() => Id;
    public void SetKey(int key) => Id = key;
}

/// <summary>An EF Core context backing the tests with the in-memory provider.</summary>
public sealed class TestContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<TestDoc> Docs => Set<TestDoc>();
}
