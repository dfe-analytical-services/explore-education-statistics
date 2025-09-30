#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public static class DbUtils
{
    public static ContentDbContext InMemoryApplicationDbContext(
        string dbName,
        bool updateTimestamps = true
    )
    {
        return InMemoryContentDbContext(dbName, updateTimestamps);
    }

    public static ContentDbContext InMemoryApplicationDbContext(bool updateTimestamps = true)
    {
        return InMemoryContentDbContext(updateTimestamps);
    }

    public static UsersAndRolesDbContext InMemoryUserAndRolesDbContext(
        string dbName,
        bool updateTimestamps = true
    )
    {
        var builder = new DbContextOptionsBuilder<UsersAndRolesDbContext>();
        builder.UseInMemoryDatabase(databaseName: dbName, b => b.EnableNullChecks(false));
        return new UsersAndRolesDbContext(builder.Options, updateTimestamps);
    }

    public static UsersAndRolesDbContext InMemoryUserAndRolesDbContext(bool updateTimestamps = true)
    {
        return InMemoryUserAndRolesDbContext(Guid.NewGuid().ToString(), updateTimestamps);
    }
}
