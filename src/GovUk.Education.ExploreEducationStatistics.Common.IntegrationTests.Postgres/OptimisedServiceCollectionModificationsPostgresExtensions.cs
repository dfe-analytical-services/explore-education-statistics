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
        // var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TDbContext));
        //
        // if (descriptor != null)
        // {
        //     services.Remove(descriptor);
        // }

        serviceModifications.AddDbContext<TDbContext>(options =>
            options.UseNpgsql(connectionString).EnableSensitiveDataLogging().EnableDetailedErrors()
        );

        // services.AddDbContext<TDbContext>(options =>
        //     options.UseNpgsql(connectionString).EnableSensitiveDataLogging().EnableDetailedErrors()
        // );
        return serviceModifications;
    }
}
