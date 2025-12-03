using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Postgres;

public static class OptimisedWebApplicationFactoryBuilderPostgresExtensions
{
    public static OptimisedWebApplicationFactoryBuilder<TStartup> WithPostgres<TStartup, TDbContext>(
        this OptimisedWebApplicationFactoryBuilder<TStartup> builder,
        string connectionString
    )
        where TStartup : class
        where TDbContext : DbContext
    {
        builder.AddServiceRegistration(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TDbContext));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<TDbContext>(options =>
                options.UseNpgsql(connectionString).EnableSensitiveDataLogging().EnableDetailedErrors()
            );

            using var serviceScope = services
                .BuildServiceProvider()
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();

            using var context = serviceScope.ServiceProvider.GetRequiredService<TDbContext>();
            context.Database.Migrate();
        });

        return builder;
    }
}
