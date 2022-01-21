using System;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Database
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

        public static ContentDbContext InMemoryContentDbContext(string dbName)
        {
            return new ContentDbContext(InMemoryContentDbContextOptions(dbName));
        }

        public static ContentDbContext InMemoryContentDbContext()
        {
            return new ContentDbContext(InMemoryContentDbContextOptions());
        }
    }
}
