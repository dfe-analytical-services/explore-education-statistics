#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Database;

public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Register an interceptor to convert requested OPTION hints into
    /// an actual OPTION clause for SQL queries.
    ///
    /// OPTION hints can be requested for specific queries by using the
    /// <see cref="EfQueryableExtensions.WithSqlServerOptions{T}"/> method.
    ///
    /// Note that this is SQL Server-specific.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static DbContextOptionsBuilder EnableSqlServerQueryOptionsInterceptor(this DbContextOptionsBuilder builder)
    {
        var interceptor = new SqlServerQueryOptionsInterceptor(new SqlServerQueryOptionsInterceptorSqlProcessor());
        return builder.AddInterceptors(interceptor);
    }
}
