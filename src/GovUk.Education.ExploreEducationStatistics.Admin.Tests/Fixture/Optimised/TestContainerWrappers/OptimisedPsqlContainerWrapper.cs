using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised.TestContainerWrappers;

/// <summary>
/// This is a convenience wrapper around a PSQL TestContainer, to allow clearer visibility of the management and
/// lifecycle of test containers.
/// </summary>
public class OptimisedPsqlContainerWrapper
{
    private readonly PostgreSqlContainer _psqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16.1-alpine")
        .Build();

    internal PostgreSqlContainer GetContainer()
    {
        return _psqlContainer;
    }

    /// <summary>
    /// Start a test container if one is not already running and available.
    /// </summary>
    internal async Task Start()
    {
        await _psqlContainer.StartAsync();
    }

    /// <summary>
    /// Stop a test container if one is running.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    internal async Task Stop()
    {
        await _psqlContainer.StopAsync();
    }

    /// <summary>
    ///
    /// Allows a direct DbContext for interacting with the test container, separate from any WebApplicationFactory
    /// being used for testing.
    ///
    /// </summary>
    /// TODO EES-6450 - we're currently using reusable DbContexts direct from the WebApplicationFactory under test
    /// for interacting with the databases, which is working fine. Consider removing this, or switching over to doing
    /// narrower lookups of DbContexts like this rather than looking up from the factory for test data
    /// setup / assertions.
    // ReSharper disable once UnusedMember.Global
    public PublicDataDbContext GetPublicDataDbContext()
    {
        var services = new ServiceCollection();
        services.AddDbContext<PublicDataDbContext>(options => options.UseNpgsql(_psqlContainer.GetConnectionString()));
        return services.BuildServiceProvider().GetRequiredService<PublicDataDbContext>();
    }
}
