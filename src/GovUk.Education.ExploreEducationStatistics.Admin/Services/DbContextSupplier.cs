#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class DbContextSupplier(IConfiguration configuration) : IDbContextSupplier
{
    private readonly string _contentDbConnectionString = configuration.GetConnectionString("ContentDb")!;
    private readonly string _statisticsDbConnectionString = configuration.GetConnectionString("StatisticsDb")!;

    public TDbContext CreateDbContext<TDbContext>()
        where TDbContext : DbContext
    {
        return (
            typeof(TDbContext).Name switch
            {
                nameof(ContentDbContext) => CreateContentDbContext() as TDbContext,
                nameof(StatisticsDbContext) => CreateStatisticsDbContext() as TDbContext,
                _ => throw new ArgumentOutOfRangeException(
                    "Unable to provide DbContext of type " + typeof(TDbContext).Name
                ),
            }
        )!;
    }

    public TDbContext CreateDbContextDelegate<TDbContext>()
        where TDbContext : DbContext
    {
        return (
            typeof(TDbContext).Name switch
            {
                nameof(ContentDbContext) => CreateContentDbContextDelegate() as TDbContext,
                nameof(StatisticsDbContext) => CreateStatisticsDbContextDelegate() as TDbContext,
                _ => throw new ArgumentException(
                    $"Unable to provide DbContext delegate of type " + $"{typeof(TDbContext).Name}"
                ),
            }
        )!;
    }

    private ContentDbContext CreateContentDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ContentDbContext>();
        optionsBuilder.UseSqlServer(
            _contentDbConnectionString,
            providerOptions => providerOptions.EnableCustomRetryOnFailure()
        );
        return new ContentDbContext(optionsBuilder.Options);
    }

    private StatisticsDbContext CreateStatisticsDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<StatisticsDbContext>();
        optionsBuilder.UseSqlServer(
            _statisticsDbConnectionString,
            providerOptions => providerOptions.EnableCustomRetryOnFailure()
        );
        return new StatisticsDbContext(optionsBuilder.Options);
    }

    private ContentDbContext CreateContentDbContextDelegate()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ContentDbContext>();
        optionsBuilder.UseSqlServer(_contentDbConnectionString);
        return new ContentDbContext(optionsBuilder.Options);
    }

    private StatisticsDbContext CreateStatisticsDbContextDelegate()
    {
        var optionsBuilder = new DbContextOptionsBuilder<StatisticsDbContext>();
        optionsBuilder.UseSqlServer(_statisticsDbConnectionString);
        return new StatisticsDbContext(optionsBuilder.Options);
    }
}
