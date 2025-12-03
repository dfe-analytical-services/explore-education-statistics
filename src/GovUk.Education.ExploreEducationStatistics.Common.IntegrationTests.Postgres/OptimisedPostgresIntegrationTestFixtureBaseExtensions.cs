using Testcontainers.PostgreSql;

namespace GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.Postgres;

public static class OptimisedAzuriteIntegrationTestFixtureBaseExtensions
{
    public static string CreatePostgreSqlContainer(this OptimisedIntegrationTestFixtureBase fixture)
    {
        var container = new PostgreSqlBuilder().WithImage("postgres:16.1-alpine").Build();
        fixture.AddContainer(container);
        return container.GetConnectionString();
    }
}
