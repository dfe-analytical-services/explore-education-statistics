#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
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
        IDenseObservationsMatchedFilterItemsStrategy? denseObservationsMatchedFilterItemsStrategy = null
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
