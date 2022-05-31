using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public static class DbUtils
    {
        public static ContentDbContext InMemoryApplicationDbContext(string dbName) => InMemoryContentDbContext(dbName);

        public static ContentDbContext InMemoryApplicationDbContext()
        {
            return InMemoryContentDbContext();
        }

        public static UsersAndRolesDbContext InMemoryUserAndRolesDbContext(string dbName, bool updateTimestamps = true)
        {
            var builder = new DbContextOptionsBuilder<UsersAndRolesDbContext>();
            builder.UseInMemoryDatabase(databaseName: dbName, b => b.EnableNullChecks(false));

            var operationalStoreOptions = Options.Create(new OperationalStoreOptions());
            return new UsersAndRolesDbContext(builder.Options, operationalStoreOptions, updateTimestamps);
        }

        public static UsersAndRolesDbContext InMemoryUserAndRolesDbContext(bool updateTimestamps = true)
        {
            return InMemoryUserAndRolesDbContext(Guid.NewGuid().ToString(), updateTimestamps);
        }
    }
}
