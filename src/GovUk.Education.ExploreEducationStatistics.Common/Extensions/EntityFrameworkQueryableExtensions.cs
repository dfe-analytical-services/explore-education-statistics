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
    /// are then picked up by <see cref="SqlServerQueryOptionsInterceptor"/> and added
    /// to the end of the query.
    ///
    /// In order for the interceptor to take effect, it must be registered
    /// with the matching DbContext at startup using
    /// <see cref="DbContextOptionsBuilderExtensions.EnableSqlServerQueryOptionsInterceptor"/>.
    ///
    /// Note that this can only be used with a SQL Server-specific DbContext. 
    /// </summary>
    /// <param name="source">A SQL Server-based Linq query.</param>
    /// <param name="sqlOptionsString">
    /// The OPTION string to include e.g. OPTION(HASH JOIN).
    /// </param>
    /// <typeparam name="T">The type of the IQueryable.</typeparam>
    /// <returns>
    /// An IQueryable with the additional OPTION hints added as tags /
    /// leading comments.
    /// </returns>
    public static IQueryable<T> WithSqlServerOptions<T>(
        this IQueryable<T> source, string sqlOptionsString) where T : class?
        => source.TagWith($"WithOptions: {sqlOptionsString}");
}
