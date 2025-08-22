using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

/**
 * Provide extension to filter by primary key generically.
 * https://stackoverflow.com/a/55272426
 */
public static class FindExtensions
{
    public static IQueryable<TEntity> FindByPrimaryKey<TEntity>(this IQueryable<TEntity> queryable,
        DbContext context, object key) where TEntity : class
    {
        return FindByPrimaryKey(queryable, context, new[] { key });
    }

    public static IQueryable<TEntity> FindByPrimaryKey<TEntity>(this IQueryable<TEntity> queryable,
        DbContext context, object[] key) where TEntity : class
    {
        return queryable.Where(context.FindByPrimaryKeyPredicate<TEntity>(key));
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
                    Expression.PropertyOrField(Expression.Constant(new { id = id[i] }), "id"),
                    p.ClrType)))
            .Aggregate(Expression.AndAlso);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static IReadOnlyList<IProperty> GetPrimaryKeyProperties<T>(this DbContext dbContext)
    {
        return dbContext.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties;
    }
}
