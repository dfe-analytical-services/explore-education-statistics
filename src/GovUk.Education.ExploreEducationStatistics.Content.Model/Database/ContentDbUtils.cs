using System;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Database
{
    public static class ContentDbUtils
    {
        public static ContentDbContext InMemoryContentDbContext(string dbName)
        {
            var builder = new DbContextOptionsBuilder<ContentDbContext>();
            builder.UseInMemoryDatabase(databaseName: dbName);
            return new ContentDbContext(builder.Options);
        }

        public static ContentDbContext InMemoryContentDbContext()
        {
            return InMemoryContentDbContext(Guid.NewGuid().ToString());
        }
    }
}