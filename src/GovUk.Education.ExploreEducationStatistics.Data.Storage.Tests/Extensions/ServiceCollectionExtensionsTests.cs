#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb.Interfaces;
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
        using var scope = BuildWriterServiceProvider().CreateScope();

        Assert.IsType<StorageDataSetWriterResolver>(
            scope.ServiceProvider.GetRequiredService<IStorageDataSetWriterResolver>()
        );

        var factory = scope.ServiceProvider.GetRequiredService<StatisticsDbDataSetWriterFactory>();

        var writer = factory.Create(Guid.NewGuid());
        Assert.IsType<StatisticsDbDataSetWriter>(writer);
    }

    [Fact]
    public void AddStorageDataSetWriters_SharesImporterLocationCacheAcrossScopes()
    {
        var serviceProvider = BuildWriterServiceProvider();

        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();

        Assert.Same(
            serviceProvider.GetRequiredService<IImporterLocationCache>(),
            scope1.ServiceProvider.GetRequiredService<IImporterLocationCache>()
        );
        Assert.Same(
            scope1.ServiceProvider.GetRequiredService<IImporterLocationCache>(),
            scope2.ServiceProvider.GetRequiredService<IImporterLocationCache>()
        );
    }

    private static ServiceProvider BuildWriterServiceProvider()
    {
        return new ServiceCollection()
            .AddLogging()
            .AddScoped(_ => InMemoryStatisticsDbContext())
            .AddSingleton(Mock.Of<IDatabaseHelper>())
            .AddSingleton(Mock.Of<IGuidGenerator>())
            .AddStorageDataSetWriters()
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
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
