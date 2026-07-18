using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.Repository.Extensions;

public static class DbContextExtensions
{
    private static readonly ConcurrentDictionary<(Type Context, Type Entity), KeyProperty[]> KeyCache = new();

    /// <summary>
    ///     Builds a predicate that matches an entity by its primary key.
    /// </summary>
    /// <remarks>
    ///     The key value is referenced through a closure holder rather than embedded as a literal
    ///     <see cref="ConstantExpression" />, so EF Core parameterizes the query. Embedding the value
    ///     produced a distinct compiled-query cache entry (and a non-parameterized SQL literal) for
    ///     every id. The primary-key metadata is cached per (context type, entity type) to avoid the
    ///     model lookup on every call, and <see cref="EF.Property{TProperty}(object,string)" /> is used
    ///     so shadow keys (without a CLR property) are supported.
    /// </remarks>
    public static Expression<Func<TEntity, bool>> GetFindByKeyExpression<TEntity, TKey>(this DbContext dbContext, TKey id)
    {
        var keyProperties = GetKeyProperties<TEntity>(dbContext);
        if (keyProperties.Length == 0)
        {
            return null;
        }

        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        Expression expression = null;

        if (keyProperties.Length == 1)
        {
            var property = BuildPropertyAccess(parameter, keyProperties[0]);
            var idExpression = BuildKeyValueAccess(id);
            expression = Expression.Equal(property, EnsureType(idExpression, property.Type));
        }
        else
        {
            var idConstant = Expression.Constant(id, typeof(TKey));
            foreach (var keyProperty in keyProperties)
            {
                var idPartProperty = typeof(TKey).GetProperty(keyProperty.Name);
                if (idPartProperty == null)
                {
                    throw new InvalidOperationException(
                        $"The composite key property '{keyProperty.Name}' of '{typeof(TEntity).Name}' has no " +
                        $"matching property on the key type '{typeof(TKey).Name}'. A partial key predicate would " +
                        "match the wrong rows.");
                }

                var property = BuildPropertyAccess(parameter, keyProperty);
                // Member access on the key constant is parameterized by EF Core.
                var idPartValue = Expression.Property(idConstant, idPartProperty);
                var equality = Expression.Equal(property, EnsureType(idPartValue, property.Type));
                expression = expression == null ? equality : Expression.AndAlso(expression, equality);
            }
        }

        return expression == null ? null : Expression.Lambda<Func<TEntity, bool>>(expression, parameter);
    }

    /// <summary>
    ///     Ensures a deterministic order before paging. If the query is already ordered, it is returned
    ///     unchanged; otherwise it is ordered by the primary key. Skip/Take without an OrderBy produces
    ///     non-deterministic pages (rows may repeat or vanish between pages) and warns in EF Core.
    /// </summary>
    public static IQueryable<TEntity> OrderByPrimaryKeyIfUnordered<TEntity>(this IQueryable<TEntity> query, DbContext dbContext)
        where TEntity : class
    {
        if (IsOrdered(query.Expression))
        {
            return query;
        }

        var keyProperties = GetKeyProperties<TEntity>(dbContext);
        if (keyProperties.Length == 0)
        {
            // No primary key to fall back on (e.g. a keyless entity); leave ordering to the caller.
            return query;
        }

        var ordered = query;
        for (var i = 0; i < keyProperties.Length; i++)
        {
            var keyProperty = keyProperties[i];
            var parameter = Expression.Parameter(typeof(TEntity), "entity");
            var access = BuildPropertyAccess(parameter, keyProperty);
            var selector = Expression.Lambda(access, parameter);

            var methodName = i == 0 ? nameof(Queryable.OrderBy) : nameof(Queryable.ThenBy);
            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TEntity), keyProperty.ClrType);
            ordered = (IQueryable<TEntity>)method.Invoke(null, new object[] { ordered, selector })!;
        }

        return ordered;
    }

    private static bool IsOrdered(Expression expression)
    {
        var detector = new OrderingDetector();
        detector.Visit(expression);
        return detector.Found;
    }

    private static KeyProperty[] GetKeyProperties<TEntity>(DbContext dbContext)
    {
        return KeyCache.GetOrAdd((dbContext.GetType(), typeof(TEntity)), _ =>
        {
            var properties = dbContext.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties;
            return properties == null
                ? Array.Empty<KeyProperty>()
                : properties.Select(p => new KeyProperty(p.Name, p.ClrType)).ToArray();
        });
    }

    private static MethodCallExpression BuildPropertyAccess(ParameterExpression parameter, KeyProperty keyProperty)
    {
        // EF.Property<T>(entity, name) reads both mapped and shadow properties.
        return Expression.Call(
            typeof(EF),
            nameof(EF.Property),
            new[] { keyProperty.ClrType },
            parameter,
            Expression.Constant(keyProperty.Name));
    }

    private static Expression BuildKeyValueAccess<TKey>(TKey id)
    {
        // Wrapping the value in a holder and reading its field reproduces the closure pattern the C#
        // compiler emits for `x => x.Id == id`, which EF Core parameterizes.
        var holder = new KeyValueHolder<TKey>(id);
        return Expression.Field(Expression.Constant(holder), nameof(KeyValueHolder<TKey>.Value));
    }

    private static Expression EnsureType(Expression expression, Type targetType)
    {
        return expression.Type == targetType ? expression : Expression.Convert(expression, targetType);
    }

    private sealed class OrderingDetector : ExpressionVisitor
    {
        private static readonly HashSet<string> OrderingMethods = new()
        {
            nameof(Queryable.OrderBy), nameof(Queryable.OrderByDescending),
            nameof(Queryable.ThenBy), nameof(Queryable.ThenByDescending)
        };

        public bool Found { get; private set; }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable) && OrderingMethods.Contains(node.Method.Name))
            {
                Found = true;
            }

            return base.VisitMethodCall(node);
        }
    }

    private readonly struct KeyProperty
    {
        public KeyProperty(string name, Type clrType)
        {
            Name = name;
            ClrType = clrType;
        }

        public string Name { get; }
        public Type ClrType { get; }
    }

    private sealed class KeyValueHolder<T>
    {
        public readonly T Value;

        public KeyValueHolder(T value)
        {
            Value = value;
        }
    }
}
