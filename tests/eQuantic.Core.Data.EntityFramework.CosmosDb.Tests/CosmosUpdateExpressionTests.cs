using System.Linq.Expressions;
using eQuantic.Core.Data.EntityFramework.CosmosDb.Repository;

namespace eQuantic.Core.Data.EntityFramework.CosmosDb.Tests;

/// <summary>
///     Guards the update-expression parser that powers Cosmos <c>UpdateMany</c>: it must read each property
///     assignment and evaluate its value against the loaded entity (so <c>x.Count + 1</c> works), and
///     reject anything that is not a member-initialization.
/// </summary>
public class CosmosUpdateExpressionTests
{
    [Test]
    public void ExtractSetters_ReadsConstantAndEntityReferencingAssignments()
    {
        Expression<Func<TestDoc, TestDoc>> update = x => new TestDoc { Status = "Paid", Count = x.Count + 1 };

        var setters = CosmosUpdateExpression.ExtractSetters(update);
        var byName = setters.ToDictionary(s => s.Property.Name, s => s.Value);

        Assert.That(setters, Has.Count.EqualTo(2));
        Assert.That(byName.ContainsKey("Status") && byName.ContainsKey("Count"), Is.True);

        var doc = new TestDoc { Count = 4 };
        Assert.That(byName["Status"](doc), Is.EqualTo("Paid"));
        // Evaluated against the entity — Count + 1 = 5, not a constant.
        Assert.That(byName["Count"](doc), Is.EqualTo(5));
    }

    [Test]
    public void ExtractSetters_Throws_WhenNotMemberInitialization()
    {
        Expression<Func<TestDoc, TestDoc>> update = x => x;

        Assert.That(() => CosmosUpdateExpression.ExtractSetters(update),
            Throws.TypeOf<NotSupportedException>());
    }

    [Test]
    public void ExtractSetters_Throws_WhenNull()
    {
        Assert.That(() => CosmosUpdateExpression.ExtractSetters<TestDoc>(null!),
            Throws.TypeOf<ArgumentNullException>());
    }
}
