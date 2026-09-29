#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Utils;

public static class StorageDataSetTestUtils
{
    /// <summary>
    /// Builds a real <see cref="StorageDataSetResolver" /> over the given (typically in-memory) contexts, so that
    /// the storage type is looked up from the seeded data <c>File</c> rows as it is in production.
    /// </summary>
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
            statisticsDbDataSetResolver: BuildStatisticsDbDataSetResolver(
                statisticsDbContext,
                observationService,
                allObservationsMatchedFilterItemsStrategy,
                sparseObservationsMatchedFilterItemsStrategy,
                denseObservationsMatchedFilterItemsStrategy
            )
        );
    }

    /// <summary>
    /// Builds a real <see cref="StatisticsDbDataSetResolver" /> over the given (typically in-memory) context.
    /// The raw SQL temp table plumbing cannot run against the in-memory provider, so those collaborators default
    /// to Strict mocks and tests seed <c>MatchedObservations</c> directly instead.
    /// </summary>
    public static StatisticsDbDataSetResolver BuildStatisticsDbDataSetResolver(
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

        return new StatisticsDbDataSetResolver(services.BuildServiceProvider());
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
