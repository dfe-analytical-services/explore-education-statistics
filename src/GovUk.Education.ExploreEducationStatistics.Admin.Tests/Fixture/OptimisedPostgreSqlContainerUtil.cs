using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

public class OptimisedPostgreSqlContainerUtil
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16.1-alpine")
        .WithReuse(true)
        .Build();

    public async Task Start()
    {
        await _postgreSqlContainer.StartAsync();
    }

    public PostgreSqlContainer GetContainer()
    {
        return _postgreSqlContainer;
    }

    public async Task Stop()
    {
        await _postgreSqlContainer.StopAsync();
    }

    public PublicDataDbContext GetDbContext()
    {
        var services = new ServiceCollection();
        services.AddDbContext<PublicDataDbContext>(
            options => options.UseNpgsql(_postgreSqlContainer.GetConnectionString()));
        return services.BuildServiceProvider().GetRequiredService<PublicDataDbContext>();
    }
}
