using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils
{
    public static class ContentDbUtils
    {
        public static DbContextOptions<ContentDbContext> InMemoryContentDbContextOptions(string databaseName)
        {
            var builder = new DbContextOptionsBuilder<ContentDbContext>()
                .UseInMemoryDatabase(databaseName,
                    b => b.EnableNullChecks(false));

            return builder.Options;
        }

        public static DbContextOptions<ContentDbContext> InMemoryContentDbContextOptions()
        {
            return InMemoryContentDbContextOptions(Guid.NewGuid().ToString());
        }

        public static ContentDbContext InMemoryContentDbContext(string databaseName, bool updateTimestamps = true)
        {
            return new ContentDbContext(InMemoryContentDbContextOptions(databaseName), updateTimestamps);
        }

        public static ContentDbContext InMemoryContentDbContext(bool updateTimestamps = true)
        {
            return new ContentDbContext(InMemoryContentDbContextOptions(), updateTimestamps);
        }
    }
}
