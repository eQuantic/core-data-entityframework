using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.Repository.Extensions;

public static class DbContextExtensions
{
    public static Expression<Func<TEntity, bool>> GetFindByKeyExpression<TEntity, TKey>(this DbContext dbContext, TKey id)
    {
        var keyProperties = dbContext.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties;
        if (keyProperties == null || !keyProperties.Any())
        {
            return null;
        }

        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        Expression expression = null;

        if (keyProperties.Count == 1)
        {
            var keyProperty = keyProperties[0];
            var property = Expression.Property(parameter, keyProperty.Name);
            var idConstant = Expression.Constant(id, typeof(TKey));
            
            Expression idExpression = idConstant;
            if (property.Type != typeof(TKey))
            {
                idExpression = Expression.Convert(idConstant, property.Type);
            }
            expression = Expression.Equal(property, idExpression);
        }
        else
        {
            foreach (var keyProperty in keyProperties)
            {
                var property = Expression.Property(parameter, keyProperty.Name);
                var idPartProperty = typeof(TKey).GetProperty(keyProperty.Name);
                
                if (idPartProperty == null) continue;
                
                var idPartValue = Expression.Property(Expression.Constant(id, typeof(TKey)), idPartProperty);
                
                Expression idPartExpression = idPartValue;
                if (property.Type != idPartProperty.PropertyType)
                {
                    idPartExpression = Expression.Convert(idPartValue, property.Type);
                }
                
                var equality = Expression.Equal(property, idPartExpression);
                expression = expression == null ? equality : Expression.AndAlso(expression, equality);
            }
        }

        return expression == null ? null : Expression.Lambda<Func<TEntity, bool>>(expression, parameter);
    }
}