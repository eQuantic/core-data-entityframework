using eQuantic.Core.Data.EntityFramework.Repository;

namespace eQuantic.Core.Data.EntityFramework.Tests.Fakes;

/// <summary>
///     A concrete repository so the assembly-scanning registration (AddCustomRepositories) has
///     something to discover. It inherits the marker <c>IRepository</c> transitively through
///     <see cref="QueryableRepository{TUnitOfWork,TEntity,TKey}" />.
/// </summary>
internal sealed class FakeRepository : QueryableRepository<FakeEntity, int>
{
    public FakeRepository(FakeQueryableUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }
}
