using eQuantic.Core.Data.EntityFramework.CosmosDb.Repository;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.CosmosDb.Tests;

/// <summary>
///     Exercises the Cosmos set's bulk operations against the EF Core in-memory provider (no emulator).
///     Cosmos has no server-side set-based delete/update, so the set loads and modifies through the context;
///     these tests prove the load-then-modify path deletes/updates the right rows, returns the matched
///     count, and — critically — that <c>UpdateMany</c> touches only the assigned members.
/// </summary>
public class CosmosSetTests
{
    private static TestContext NewContext(params TestDoc[] seed)
    {
        var options = new DbContextOptionsBuilder<TestContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new TestContext(options);
        if (seed.Length > 0)
        {
            context.Docs.AddRange(seed);
            context.SaveChanges();
        }

        return context;
    }

    [Test]
    public void DeleteMany_RemovesMatching_ReturnsCount()
    {
        using var context = NewContext(
            new TestDoc { Id = 1, Status = "New" },
            new TestDoc { Id = 2, Status = "Paid" },
            new TestDoc { Id = 3, Status = "Paid" });
        var set = new Set<TestDoc>(context);

        var deleted = set.DeleteMany(d => d.Status == "Paid");

        Assert.That(deleted, Is.EqualTo(2));
        Assert.That(context.Docs.Count(), Is.EqualTo(1));
        Assert.That(context.Docs.Single().Id, Is.EqualTo(1));
    }

    [Test]
    public async Task DeleteManyAsync_RemovesMatching_ReturnsCount()
    {
        await using var context = NewContext(
            new TestDoc { Id = 1, Status = "New" },
            new TestDoc { Id = 2, Status = "Paid" });
        var set = new Set<TestDoc>(context);

        var deleted = await set.DeleteManyAsync(d => d.Status == "Paid");

        Assert.That(deleted, Is.EqualTo(1));
        Assert.That(context.Docs.Count(), Is.EqualTo(1));
    }

    [Test]
    public void DeleteMany_NoMatch_ReturnsZero()
    {
        using var context = NewContext(new TestDoc { Id = 1, Status = "New" });
        var set = new Set<TestDoc>(context);

        Assert.That(set.DeleteMany(d => d.Status == "Paid"), Is.EqualTo(0));
        Assert.That(context.Docs.Count(), Is.EqualTo(1));
    }

    [Test]
    public void UpdateMany_AppliesOnlyTheSetMembers_LeavingOthersIntact()
    {
        using var context = NewContext(
            new TestDoc { Id = 1, Status = "New", Count = 4 },
            new TestDoc { Id = 2, Status = "New", Count = 7 });
        var set = new Set<TestDoc>(context);

        var updated = set.UpdateMany(d => d.Status == "New", d => new TestDoc { Status = "Paid" });

        Assert.That(updated, Is.EqualTo(2));
        var docs = context.Docs.OrderBy(d => d.Id).ToList();
        Assert.That(docs.All(d => d.Status == "Paid"), Is.True);
        // Count was not part of the update expression, so it must be preserved (not reset to 0).
        Assert.That(docs[0].Count, Is.EqualTo(4));
        Assert.That(docs[1].Count, Is.EqualTo(7));
    }

    [Test]
    public async Task UpdateManyAsync_EvaluatesEntityReferencingValues()
    {
        await using var context = NewContext(
            new TestDoc { Id = 1, Status = "New", Count = 4 },
            new TestDoc { Id = 2, Status = "New", Count = 10 });
        var set = new Set<TestDoc>(context);

        var updated = await set.UpdateManyAsync(d => d.Status == "New", d => new TestDoc { Count = d.Count + 1 });

        Assert.That(updated, Is.EqualTo(2));
        var docs = context.Docs.OrderBy(d => d.Id).ToList();
        Assert.That(docs[0].Count, Is.EqualTo(5));
        Assert.That(docs[1].Count, Is.EqualTo(11));
    }
}
