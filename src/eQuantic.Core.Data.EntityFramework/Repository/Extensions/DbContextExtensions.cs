using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace eQuantic.Core.Data.EntityFramework.Repository.Extensions;

public static class DbContextExtensions
{
    public static Expression<Func<TEntity, bool>> GetFindByKeyExpression<TEntity, TKey>(this DbContext dbContext, TKey id)
    {
        var primaryKey = dbContext.Model.FindEntityType(typeof(TEntity))!.FindPrimaryKey()!.Properties
            .FirstOrDefault();
        if (primaryKey == null)
        {
            return null;
        }

        var keyProperty = typeof(TEntity).GetProperty(primaryKey.Name);

        if (keyProperty == null)
        {
            return null;
        }

        // Create entity => portion of lambda expression
        var parameter = Expression.Parameter(typeof(TEntity), "entity");

        // create entity.Id portion of lambda expression
        var property = Expression.Property(parameter, keyProperty.Name);

        // create 'id' portion of lambda expression
        var equalsTo = Expression.Constant(id);

        // create entity.Id == 'id' portion of lambda expression
        var equality = Expression.Equal(property, equalsTo);

        // finally create entire expression - entity => entity.Id == 'id'
        var retVal = Expression.Lambda<Func<TEntity, bool>>(equality, new[] { parameter });
        return retVal;
    }
}