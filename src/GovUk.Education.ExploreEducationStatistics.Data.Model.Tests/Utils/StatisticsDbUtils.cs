using System;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils
{
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

        public static PublicStatisticsDbContext InMemoryPublicStatisticsDbContext(
            string databaseName,
            bool updateTimestamps = true)
        {
            var builder = new DbContextOptionsBuilder<PublicStatisticsDbContext>();
            builder.UseInMemoryDatabase(databaseName,
                b => b.EnableNullChecks(false));
            return new PublicStatisticsDbContext(builder.Options, null, updateTimestamps);
        }

        public static PublicStatisticsDbContext InMemoryPublicStatisticsDbContext(bool updateTimestamps = true)
        {
            return InMemoryPublicStatisticsDbContext(Guid.NewGuid().ToString(), updateTimestamps);
        }
    }
}
