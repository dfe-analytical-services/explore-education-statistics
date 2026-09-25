#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Utils;

public static class StorageDataSetTestUtils
{
    /// <summary>
    /// Builds a real <see cref="StatisticsDbDataSetResolver" /> over the given (typically in-memory) context.
    /// The raw SQL temp table plumbing cannot run against the in-memory provider, so those collaborators default
    /// to Strict mocks and tests seed <c>MatchedObservations</c> directly instead.
    /// </summary>
    public static IStorageDataSetResolver BuildStatisticsDbDataSetResolver(
        StatisticsDbContext statisticsDbContext,
        IObservationService? observationService = null,
        IAllObservationsMatchedFilterItemsStrategy? allObservationsMatchedFilterItemsStrategy = null,
        ISparseObservationsMatchedFilterItemsStrategy? sparseObservationsMatchedFilterItemsStrategy = null,
        IDenseObservationsMatchedFilterItemsStrategy? denseObservationsMatchedFilterItemsStrategy = null
    )
    {
        return new StatisticsDbDataSetResolver(
            context: statisticsDbContext,
            observationService: observationService ?? Mock.Of<IObservationService>(Strict),
            filterRepository: new FilterRepository(statisticsDbContext),
            indicatorGroupRepository: new IndicatorGroupRepository(statisticsDbContext),
            locationRepository: new LocationRepository(statisticsDbContext),
            allObservationsMatchedFilterItemsStrategy: allObservationsMatchedFilterItemsStrategy
                ?? Mock.Of<IAllObservationsMatchedFilterItemsStrategy>(Strict),
            sparseObservationsMatchedFilterItemsStrategy: sparseObservationsMatchedFilterItemsStrategy
                ?? Mock.Of<ISparseObservationsMatchedFilterItemsStrategy>(Strict),
            denseObservationsMatchedFilterItemsStrategy: denseObservationsMatchedFilterItemsStrategy
                ?? Mock.Of<IDenseObservationsMatchedFilterItemsStrategy>(Strict),
            logger: Mock.Of<ILogger<StatisticsDbDataSet>>()
        );
    }

    /// <summary>
    /// A Strict <see cref="IStorageDataSetResolver" /> mock that resolves <paramref name="subjectId" />
    /// to the given data set.
    /// </summary>
    public static Mock<IStorageDataSetResolver> MockStorageDataSetResolver(Guid subjectId, IStorageDataSet dataSet)
    {
        var resolver = new Mock<IStorageDataSetResolver>(Strict);

        resolver.Setup(r => r.Resolve(subjectId, It.IsAny<CancellationToken>())).ReturnsAsync(dataSet);

        return resolver;
    }
}
