using System;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class DbContextSupplier : IDbContextSupplier
{
    /// <summary>
    /// DbContextOptions that are unique per Function job. It is therefore a transient dependency in CI, and
    /// thus this component must also be a transient DI dependency for other services to use.
    /// </summary>
    private readonly DbContextOptions<ContentDbContext> _contentDbContextOptions;

    public DbContextSupplier(DbContextOptions<ContentDbContext> contentDbContextOptions)
    {
        _contentDbContextOptions = contentDbContextOptions;
    }
    
    public TDbContext CreateDbContext<TDbContext>() where TDbContext : DbContext
    {
        return typeof(TDbContext).Name switch
        {
            nameof(ContentDbContext) => CreateContentDbContext() as TDbContext,
            nameof(StatisticsDbContext) => CreateStatisticsDbContext() as TDbContext,
            _ => throw new ArgumentOutOfRangeException("Unable to provide DbContext of type " + 
                                                       typeof(TDbContext).Name)
        };
    }

    public TDbContext CreateDbContextDelegate<TDbContext>() where TDbContext : DbContext
    {
        return typeof(TDbContext).Name switch
        {
            nameof(ContentDbContext) => CreateContentDbContextDelegate() as TDbContext,
            nameof(StatisticsDbContext) => CreateStatisticsDbContextDelegate() as TDbContext,
            _ => throw new ArgumentException($"Unable to provide DbContext delegate of type " +
                                             $"{typeof(TDbContext).Name}")
        };
    }

    public ContentDbContext CreateContentDbContext()
    {
        return new ContentDbContext(_contentDbContextOptions);
    }

    public StatisticsDbContext CreateStatisticsDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<StatisticsDbContext>();
        optionsBuilder.UseSqlServer(ConnectionUtils.GetAzureSqlConnectionString("StatisticsDb"),
            providerOptions => providerOptions.EnableRetryOnFailure());
        return new StatisticsDbContext(optionsBuilder.Options);
    }
    
    public ContentDbContext CreateContentDbContextDelegate()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ContentDbContext>();
        optionsBuilder.UseSqlServer(ConnectionUtils.GetAzureSqlConnectionString("ContentDb"));
        return new ContentDbContext(optionsBuilder.Options);
    }

    public StatisticsDbContext CreateStatisticsDbContextDelegate()
    {
        var optionsBuilder = new DbContextOptionsBuilder<StatisticsDbContext>();
        optionsBuilder.UseSqlServer(ConnectionUtils.GetAzureSqlConnectionString("StatisticsDb"));
        return new StatisticsDbContext(optionsBuilder.Options);
    }
}