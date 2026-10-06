#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddStorageDataSets_ResolvesStorageDataSetResolver()
    {
        using var scope = BuildServiceProvider().CreateScope();

        Assert.IsType<StorageDataSetResolver>(scope.ServiceProvider.GetRequiredService<IStorageDataSetResolver>());
    }

    [Fact]
    public void AddStorageDataSets_CreatesStatisticsDbDataSet()
    {
        using var scope = BuildServiceProvider().CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<StatisticsDbDataSetFactory>();

        // The data set is created per subject rather than registered, so its collaborators are only checked here
        Assert.IsType<StatisticsDbDataSet>(factory.Create(Guid.NewGuid()));
    }

    // Only the db contexts and logging are registered, so that resolving proves the extension registers every
    // other collaborator a data set needs
    private static ServiceProvider BuildServiceProvider()
    {
        return new ServiceCollection()
            .AddLogging()
            .AddScoped(_ => InMemoryContentDbContext())
            .AddScoped(_ => InMemoryStatisticsDbContext())
            .AddStorageDataSets()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }
}
