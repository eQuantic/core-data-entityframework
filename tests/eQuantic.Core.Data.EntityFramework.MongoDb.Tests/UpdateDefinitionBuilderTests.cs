using System.Linq.Expressions;
using eQuantic.Core.Data.EntityFramework.MongoDb;
using eQuantic.Core.Data.Repository;

namespace eQuantic.Core.Data.EntityFramework.MongoDb.Tests;

/// <summary>
///     Guards the fix for the silent data-corruption defect: an update expression that reads the entity
///     (e.g. x => x.Count + 1) was evaluated against a default instance and wrote a constant. It must now
///     be rejected, while constant/closure values keep working.
/// </summary>
public class UpdateDefinitionBuilderTests
{
    private sealed class Doc : IEntity
    {
        public int Count { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Test]
    public void BuildUpdateDefinition_ConstantValue_BuildsDefinition()
    {
        Expression<Func<Doc, Doc>> update = _ => new Doc { Name = "fixed" };

        var definition = UpdateDefinitionBuilder.BuildUpdateDefinition(update);

        Assert.That(definition, Is.Not.Null);
    }

    [Test]
    public void BuildUpdateDefinition_CapturedValue_BuildsDefinition()
    {
        var captured = 42;
        Expression<Func<Doc, Doc>> update = _ => new Doc { Count = captured };

        var definition = UpdateDefinitionBuilder.BuildUpdateDefinition(update);

        Assert.That(definition, Is.Not.Null);
    }

    [Test]
    public void BuildUpdateDefinition_ExpressionReferencingEntity_Throws()
    {
        Expression<Func<Doc, Doc>> update = d => new Doc { Count = d.Count + 1 };

        Assert.Throws<NotSupportedException>(() => UpdateDefinitionBuilder.BuildUpdateDefinition(update));
    }
}
