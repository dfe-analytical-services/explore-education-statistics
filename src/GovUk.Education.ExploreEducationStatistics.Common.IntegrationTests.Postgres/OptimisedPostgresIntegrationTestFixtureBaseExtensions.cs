using Testcontainers.PostgreSql;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Postgres;

public static class OptimisedPostgreSqlIntegrationTestFixtureBaseExtensions
{
    public static Func<string> RegisterPostgreSqlContainer<TStartup>(
        this OptimisedIntegrationTestFixtureBase<TStartup> fixture
    )
        where TStartup : class
    {
        var container = new PostgreSqlBuilder().WithImage("postgres:16.1-alpine").Build();
        fixture.AddContainer(container);
        return () => container.GetConnectionString();
    }
}
