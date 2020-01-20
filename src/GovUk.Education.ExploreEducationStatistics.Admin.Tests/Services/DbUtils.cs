using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public static class DbUtils
    {
        public static ContentDbContext InMemoryApplicationDbContext(string dbName)
        {
            var builder = new DbContextOptionsBuilder<ContentDbContext>();
            builder.UseInMemoryDatabase(databaseName: dbName);
            var options = builder.Options;
            return new ContentDbContext(options);
        }   
        
        public static ContentDbContext InMemoryApplicationDbContext()
        {
            return InMemoryApplicationDbContext(Guid.NewGuid().ToString());
        }          
        
        public static StatisticsDbContext InMemoryStatisticsDbContext(string dbName)
        {
            var builder = new DbContextOptionsBuilder<StatisticsDbContext>();
            builder.UseInMemoryDatabase(databaseName: dbName);
            var options = builder.Options;
            return new StatisticsDbContext(options, null);
        }   
        
        public static StatisticsDbContext InMemoryStatisticsDbContext()
        {
            return InMemoryStatisticsDbContext(Guid.NewGuid().ToString());
        }  
    }
}