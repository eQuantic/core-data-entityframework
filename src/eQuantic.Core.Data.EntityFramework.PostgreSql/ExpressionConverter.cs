#if NET7_0_OR_GREATER && !NET10_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;

namespace eQuantic.Core.Data.EntityFramework.PostgreSql;

internal class ExpressionConverter<TEntity>
{
    public static Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> ConvertExpression(
        Expression<Func<TEntity, TEntity>> updateExpression)
    {
        ArgumentNullException.ThrowIfNull(updateExpression);

        var parameter = Expression.Parameter(typeof(SetPropertyCalls<TEntity>), "e");
        var methodCalls = new ExpressionRewriter<TEntity>(parameter).Rewrite(updateExpression.Body);
        var block = methodCalls.Last();
        
        return Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(block, parameter);
    }

    private class ExpressionRewriter<T>(ParameterExpression parameter) : ExpressionVisitor
    {
        private readonly List<Expression> _setPropertyCalls = [];

        public IEnumerable<Expression> Rewrite(Expression body)
        {
            if (body is not MemberInitExpression memberInit)
                throw new NotSupportedException("Only MemberInit expressions are supported.");

            MethodCallExpression currentExpression = null;
            foreach (var binding in memberInit.Bindings.OfType<MemberAssignment>())
            {
                currentExpression = RewriteBinding(binding, currentExpression);
            }

            return _setPropertyCalls;
        }

        private MethodCallExpression RewriteBinding(MemberAssignment binding, MethodCallExpression callExpression = null)
        {
            var propertyInfo = (PropertyInfo)binding.Member;
            var propertyType = propertyInfo.PropertyType;
            var entityParameter = Expression.Parameter(typeof(T), "entity");
            var propertyAccess = Expression.MakeMemberAccess(entityParameter, propertyInfo);
            var propertyLambda = Expression.Lambda(propertyAccess, entityParameter);
            var setPropertyMethod = typeof(SetPropertyCalls<T>)
                .GetMethods()
                .Where(m => m.Name == nameof(SetPropertyCalls<T>.SetProperty) && m.IsGenericMethod)
                .Single(m =>
                {
                    var parameters = m.GetParameters();
                    var genericArgs = m.GetGenericArguments();
                    return genericArgs.Length == 1 &&
                           parameters.Length == 2 &&
                           IsGenericFunc(parameters[0].ParameterType) &&
                           parameters[1].ParameterType == genericArgs[0];
                })
                .MakeGenericMethod(propertyType);
            
            var setPropertyCall = Expression.Call(
                callExpression != null ? callExpression.Reduce() : parameter,
                setPropertyMethod,
                propertyLambda,
                binding.Expression
            );

            _setPropertyCalls.Add(setPropertyCall);
            
            return setPropertyCall;
        }

        private static bool IsGenericFunc(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(Func<,>);
        }
    }
}
#endif
#if NET10_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;

namespace eQuantic.Core.Data.EntityFramework.PostgreSql;

internal class ExpressionConverter<TEntity>
{
    public static Action<UpdateSettersBuilder<TEntity>> ConvertExpression(
        Expression<Func<TEntity, TEntity>> updateExpression)
    {
        ArgumentNullException.ThrowIfNull(updateExpression);

        var parameter = Expression.Parameter(typeof(UpdateSettersBuilder<TEntity>), "e");
        var methodCalls = new ExpressionRewriter<TEntity>(parameter).Rewrite(updateExpression.Body);
        var lastCall = methodCalls.Last();
        var body = lastCall.Type == typeof(void) 
            ? lastCall 
            : Expression.Block(typeof(void), lastCall);
        return Expression.Lambda<Action<UpdateSettersBuilder<TEntity>>>(body, parameter).Compile();
    }

    private class ExpressionRewriter<T>(ParameterExpression parameter) : ExpressionVisitor
    {
        private readonly List<Expression> _setPropertyCalls = [];

        public IEnumerable<Expression> Rewrite(Expression body)
        {
            if (body is not MemberInitExpression memberInit)
                throw new NotSupportedException("Only MemberInit expressions are supported.");

            MethodCallExpression currentExpression = null;
            foreach (var binding in memberInit.Bindings.OfType<MemberAssignment>())
            {
                currentExpression = RewriteBinding(binding, currentExpression);
            }

            return _setPropertyCalls;
        }

        private MethodCallExpression RewriteBinding(MemberAssignment binding, MethodCallExpression callExpression = null)
        {
            var propertyInfo = (PropertyInfo)binding.Member;
            var propertyType = propertyInfo.PropertyType;
    
            var entityParameter = Expression.Parameter(typeof(T), "entity");
            var propertyAccess = Expression.MakeMemberAccess(entityParameter, propertyInfo);
            var propertyLambda = Expression.Lambda(propertyAccess, entityParameter);
            
            var setPropertyMethod = typeof(UpdateSettersBuilder<T>)
                .GetMethods()
                .Where(m => m.Name == nameof(UpdateSettersBuilder<T>.SetProperty) && m.IsGenericMethod)
                .Single(m => 
                {
                    var parameters = m.GetParameters();
                    var genericArgs = m.GetGenericArguments();
                    return genericArgs.Length == 1 &&
                           parameters.Length == 2 && 
                           IsExpressionFunc(parameters[0].ParameterType) &&
                           parameters[1].ParameterType == genericArgs[0];
                })
                .MakeGenericMethod(propertyType);
    
            var setPropertyCall = Expression.Call(
                callExpression != null ? callExpression : parameter,
                setPropertyMethod,
                propertyLambda,
                binding.Expression
            );

            _setPropertyCalls.Add(setPropertyCall);
    
            return setPropertyCall;
        }

        private static bool IsExpressionFunc(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Expression<>))
            {
                var delegateType = type.GetGenericArguments()[0];
                
                return delegateType.IsGenericType && 
                       delegateType.GetGenericTypeDefinition() == typeof(Func<,>);
            }
            return false;
        }
    }
}
#endif