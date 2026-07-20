using System.Linq.Expressions;
using System.Reflection;
using eQuantic.Core.Data.EntityFramework.Repository;
using eQuantic.Core.Data.Repository;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.CosmosDb.Repository;

/// <summary>
///     The Azure Cosmos DB entity set. Cosmos has no server-side set-based delete/update
///     (<c>ExecuteDelete</c>/<c>ExecuteUpdate</c> are relational-only), so the bulk operations load the
///     matching entities through the <see cref="DbContext" /> and delete/modify them via the change
///     tracker before saving. Every other set behaviour is inherited from <see cref="SetBase{TEntity}" />.
/// </summary>
public class Set<TEntity> : SetBase<TEntity> where TEntity : class, IEntity
{
    public Set(DbContext context) : base(context)
    {
    }

    public override long DeleteMany(Expression<Func<TEntity, bool>> filter)
    {
        var items = InternalDbSet.Where(filter).ToList();
        if (items.Count == 0)
        {
            return 0;
        }

        InternalDbSet.RemoveRange(items);
        DbContext.SaveChanges();
        return items.Count;
    }

    public override async Task<long> DeleteManyAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
    {
        var items = await InternalDbSet.Where(filter).ToListAsync(cancellationToken).ConfigureAwait(false);
        if (items.Count == 0)
        {
            return 0;
        }

        InternalDbSet.RemoveRange(items);
        await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return items.Count;
    }

    public override long UpdateMany(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateExpression)
    {
        var setters = CosmosUpdateExpression.ExtractSetters(updateExpression);
        var items = InternalDbSet.Where(filter).ToList();
        if (items.Count == 0)
        {
            return 0;
        }

        Apply(items, setters);
        DbContext.SaveChanges();
        return items.Count;
    }

    public override async Task<long> UpdateManyAsync(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updateExpression, CancellationToken cancellationToken = default)
    {
        var setters = CosmosUpdateExpression.ExtractSetters(updateExpression);
        var items = await InternalDbSet.Where(filter).ToListAsync(cancellationToken).ConfigureAwait(false);
        if (items.Count == 0)
        {
            return 0;
        }

        Apply(items, setters);
        await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return items.Count;
    }

    private static void Apply(IReadOnlyList<TEntity> items,
        IReadOnlyList<(PropertyInfo Property, Func<TEntity, object?> Value)> setters)
    {
        foreach (var item in items)
        {
            foreach (var (property, value) in setters)
            {
                property.SetValue(item, value(item));
            }
        }
    }
}
