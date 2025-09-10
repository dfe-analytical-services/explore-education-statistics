#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class EntityFrameworkQueryableExtensions
{
    /// <summary>
    /// Allows Linq queries to add SQL "OPTIONS()" hints to inform SQL Server
    /// execution plans of preferred execution paths.
    ///
    /// The hints are added as comments to the generated SQL statement, which
    /// are then picked up by <see cref="QueryOptionsInterceptor"/> and added
    /// to the end of the query.
    /// </summary>
    /// <param name="source">A SQL Server-based Linq query.</param>
    /// <param name="sqlOptionsString">
    /// The OPTION string to include e.g. OPTION(HASH JOIN).
    /// </param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IQueryable<T> WithSqlOptions<T>(
        this IQueryable<T> source, string sqlOptionsString) where T : class?
        => source.TagWith($"WithOptions: {sqlOptionsString}");
}
