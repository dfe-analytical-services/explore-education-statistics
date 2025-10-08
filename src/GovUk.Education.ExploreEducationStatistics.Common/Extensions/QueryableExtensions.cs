#nullable enable
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class QueryableExtensions
{
    public static Task<Either<ActionResult, T>> FirstOrNotFound<T>(this IQueryable<T> source)
        where T : class? => source.FirstOrDefault().OrNotFound();

    public static Task<Either<ActionResult, T>> FirstOrNotFound<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>> predicate
    )
        where T : class? => source.FirstOrDefault(predicate).OrNotFound();

    public static Task<Either<ActionResult, T>> FirstOrNotFoundAsync<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default
    )
        where T : class? => source.FirstOrDefaultAsync(predicate, cancellationToken).OrNotFound();

    public static Task<Either<ActionResult, T>> FirstOrNotFoundAsync<T>(
        this IQueryable<T> source,
        CancellationToken cancellationToken = default
    )
        where T : class? => source.FirstOrDefaultAsync(cancellationToken).OrNotFound();

    public static Task<Either<ActionResult, T>> SingleOrNotFound<T>(this IQueryable<T> source)
        where T : class? => source.SingleOrDefault().OrNotFound();

    public static Task<Either<ActionResult, T>> SingleOrNotFound<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>> predicate
    )
        where T : class? => source.SingleOrDefault(predicate).OrNotFound();

    public static Task<Either<ActionResult, T>> SingleOrNotFoundAsync<T>(
        this IQueryable<T> source,
        CancellationToken cancellationToken = default
    )
        where T : class? => source.SingleOrDefaultAsync(cancellationToken).OrNotFound();

    public static Task<Either<ActionResult, T>> SingleOrNotFoundAsync<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default
    )
        where T : class? => source.SingleOrDefaultAsync(predicate, cancellationToken).OrNotFound();

    /// <summary>
    /// Apply offset pagination to a queryable.
    /// </summary>
    /// <param name="source">An IQueryable&lt;out T&gt; to return elements from</param>
    /// <param name="page">The page number which is used to calculate the offset</param>
    /// <param name="pageSize">The number of elements that should be returned</param>
    /// <typeparam name="T">The type of the data in the data source.</typeparam>
    /// <returns>An IQueryable&lt;out T&gt; that contains elements that occur after the specified index in the input sequence.</returns>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> source, int page, int pageSize)
    {
        return source.Skip((page - 1) * pageSize).Take(pageSize);
    }

    /// <summary>
    /// Facilitate declaring an optional Where clause inline.
    /// </summary>
    /// <param name="source">An IQueryable&lt;out T&gt; to return elements from</param>
    /// <param name="predicate">If the predicate resolves to true, the Where clause will be appended to the query</param>
    /// <typeparam name="T">The type of the data in the data source.</typeparam>
    /// <returns>An IQueryable&lt;out T&gt; optionally filtered by the specified Where clause depending on the value of predicate.</returns>
    public static ConditionalQueryable<T> If<T>(this IQueryable<T> source, bool predicate)
        where T : class => new(source, predicate);

    public class ConditionalQueryable<T>(IQueryable<T> source, bool predicate)
        where T : class
    {
        public IQueryable<T> Include<TProperty>(Expression<Func<T, TProperty>> includeClause) =>
            predicate ? source.Include(includeClause) : source;

        public IQueryable<T> ThenWhere(Expression<Func<T, bool>> whereClause) =>
            predicate ? source.Where(whereClause) : source;
    }
}
