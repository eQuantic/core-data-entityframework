using System;
using System.Linq;
using eQuantic.Core.Data.EntityFramework.Repository.Options;
using eQuantic.Core.Data.Repository;
using eQuantic.Core.Data.Repository.Sql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace eQuantic.Core.Data.EntityFramework.Repository.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQueryableRepositories<TQueryableUnitOfWork>(this IServiceCollection services)
        where TQueryableUnitOfWork : class, IDefaultUnitOfWork
    {
        return AddQueryableRepositories<TQueryableUnitOfWork>(services, _ => { });
    }

    public static IServiceCollection AddQueryableRepositories<TUnitOfWorkInterface, TUnitOfWorkImpl>(
        this IServiceCollection services)
        where TUnitOfWorkInterface : ISqlUnitOfWork<TUnitOfWorkInterface>
        where TUnitOfWorkImpl : class, ISqlUnitOfWork<TUnitOfWorkInterface>
    {
        return AddQueryableRepositories<TUnitOfWorkInterface, TUnitOfWorkImpl>(services, _ => {});
    }

    public static IServiceCollection AddQueryableRepositories<TUnitOfWorkInterface, TUnitOfWorkImpl>(this IServiceCollection services,
        Action<RepositoryOptions> options)
        where TUnitOfWorkInterface : ISqlUnitOfWork<TUnitOfWorkInterface>
        where TUnitOfWorkImpl : class, ISqlUnitOfWork<TUnitOfWorkInterface>
    {
        var repoOptions = GetOptions(options);
        var lifetime = repoOptions.GetLifetime();
        
        AddUnitOfWork<TUnitOfWorkInterface, TUnitOfWorkImpl>(services, lifetime);
        AddGenericRepositories(services, lifetime);

        return services;
    }

    public static IServiceCollection AddQueryableRepositories<TQueryableUnitOfWork>(this IServiceCollection services,
        Action<RepositoryOptions> options)
        where TQueryableUnitOfWork : class, IDefaultUnitOfWork
    {
        var repoOptions = GetOptions(options);
        var lifetime = repoOptions.GetLifetime();
        
        AddUnitOfWork<IDefaultUnitOfWork, TQueryableUnitOfWork>(services, lifetime);
        AddGenericRepositories(services, lifetime);

        return services;
    }

    public static IServiceCollection AddCustomRepositories<TQueryableUnitOfWork>(this IServiceCollection services,
        Action<RepositoryOptions> options)
        where TQueryableUnitOfWork : class, IDefaultUnitOfWork
    {
        var repoOptions = GetOptions(options);
        var lifetime = repoOptions.GetLifetime();
        
        AddUnitOfWork<IDefaultUnitOfWork, TQueryableUnitOfWork>(services, lifetime);
        AddRepositories(services, repoOptions);

        return services;
    }

    private static void AddUnitOfWork<TUnitOfWorkInterface, TUnitOfWorkImpl>(IServiceCollection services, ServiceLifetime lifetime)
        where TUnitOfWorkInterface : ISqlUnitOfWork
        where TUnitOfWorkImpl : class, ISqlUnitOfWork
    {
        services.TryAdd(new ServiceDescriptor(typeof(TUnitOfWorkInterface), typeof(TUnitOfWorkImpl), lifetime));
        services.TryAdd(new ServiceDescriptor(typeof(ISqlUnitOfWork<TUnitOfWorkInterface>), sp => sp.GetRequiredService<TUnitOfWorkInterface>(), lifetime));
        services.TryAdd(new ServiceDescriptor(typeof(IQueryableUnitOfWork<TUnitOfWorkInterface>), sp => sp.GetRequiredService<TUnitOfWorkInterface>(), lifetime));
        
        services.TryAdd(new ServiceDescriptor(typeof(ISqlUnitOfWork), sp => sp.GetRequiredService<TUnitOfWorkInterface>(), lifetime));
        services.TryAdd(new ServiceDescriptor(typeof(IQueryableUnitOfWork), sp => sp.GetRequiredService<TUnitOfWorkInterface>(), lifetime));
    }

    private static void AddGenericRepositories(IServiceCollection services, ServiceLifetime lifetime)
    {
        services.TryAdd(new ServiceDescriptor(typeof(Repository<,,>), typeof(Repository<,,>), lifetime));
        services.TryAdd(new ServiceDescriptor(typeof(AsyncRepository<,,>), typeof(AsyncRepository<,,>), lifetime));
        services.TryAdd(new ServiceDescriptor(typeof(QueryableRepository<,,>), typeof(QueryableRepository<,,>),
            lifetime));
        services.TryAdd(new ServiceDescriptor(typeof(AsyncQueryableRepository<,,>),
            typeof(AsyncQueryableRepository<,,>), lifetime));
    }
    private static void AddRepositories(IServiceCollection services, RepositoryOptions repoOptions)
    {
        var types = repoOptions.GetAssemblies()
            .SelectMany(o => o.GetTypes())
            .Where(o => o is { IsAbstract: false, IsInterface: false } &&
                        o.GetInterfaces().Any(i => i == typeof(IRepository)));
        foreach (var type in types)
        {
            AddRepository(typeof(IRepository<,,>), type, services);
            AddRepository(typeof(IAsyncRepository<,,>), type, services);
            AddRepository(typeof(IQueryableRepository<,,>), type, services);
            AddRepository(typeof(IAsyncQueryableRepository<,,>), type, services);
        }
    }

    private static void AddRepository(Type interfaceType, Type type, IServiceCollection services)
    {
        var interfaces = type.GetInterfaces();
        var repoInterface = interfaces.FirstOrDefault(o =>
            o.GenericTypeArguments.Length > 0 && o.GetGenericTypeDefinition() == interfaceType);

        if (repoInterface == null)
        {
            return;
        }

        var uowType = repoInterface.GenericTypeArguments[0];
        var entityType = repoInterface.GenericTypeArguments[1];
        var keyType = repoInterface.GenericTypeArguments[2];

        services.AddTransient(interfaceType.MakeGenericType(uowType, entityType, keyType), type);
    }

    private static RepositoryOptions GetOptions(Action<RepositoryOptions> options)
    {
        var repoOptions = new RepositoryOptions();
        options.Invoke(repoOptions);
        return repoOptions;
    }
}
