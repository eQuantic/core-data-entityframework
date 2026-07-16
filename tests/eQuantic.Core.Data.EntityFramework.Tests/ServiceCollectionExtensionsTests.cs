using System.Linq;
using eQuantic.Core.Data.EntityFramework.Repository.Extensions;
using eQuantic.Core.Data.EntityFramework.Tests.Fakes;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Sql;
using Microsoft.Extensions.DependencyInjection;

namespace eQuantic.Core.Data.EntityFramework.Tests;

/// <summary>
///     Guards the fix for the DI-registration defect: a non-relational unit of work must not have
///     <see cref="ISqlUnitOfWork" /> registered against it, and <see cref="IQueryableUnitOfWork" />
///     must be registered exactly once.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddQueryableRepositories_NonSqlUnitOfWork_DoesNotRegisterSqlUnitOfWork()
    {
        var services = new ServiceCollection();

        services.AddQueryableRepositories<FakeQueryableUnitOfWork>();

        Assert.That(services.Any(d => d.ServiceType == typeof(ISqlUnitOfWork)), Is.False,
            "ISqlUnitOfWork must not be registered for a unit of work that does not implement it.");
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
