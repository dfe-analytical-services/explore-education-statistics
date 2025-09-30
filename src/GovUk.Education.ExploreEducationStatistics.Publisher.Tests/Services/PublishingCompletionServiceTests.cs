using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public class PublishingCompletionServiceTests
{
    private readonly ContentDbContextMockBuilder _contentDbContext = new();
    private readonly ContentServiceMockBuilder _contentService = new();
    private readonly MethodologyServiceMockBuilder _methodologyService = new();
    private readonly NotificationsServiceMockBuilder _notificationsService = new();
    private readonly ReleasePublishingStatusServiceMockBuilder _releasePublishingStatusService = new();
    private readonly PublicationCacheServiceMockBuilder _publicationCacheService = new();
    private readonly ReleaseServiceMockBuilder _releaseService = new();
    private readonly RedirectsCacheServiceMockBuilder _redirectsCacheService = new();
    private readonly DataSetPublishingServiceMockBuilder _dataSetPublishingService = new();
    private readonly PublisherEventRaiserMockBuilder _publisherEventRaiser = new();

    /// <summary>
    /// Ensure that GetSUT is called immediate before the ACT phase.
    /// This is because the _contentDbContext builder disposes the dbContext used during ARRANGE
    /// and creates a fresh copy to ensure that any queries performed rehydrate the entities properly.
    /// </summary>
    private PublishingCompletionService GetSut() =>
        new(
            _contentDbContext.Build(),
            _contentService.Build(),
            _methodologyService.Build(),
            _notificationsService.Build(),
            _releasePublishingStatusService.Build(),
            _publicationCacheService.Build(),
            _releaseService.Build(),
            _redirectsCacheService.Build(),
            _dataSetPublishingService.Build(),
            _publisherEventRaiser.Build());

    public class BasicTests : PublishingCompletionServiceTests
    {
        [Fact]
        public void Can_instantiate_Sut() => Assert.NotNull(GetSut());
    }

    public class ContainerRegistrationTests : PublishingCompletionServiceTests
    {
        [Fact]
        public void EnsureSUTCanBeResolved()
        {
            var host = new HostBuilder()
                .ConfigureTestAppConfiguration()
                .ConfigureServices()
                .Build();

            ActivatorUtilities.CreateInstance<PublishingCompletionService>(host.Services);
        }
    }

    public class CompletePublishingIfAllPriorStagesCompleteTests : PublishingCompletionServiceTests
    {
        public class NotReadyTests : CompletePublishingIfAllPriorStagesCompleteTests
        {
            [Fact]
            public async Task WhenNoPublicationsReady_ThenNothingHappens()
            {
                // ARRANGE
                ReleasePublishingKey releasePublishingKey1 = new(Guid.NewGuid(), Guid.NewGuid());
                var releasePublishingStatus1 = new ReleasePublishingStatusBuilder(releasePublishingKey1)
                    .WhereContentStatusIs(ReleasePublishingStatusContentStage.Queued)
                    .Build();

                ReleasePublishingKey releasePublishingKey2 = new(Guid.NewGuid(), Guid.NewGuid());
                var releasePublishingStatus2 = new ReleasePublishingStatusBuilder(releasePublishingKey2)
                    .WhereFilesStatusIs(ReleasePublishingStatusFilesStage.NotStarted)
                    .Build();

                var notReadyKeys = new List<ReleasePublishingKey>()
                {
                    releasePublishingKey1,
                    releasePublishingKey2
                };

                _releasePublishingStatusService
                    .WhereGetReturns(releasePublishingKey1, releasePublishingStatus1)
                    .WhereGetReturns(releasePublishingKey2, releasePublishingStatus2);

                var sut = GetSut();

                // ACT
                await sut.CompletePublishingIfAllPriorStagesComplete(notReadyKeys);

                // ASSERT
                _releasePublishingStatusService.Assert.UpdatePublishingStageWasNotCalled();
            }
        }

        public class ReadyTests : CompletePublishingIfAllPriorStagesCompleteTests
        {
            /*
             * Given
             *      Publication 1
             *      - with Release Version 1
             *          - Id: ReleaseVersionId1
             *          - Status: Complete
             *
             *      Publication 2
             *      - with Release Version 2
             *          - Id: ReleaseVersionId2
             *          - Status: Complete
             *
             */
            private static readonly Guid PublicationId1 = Guid.NewGuid();
            private static readonly Guid PublicationId2 = Guid.NewGuid();

            private ReleaseVersion _releaseVersion1 = null!;
            private ReleaseVersion _releaseVersion2 = null!;

            private IReadOnlyList<ReleasePublishingKey> SetupHappyPath(
                Func<PublicationBuilder, PublicationBuilder>? publication1BuilderModification = null)
            {
                // Create some sample publications
                publication1BuilderModification ??= (publicationBuilder) => publicationBuilder;
                var publication1 = publication1BuilderModification(new PublicationBuilder(PublicationId1, "publication-slug-1"))
                    .Build();

                var publication2 = new PublicationBuilder(PublicationId2, "publication-slug-2")
                    .Build();

                // Create some sample release versions
                _releaseVersion1 = new ReleaseVersionBuilder()
                    .WithPublicationId(publication1.Id)
                    .ForRelease(release => release
                        .WithReleaseSlug("release-slug-1"))
                    .Build();

                _releaseVersion2 = new ReleaseVersionBuilder()
                    .WithPublicationId(publication2.Id)
                    .ForRelease(release => release
                        .WithReleaseSlug("release-slug-2"))
                    .Build();

                // Assign the release versions a status id
                ReleasePublishingKey[] readyKeys =
                [
                    new(_releaseVersion1.Id, ReleaseStatusId: Guid.NewGuid()),
                    new(_releaseVersion2.Id, ReleaseStatusId: Guid.NewGuid()),
                ];

                // Set the Status Service to report on whether a release version is ready to be published or not ie Complete 
                foreach (var readyKey in readyKeys)
                {
                    _releasePublishingStatusService.WhereGetReturns(
                        readyKey,
                        new ReleasePublishingStatusBuilder(readyKey)
                            .WhereContentStatusIs(ReleasePublishingStatusContentStage.Complete)
                            .Build());
                }

                // Add Release Versions and their Publications to the mock Database
                _contentDbContext.With(_releaseVersion1);
                _contentDbContext.With(_releaseVersion2);
                _contentDbContext.With(publication1);
                _contentDbContext.With(publication2);

                // Add release versions to release service
                _releaseService.WhereGetReturns(_releaseVersion1);
                _releaseService.WhereGetReturns(_releaseVersion2);

                // Set the latest published release version for each publication expected after publishing
                _releaseService.WherePublicationLatestPublishedReleaseVersionIs(PublicationId1, _releaseVersion1);
                _releaseService.WherePublicationLatestPublishedReleaseVersionIs(PublicationId2, _releaseVersion2);

                return readyKeys;
            }

            public class UpdatePublishingStageTests : ReadyTests
            {
                [Fact]
                public async Task PublishingStatusSetToStarted()
                {
                    // ARRANGE
                    // Create some other keys that we'll set to "not complete"
                    ReleasePublishingKey[] notReadyKeys =
                    [
                        new(Guid.NewGuid(), Guid.NewGuid()),
                        new(Guid.NewGuid(), Guid.NewGuid()),
                    ];

                    foreach (var notReadyKey in notReadyKeys)
                    {
                        _releasePublishingStatusService.WhereGetReturns(
                            notReadyKey,
                            new ReleasePublishingStatusBuilder(notReadyKey)
                                .WhereContentStatusIs(ReleasePublishingStatusContentStage.Scheduled)
                                .Build());
                    }

                    var readyKeys = SetupHappyPath();
                    var allPublishingKeys = readyKeys.Concat(notReadyKeys).ToList();

                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(allPublishingKeys);

                    // ASSERT
                    // Update Publishing Stage Was Called
                    foreach (var readyKey in readyKeys)
                    {
                        _releasePublishingStatusService.Assert.UpdatePublishingStageWasCalled(
                            readyKey,
                            ReleasePublishingStatusPublishingStage.Started);
                    }

                    // Update Publishing Stage Was Not Called
                    foreach (var notReadyKey in notReadyKeys)
                    {
                        _releasePublishingStatusService.Assert.UpdatePublishingStageWasNotCalled(notReadyKey);
                    }
                }

                [Fact]
                public async Task PublishingStatusSetToComplete()
                {
                    // ARRANGE
                    // Create some other keys that we'll set to "not complete"
                    ReleasePublishingKey[] notReadyKeys =
                    [
                        new(Guid.NewGuid(), Guid.NewGuid()),
                        new(Guid.NewGuid(), Guid.NewGuid()),
                    ];

                    foreach (var notReadyKey in notReadyKeys)
                    {
                        _releasePublishingStatusService.WhereGetReturns(
                            notReadyKey,
                            new ReleasePublishingStatusBuilder(notReadyKey)
                                .WhereContentStatusIs(ReleasePublishingStatusContentStage.Scheduled)
                                .Build());
                    }

                    var readyKeys = SetupHappyPath();
                    var allPublishingKeys = readyKeys.Concat(notReadyKeys).ToList();

                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(allPublishingKeys);

                    // ASSERT
                    // Keys were set to Completed at the end
                    foreach (var readyKey in readyKeys)
                    {
                        _releasePublishingStatusService.Assert.UpdatePublishingStageWasCalled(
                            readyKey,
                            ReleasePublishingStatusPublishingStage.Complete);
                    }

                    // Not ready Keys were not set to anything
                    foreach (var notReadyKey in notReadyKeys)
                    {
                        _releasePublishingStatusService.Assert.UpdatePublishingStageWasNotCalled(notReadyKey);
                    }
                }
            }

            public class CompletePublishingTests : ReadyTests
            {
                [Fact]
                public async Task CompletePublishingCalledOnReleaseService()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    foreach (var readyKey in readyKeys)
                    {
                        _releaseService.Assert.CompletePublishingWasCalled(readyKey.ReleaseVersionId);
                    }
                }
            }

            public class MethodologiesTests : ReadyTests
            {
                private void SetupNoMethodologies(Guid releaseVersionId)
                {
                    var releaseVersion = _releaseService.Get(releaseVersionId);
                    _methodologyService.WhereGetLatestVersionByReleaseReturnsNoMethodologies(releaseVersion);
                }

                private void SetupMethodologies(Guid releaseVersionId, MethodologyVersion methodologyToPublish)
                {
                    var releaseVersion = _releaseService.Get(releaseVersionId);
                    _methodologyService.WhereGetLatestVersionByReleaseReturns(releaseVersion, methodologyToPublish);
                    _methodologyService.WhereIsBeingPublishedAlongsideRelease(methodologyToPublish, releaseVersion);
                }

                private void SetupMethodologies(
                    Guid releaseVersionId,
                    MethodologyVersion methodologyToPublish,
                    MethodologyVersion methodologyNotToPublish)
                {
                    var releaseVersion = _releaseService.Get(releaseVersionId);
                    _methodologyService.WhereGetLatestVersionByReleaseReturns(releaseVersion,
                        methodologyToPublish,
                        methodologyNotToPublish);
                    _methodologyService.WhereIsBeingPublishedAlongsideRelease(methodologyToPublish, releaseVersion);
                    _methodologyService.WhereIsNotBeingPublishedAlongsideRelease(methodologyNotToPublish,
                        releaseVersion);
                }

                [Fact]
                public async Task WhenNoMethodologies_ThenNoMethodologiesPublished()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();
                    foreach (var key in readyKeys)
                    {
                        SetupNoMethodologies(key.ReleaseVersionId);
                    }

                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    _methodologyService.Assert.NoMethodologiesPublished();
                }

                [Fact]
                public async Task WhenHasMethodology_ThenMethodologyPublished()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();
                    var methodologies = new MethodologyVersion[readyKeys.Count];

                    for (var i = 0; i < readyKeys.Count; i++)
                    {
                        var key = readyKeys[i];
                        methodologies[i] = new MethodologyVersion();
                        SetupMethodologies(key.ReleaseVersionId, methodologies[i]);
                    }

                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    foreach (var methodology in methodologies)
                    {
                        _methodologyService.Assert.MethodologyPublished(methodology);
                    }
                }

                [Fact]
                public async Task WhenHasSomeMethodologiesToPublish_ThenOnlyMethodologiesToBePublishedArePublished()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();
                    var methodologiesToPublish = new MethodologyVersion[readyKeys.Count];
                    var methodologiesNotToPublish = new MethodologyVersion[readyKeys.Count];

                    for (var i = 0; i < readyKeys.Count; i++)
                    {
                        var key = readyKeys[i];
                        methodologiesToPublish[i] = new MethodologyVersion();
                        methodologiesNotToPublish[i] = new MethodologyVersion();
                        SetupMethodologies(key.ReleaseVersionId,
                            methodologiesToPublish[i],
                            methodologiesNotToPublish[i]);
                    }

                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    foreach (var methodology in methodologiesToPublish)
                    {
                        _methodologyService.Assert.MethodologyPublished(methodology);
                    }

                    foreach (var methodology in methodologiesNotToPublish)
                    {
                        _methodologyService.Assert.MethodologyNotPublished(methodology);
                    }
                }
            }

            public class UpdatePublicationTests : ReadyTests
            {
                [Fact]
                public async Task WhenPublishedReleaseVersionIsLatestVersion_ThenPublicationLatestReleaseVersionSetToIt()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    _contentDbContext.Assert.PublicationsLatestPublishedReleaseVersionIdIs(PublicationId1, _releaseVersion1.Id);
                    _contentDbContext.Assert.PublicationsLatestPublishedReleaseVersionIdIs(PublicationId2, _releaseVersion2.Id);
                }

                [Fact]
                public async Task WhenPublishedReleaseVersionIsNotLatestVersion_ThenPublicationLatestReleaseVersionSetToLatestReleaseVersion()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();

                    var releaseVersionId1V2 = Guid.NewGuid();
                    var releaseVersion1V2 = new ReleaseVersionBuilder(releaseVersionId1V2)
                        .ForRelease(_releaseVersion1.Release)
                        .Build();

                    // Set that release version "1 version 2" is the latest for the Publication
                    _releaseService.WherePublicationLatestPublishedReleaseVersionIs(
                        releaseVersion1V2.Release.PublicationId,
                        releaseVersion1V2);

                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    _contentDbContext.Assert.PublicationsLatestPublishedReleaseVersionIdIs(PublicationId1, releaseVersionId1V2);
                }
            }

            public class UpdatePublicationCacheTests : ReadyTests
            {
                [Fact]
                public async Task AllPublicationsUpdatedInCache()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    _publicationCacheService.Assert.PublicationUpdated(_releaseVersion1.Release.Publication.Slug);
                    _publicationCacheService.Assert.PublicationUpdated(_releaseVersion2.Release.Publication.Slug);
                }

                [Fact]
                public async Task
                    GivenASupersededPublication_WhenPublicationIsAlreadyPublished_ThenSupersededPublicationNotUpdatedInCache()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();

                    // Set publication 1 to have a published release prior to the current publishing run
                    WherePreviousPublicationLatestPublishedReleaseVersionIs(
                        publicationId: PublicationId1,
                        releaseVersion: new ReleaseVersionBuilder()
                            .WithPublicationId(PublicationId1)
                            .ForRelease(release => release
                                .WithReleaseSlug("release-slug-previous"))
                            .Build());

                    // Create a publication that is superseded by publication 1
                    var supersededPublication = new PublicationBuilder(Guid.NewGuid(), "publication-slug-superseded")
                        .SupersededBy(_releaseVersion1.Release.Publication.Id)
                        .Build();
                    _contentDbContext.With(supersededPublication);

                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    // The publication cache should not be updated for the superseded publication
                    _publicationCacheService.Assert.PublicationNotUpdated(supersededPublication.Slug);
                }

                [Fact]
                public async Task GivenASupersededPublication_WhenPublicationIsNotAlreadyPublished_ThenSupersededPublicationUpdatedInCache()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();

                    // Create a publication that is superseded by publication 1.
                    // Publication 1 has no published releases prior to the current publishing run
                    var supersededPublication = new PublicationBuilder(Guid.NewGuid(), "publication-slug-superseded")
                        .SupersededBy(_releaseVersion1.Release.Publication.Id)
                        .Build();
                    _contentDbContext.With(supersededPublication);

                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    // The publication cache should be updated for the superseded publication
                    _publicationCacheService.Assert.PublicationUpdated(supersededPublication.Slug);
                }
            }

            public class ContentServiceTests : ReadyTests
            {
                [Fact]
                public async Task PreviousVersionsDownloadFilesDeleted()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    _contentService.Assert.DeletePreviousVersionsDownloadFilesCalled(_releaseVersion1.Id, _releaseVersion2.Id);
                }

                [Fact]
                public async Task PreviousVersionsContentDeleted()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    _contentService.Assert.DeletePreviousVersionsContentCalled(_releaseVersion1.Id, _releaseVersion2.Id);
                }

                [Fact]
                public async Task CachedTaxonomyBlobsUpdated()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    _contentService.Assert.UpdateCachedTaxonomyBlobsCalled();
                }
            }

            public class NotificationServiceTests : ReadyTests
            {
                [Fact]
                public async Task NotificationsSent()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    _notificationsService.Assert.NotifySubscribersIfApplicableCalled(_releaseVersion1.Id, _releaseVersion2.Id);
                    _notificationsService.Assert.SendReleasePublishingFeedbackEmailsCalled(_releaseVersion1.Id, _releaseVersion2.Id);
                }
            }

            public class RedirectsCacheServiceTests : ReadyTests
            {
                [Fact]
                public async Task RedirectsUpdated()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    _redirectsCacheService.Assert.UpdateRedirectsCalled();
                }
            }

            public class DataSetPublishingServiceTests : ReadyTests
            {
                [Fact]
                public async Task DataSetsWerePublished()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    _dataSetPublishingService.Assert.DataSetsWerePublished(_releaseVersion1.Id, _releaseVersion2.Id);
                }
            }

            public class EventRaiserTests : ReadyTests
            {
                [Fact]
                public async Task WhenReleaseVersionsArePublished_ThenEventIsRaised()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    var expectedInfo = new PublishedPublicationInfo
                    {
                        PublicationId = _releaseVersion1.Release.PublicationId,
                        PublicationSlug = _releaseVersion1.Release.Publication.Slug,
                        LatestPublishedReleaseId = _releaseVersion1.ReleaseId,
                        LatestPublishedReleaseVersionId = _releaseVersion1.Id,
                        PreviousLatestPublishedReleaseId = null,
                        PreviousLatestPublishedReleaseVersionId = null,
                        PublishedReleaseVersions = [
                            new PublishedReleaseVersionInfo
                            {
                                ReleaseId = _releaseVersion1.ReleaseId,
                                ReleaseVersionId = _releaseVersion1.Id,
                                ReleaseSlug = _releaseVersion1.Release.Slug,
                                PublicationId = _releaseVersion1.Release.PublicationId
                            }
                        ],
                        IsPublicationArchived = false
                    };

                    _publisherEventRaiser.Assert.ReleaseVersionPublishedEventWasRaised(evt => evt == expectedInfo);
                }

                [Fact]
                public async Task GivenReleaseAlreadyPublished_WhenReleaseVersionIsPublished_ThenEventIsRaisedWithPreviousReleaseId()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();
                    var previousReleaseVersion = new ReleaseVersionBuilder()
                        .WithPublicationId(PublicationId1)
                        .ForRelease(release => release
                            .WithReleaseSlug("release-slug-previous"))
                        .Build();

                    // Set publication 1 to have a published release prior to the current publishing run
                    WherePreviousPublicationLatestPublishedReleaseVersionIs(publicationId: PublicationId1, releaseVersion: previousReleaseVersion);

                    // Set release version 1 to be the latest published release version after publishing
                    _releaseService.WherePublicationLatestPublishedReleaseVersionIs(PublicationId1, _releaseVersion1);

                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    var expectedInfo = new PublishedPublicationInfo
                    {
                        PublicationId = _releaseVersion1.Release.PublicationId,
                        PublicationSlug = _releaseVersion1.Release.Publication.Slug,
                        LatestPublishedReleaseId = _releaseVersion1.ReleaseId,
                        LatestPublishedReleaseVersionId = _releaseVersion1.Id,
                        PreviousLatestPublishedReleaseId = previousReleaseVersion.ReleaseId, // Assert info contains previous release id
                        PreviousLatestPublishedReleaseVersionId = previousReleaseVersion.Id, // Assert info contains previous release version id
                        PublishedReleaseVersions = [
                            new PublishedReleaseVersionInfo
                            {
                                ReleaseId = _releaseVersion1.ReleaseId,
                                ReleaseVersionId = _releaseVersion1.Id,
                                ReleaseSlug = _releaseVersion1.Release.Slug,
                                PublicationId = _releaseVersion1.Release.PublicationId
                            }
                        ],
                        IsPublicationArchived = false
                    };
                    _publisherEventRaiser.Assert.ReleaseVersionPublishedEventWasRaised(evt => evt == expectedInfo);
                }

                [Fact]
                public async Task WhenPublishedReleaseVersionIsNotLatestVersion_ThenEventIsRaised()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();

                    var releaseVersionId1V2 = Guid.NewGuid();
                    var releaseVersion1V2 = new ReleaseVersionBuilder(releaseVersionId1V2)
                        .ForRelease(_releaseVersion1.Release)
                        .Build();

                    // Set publication 1 to have published release version 1V2 prior to the current publishing run
                    WherePreviousPublicationLatestPublishedReleaseVersionIs(publicationId: PublicationId1, releaseVersion: releaseVersion1V2);

                    // Set release version 1V2 to be the latest after publishing, not release version 1
                    _releaseService.WherePublicationLatestPublishedReleaseVersionIs(PublicationId1, releaseVersion1V2);

                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    var expectedInfo = new PublishedPublicationInfo
                    {
                        PublicationId = _releaseVersion1.Release.PublicationId,
                        PublicationSlug = _releaseVersion1.Release.Publication.Slug,
                        LatestPublishedReleaseId = releaseVersion1V2.ReleaseId,
                        LatestPublishedReleaseVersionId = releaseVersionId1V2, // Assert latest release version is still 1v2
                        PreviousLatestPublishedReleaseId = releaseVersion1V2.ReleaseId,
                        PreviousLatestPublishedReleaseVersionId = releaseVersion1V2.Id,
                        PublishedReleaseVersions = [
                            new PublishedReleaseVersionInfo
                            {
                                ReleaseId = _releaseVersion1.ReleaseId,
                                ReleaseVersionId = _releaseVersion1.Id,
                                ReleaseSlug = _releaseVersion1.Release.Slug,
                                PublicationId = _releaseVersion1.Release.PublicationId
                            }
                        ],
                        IsPublicationArchived = false
                    };

                    _publisherEventRaiser.Assert.ReleaseVersionPublishedEventWasRaised(evt => evt == expectedInfo);
                }

                [Fact]
                public async Task GivenASupersededPublication_WhenPublicationIsNotAlreadyPublished_ThenPublicationArchivedEventRaised()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();

                    // Create a publication that is superseded by publication 1.
                    // Publication 1 has no published releases prior to the current publishing run
                    var supersededPublication = new PublicationBuilder(Guid.NewGuid(), "publication-slug-superseded")
                        .SupersededBy(_releaseVersion1.Release.Publication.Id)
                        .Build();
                    _contentDbContext.With(supersededPublication);

                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    _publisherEventRaiser.Assert.PublicationArchivedEventWasRaised(supersededPublication);
                }

                [Fact]
                public async Task GivenASupersededPublication_WhenPublicationIsAlreadyPublished_ThenPublicationArchivedEventNotRaised()
                {
                    // ARRANGE
                    var readyKeys = SetupHappyPath();

                    // Set publication 1 to have a published release prior to the current publishing run
                    WherePreviousPublicationLatestPublishedReleaseVersionIs(
                        publicationId: PublicationId1,
                        releaseVersion: new ReleaseVersionBuilder()
                            .WithPublicationId(PublicationId1)
                            .ForRelease(release => release
                                .WithReleaseSlug("release-slug-previous"))
                            .Build());

                    // Create a publication that is superseded by publication 1
                    var supersededPublication = new PublicationBuilder(Guid.NewGuid(), "publication-slug-superseded")
                        .SupersededBy(_releaseVersion1.Release.Publication.Id)
                        .Build();
                    _contentDbContext.With(supersededPublication);

                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    _publisherEventRaiser.Assert.PublicationArchivedEventWasNotRaised();
                }


                [Fact]
                public async Task GivenASupersededPublication_WhenReleasePublished_ThenIsPublicationArchivedIsTrue()
                {
                    // ARRANGE
                    // Create a publication that supersedes publication 1 - making publication 1 archived
                    var supersedingPublication = new PublicationBuilder(Guid.NewGuid(), "publication-slug-superseded")
                        .HasPublishedReleaseVersion()
                        .Build();
                    _contentDbContext.With(supersedingPublication);

                    var readyKeys = SetupHappyPath(
                        publication1 => publication1.SupersededBy(supersedingPublication));

                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(readyKeys);

                    // ASSERT
                    _publisherEventRaiser.Assert.ReleaseVersionPublishedEventWasRaised(info =>
                        info.PublicationId == _releaseVersion1.Release.PublicationId
                        && info.IsPublicationArchived);
                }
            }

            private void WherePreviousPublicationLatestPublishedReleaseVersionIs(Guid publicationId, ReleaseVersion releaseVersion)
            {
                _contentDbContext.With(releaseVersion);
                _contentDbContext.WherePublication(publicationId, publication => publication.LatestPublishedReleaseVersionId = releaseVersion.Id);
            }
        }
    }
}
