#nullable enable
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

public class TestApplicationFactory : TestApplicationFactory<TestStartup, WebAppFactory, TestApplicationFactory>
{
    public async Task ClearTestData<TDbContext>() where TDbContext : DbContext
    {
        await using var context = GetDbContext<TDbContext>();

        var tables = context.Model.GetEntityTypes()
            .Select(type => type.GetTableName())
            .Distinct()
            .ToList();

        foreach (var table in tables)
        {
            await context.Database.ExecuteSqlRawAsync(@$"TRUNCATE TABLE ""{table}"" RESTART IDENTITY CASCADE;");
        }
    }

    public TestApplicationFactory SetUser(ClaimsPrincipal user)
    {
        return ConfigureServices(services => services.AddScoped(_ => user));
    }
}

sealed class WebApplicationFactory : TestApplicationFactory<TestStartup, WebApplicationFactory>.WebApplicationFactory
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

    protected override IHostBuilder CreateHostBuilder()
    {
        return base
            .CreateHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddDbContext<PublicDataDbContext>(
                    options => options.UseNpgsql(_postgreSqlContainer.GetConnectionString()));

                using var serviceScope = services.BuildServiceProvider()
                    .GetRequiredService<IServiceScopeFactory>()
                    .CreateScope();

                using var context = serviceScope.ServiceProvider.GetRequiredService<PublicDataDbContext>();
                context.Database.Migrate();
            });
    }
}
