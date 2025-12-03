using Testcontainers.PostgreSql;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Postgres;

public static class OptimisedPostgreSqlIntegrationTestFixtureBaseExtensions
{
    public static Func<string> RegisterPostgreSqlContainer(this TestContainerRegistrations registrations)
    {
        var container = new PostgreSqlBuilder().WithImage("postgres:16.1-alpine").Build();
        registrations.RegisterContainer(container);
        return () => container.GetConnectionString();
    }
}
