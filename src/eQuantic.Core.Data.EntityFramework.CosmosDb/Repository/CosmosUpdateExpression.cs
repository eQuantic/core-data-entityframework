using System.Linq.Expressions;
using System.Reflection;

namespace eQuantic.Core.Data.EntityFramework.CosmosDb.Repository;

/// <summary>
///     Turns a member-initialization update expression — e.g.
///     <c>x =&gt; new Order { Status = "Paid", Total = x.Total + 1 }</c> — into the set of property
///     assignments to apply. Azure Cosmos DB has no server-side set-based update (<c>ExecuteUpdate</c> is
///     relational-only), so <see cref="Set{TEntity}" /> loads the matching entities and applies these
///     setters to each. The value of every assignment is evaluated against the loaded entity, so
///     expressions that reference the entity itself (<c>x.Total + 1</c>) are honoured correctly.
/// </summary>
internal static class CosmosUpdateExpression
{
    public static IReadOnlyList<(PropertyInfo Property, Func<TEntity, object?> Value)> ExtractSetters<TEntity>(
        Expression<Func<TEntity, TEntity>> updateExpression)
    {
        if (updateExpression is null)
        {
            throw new ArgumentNullException(nameof(updateExpression));
        }

        if (updateExpression.Body is not MemberInitExpression memberInit)
        {
            throw new NotSupportedException(
                "UpdateMany requires a member-initialization expression, " +
                "e.g. x => new Entity { Status = \"Paid\", Total = x.Total + 1 }.");
        }

        var parameter = updateExpression.Parameters[0];
        var setters = new List<(PropertyInfo, Func<TEntity, object?>)>(memberInit.Bindings.Count);

        foreach (var binding in memberInit.Bindings)
        {
            if (binding is not MemberAssignment assignment || assignment.Member is not PropertyInfo property)
            {
                throw new NotSupportedException(
                    $"Unsupported binding '{binding.Member.Name}' in UpdateMany; only property " +
                    "assignments are supported.");
            }

            // Box the assigned value to object and compile against the entity parameter so member
            // references such as `x.Total + 1` evaluate against each loaded entity.
            var body = Expression.Convert(assignment.Expression, typeof(object));
            var valueLambda = Expression.Lambda<Func<TEntity, object?>>(body, parameter);
            setters.Add((property, valueLambda.Compile()));
        }

        return setters;
    }
}
