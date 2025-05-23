#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Extensions.Hosting;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures;

public sealed class TestApplicationFactory : TestApplicationFactory<Startup>
{
    public async Task ClearAllTestData()
    {
        await EnsureDatabaseDeleted<ContentDbContext>();
        await EnsureDatabaseDeleted<StatisticsDbContext>();
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return base
            .CreateHostBuilder()
            .ConfigureServices(services =>
            {
                services
                    .UseInMemoryDbContext<ContentDbContext>()
                    .UseInMemoryDbContext<StatisticsDbContext>()
                    .ReplaceService(new Mock<IPublicBlobStorageService>())
                    .ReplaceService(new Mock<IAnalyticsPathResolver>());
            });
    }
}
