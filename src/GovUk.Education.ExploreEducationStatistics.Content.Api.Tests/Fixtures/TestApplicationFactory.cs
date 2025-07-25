#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures;

public sealed class TestApplicationFactory : TestApplicationFactory<Startup>
{
    private readonly HashSet<string> _additionalAppsettingsFiles = new();

    public async Task ClearAllTestData()
    {
        await EnsureDatabaseDeleted<ContentDbContext>();
        await EnsureDatabaseDeleted<StatisticsDbContext>();
    }

    public TestApplicationFactory AddAppSettings(string filename)
    {
        _additionalAppsettingsFiles.Add(filename);
        return this;
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return base
            .CreateHostBuilder()
            .ConfigureAppConfiguration((_, builder) =>
            {
                var configuration = new ConfigurationBuilder();

                _additionalAppsettingsFiles.ForEach(settingsFile =>
                {
                    configuration.AddJsonFile(
                        Path.Combine(Assembly.GetExecutingAssembly().GetDirectoryPath(),
                            settingsFile), optional: false);
                });

                builder.AddConfiguration(configuration.Build());
            })
            .ConfigureServices(services =>
            {
                services
                    .UseInMemoryDbContext<ContentDbContext>()
                    .UseInMemoryDbContext<StatisticsDbContext>()
                    .ReplaceService(new Mock<IPublicBlobStorageService>())
                    .ReplaceService(new Mock<IAnalyticsPathResolver>(), optional: true);
            });
    }
}
