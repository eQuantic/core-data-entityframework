using eQuantic.Core.Data.EntityFramework.Repository;
using eQuantic.Core.Data.EntityFramework.Tests.Fakes;

namespace eQuantic.Core.Data.EntityFramework.Tests;

/// <summary>
///     Covers unit-of-work ownership on disposal. The repository receives its <c>UnitOfWork</c> by
///     injection, so it must NOT dispose it (its creator — the DI container or the caller — owns the
///     lifetime). This also removes the previous double-dispose of the shared DbContext.
/// </summary>
public class RepositoryDisposalTests
{
    [Test]
    public void Dispose_AsyncQueryableRepository_DoesNotDisposeInjectedUnitOfWork()
    {
        var unitOfWork = new FakeQueryableUnitOfWork();
        var repository = new AsyncQueryableRepository<FakeEntity, int>(unitOfWork);

        repository.Dispose();

        Assert.That(unitOfWork.DisposeCount, Is.EqualTo(0),
            "The repository must not dispose a UnitOfWork it did not create.");
    }

    [Test]
    public void Dispose_AsyncQueryableRepository_IsIdempotent()
    {
        var unitOfWork = new FakeQueryableUnitOfWork();
        var repository = new AsyncQueryableRepository<FakeEntity, int>(unitOfWork);

        repository.Dispose();
        repository.Dispose();

        Assert.That(unitOfWork.DisposeCount, Is.EqualTo(0));
    }

    [Test]
    public void Dispose_QueryableReadRepository_DoesNotDisposeInjectedUnitOfWork()
    {
        var unitOfWork = new FakeQueryableUnitOfWork();
        var repository = new eQuantic.Core.Data.EntityFramework.Repository.Read
            .QueryableReadRepository<FakeEntity, int>(unitOfWork);

        repository.Dispose();

        Assert.That(unitOfWork.DisposeCount, Is.EqualTo(0));
    }
}
