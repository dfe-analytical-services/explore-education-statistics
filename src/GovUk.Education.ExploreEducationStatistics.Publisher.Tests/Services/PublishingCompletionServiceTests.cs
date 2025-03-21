using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public class PublishingCompletionServiceTests
{
    private readonly ContentDbContextBuilder _contentDbContextBuilder = new();
    private readonly ContentServiceBuilder _contentServiceBuilder = new();
    private readonly MethodologyServiceBuilder _methodologyServiceBuilder = new();
    private readonly NotificationsServiceBuilder _notificationsServiceBuilder = new();
    private readonly ReleasePublishingStatusServiceBuilder _releasePublishingStatusServiceBuilder = new();
    private readonly PublicationCacheServiceBuilder _publicationCacheServiceBuilder = new();
    private readonly ReleaseServiceBuilder _releaseServiceBuilder = new();
    private readonly RedirectsCacheServiceBuilder _redirectsCacheServiceBuilder = new();
    private readonly DataSetPublishingServiceBuilder _dataSetPublishingServiceBuilder = new();
    private readonly EventRaiserServiceBuilder _eventRaiserServiceBuilder = new();

    private PublishingCompletionService GetSut() =>
        new(
            _contentDbContextBuilder.Build(),
            _contentServiceBuilder.Build(),
            _methodologyServiceBuilder.Build(),
            _notificationsServiceBuilder.Build(),
            _releasePublishingStatusServiceBuilder.Build(),
            _publicationCacheServiceBuilder.Build(),
            _releaseServiceBuilder.Build(),
            _redirectsCacheServiceBuilder.Build(),
            _dataSetPublishingServiceBuilder.Build(),
            _eventRaiserServiceBuilder.Build());

    public class BasicTests : PublishingCompletionServiceTests
    {
        [Fact]
        public void Can_instantiate_Sut() => Assert.NotNull(GetSut());
    }
    
    public class ContainerRegistrationTests : PublishingCompletionServiceTests
    {
        [Fact]
        public void EnsureDependenciesCanBeResolved()
        {
            var host = new HostBuilder().ConfigurePublisherHostBuilder().Build();
            
            Assert.All(
        [
                    () => Assert.NotNull(host.Services.GetRequiredService<ContentDbContext>()),
                    () => Assert.NotNull(host.Services.GetRequiredService<IContentService>()),
                    () => Assert.NotNull(host.Services.GetRequiredService<IMethodologyService>()),
                    () => Assert.NotNull(host.Services.GetRequiredService<INotificationsService>()),
                    () => Assert.NotNull(host.Services.GetRequiredService<IReleasePublishingStatusService>()),
                    () => Assert.NotNull(host.Services.GetRequiredService<IPublicationCacheService>()),
                    () => Assert.NotNull(host.Services.GetRequiredService<IReleaseService>()),
                    () => Assert.NotNull(host.Services.GetRequiredService<IRedirectsCacheService>()),
                    () => Assert.NotNull(host.Services.GetRequiredService<IDataSetPublishingService>()),
                    () => Assert.NotNull(host.Services.GetRequiredService<IEventRaiserService>()),
                ],
                assertion => assertion()
            );
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
                
                var releasePublishingKeys = new List<ReleasePublishingKey>()
                {
                    releasePublishingKey1, 
                    releasePublishingKey2
                };

                _releasePublishingStatusServiceBuilder
                    .WhereGetReturns(releasePublishingKey1, releasePublishingStatus1)
                    .WhereGetReturns(releasePublishingKey2, releasePublishingStatus2);

                var sut = GetSut();

                // ACT
                await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                // ASSERT
                _releasePublishingStatusServiceBuilder.Assert.UpdatePublishingStageWasNotCalled();
            }
        }

        public class ReadyTests : CompletePublishingIfAllPriorStagesCompleteTests
        {
            private const string ReleaseVersionId1String = "00000001-0000-0000-0000-000000000000";
            private const string ReleaseVersionId11String = "00000011-0000-0000-0000-000000000000";
            private const string ReleaseVersionId2String = "00000002-0000-0000-0000-000000000000";
            
            private static readonly Guid ReleaseVersionId1 = Guid.Parse(ReleaseVersionId1String);
            private static readonly Guid ReleaseVersionId11 = Guid.Parse(ReleaseVersionId11String);
            private static readonly Guid ReleaseVersionId2 = Guid.Parse(ReleaseVersionId2String);

            private const string ReleaseId1String = "00000001-0000-0000-0000-000000000000";
            private const string ReleaseId2String = "00000002-0000-0000-0000-000000000000";

            private static readonly Guid ReleaseId1 = Guid.Parse(ReleaseId1String);
            private static readonly Guid ReleaseId2 = Guid.Parse(ReleaseId2String);

            private static readonly Guid PublicationId1 = Guid.Parse("00000000-0000-0001-0000-000000000000");
            private static readonly Guid PublicationId2 = Guid.Parse("00000000-0000-0002-0000-000000000000");
            private static readonly Guid PublicationId22 = Guid.Parse("00000000-0000-0022-0000-000000000000");
            
            private readonly ReleasePublishingKey[] _readyReleasePublishingKeys =
            [
                new(ReleaseVersionId1, Guid.Parse("00000000-0001-0000-0000-000000000000")), 
                new(ReleaseVersionId2, Guid.Parse("00000000-0002-0000-0000-000000000000")), 
            ];

            private readonly ReleasePublishingKey[] _notReadyReleasePublishingKeys =
            [
                new(Guid.Parse("00000003-0000-0000-0000-000000000000"), Guid.Parse("00000000-0003-0000-0000-000000000000")), 
                new(Guid.Parse("00000004-0000-0000-0000-000000000000"), Guid.Parse("00000000-0004-0000-0000-000000000000")),
            ];

            private Publication _publication1 = null!;
            private Publication _publication2 = null!;
            private Publication _publication22SupercededBy2 = null!;
            private ReleaseVersion _releaseVersion1 = null!;
            private ReleaseVersion _releaseVersion2 = null!;
            private ReleaseVersion _releaseVersion11 = null!;
            private readonly string _publicationSlug1 = "publication-slug-1";
            private readonly string _publicationSlug2 = "publication-slug-2";
            private readonly string _releaseSlug1 = "release-slug-1";
            private readonly string _releaseSlug2 = "release-slug-2";
            
            private IReadOnlyList<ReleasePublishingKey> SetupHappyPath()
            {
                var releasePublishingKeys = 
                    _readyReleasePublishingKeys
                    .Concat(_notReadyReleasePublishingKeys)
                    .ToList();
                
                foreach (var readyKey in _readyReleasePublishingKeys)
                {
                    _releasePublishingStatusServiceBuilder.WhereGetReturns(
                        readyKey,
                        new ReleasePublishingStatusBuilder(readyKey).Build());
                }
                foreach (var notReadyKey in _notReadyReleasePublishingKeys)
                {
                    _releasePublishingStatusServiceBuilder.WhereGetReturns(
                        notReadyKey,
                        new ReleasePublishingStatusBuilder(notReadyKey)
                            .WhereContentStatusIs(ReleasePublishingStatusContentStage.Scheduled)
                            .Build());
                }

                _releaseVersion1 = new ReleaseVersionBuilder(ReleaseVersionId1)
                    .WithPublicationId(PublicationId1)
                    .ForRelease(release => release
                        .WithReleaseId(ReleaseId1)
                        .WithReleaseSlug(_releaseSlug1))
                    .Build();

                _releaseVersion11 = new ReleaseVersionBuilder(ReleaseVersionId11)
                    .WithPublicationId(PublicationId1)
                    .ForRelease(release => release
                        .WithReleaseId(ReleaseId1)
                        .WithReleaseSlug(_releaseSlug1))
                    .Build();

                _releaseVersion2 = new ReleaseVersionBuilder(ReleaseVersionId2)
                    .WithPublicationId(PublicationId2)
                    .ForRelease(release => release
                        .WithReleaseId(ReleaseId2)
                        .WithReleaseSlug(_releaseSlug2))
                    .Build();

                _publication1 = new PublicationBuilder(PublicationId1, _publicationSlug1).Build();
                _publication2 = new PublicationBuilder(PublicationId2, _publicationSlug2).Build();
                _publication22SupercededBy2 = new PublicationBuilder(PublicationId22, "publication-slug-22").SupersededBy(_publication2.Id).Build();
                
                _releaseServiceBuilder.WhereGetReturns(ReleaseVersionId1, _releaseVersion1);
                _releaseServiceBuilder.WhereGetReturns(ReleaseVersionId2, _releaseVersion2);
                _methodologyServiceBuilder.WhereGetLatestVersionByReleaseReturnsNoMethodologies(_releaseVersion1);
                _methodologyServiceBuilder.WhereGetLatestVersionByReleaseReturnsNoMethodologies(_releaseVersion2);
                _contentDbContextBuilder.With(_releaseVersion1);
                _contentDbContextBuilder.With(_releaseVersion2);
                _contentDbContextBuilder.With(_publication1);
                _contentDbContextBuilder.With(_publication2);
                _contentDbContextBuilder.With(_publication22SupercededBy2);
                _releaseServiceBuilder.WherePublicationLatestPublishedReleaseVersionIs(PublicationId1, _releaseVersion1);
                _releaseServiceBuilder.WherePublicationLatestPublishedReleaseVersionIs(PublicationId2, _releaseVersion2);
                
                return releasePublishingKeys;
            }

            public class UpdatePublishingStageTests : ReadyTests
            {
                [Fact]
                public async Task PublishingStatusSetToStarted()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    foreach (var readyKey in _readyReleasePublishingKeys)
                    {
                        _releasePublishingStatusServiceBuilder.Assert.UpdatePublishingStageWasCalled(
                            readyKey,
                            ReleasePublishingStatusPublishingStage.Started);
                    }

                    foreach (var notReadyKey in _notReadyReleasePublishingKeys)
                    {
                        _releasePublishingStatusServiceBuilder.Assert.UpdatePublishingStageWasNotCalled(notReadyKey);
                    }
                }
                
                [Fact]
                public async Task PublishingStatusSetToComplete()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    foreach (var readyKey in _readyReleasePublishingKeys)
                    {
                        _releasePublishingStatusServiceBuilder.Assert.UpdatePublishingStageWasCalled(
                            readyKey,
                            ReleasePublishingStatusPublishingStage.Complete);
                    }

                    foreach (var notReadyKey in _notReadyReleasePublishingKeys)
                    {
                        _releasePublishingStatusServiceBuilder.Assert.UpdatePublishingStageWasNotCalled(notReadyKey);
                    }
                }
            }
            
            public class CompletePublishingTests : ReadyTests
            {
                [Fact]
                public async Task CompletePublishingCalledOnReleaseService()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    var sut = GetSut();
                    
                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    foreach (var readyKey in _readyReleasePublishingKeys)
                    {
                        _releaseServiceBuilder.Assert.CompletePublishingWasCalled(readyKey.ReleaseVersionId);
                    }
                }
            }

            public class MethodologiesTests : ReadyTests
            {
                private void SetupNoMethodologies(Guid releaseVersionId)
                {
                    var releaseVersion = _releaseServiceBuilder.Get(releaseVersionId);
                    _methodologyServiceBuilder.WhereGetLatestVersionByReleaseReturnsNoMethodologies(releaseVersion);
                }
                
                private void SetupMethodologies(Guid releaseVersionId, MethodologyVersion methodologyToPublish)
                {
                    var releaseVersion = _releaseServiceBuilder.Get(releaseVersionId);
                    _methodologyServiceBuilder.WhereGetLatestVersionByReleaseReturns(releaseVersion, methodologyToPublish);
                    _methodologyServiceBuilder.WhereIsBeingPublishedAlongsideRelease(methodologyToPublish, releaseVersion);
                }
                
                private void SetupMethodologies(Guid releaseVersionId, MethodologyVersion methodologyToPublish, MethodologyVersion methodologyNotToPublish)
                {
                    var releaseVersion = _releaseServiceBuilder.Get(releaseVersionId);
                    _methodologyServiceBuilder.WhereGetLatestVersionByReleaseReturns(releaseVersion, methodologyToPublish, methodologyNotToPublish);
                    _methodologyServiceBuilder.WhereIsBeingPublishedAlongsideRelease(methodologyToPublish, releaseVersion);
                    _methodologyServiceBuilder.WhereIsNotBeingPublishedAlongsideRelease(methodologyNotToPublish, releaseVersion);
                }

                [Fact]
                public async Task WhenNoMethodologies_ThenNoMethodologiesPublished()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    foreach (var key in _readyReleasePublishingKeys)
                    {
                        SetupNoMethodologies(key.ReleaseVersionId);
                    }
                    
                    var sut = GetSut();
                    
                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    _methodologyServiceBuilder.Assert.NoMethodologiesPublished();
                }
                
                [Fact]
                public async Task WhenHasMethodology_ThenMethodologyPublished()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    var methodologies = new MethodologyVersion[_readyReleasePublishingKeys.Length];
                    
                    for (var i = 0; i < _readyReleasePublishingKeys.Length; i++)
                    {
                        var key = _readyReleasePublishingKeys[i];
                        methodologies[i] = new MethodologyVersion();
                        SetupMethodologies(key.ReleaseVersionId, methodologies[i]);
                    }

                    var sut = GetSut();
                    
                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    foreach (var methodology in methodologies)
                    {
                        _methodologyServiceBuilder.Assert.MethodologyPublished(methodology);
                    }
                }
                
                [Fact]
                public async Task WhenHasSomeMethodologiesToPublish_ThenOnlyMethodologiesToBePublishedArePublished()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    var methodologiesToPublish = new MethodologyVersion[_readyReleasePublishingKeys.Length];
                    var methodologiesNotToPublish = new MethodologyVersion[_readyReleasePublishingKeys.Length];
                    
                    for (var i = 0; i < _readyReleasePublishingKeys.Length; i++)
                    {
                        var key = _readyReleasePublishingKeys[i];
                        methodologiesToPublish[i] = new MethodologyVersion();
                        methodologiesNotToPublish[i] = new MethodologyVersion();
                        SetupMethodologies(key.ReleaseVersionId, methodologiesToPublish[i], methodologiesNotToPublish[i]);
                    }

                    var sut = GetSut();
                    
                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    foreach (var methodology in methodologiesToPublish)
                    {
                        _methodologyServiceBuilder.Assert.MethodologyPublished(methodology);
                    }
                    foreach (var methodology in methodologiesNotToPublish)
                    {
                        _methodologyServiceBuilder.Assert.MethodologyNotPublished(methodology);
                    }
                }
            }

            public class UpdatePublicationTests : ReadyTests
            {
                [Fact]
                public async Task WhenPublishedReleaseVersionIsLatestVersion_ThenPublicationLatestReleaseVersionSetToIt()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    var sut = GetSut();
                    
                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    _contentDbContextBuilder.Assert.PublicationsLatestPublishedReleaseVersionIdIs(PublicationId1, ReleaseVersionId1);
                    _contentDbContextBuilder.Assert.PublicationsLatestPublishedReleaseVersionIdIs(PublicationId2, ReleaseVersionId2);
                }
                
                [Fact]
                public async Task WhenPublishedReleaseVersionIsNotLatestVersion_ThenPublicationLatestReleaseVersionSetToLatestReleaseVersion()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    
                    // Set that release version 11 is the latest, not release version 1
                    _releaseServiceBuilder.WherePublicationLatestPublishedReleaseVersionIs(PublicationId1, _releaseVersion11);
                    
                    var sut = GetSut();
                    
                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    _contentDbContextBuilder.Assert.PublicationsLatestPublishedReleaseVersionIdIs(PublicationId1, ReleaseVersionId11);
                }
            }
            
            public class UpdatePublicationCacheTests : ReadyTests
            {
                [Fact]
                public async Task AllPublicationsUpdatedInCache()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    var sut = GetSut();
                    
                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    _publicationCacheServiceBuilder.Assert.PublicationUpdated(_publication1.Slug);
                    _publicationCacheServiceBuilder.Assert.PublicationUpdated(_publication2.Slug);
                    _publicationCacheServiceBuilder.Assert.PublicationUpdated(_publication22SupercededBy2.Slug);
                }
            }

            public class ContentServiceTests : ReadyTests
            {
                [Fact]
                public async Task PreviousVersionsDownloadFilesDeleted()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    _contentServiceBuilder.Assert.DeletePreviousVersionsDownloadFilesCalled(ReleaseVersionId1, ReleaseVersionId2);
                }
                
                [Fact]
                public async Task PreviousVersionsContentDeleted()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    _contentServiceBuilder.Assert.DeletePreviousVersionsContentCalled(ReleaseVersionId1, ReleaseVersionId2);
                }
                
                [Fact]
                public async Task CachedTaxonomyBlobsUpdated()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    _contentServiceBuilder.Assert.UpdateCachedTaxonomyBlobsCalled();
                }
            }

            public class NotificationServiceTests : ReadyTests
            {
                [Fact]
                public async Task NotifySubscribersIfApplicable()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    _notificationsServiceBuilder.Assert.NotifySubscribersIfApplicableCalled(ReleaseVersionId1, ReleaseVersionId2);
                }
            }
            
            public class RedirectsCacheServiceTests : ReadyTests
            {
                [Fact]
                public async Task RedirectsUpdated()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    _redirectsCacheServiceBuilder.Assert.UpdateRedirectsCalled();
                }
            }

            public class DataSetPublishingServiceTests : ReadyTests
            {
                [Fact]
                public async Task DataSetsWerePublished()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    var sut = GetSut();

                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    _dataSetPublishingServiceBuilder.Assert.DataSetsWerePublished(ReleaseVersionId1, ReleaseVersionId2);
                }
            }
            
            public class EventRaiserTests : ReadyTests
            {
                [Fact]
                public async Task WhenReleaseVersionIsPublished_ThenEventIsRaised()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    var sut = GetSut();
                    
                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    var expectedInfo = new PublishingCompletionService.PublishedReleaseVersionInfo
                    {
                        ReleaseVersionId = ReleaseVersionId1,
                        ReleaseId = ReleaseId1,
                        ReleaseSlug = _releaseSlug1,
                        PublicationId = PublicationId1,
                        PublicationSlug = _publicationSlug1,
                        PublicationLatestPublishedReleaseVersionId = ReleaseVersionId1
                    };
                    _eventRaiserServiceBuilder.Assert.EventWasRaised(evt => evt == expectedInfo); 
                }
                
                [Fact]
                public async Task WhenPublishedReleaseVersionIsNotLatestVersion_ThenEventIsRaised()
                {
                    // ARRANGE
                    var releasePublishingKeys = SetupHappyPath();
                    
                    // Set that release version 11 is the latest, not release version 1
                    _releaseServiceBuilder.WherePublicationLatestPublishedReleaseVersionIs(PublicationId1, _releaseVersion11);
                    
                    var sut = GetSut();
                    
                    // ACT
                    await sut.CompletePublishingIfAllPriorStagesComplete(releasePublishingKeys);

                    // ASSERT
                    var expectedInfo = new PublishingCompletionService.PublishedReleaseVersionInfo
                    {
                        ReleaseVersionId = ReleaseVersionId1,
                        ReleaseId = ReleaseId1,
                        ReleaseSlug = _releaseSlug1,
                        PublicationId = PublicationId1,
                        PublicationSlug = _publicationSlug1,
                        PublicationLatestPublishedReleaseVersionId = ReleaseVersionId11 // Assert latest release is different
                    };
                    _eventRaiserServiceBuilder.Assert.EventWasRaised(evt => evt == expectedInfo); 
                }
            }
        }
    }
}
