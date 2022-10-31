using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;

public static class DbUtils
{
    public static StatisticsDbContext CreateStatisticsDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<StatisticsDbContext>();
        optionsBuilder.UseSqlServer(ConnectionUtils.GetAzureSqlConnectionString("StatisticsDb"),
            providerOptions => providerOptions.EnableRetryOnFailure());

        return new StatisticsDbContext(optionsBuilder.Options);
    }
}