using eQuantic.Core.Data.EntityFramework.CosmosDb;
using eQuantic.Core.Data.Repository.Options;

namespace eQuantic.Core.Data.EntityFramework.CosmosDb.Tests;

/// <summary>
///     Verifies the Cosmos <c>WithPartitionKey</c> convenience registers a before-customization on the
///     <see cref="QueryOptions{TEntity}" /> and returns the same instance for chaining. The customization
///     itself calls EF Core Cosmos' <c>WithPartitionKey</c>, which only runs against Cosmos, so it is not
///     executed here (the in-memory provider does not support it).
/// </summary>
public class CosmosQueryOptionsExtensionsTests
{
    [Test]
    public void WithPartitionKey_RegistersBeforeCustomization_AndChains()
    {
        var options = new QueryOptions<TestDoc>();
        Assert.That(options.BeforeCustomization, Is.Null);

        var result = options.WithPartitionKey("tenant-1");

        Assert.That(result, Is.SameAs(options));
        Assert.That(options.BeforeCustomization, Is.Not.Null);
    }

    [Test]
    public void WithPartitionKey_Throws_WhenOptionsNull()
    {
        QueryOptions<TestDoc> options = null!;
        Assert.That(() => options.WithPartitionKey("tenant-1"), Throws.TypeOf<ArgumentNullException>());
    }
}
