#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Functions.ConnectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class DbContextSupplier : IDbContextSupplier
{
    public TDbContext CreateDbContext<TDbContext>() where TDbContext : DbContext
    {
        return (typeof(TDbContext).Name switch
        {
            nameof(ContentDbContext) => CreateContentDbContext() as TDbContext,
            nameof(StatisticsDbContext) => CreateStatisticsDbContext() as TDbContext,
            _ => throw new ArgumentOutOfRangeException("Unable to provide DbContext of type " + 
                                                       typeof(TDbContext).Name)
        })!;
    }

    public TDbContext CreateDbContextDelegate<TDbContext>() where TDbContext : DbContext
    {
        return (typeof(TDbContext).Name switch
        {
            nameof(ContentDbContext) => CreateContentDbContextDelegate() as TDbContext,
            nameof(StatisticsDbContext) => CreateStatisticsDbContextDelegate() as TDbContext,
            _ => throw new ArgumentException($"Unable to provide DbContext delegate of type " +
                                             $"{typeof(TDbContext).Name}")
        })!;
    }

    private ContentDbContext CreateContentDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ContentDbContext>();
        optionsBuilder.UseSqlServer(
            GetAzureSqlConnectionString("ContentDb"),
            providerOptions => providerOptions.EnableCustomRetryOnFailure());
        return new ContentDbContext(optionsBuilder.Options);
    }

    private static StatisticsDbContext CreateStatisticsDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<StatisticsDbContext>();
        optionsBuilder.UseSqlServer(
            GetAzureSqlConnectionString("StatisticsDb"),
            providerOptions => providerOptions.EnableCustomRetryOnFailure());
        return new StatisticsDbContext(optionsBuilder.Options);
    }

    private static ContentDbContext CreateContentDbContextDelegate()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ContentDbContext>();
        optionsBuilder.UseSqlServer(
            GetAzureSqlConnectionString("ContentDb"));
        return new ContentDbContext(optionsBuilder.Options);
    }

    private static StatisticsDbContext CreateStatisticsDbContextDelegate()
    {
        var optionsBuilder = new DbContextOptionsBuilder<StatisticsDbContext>();
        optionsBuilder.UseSqlServer(
            GetAzureSqlConnectionString("StatisticsDb"));
        return new StatisticsDbContext(optionsBuilder.Options);
    }
}