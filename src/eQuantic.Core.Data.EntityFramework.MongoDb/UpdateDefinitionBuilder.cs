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
            if (binding is not MemberAssignment memberAssignment) 
                continue;
            
            var memberName = memberAssignment.Member.Name;
            var value = GetValueFromExpression(memberAssignment.Expression, updateExpression.Parameters);
                
            if (value != null && !IsSimpleType(value.GetType()))
            {
                updates.AddRange(BuildNestedUpdate<TEntity>(memberName, value));
            }
            else
            {
                updates.Add(updateDefinitionBuilder.Set(memberName, value));
            }
        }

        return updates.Count > 0 ? updateDefinitionBuilder.Combine(updates) : null;
    }

    private static object? GetValueFromExpression(Expression expression, IReadOnlyList<ParameterExpression> parameters)
    {
        // The value is evaluated against a fresh default instance of the entity. Any assignment that
        // reads the entity (e.g. x => x.Count + 1) would therefore be evaluated against Count == 0 and
        // silently write the constant 1 to every matched document instead of incrementing it. Reject
        // such expressions loudly rather than corrupting data; only constant/closure values are safe.
        if (ReferencesParameter(expression, parameters))
        {
            throw new NotSupportedException(
                "The MongoDB update builder does not support update expressions that reference the " +
                "entity being updated (e.g. x => x.Count + 1). Such an expression would be evaluated " +
                "against a default instance and would overwrite documents with a constant value. Use a " +
                "constant or captured value instead.");
        }

        var lambda = Expression.Lambda(expression, parameters);
        var compiledLambda = lambda.Compile();
        return compiledLambda.DynamicInvoke(Activator.CreateInstance(parameters[0].Type));
    }

    private static bool ReferencesParameter(Expression expression, IReadOnlyList<ParameterExpression> parameters)
    {
        var detector = new ParameterReferenceDetector(parameters);
        detector.Visit(expression);
        return detector.Found;
    }

    private sealed class ParameterReferenceDetector : ExpressionVisitor
    {
        private readonly IReadOnlyList<ParameterExpression> _parameters;

        public ParameterReferenceDetector(IReadOnlyList<ParameterExpression> parameters)
        {
            _parameters = parameters;
        }

        public bool Found { get; private set; }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_parameters.Contains(node))
            {
                Found = true;
            }

            return base.VisitParameter(node);
        }
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
