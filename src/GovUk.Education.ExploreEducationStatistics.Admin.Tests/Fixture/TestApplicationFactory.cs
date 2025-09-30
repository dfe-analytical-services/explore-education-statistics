#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

public class TestApplicationFactory : TestApplicationFactory<TestStartup>
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16.1-alpine")
        .Build();

    public async Task Initialize()
    {
        await _postgreSqlContainer.StartAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    public async Task ClearTestData<TDbContext>()
        where TDbContext : DbContext
    {
        var context = GetDbContext<TDbContext>();
        await context.ClearTestData();
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return base.CreateHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .UseInMemoryDbContext<ContentDbContext>()
                    .UseInMemoryDbContext<StatisticsDbContext>()
                    .AddDbContext<PublicDataDbContext>(options =>
                    {
                        options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
                    });

                using var serviceScope = services
                    .BuildServiceProvider()
                    .GetRequiredService<IServiceScopeFactory>()
                    .CreateScope();

                using var context =
                    serviceScope.ServiceProvider.GetRequiredService<PublicDataDbContext>();
                context.Database.Migrate();
            });
    }
}
