using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Postgres;

public static class OptimisedServiceCollectionModificationsPostgresExtensions
{
    public static OptimisedServiceAndConfigModifications AddPostgres<TDbContext>(
        this OptimisedServiceAndConfigModifications serviceModifications,
        string connectionString
    )
        where TDbContext : DbContext
    {
        serviceModifications.AddDbContext<TDbContext>(options =>
            options.UseNpgsql(connectionString).EnableSensitiveDataLogging().EnableDetailedErrors()
        );

        return serviceModifications;
    }
}
