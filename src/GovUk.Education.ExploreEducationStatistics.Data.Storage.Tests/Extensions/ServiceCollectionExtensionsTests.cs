#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    public void AddStorageDataSets_CreatesParquetV1DataSet()
    {
        using var scope = BuildServiceProvider().CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<ParquetV1DataSetFactory>();

        var dataSet = factory.Create(Guid.NewGuid());
        Assert.IsType<ParquetV1DataSet>(dataSet);
    }

    [Fact]
    public void AddStorageDataSets_ConfiguresParquetDataSetBasePath()
    {
        using var scope = BuildServiceProvider().CreateScope();

        var pathResolver = scope.ServiceProvider.GetRequiredService<IParquetDataSetPathResolver>();

        Assert.Equal("/data-files-base-path", pathResolver.BasePath());
    }

    [Fact]
    public void AddStorageDataSetWriters_CreatesStatisticsDbDataSetWriter()
    {
        using var scope = BuildServiceProvider(withWriters: true).CreateScope();

        Assert.IsType<StorageDataSetWriterResolver>(
            scope.ServiceProvider.GetRequiredService<IStorageDataSetWriterResolver>()
        );

        var factory = scope.ServiceProvider.GetRequiredService<StatisticsDbDataSetWriterFactory>();

        var writer = factory.Create(Guid.NewGuid());
        Assert.IsType<StatisticsDbDataSetWriter>(writer);
    }

    [Fact]
    public void AddStorageDataSetWriters_CreatesParquetV1DataSetWriter()
    {
        using var scope = BuildServiceProvider(withWriters: true).CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<ParquetV1DataSetWriterFactory>();

        var writer = factory.Create(Guid.NewGuid());
        Assert.IsType<ParquetV1DataSetWriter>(writer);
        Assert.IsType<ParquetDataSetWriter>(scope.ServiceProvider.GetRequiredService<IParquetDataSetWriter>());
    }

    private static ServiceProvider BuildServiceProvider(bool withWriters = false)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?> { { "DataFiles:BasePath", "/data-files-base-path" } }
            )
            .Build();

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(Mock.Of<IHostEnvironment>(environment => environment.EnvironmentName == "Test"))
            .AddScoped(_ => InMemoryContentDbContext())
            .AddScoped(_ => InMemoryStatisticsDbContext())
            .AddStorageDataSets(configuration);

        if (withWriters)
        {
            services
                .AddSingleton(Mock.Of<IDatabaseHelper>())
                .AddSingleton(Mock.Of<IGuidGenerator>())
                .AddStorageDataSetWriters();
        }

        return services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true }
        );
    }
}
