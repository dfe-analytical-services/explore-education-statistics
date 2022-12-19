#nullable enable
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GovUk.Education.ExploreEducationStatistics.Common.Database;

public static class SqlServerDbContextOptionsBuilderExtensions
{
    public static SqlServerDbContextOptionsBuilder EnableCustomRetryOnFailure(
        this SqlServerDbContextOptionsBuilder builder,
        int? maxRetryCount = null,
        TimeSpan? maxRetryDelay = null)
    {
        return builder.ExecutionStrategy(dependencies =>
            new MessageAwareSqlServerRetryingExecutionStrategy(
                dependencies, 
                maxRetryCount ?? 6, 
                maxRetryDelay ?? TimeSpan.FromSeconds(30)));
    }
}