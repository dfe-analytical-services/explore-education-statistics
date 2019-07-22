using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public static class DbUtils
    {
        public static ApplicationDbContext InMemoryApplicationDbContext(string dbName)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase(databaseName: dbName);
            var options = builder.Options;
            return new ApplicationDbContext(options);
        }   
    }
}