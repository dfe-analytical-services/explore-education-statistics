using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;

public class TestApplicationFactory : TestApplicationFactory<Startup>
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

    public async Task ClearTestData<TDbContext>() where TDbContext : DbContext
    {
        await using var context = GetDbContext<TDbContext>();

        var tables = context.Model.GetEntityTypes()
            .Select(type => type.GetTableName())
            .Distinct()
            .Cast<string>()
            .ToList();

        foreach (var table in tables)
        {
            await context.Database.ExecuteSqlRawAsync(@$"TRUNCATE TABLE ""{table}"" RESTART IDENTITY CASCADE;");
        }
    }

    public async Task ClearAllTestData()
    {
        await ClearTestData<PublicDataDbContext>();
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return base
            .CreateHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddDbContext<PublicDataDbContext>(
                    options => options
                        .UseNpgsql(
                            _postgreSqlContainer.GetConnectionString(),
                            psqlOptions => psqlOptions.EnableRetryOnFailure()));
            });
    }
}
