using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Driver;

namespace eQuantic.Core.Data.EntityFramework.MongoDb;

internal static class UpdateDefinitionBuilder
{
    public static UpdateDefinition<TEntity>? BuildUpdateDefinition<TEntity>(Expression<Func<TEntity, TEntity>> updateExpression)
    {
        if (updateExpression.Body is not MemberInitExpression memberInit)
            throw new ArgumentException("The update expression must be a MemberInitExpression.", nameof(updateExpression));

        var updateDefinitionBuilder = Builders<TEntity>.Update;
        List<UpdateDefinition<TEntity>> updates = new();

        foreach (var binding in memberInit.Bindings)
        {
            if (binding is MemberAssignment memberAssignment)
            {
                var memberName = memberAssignment.Member.Name;
                var value = GetValueFromExpression(memberAssignment.Expression, updateExpression.Parameters);
                
                if (value != null && !IsSimpleType(value.GetType()))
                {
                    foreach (var subUpdate in BuildNestedUpdate<TEntity>(memberName, value))
                    {
                        updates.Add(subUpdate);
                    }
                }
                else
                {
                    updates.Add(updateDefinitionBuilder.Set(memberName, value));
                }
            }
        }

        return updates.Count > 0 ? updateDefinitionBuilder.Combine(updates) : null;
    }

    private static object? GetValueFromExpression(Expression expression, IReadOnlyList<ParameterExpression> parameters)
    {
        var lambda = Expression.Lambda(expression, parameters);
        var compiledLambda = lambda.Compile();
        return compiledLambda.DynamicInvoke(Activator.CreateInstance(parameters[0].Type));
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime);
    }

    private static IEnumerable<UpdateDefinition<TEntity>> BuildNestedUpdate<TEntity>(string prefix, object obj)
    {
        var updateDefinitionBuilder = Builders<TEntity>.Update;
        var updates = new List<UpdateDefinition<TEntity>>();

        foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = property.GetValue(obj);
            if (value == null) continue;

            var fieldPath = $"{prefix}.{property.Name}";

            if (IsSimpleType(property.PropertyType))
            {
                updates.Add(updateDefinitionBuilder.Set(fieldPath, value));
            }
            else
            {
                updates.AddRange(BuildNestedUpdate<TEntity>(fieldPath, value));
            }
        }

        return updates;
    }
}
