using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Reflection;
using WebAPI.Data;

namespace WebAPI.Infrastructure.Interceptors;

// The idea is coming from this package: https://github.com/rjperes/EFSoftDeletes/blob/master/Interceptors/SoftDeleteExecuteDeleteInterceptor.cs
// This Interceptor can intercept the ExecuteDelete in MemberEndpoints.deleteMemberById
public sealed class BaseEntityExecuteDeleteInterceptor : IQueryExpressionInterceptor
{
    public Expression QueryCompilationStarting(Expression queryExpression, QueryExpressionEventData eventData)
    {
        return new SoftDeletableQueryExpressionVisitor().Visit(queryExpression);
    }

    class SoftDeletableQueryExpressionVisitor : ExpressionVisitor
    {
        private const string _isDeletedProperty = nameof(BaseEntity.IsDeleted);

        private static readonly MethodInfo _executeDeleteMethod = typeof(EntityFrameworkQueryableExtensions).GetMethod(nameof(EntityFrameworkQueryableExtensions.ExecuteDelete), BindingFlags.Public | BindingFlags.Static)!;
        private static readonly MethodInfo _executeUpdateMethod = typeof(EntityFrameworkQueryableExtensions).GetMethod(nameof(EntityFrameworkQueryableExtensions.ExecuteUpdate), BindingFlags.Public | BindingFlags.Static)!;
        private static readonly MethodInfo _propertyMethod      = typeof(EF).GetMethod(nameof(EF.Property), BindingFlags.Static | BindingFlags.Public)!;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.IsGenericMethod && node.Method.GetGenericMethodDefinition() == _executeDeleteMethod)
            {
                Type entityType = node.Method.GetGenericArguments()[0];

                bool isSoftDeletable = typeof(BaseEntity).IsAssignableFrom(entityType);

                if (isSoftDeletable)
                {
                    MethodInfo setPropertyMethod = typeof(SetPropertyCalls<>).MakeGenericType(entityType).GetMethods().Single(m =>
                        m.Name == nameof(SetPropertyCalls<object>.SetProperty) &&
                        m.IsGenericMethod &&
                        m.GetGenericArguments().Length == 1 &&
                        m.GetParameters().Length == 2 &&
                        m.GetParameters()[1].ParameterType.IsGenericMethodParameter &&
                        m.GetParameters()[1].Name == "valueExpression"
                    )
                    .MakeGenericMethod(typeof(bool));

                    var parameter = Expression.Parameter(entityType, "p");

                    var propertyCall = Expression.Call(null, _propertyMethod.MakeGenericMethod(typeof(bool)), parameter, Expression.Constant(_isDeletedProperty));

                    var propertyCallLambda = Expression.Lambda(propertyCall, parameter);

                    var setterParameter = Expression.Parameter(typeof(SetPropertyCalls<>).MakeGenericType(entityType), "setters");

                    var setPropertyCall = Expression.Call(setterParameter, setPropertyMethod, propertyCallLambda, Expression.Constant(true));

                    var lambda = Expression.Lambda(setPropertyCall, setterParameter);

                    return Expression.Call(node.Object, _executeUpdateMethod.MakeGenericMethod(entityType), node.Arguments[0], lambda);
                }
            }

            return base.VisitMethodCall(node);
        }
    }
}
