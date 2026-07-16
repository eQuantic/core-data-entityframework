using eQuantic.Core.Data.Repository;

namespace eQuantic.Core.Data.EntityFramework.Tests.Fakes;

/// <summary>
///     Minimal entity used to close the generic type parameters of the repositories under test.
/// </summary>
internal sealed class FakeEntity : IEntity
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
