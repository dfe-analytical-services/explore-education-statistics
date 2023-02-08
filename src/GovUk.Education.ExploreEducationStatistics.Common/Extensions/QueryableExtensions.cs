#nullable enable
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class QueryableExtensions
{
    public static Task<Either<ActionResult, T>> FirstOrNotFound<T>(
        this IQueryable<T> source) where T : class?
        => source.FirstOrDefault().OrNotFound();

    public static Task<Either<ActionResult, T>> FirstOrNotFound<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>> predicate) where T : class?
        => source.FirstOrDefault(predicate).OrNotFound();

    public static Task<Either<ActionResult, T>> FirstOrNotFoundAsync<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) where T : class?
        => source.FirstOrDefaultAsync(predicate, cancellationToken).OrNotFound();

    public static Task<Either<ActionResult, T>> FirstOrNotFoundAsync<T>(
        this IQueryable<T> source,
        CancellationToken cancellationToken = default) where T : class?
        => source.FirstOrDefaultAsync(cancellationToken).OrNotFound();

    public static Task<Either<ActionResult, T>> SingleOrNotFound<T>(
        this IQueryable<T> source) where T : class?
        => source.SingleOrDefault().OrNotFound();

    public static Task<Either<ActionResult, T>> SingleOrNotFound<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>> predicate) where T : class?
        => source.SingleOrDefault(predicate).OrNotFound();

    public static Task<Either<ActionResult, T>> SingleOrNotFoundAsync<T>(
        this IQueryable<T> source,
        CancellationToken cancellationToken = default) where T : class?
        => source.SingleOrDefaultAsync(cancellationToken).OrNotFound();

    public static Task<Either<ActionResult, T>> SingleOrNotFoundAsync<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) where T : class?
        => source.SingleOrDefaultAsync(predicate, cancellationToken).OrNotFound();
}
