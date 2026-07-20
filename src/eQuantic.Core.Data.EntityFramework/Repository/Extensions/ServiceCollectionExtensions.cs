using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using eQuantic.Core.Data.EntityFramework.Repository.Options;
using eQuantic.Core.Data.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace eQuantic.Core.Data.EntityFramework.Repository.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQueryableRepositories<TQueryableUnitOfWork>(this IServiceCollection services)
        where TQueryableUnitOfWork : class, IQueryableUnitOfWork
    {
        return AddQueryableRepositories<TQueryableUnitOfWork>(services, _ => { });
    }

    public static IServiceCollection AddQueryableRepositories<TUnitOfWorkInterface, TUnitOfWorkImpl>(
        this IServiceCollection services)
        where TUnitOfWorkInterface : IQueryableUnitOfWork
        where TUnitOfWorkImpl : class, TUnitOfWorkInterface
    {
        return AddQueryableRepositories<TUnitOfWorkInterface, TUnitOfWorkImpl>(services, _ => {});
    }

    public static IServiceCollection AddQueryableRepositories<TUnitOfWorkInterface, TUnitOfWorkImpl>(this IServiceCollection services,
        Action<RepositoryOptions> options)
        where TUnitOfWorkInterface : IQueryableUnitOfWork
        where TUnitOfWorkImpl : class, TUnitOfWorkInterface
    {
        var repoOptions = GetOptions(options);
        var lifetime = repoOptions.GetLifetime();

        AddUnitOfWork<TUnitOfWorkInterface, TUnitOfWorkImpl>(services, lifetime);
        AddGenericRepositories(services, lifetime);

        return services;
    }

    public static IServiceCollection AddQueryableRepositories<TQueryableUnitOfWork>(this IServiceCollection services,
        Action<RepositoryOptions> options)
        where TQueryableUnitOfWork : class, IQueryableUnitOfWork
    {
        var repoOptions = GetOptions(options);
        var lifetime = repoOptions.GetLifetime();

        AddUnitOfWork<IQueryableUnitOfWork, TQueryableUnitOfWork>(services, lifetime);
        AddGenericRepositories(services, lifetime);

        return services;
    }

    public static IServiceCollection AddCustomRepositories<TQueryableUnitOfWork>(this IServiceCollection services,
        Action<RepositoryOptions> options)
        where TQueryableUnitOfWork : class, IQueryableUnitOfWork
    {
        var repoOptions = GetOptions(options);
        var lifetime = repoOptions.GetLifetime();

        AddUnitOfWork<IQueryableUnitOfWork, TQueryableUnitOfWork>(services, lifetime);
        AddRepositories(services, repoOptions);

        return services;
    }

    private static void AddUnitOfWork<TUnitOfWorkInterface, TUnitOfWorkImpl>(IServiceCollection services, ServiceLifetime lifetime)
        where TUnitOfWorkInterface : IQueryableUnitOfWork
        where TUnitOfWorkImpl : class, IQueryableUnitOfWork
    {
        services.TryAdd(new ServiceDescriptor(typeof(TUnitOfWorkInterface), typeof(TUnitOfWorkImpl), lifetime));
        services.TryAdd(new ServiceDescriptor(typeof(IQueryableUnitOfWork), sp => sp.GetRequiredService<TUnitOfWorkInterface>(), lifetime));
    }

    private static void AddGenericRepositories(IServiceCollection services, ServiceLifetime lifetime)
    {
        services.TryAdd(new ServiceDescriptor(typeof(IQueryableRepository<,>), typeof(QueryableRepository<,>), lifetime));
        services.TryAdd(new ServiceDescriptor(typeof(IAsyncQueryableRepository<,>), typeof(AsyncQueryableRepository<,>), lifetime));
    }

    private static void AddRepositories(IServiceCollection services, RepositoryOptions repoOptions)
    {
        var lifetime = repoOptions.GetLifetime();
        var types = repoOptions.GetAssemblies()
            .SelectMany(GetLoadableTypes)
            .Where(o => o is { IsAbstract: false, IsInterface: false } &&
                        o.GetInterfaces().Any(i => i == typeof(IRepository)));
        foreach (var type in types)
        {
            AddRepository(typeof(IRepository<,>), type, services, lifetime);
            AddRepository(typeof(IAsyncRepository<,>), type, services, lifetime);
            AddRepository(typeof(IQueryableRepository<,>), type, services, lifetime);
            AddRepository(typeof(IAsyncQueryableRepository<,>), type, services, lifetime);
        }
    }

    private static void AddRepository(Type interfaceType, Type type, IServiceCollection services, ServiceLifetime lifetime)
    {
        var interfaces = type.GetInterfaces();
        var repoInterface = interfaces.FirstOrDefault(o =>
            o.GenericTypeArguments.Length > 0 && o.GetGenericTypeDefinition() == interfaceType);

        if (repoInterface == null)
        {
            return;
        }

        var entityType = repoInterface.GenericTypeArguments[0];
        var keyType = repoInterface.GenericTypeArguments[1];

        // Honour the configured lifetime instead of forcing Transient, and use TryAdd so calling the
        // registration twice does not produce duplicate descriptors.
        services.TryAdd(new ServiceDescriptor(
            interfaceType.MakeGenericType(entityType, keyType), type, lifetime));
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        // Assembly.GetTypes() throws ReflectionTypeLoadException when a dependency cannot be loaded;
        // fall back to the types that did load instead of failing the whole registration at startup.
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
    }

    private static RepositoryOptions GetOptions(Action<RepositoryOptions> options)
    {
        var repoOptions = new RepositoryOptions();
        options.Invoke(repoOptions);
        return repoOptions;
    }
}
