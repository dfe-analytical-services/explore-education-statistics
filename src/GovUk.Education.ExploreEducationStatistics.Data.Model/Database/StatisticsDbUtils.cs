﻿using System;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Database
{
    public static class StatisticsDbUtils
    {
        public static StatisticsDbContext InMemoryStatisticsDbContext(string dbName)
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(databaseName: dbName);
            return new StatisticsDbContext(builder.Options, null);
        }

        public static StatisticsDbContext InMemoryStatisticsDbContext()
        {
            return InMemoryStatisticsDbContext(Guid.NewGuid().ToString());
        }

        public static PublicStatisticsDbContext InMemoryPublicStatisticsDbContext(string dbName)
        {
            var builder = new DbContextOptionsBuilder<PublicStatisticsDbContext>();
            builder.UseInMemoryDatabase(databaseName: dbName);
            return new PublicStatisticsDbContext(builder.Options, null);
        }

        public static PublicStatisticsDbContext InMemoryPublicStatisticsDbContext()
        {
            return InMemoryPublicStatisticsDbContext(Guid.NewGuid().ToString());
        }
    }
}