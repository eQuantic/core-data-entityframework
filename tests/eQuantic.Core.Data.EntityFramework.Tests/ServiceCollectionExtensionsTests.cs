using System.Linq;
using eQuantic.Core.Data.EntityFramework.Repository.Extensions;
using eQuantic.Core.Data.EntityFramework.Tests.Fakes;
using eQuantic.Core.Data.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.Data.EntityFramework.Tests;

/// <summary>
///     Guards the DI-registration behaviour of the base (SQL-agnostic) registration: the generic
///     repositories are wired, <see cref="IQueryableUnitOfWork" /> is registered exactly once, and no
///     relational SQL unit-of-work contract is registered (that moved to the Relational provider in v5).
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddQueryableRepositories_WiresGenericRepositories()
    {
        var services = new ServiceCollection();

        services.AddQueryableRepositories<FakeQueryableUnitOfWork>();

        Assert.That(services.Any(d => d.ServiceType == typeof(IQueryableRepository<,>)), Is.True,
            "The open-generic IQueryableRepository must be registered by the base registration.");
        Assert.That(services.Any(d => d.ServiceType == typeof(IAsyncQueryableRepository<,>)), Is.True,
            "The open-generic IAsyncQueryableRepository must be registered by the base registration.");
    }

    [Test]
    public void AddQueryableRepositories_IsSqlAgnostic_DoesNotRegisterSqlUnitOfWork()
    {
        var services = new ServiceCollection();

        services.AddQueryableRepositories<FakeQueryableUnitOfWork>();

        // ISqlUnitOfWork moved out of eQuantic.Core.Data into the Relational provider in v5, so the base
        // registration is SQL-agnostic and must not register it. Checked by name so this base test does
        // not reference the moved type.
        Assert.That(services.Any(d => d.ServiceType.Name == "ISqlUnitOfWork"), Is.False,
            "The base registration must not register ISqlUnitOfWork (it moved to the Relational provider).");
    }

    [Test]
    public void AddQueryableRepositories_RegistersQueryableUnitOfWork_ExactlyOnce()
    {
        var services = new ServiceCollection();

        services.AddQueryableRepositories<FakeQueryableUnitOfWork>();

        Assert.That(services.Count(d => d.ServiceType == typeof(IQueryableUnitOfWork)), Is.EqualTo(1),
            "IQueryableUnitOfWork must be registered exactly once (the duplicate TryAdd was removed).");
    }

    [Test]
    public void AddQueryableRepositories_RegistersConcreteUnitOfWork_AsQueryableUnitOfWork()
    {
        var services = new ServiceCollection();

        services.AddQueryableRepositories<FakeQueryableUnitOfWork>();

        Assert.That(
            services.Any(d => d.ServiceType == typeof(IQueryableUnitOfWork)
                              && d.ImplementationType == typeof(FakeQueryableUnitOfWork)),
            Is.True);
    }

    [Test]
    public void AddCustomRepositories_HonoursConfiguredLifetime()
    {
        var services = new ServiceCollection();

        services.AddCustomRepositories<FakeQueryableUnitOfWork>(o => o
            .AddLifetime(ServiceLifetime.Scoped)
            .FromAssembly(typeof(FakeRepository).Assembly));

        var descriptor = services.FirstOrDefault(d => d.ImplementationType == typeof(FakeRepository));
        Assert.That(descriptor, Is.Not.Null, "The scanned repository should have been registered.");
        Assert.That(descriptor!.Lifetime, Is.EqualTo(ServiceLifetime.Scoped),
            "AddRepository must honour the configured lifetime instead of forcing Transient.");
    }
}
