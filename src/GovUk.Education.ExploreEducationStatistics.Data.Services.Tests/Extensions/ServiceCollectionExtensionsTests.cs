#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddStorageDataSets_ResolvesStorageDataSetResolver()
    {
        // Only the db contexts and logging are registered, so that ValidateOnBuild proves the extension registers
        // every other collaborator a data set needs
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddScoped(_ => InMemoryContentDbContext())
            .AddScoped(_ => InMemoryStatisticsDbContext())
            .AddStorageDataSets()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

        using var scope = serviceProvider.CreateScope();

        Assert.IsType<StorageDataSetResolver>(scope.ServiceProvider.GetRequiredService<IStorageDataSetResolver>());
    }
}
