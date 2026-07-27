using eQuantic.Core.Data.Evolution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace eQuantic.Core.Data.EntityFramework.Relational.Evolution;

/// <summary>Registers the drift check over an Entity Framework context.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers <see cref="IDatabaseSnapshotSource" /> over <typeparamref name="TContext" />, so the model can
    ///     be compared against the database it actually runs on — by <c>eqdata drift</c>, or in code.
    ///     <para>
    ///         Opt-in rather than automatic, and scoped rather than singleton: it reads through the context's own
    ///         connection, so it has to live as long as the context does and no longer.
    ///     </para>
    ///     <example>
    ///         <code>
    ///         services.AddDbContext&lt;ShopContext&gt;(options => options.UseNpgsql(connectionString));
    ///         services.AddDriftCheck&lt;ShopContext&gt;();
    ///         </code>
    ///     </example>
    /// </summary>
    /// <typeparam name="TContext">The context whose model and connection are used.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDriftCheck<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        services.TryAddScoped<IDatabaseSnapshotSource>(provider =>
            new EntityFrameworkDatabaseSnapshotSource(provider.GetRequiredService<TContext>()));
        return services;
    }
}
