using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Utils
{
    public class DbUtils
    {
        public static StatisticsDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<StatisticsDbContext>();
            optionsBuilder.UseSqlServer(ConnectionUtils.GetAzureSqlConnectionString("StatisticsDb"),
                providerOptions => providerOptions.EnableRetryOnFailure());

            return new StatisticsDbContext(optionsBuilder.Options);
        }
    }
}