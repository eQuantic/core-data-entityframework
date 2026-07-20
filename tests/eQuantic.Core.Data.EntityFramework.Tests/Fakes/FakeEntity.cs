using eQuantic.Core.Data.Repository;

namespace eQuantic.Core.Data.EntityFramework.Tests.Fakes;

/// <summary>
///     Minimal entity used to close the generic type parameters of the repositories under test.
///     Implements <see cref="IEntity{TKey}" /> via <c>GetKey</c>/<c>SetKey</c> as required by the v5
///     read-repository contracts.
/// </summary>
internal sealed class FakeEntity : IEntity<int>
{
    public int Id { get; set; }
    public string? Name { get; set; }

    public int GetKey() => Id;

    public void SetKey(int key) => Id = key;
}
