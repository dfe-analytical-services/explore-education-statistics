using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests;

[Collection(CacheTestFixture.CollectionName)]
public class FunctionsIntegrationTest : 
    CacheServiceTestFixture,
    IClassFixture<FunctionsIntegrationTestFixture>,
    IClassFixture<CacheTestFixture>
{
    protected readonly DataFixture DataFixture = new();
    private readonly IHost _host;
    
    protected FunctionsIntegrationTest(FunctionsIntegrationTestFixture fixture)
    {
        _host = fixture.Host;
    }

    protected async Task AddTestData<TDbContext>(Action<TDbContext> supplier) where TDbContext : DbContext
    {
        await using var context = GetDbContext<TDbContext>();

        supplier.Invoke(context);
        await context.SaveChangesAsync();
    }

    protected async Task EnsureDatabaseDeleted<TDbContext>() where TDbContext : DbContext
    {
        await using var context = GetDbContext<TDbContext>();
        await context.Database.EnsureDeletedAsync();
    }

    protected TDbContext GetDbContext<TDbContext>() where TDbContext : DbContext
    {
        var scope = _host.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TDbContext>();
    }

    protected TService GetRequiredService<TService>()
    {
        return _host.Services.GetRequiredService<TService>();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class FunctionsIntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16.1-alpine")
        .Build();

    internal IHost Host;
    
    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        Host = ConfigureHost();
    }
    
    private IHost ConfigureHost()
    {
        return new HostBuilder()
            .ConfigureAppConfiguration((hostContext, _) => 
                hostContext.HostingEnvironment.EnvironmentName = HostEnvironmentExtensions.IntegrationTestEnvironment
            )
            .ConfigureHostBuilder()
            .ConfigureServices(services =>
            {
                services.UseInMemoryDbContext<ContentDbContext>();

                services.AddDbContext<PublicDataDbContext>(
                    options => options.UseNpgsql(_postgreSqlContainer.GetConnectionString()));

                using var serviceScope = services.BuildServiceProvider()
                    .GetRequiredService<IServiceScopeFactory>()
                    .CreateScope();

                using var context = serviceScope.ServiceProvider.GetRequiredService<PublicDataDbContext>();
                context.Database.Migrate();
            })
            .Build();
    }
}
