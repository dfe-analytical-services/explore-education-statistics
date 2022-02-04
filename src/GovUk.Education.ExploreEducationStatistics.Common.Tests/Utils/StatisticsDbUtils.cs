using System;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils
{
    public static class StatisticsDbUtils
    {
        public static StatisticsDbContext InMemoryStatisticsDbContext(string databaseName)
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(databaseName,
                b => b.EnableNullChecks(false));
            return new StatisticsDbContext(builder.Options, null);
        }

        public static StatisticsDbContext InMemoryStatisticsDbContext()
        {
            return InMemoryStatisticsDbContext(Guid.NewGuid().ToString());
        }

        public static PublicStatisticsDbContext InMemoryPublicStatisticsDbContext(string databaseName)
        {
            var builder = new DbContextOptionsBuilder<PublicStatisticsDbContext>();
            builder.UseInMemoryDatabase(databaseName,
                b => b.EnableNullChecks(false));
            return new PublicStatisticsDbContext(builder.Options, null);
        }

        public static PublicStatisticsDbContext InMemoryPublicStatisticsDbContext()
        {
            return InMemoryPublicStatisticsDbContext(Guid.NewGuid().ToString());
        }
    }
}
