using System;
using System.Linq;
using System.Linq.Expressions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Extensions;

public static class QueryableExtensions
{
    public static ConditionalQueryable<TSource> If<TSource>(this IQueryable<TSource> query, bool predicate) => new(query, predicate);
    
    public class ConditionalQueryable<TSource>(IQueryable<TSource> query, bool predicate)
    {
        public IQueryable<TSource> ThenWhere(Expression<Func<TSource, bool>> whereClause) =>
            predicate
                ? query.Where(whereClause) 
                : query;
    }
}
