#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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

        var dataSet = factory.Create(Guid.NewGuid());
        Assert.IsType<StatisticsDbDataSet>(dataSet);
    }

    [Fact]
    public void AddStorageDataSetWriters_CreatesStatisticsDbDataSetWriter()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddScoped(_ => InMemoryStatisticsDbContext())
            .AddSingleton(Mock.Of<IDatabaseHelper>())
            .AddSingleton(Mock.Of<IGuidGenerator>())
            .AddStorageDataSetWriters()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

        using var scope = serviceProvider.CreateScope();

        Assert.IsType<StorageDataSetWriterResolver>(
            scope.ServiceProvider.GetRequiredService<IStorageDataSetWriterResolver>()
        );

        var factory = scope.ServiceProvider.GetRequiredService<StatisticsDbDataSetWriterFactory>();

        var writer = factory.Create(Guid.NewGuid());
        Assert.IsType<StatisticsDbDataSetWriter>(writer);
    }

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
