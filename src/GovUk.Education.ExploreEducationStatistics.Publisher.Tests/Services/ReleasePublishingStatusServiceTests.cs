using Azure;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public abstract class ReleasePublishingStatusServiceTests
{
    private readonly AsyncPageable<ReleasePublishingStatus> _emptyTableResponse =
        BuildResponse<ReleasePublishingStatus>([]);

    public class GetScheduledReleasesForPublishingRelativeToDateTests : ReleasePublishingStatusServiceTests
    {
        public static readonly TheoryData<DateComparison> DateComparisonTheoryData =
            new(EnumUtil.GetEnums<DateComparison>());

        [Theory]
        [MemberData(nameof(DateComparisonTheoryData))]
        public async Task QueryFilterIncludesExpectedStageAndPublishDate(DateComparison dateComparison)
        {
            // Arrange
            var referenceDate = DateTimeOffset.Parse("2025-01-01T12:00:00Z");
            var expectedDateOperator = dateComparison switch
            {
                DateComparison.Before => "lt",
                DateComparison.BeforeOrOn => "le",
                DateComparison.After => "gt",
                DateComparison.AfterOrOn => "ge",
                DateComparison.Equal => "eq",
                DateComparison.NotEqual => "ne",
                _ => throw new ArgumentOutOfRangeException(nameof(dateComparison), dateComparison, null)
            };
            var expectedFilter =
                $"OverallStage eq 'Scheduled' and Publish {expectedDateOperator} datetime'2025-01-01T12:00:00Z'";

            var publisherTableStorageService = new Mock<IPublisherTableStorageService>(MockBehavior.Strict);

            publisherTableStorageService.Setup(service =>
                    service.QueryEntities<ReleasePublishingStatus>(
                        TableStorageTableNames.PublisherReleaseStatusTableName,
                        expectedFilter,
                        1000,
                        null,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(_emptyTableResponse);

            var service = BuildService(publisherTableStorageService: publisherTableStorageService.Object);

            // Act
            await service.GetScheduledReleasesForPublishingRelativeToDate(
                dateComparison,
                referenceDate);

            // Assert
            MockUtils.VerifyAllMocks(publisherTableStorageService);
        }

        [Fact]
        public async Task ReturnsEntitiesAsReleasePublishingKeys()
        {
            // Arrange
            var referenceDate = DateTimeOffset.Parse("2025-01-01T12:00:00Z");
            ReleasePublishingKey[] expectedKeys =
            [
                new(Guid.NewGuid(), Guid.NewGuid()),
                new(Guid.NewGuid(), Guid.NewGuid())
            ];

            var publisherTableStorageService = new Mock<IPublisherTableStorageService>(MockBehavior.Strict);

            var response = BuildResponse(expectedKeys.Select(BuildReleasePublishingStatus).ToList());

            publisherTableStorageService.Setup(service =>
                    service.QueryEntities<ReleasePublishingStatus>(
                        TableStorageTableNames.PublisherReleaseStatusTableName,
                        It.IsAny<string>(),
                        1000,
                        null,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var service = BuildService(publisherTableStorageService: publisherTableStorageService.Object);

            // Act
            var result = await service.GetScheduledReleasesForPublishingRelativeToDate(
                DateComparison.BeforeOrOn,
                referenceDate);

            // Assert
            MockUtils.VerifyAllMocks(publisherTableStorageService);
            Assert.Equal(expectedKeys, result);
        }
    }

    public class GetScheduledReleasesReadyForPublishingTests : ReleasePublishingStatusServiceTests
    {
        [Fact]
        public async Task QueryFilterIncludesExpectedStages()
        {
            // Arrange
            const string expectedFilter =
                "OverallStage eq 'Started' and ContentStage eq 'Scheduled' and FilesStage eq 'Complete' and PublishingStage eq 'Scheduled'";

            var publisherTableStorageService = new Mock<IPublisherTableStorageService>(MockBehavior.Strict);

            publisherTableStorageService.Setup(service =>
                    service.QueryEntities<ReleasePublishingStatus>(
                        TableStorageTableNames.PublisherReleaseStatusTableName,
                        expectedFilter,
                        1000,
                        null,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(_emptyTableResponse);

            var service = BuildService(publisherTableStorageService: publisherTableStorageService.Object);

            // Act
            await service.GetScheduledReleasesReadyForPublishing();

            // Assert
            MockUtils.VerifyAllMocks(publisherTableStorageService);
        }

        [Fact]
        public async Task ReturnsEntitiesAsReleasePublishingKeys()
        {
            // Arrange
            ReleasePublishingKey[] expectedKeys =
            [
                new(Guid.NewGuid(), Guid.NewGuid()),
                new(Guid.NewGuid(), Guid.NewGuid())
            ];

            var publisherTableStorageService = new Mock<IPublisherTableStorageService>(MockBehavior.Strict);

            var response = BuildResponse(expectedKeys.Select(BuildReleasePublishingStatus).ToList());

            publisherTableStorageService.Setup(service =>
                    service.QueryEntities<ReleasePublishingStatus>(
                        TableStorageTableNames.PublisherReleaseStatusTableName,
                        It.IsAny<string>(),
                        1000,
                        null,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var service = BuildService(publisherTableStorageService: publisherTableStorageService.Object);

            // Act
            var result = await service.GetScheduledReleasesReadyForPublishing();

            // Assert
            MockUtils.VerifyAllMocks(publisherTableStorageService);
            Assert.Equal(expectedKeys, result);
        }
    }

    public class GetReleasesWithOverallStagesTests : ReleasePublishingStatusServiceTests
    {
        [Fact]
        public async Task QueryFilterIncludesReleaseVersionIdAndExpectedStages_WhenSingleOverallStage()
        {
            // Arrange
            var releaseVersionId = Guid.NewGuid();
            const ReleasePublishingStatusOverallStage overallStage = ReleasePublishingStatusOverallStage.Complete;
            var expectedFilter = $"PartitionKey eq '{releaseVersionId}' and OverallStage eq 'Complete'";

            var publisherTableStorageService = new Mock<IPublisherTableStorageService>(MockBehavior.Strict);

            publisherTableStorageService.Setup(service =>
                    service.QueryEntities<ReleasePublishingStatus>(
                        TableStorageTableNames.PublisherReleaseStatusTableName,
                        expectedFilter,
                        1000,
                        null,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(_emptyTableResponse);

            var service = BuildService(publisherTableStorageService: publisherTableStorageService.Object);

            // Act
            await service.GetReleasesWithOverallStages(
                releaseVersionId,
                overallStages: [overallStage]);

            // Assert
            MockUtils.VerifyAllMocks(publisherTableStorageService);
        }

        [Fact]
        public async Task QueryFilterIncludesReleaseVersionIdAndExpectedStages_WhenMultipleOverallStages()
        {
            // Arrange
            var releaseVersionId = Guid.NewGuid();
            ReleasePublishingStatusOverallStage[] overallStages =
            [
                ReleasePublishingStatusOverallStage.Scheduled,
                ReleasePublishingStatusOverallStage.Started
            ];
            var expectedFilter =
                $"PartitionKey eq '{releaseVersionId}' and (OverallStage eq 'Scheduled' or OverallStage eq 'Started')";

            var publisherTableStorageService = new Mock<IPublisherTableStorageService>(MockBehavior.Strict);

            publisherTableStorageService.Setup(service =>
                    service.QueryEntities<ReleasePublishingStatus>(
                        TableStorageTableNames.PublisherReleaseStatusTableName,
                        expectedFilter,
                        1000,
                        null,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(_emptyTableResponse);

            var service = BuildService(publisherTableStorageService: publisherTableStorageService.Object);

            // Act
            await service.GetReleasesWithOverallStages(
                releaseVersionId,
                overallStages: overallStages);

            // Assert
            MockUtils.VerifyAllMocks(publisherTableStorageService);
        }

        [Fact]
        public async Task ReturnsEntitiesAsReleasePublishingKeys()
        {
            // Arrange
            var releaseVersionId = Guid.NewGuid();
            ReleasePublishingStatusOverallStage[] overallStages =
            [
                ReleasePublishingStatusOverallStage.Complete
            ];
            ReleasePublishingKey[] expectedKeys =
            [
                new(releaseVersionId, Guid.NewGuid()),
                new(releaseVersionId, Guid.NewGuid())
            ];

            var publisherTableStorageService = new Mock<IPublisherTableStorageService>(MockBehavior.Strict);

            var response = BuildResponse(expectedKeys.Select(BuildReleasePublishingStatus).ToList());

            publisherTableStorageService.Setup(service =>
                    service.QueryEntities<ReleasePublishingStatus>(
                        TableStorageTableNames.PublisherReleaseStatusTableName,
                        It.IsAny<string>(),
                        1000,
                        null,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var service = BuildService(publisherTableStorageService: publisherTableStorageService.Object);

            // Act
            var result = await service.GetReleasesWithOverallStages(
                releaseVersionId,
                overallStages: overallStages);

            // Assert
            MockUtils.VerifyAllMocks(publisherTableStorageService);
            Assert.Equal(expectedKeys, result);
        }

        [Fact]
        public async Task ThrowsExceptionWhenOverallStagesIsEmpty()
        {
            // Arrange
            var releaseVersionId = Guid.NewGuid();
            ReleasePublishingStatusOverallStage[] overallStages = [];

            var service = BuildService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.GetReleasesWithOverallStages(
                releaseVersionId,
                overallStages: overallStages));
        }
    }

    private static ReleasePublishingStatus BuildReleasePublishingStatus(ReleasePublishingKey key) =>
        new()
        {
            PartitionKey = key.ReleaseVersionId.ToString(),
            RowKey = key.ReleaseStatusId.ToString()
        };

    private static AsyncPageable<T> BuildResponse<T>(IReadOnlyList<T> values) where T : notnull =>
        AsyncPageable<T>.FromPages([BuildResponsePage(values)]);

    private static Page<T> BuildResponsePage<T>(IReadOnlyList<T> values) =>
        Page<T>.FromValues(
            values: values,
            continuationToken: null,
            response: Mock.Of<Response>());

    private static ReleasePublishingStatusService BuildService(
        ContentDbContext? contentDbContext = null,
        IPublisherTableStorageService? publisherTableStorageService = null)
    {
        return new ReleasePublishingStatusService(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            NullLogger<ReleasePublishingStatusService>.Instance,
            publisherTableStorageService ?? Mock.Of<IPublisherTableStorageService>(MockBehavior.Strict)
        );
    }
}
