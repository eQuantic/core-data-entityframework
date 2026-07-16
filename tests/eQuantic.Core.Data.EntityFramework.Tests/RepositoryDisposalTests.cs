using eQuantic.Core.Data.EntityFramework.Repository;
using eQuantic.Core.Data.EntityFramework.Tests.Fakes;

namespace eQuantic.Core.Data.EntityFramework.Tests;

/// <summary>
///     Guards the fix for the double-dispose defect: disposing an <see cref="AsyncQueryableRepository{TUnitOfWork,TEntity,TKey}" />
///     must dispose its unit of work exactly once, not twice (the shadowed disposal flag made both the
///     base and derived disposal blocks run).
/// </summary>
public class RepositoryDisposalTests
{
    [Test]
    public void Dispose_AsyncQueryableRepository_DisposesUnitOfWork_Once()
    {
        var unitOfWork = new FakeQueryableUnitOfWork();
        var repository = new AsyncQueryableRepository<FakeQueryableUnitOfWork, FakeEntity, int>(unitOfWork);

        repository.Dispose();

        Assert.That(unitOfWork.DisposeCount, Is.EqualTo(1));
    }

    [Test]
    public void Dispose_AsyncQueryableRepository_IsIdempotent()
    {
        var unitOfWork = new FakeQueryableUnitOfWork();
        var repository = new AsyncQueryableRepository<FakeQueryableUnitOfWork, FakeEntity, int>(unitOfWork);

        repository.Dispose();
        repository.Dispose();

        Assert.That(unitOfWork.DisposeCount, Is.EqualTo(1));
    }
}
