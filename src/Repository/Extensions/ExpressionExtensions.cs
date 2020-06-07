using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using eQuantic.Core.Linq.Extensions;
using eQuantic.Core.Linq.Helpers;

namespace eQuantic.Core.Data.EntityFramework.Repository.Extensions
{
    /// <summary>
    /// Expression Extensions
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Gets the property names.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="expressions">The expressions.</param>
        /// <returns></returns>
        public static string[] GetPropertyNames<TEntity>(this Expression<Func<TEntity, object>>[] expressions)
        {
            var columnNames = new List<string>();
            foreach (var expression in expressions)
            {
                var member = expression.Body as MemberExpression;
                if (member == null)
                {
                    var op = ((UnaryExpression)expression.Body).Operand;
                    member = (MemberExpression)op;
                }
                columnNames.Add(PropertiesHelper.BuildColumnNameFromMemberExpression(member));
            }
            return columnNames.ToArray();
        }
    }
}