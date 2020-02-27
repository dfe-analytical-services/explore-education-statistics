using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
{
    /**
     * Provide extension to filter by primary key generically.
     * https://stackoverflow.com/a/55272426
     */
    public static class FindExtensions
    {
        public static IQueryable<TEntity> FindByPrimaryKey<TEntity>(this IQueryable<TEntity> queryable,
            DbContext context, object key) where TEntity : class
        {
            return FindByPrimaryKey(queryable, context, new[] {key});
        }

        public static IQueryable<TEntity> FindByPrimaryKey<TEntity>(this IQueryable<TEntity> queryable,
            DbContext context, object[] key) where TEntity : class
        {
            return queryable.Where(context.FindByPrimaryKeyPredicate<TEntity>(key));
        }

        public static IQueryable<TEntity> FindAll<TEntity>(this IQueryable<TEntity> queryable,
            DbContext dbContext,
            params object[] keyValues) where TEntity : class
        {
            var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey.Properties.Count != 1)
                throw new NotSupportedException("Only a single primary key is supported");

            var pkProperty = primaryKey.Properties[0];
            var pkPropertyType = pkProperty.ClrType;

            foreach (var keyValue in keyValues)
            {
                if (!pkPropertyType.IsAssignableFrom(keyValue.GetType()))
                    throw new ArgumentException($"Key value '{keyValue}' is not of the right type");
            }

            var pkMemberInfo = typeof(TEntity).GetProperty(pkProperty.Name);
            if (pkMemberInfo == null)
                throw new ArgumentException("Type does not contain the primary key as an accessible property");

            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var body = Expression.Call(null, ContainsMethod,
                Expression.Constant(keyValues),
                Expression.Convert(Expression.MakeMemberAccess(parameter, pkMemberInfo), typeof(object)));
            var predicateExpression = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

            return queryable.Where(predicateExpression);
        }

        // TODO Precompile expression so this doesn't happen every time
        private static Expression<Func<T, bool>> FindByPrimaryKeyPredicate<T>(this DbContext dbContext, object[] id)
        {
            var keyProperties = dbContext.GetPrimaryKeyProperties<T>();
            var parameter = Expression.Parameter(typeof(T), "e");
            var body = keyProperties
                .Select((p, i) => Expression.Equal(
                    Expression.Property(parameter, p.Name),
                    Expression.Convert(
                        Expression.PropertyOrField(Expression.Constant(new {id = id[i]}), "id"),
                        p.ClrType)))
                .Aggregate(Expression.AndAlso);
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        private static readonly MethodInfo ContainsMethod = typeof(Enumerable).GetMethods()
            .FirstOrDefault(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(object));

        private static IReadOnlyList<IProperty> GetPrimaryKeyProperties<T>(this DbContext dbContext)
        {
            return dbContext.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties;
        }
    }
}