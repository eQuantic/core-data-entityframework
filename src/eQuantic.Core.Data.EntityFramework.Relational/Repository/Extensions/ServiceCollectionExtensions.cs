using System;
using eQuantic.Core.Data.EntityFramework.Relational.Sql;
using eQuantic.Core.Data.EntityFramework.Repository.Options;
using eQuantic.Core.Data.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using BaseServiceCollectionExtensions = eQuantic.Core.Data.EntityFramework.Repository.Extensions.ServiceCollectionExtensions;

namespace eQuantic.Core.Data.EntityFramework.Relational.Repository.Extensions;

/// <summary>
///     Relational registration helpers. These wrap the SQL-agnostic base registration and additionally
///     expose <see cref="ISqlUnitOfWork" /> when the unit of work implements it. The base
///     <c>AddQueryableRepositories</c> no longer knows about <see cref="ISqlUnitOfWork" /> because that
///     contract moved into the Relational provider in v5.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the relational unit of work, the generic repositories and, when
    ///     <typeparamref name="TUnitOfWorkImpl" /> implements it, <see cref="ISqlUnitOfWork" />.
    /// </summary>
    /// <typeparam name="TUnitOfWorkInterface">The unit of work interface.</typeparam>
    /// <typeparam name="TUnitOfWorkImpl">The unit of work implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="options">Optional repository options (lifetime, assembly scanning).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRelationalRepositories<TUnitOfWorkInterface, TUnitOfWorkImpl>(
        this IServiceCollection services, Action<RepositoryOptions> options = null)
        where TUnitOfWorkInterface : IQueryableUnitOfWork
        where TUnitOfWorkImpl : class, TUnitOfWorkInterface
    {
        options ??= _ => { };

        BaseServiceCollectionExtensions.AddQueryableRepositories<TUnitOfWorkInterface, TUnitOfWorkImpl>(services, options);

        var repoOptions = new RepositoryOptions();
        options(repoOptions);
        var lifetime = repoOptions.GetLifetime();

        // Only expose ISqlUnitOfWork when the implementation actually provides it. Registering it
        // unconditionally would make resolving ISqlUnitOfWork throw for a non-SQL unit of work.
        if (typeof(ISqlUnitOfWork).IsAssignableFrom(typeof(TUnitOfWorkImpl)))
        {
            services.TryAdd(new ServiceDescriptor(typeof(ISqlUnitOfWork),
                sp => sp.GetRequiredService<TUnitOfWorkInterface>(), lifetime));
        }

        return services;
    }
}
