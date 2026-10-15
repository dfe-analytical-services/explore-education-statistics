#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Utils;

public static class StorageDataSetTestUtils
{
    public static StorageDataSetResolver BuildStorageDataSetResolver(
        ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext,
        IObservationService? observationService = null,
        IAllObservationsMatchedFilterItemsStrategy? allObservationsMatchedFilterItemsStrategy = null,
        ISparseObservationsMatchedFilterItemsStrategy? sparseObservationsMatchedFilterItemsStrategy = null,
        IDenseObservationsMatchedFilterItemsStrategy? denseObservationsMatchedFilterItemsStrategy = null,
        IParquetDataSetPathResolver? parquetDataSetPathResolver = null
    )
    {
        return new StorageDataSetResolver(
            contentDbContext: contentDbContext,
            statisticsDbDataSetFactory: BuildStatisticsDbDataSetFactory(
                statisticsDbContext,
                observationService,
                allObservationsMatchedFilterItemsStrategy,
                sparseObservationsMatchedFilterItemsStrategy,
                denseObservationsMatchedFilterItemsStrategy
            ),
            parquetV1DataSetFactory: BuildParquetV1DataSetFactory(
                parquetDataSetPathResolver ?? new TestParquetDataSetPathResolver(Path.GetTempPath())
            )
        );
    }

    public static IStorageDataSetResolver BuildStatisticsDbDataSetResolver(
        StatisticsDbContext statisticsDbContext,
        IObservationService? observationService = null,
        IAllObservationsMatchedFilterItemsStrategy? allObservationsMatchedFilterItemsStrategy = null,
        ISparseObservationsMatchedFilterItemsStrategy? sparseObservationsMatchedFilterItemsStrategy = null,
        IDenseObservationsMatchedFilterItemsStrategy? denseObservationsMatchedFilterItemsStrategy = null
    )
    {
        return new StatisticsDbOnlyResolver(
            BuildStatisticsDbDataSetFactory(
                statisticsDbContext,
                observationService,
                allObservationsMatchedFilterItemsStrategy,
                sparseObservationsMatchedFilterItemsStrategy,
                denseObservationsMatchedFilterItemsStrategy
            )
        );
    }

    public static StatisticsDbDataSetFactory BuildStatisticsDbDataSetFactory(
        StatisticsDbContext statisticsDbContext,
        IObservationService? observationService = null,
        IAllObservationsMatchedFilterItemsStrategy? allObservationsMatchedFilterItemsStrategy = null,
        ISparseObservationsMatchedFilterItemsStrategy? sparseObservationsMatchedFilterItemsStrategy = null,
        IDenseObservationsMatchedFilterItemsStrategy? denseObservationsMatchedFilterItemsStrategy = null
    )
    {
        var services = new ServiceCollection()
            .AddSingleton(statisticsDbContext)
            .AddSingleton(observationService ?? Mock.Of<IObservationService>(Strict))
            .AddSingleton(
                allObservationsMatchedFilterItemsStrategy ?? Mock.Of<IAllObservationsMatchedFilterItemsStrategy>(Strict)
            )
            .AddSingleton(
                sparseObservationsMatchedFilterItemsStrategy
                    ?? Mock.Of<ISparseObservationsMatchedFilterItemsStrategy>(Strict)
            )
            .AddSingleton(
                denseObservationsMatchedFilterItemsStrategy
                    ?? Mock.Of<IDenseObservationsMatchedFilterItemsStrategy>(Strict)
            )
            .AddSingleton(Mock.Of<ILogger<StatisticsDbDataSet>>());

        return new StatisticsDbDataSetFactory(services.BuildServiceProvider());
    }

    public static StorageDataSetWriterResolver BuildStorageDataSetWriterResolver(
        StatisticsDbContext statisticsDbContext,
        IDatabaseHelper databaseHelper,
        IImporterLocationCache? importerLocationCache = null,
        IGuidGenerator? guidGenerator = null,
        IObservationBatchImporter? observationBatchImporter = null,
        IParquetDataSetWriter? parquetDataSetWriter = null,
        IParquetDataSetPathResolver? parquetDataSetPathResolver = null
    )
    {
        var serviceProvider = BuildDataSetWriterServiceProvider(
            statisticsDbContext,
            databaseHelper,
            importerLocationCache,
            guidGenerator,
            observationBatchImporter,
            parquetDataSetWriter,
            parquetDataSetPathResolver
        );

        return new StorageDataSetWriterResolver(
            new StatisticsDbDataSetWriterFactory(serviceProvider),
            new ParquetV1DataSetWriterFactory(serviceProvider)
        );
    }

    public static StatisticsDbDataSetWriterFactory BuildStatisticsDbDataSetWriterFactory(
        StatisticsDbContext statisticsDbContext,
        IDatabaseHelper databaseHelper,
        IImporterLocationCache? importerLocationCache = null,
        IGuidGenerator? guidGenerator = null,
        IObservationBatchImporter? observationBatchImporter = null
    )
    {
        return new StatisticsDbDataSetWriterFactory(
            BuildDataSetWriterServiceProvider(
                statisticsDbContext,
                databaseHelper,
                importerLocationCache,
                guidGenerator,
                observationBatchImporter
            )
        );
    }

    public static ParquetV1DataSetWriterFactory BuildParquetV1DataSetWriterFactory(
        StatisticsDbContext statisticsDbContext,
        IDatabaseHelper databaseHelper,
        IParquetDataSetWriter parquetDataSetWriter,
        IParquetDataSetPathResolver? parquetDataSetPathResolver = null
    )
    {
        return new ParquetV1DataSetWriterFactory(
            BuildDataSetWriterServiceProvider(
                statisticsDbContext,
                databaseHelper,
                parquetDataSetWriter: parquetDataSetWriter,
                parquetDataSetPathResolver: parquetDataSetPathResolver
            )
        );
    }

    private static ServiceProvider BuildDataSetWriterServiceProvider(
        StatisticsDbContext statisticsDbContext,
        IDatabaseHelper databaseHelper,
        IImporterLocationCache? importerLocationCache = null,
        IGuidGenerator? guidGenerator = null,
        IObservationBatchImporter? observationBatchImporter = null,
        IParquetDataSetWriter? parquetDataSetWriter = null,
        IParquetDataSetPathResolver? parquetDataSetPathResolver = null
    )
    {
        return new ServiceCollection()
            .AddSingleton(statisticsDbContext)
            .AddSingleton(databaseHelper)
            .AddSingleton(importerLocationCache ?? new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>()))
            .AddSingleton(guidGenerator ?? new SequentialGuidGenerator())
            .AddSingleton(observationBatchImporter ?? Mock.Of<IObservationBatchImporter>(Strict))
            .AddSingleton(parquetDataSetWriter ?? Mock.Of<IParquetDataSetWriter>(Strict))
            .AddSingleton(BuildStatisticsDbDataSetFactory(statisticsDbContext))
            .AddSingleton(
                BuildParquetV1DataSetFactory(parquetDataSetPathResolver ?? Mock.Of<IParquetDataSetPathResolver>(Strict))
            )
            .AddTransient<ImporterLocationService>()
            .AddTransient<StatisticsDbDataSetWriterFactory>()
            .AddSingleton(Mock.Of<ILogger<ImporterLocationCache>>())
            .BuildServiceProvider();
    }

    public static ParquetV1DataSetFactory BuildParquetV1DataSetFactory(IParquetDataSetPathResolver pathResolver)
    {
        var services = new ServiceCollection().AddSingleton(pathResolver);

        return new ParquetV1DataSetFactory(services.BuildServiceProvider());
    }

    public static Mock<IStorageDataSetResolver> MockStorageDataSetResolver(Guid subjectId, IStorageDataSet dataSet)
    {
        var resolver = new Mock<IStorageDataSetResolver>(Strict);

        resolver.Setup(r => r.Resolve(subjectId, It.IsAny<CancellationToken>())).ReturnsAsync(dataSet);
        resolver.Setup(r => r.Resolve(It.Is<File>(f => f.SubjectId == subjectId))).Returns(dataSet);

        return resolver;
    }

    private class StatisticsDbOnlyResolver(StatisticsDbDataSetFactory factory) : IStorageDataSetResolver
    {
        public Task<IStorageDataSet> Resolve(Guid subjectId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IStorageDataSet>(factory.Create(subjectId));
        }

        public IStorageDataSet Resolve(File dataFile)
        {
            return factory.Create(dataFile.SubjectId!.Value);
        }

        public Task<IStorageDataSet?> TryResolve(Guid subjectId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IStorageDataSet?>(factory.Create(subjectId));
        }
    }
}
