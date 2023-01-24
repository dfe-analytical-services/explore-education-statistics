using System;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils;

public static class StatisticsDbUtils
{
    public static StatisticsDbContext InMemoryStatisticsDbContext(string databaseName, bool updateTimestamps = true)
    {
        var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
        builder.UseInMemoryDatabase(databaseName,
            b => b.EnableNullChecks(false));
        return new StatisticsDbContext(builder.Options, null, updateTimestamps);
    }

    public static StatisticsDbContext InMemoryStatisticsDbContext(bool updateTimestamps = true)
    {
        return InMemoryStatisticsDbContext(Guid.NewGuid().ToString());
    }

    public static WebApplicationFactory<TEntrypoint> ResetStatisticsDbContext<TEntrypoint>(
        this WebApplicationFactory<TEntrypoint> app
    )
        where TEntrypoint : class
    {
        return app.ResetDbContext<StatisticsDbContext, TEntrypoint>();
    }

    public static WebApplicationFactory<TEntrypoint> AddStatisticsDbTestData<TEntrypoint>(
        this WebApplicationFactory<TEntrypoint> app,
        Action<StatisticsDbContext> testData
    )
        where TEntrypoint : class
    {
        return app.AddTestData(testData);
    }
}
