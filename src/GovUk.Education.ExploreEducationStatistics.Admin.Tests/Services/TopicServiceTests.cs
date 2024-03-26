#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using Theme = GovUk.Education.ExploreEducationStatistics.Content.Model.Theme;
using Topic = GovUk.Education.ExploreEducationStatistics.Content.Model.Topic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class TopicServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task CreateTopic()
    {
        var theme = new Theme
        {
            Title = "Test theme",
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            context.Add(theme);

            await context.SaveChangesAsync();
        }

        var publishingService = new Mock<IPublishingService>(Strict);

        publishingService.Setup(s => s.TaxonomyChanged())
            .ReturnsAsync(Unit.Instance);

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            var service = SetupTopicService(contentContext: context,
                publishingService: publishingService.Object);

            var result = await service.CreateTopic(
                new TopicSaveViewModel
                {
                    Title = "Test topic",
                    ThemeId = theme.Id
                }
            );

            VerifyAllMocks(publishingService);

            Assert.True(result.IsRight);
            Assert.Equal("Test topic", result.Right.Title);
            Assert.Equal("test-topic", result.Right.Slug);
            Assert.Equal(theme.Id, result.Right.ThemeId);

            var savedTopic = await context.Topics.FindAsync(result.Right.Id);

            Assert.NotNull(savedTopic);
            Assert.Equal("Test topic", savedTopic!.Title);
            Assert.Equal("test-topic", savedTopic.Slug);
            Assert.Equal(theme.Id, savedTopic.ThemeId);
        }
    }

    [Fact]
    public async Task CreateTopic_FailsNonExistingTheme()
    {
        await using var context = DbUtils.InMemoryApplicationDbContext();

        var service = SetupTopicService(context);

        var result = await service.CreateTopic(
            new TopicSaveViewModel
            {
                Title = "Test topic",
                ThemeId = Guid.NewGuid()
            }
        );

        result.AssertBadRequest(ThemeDoesNotExist);
    }

    [Fact]
    public async Task CreateTopic_FailsNonUniqueSlug()
    {
        var theme = new Theme
        {
            Title = "Test theme",
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            context.Add(theme);
            context.Add(
                new Topic
                {
                    Title = "Test topic",
                    Slug = "test-topic",
                    Theme = theme
                }
            );

            await context.SaveChangesAsync();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            var service = SetupTopicService(context);

            var result = await service.CreateTopic(
                new TopicSaveViewModel
                {
                    Title = "Test topic",
                    ThemeId = theme.Id
                }
            );

            result.AssertBadRequest(SlugNotUnique);
        }
    }

    [Fact]
    public async Task UpdateTopic()
    {
        var theme = new Theme
        {
            Title = "New theme",
        };

        var topic = new Topic
        {
            Title = "Old title",
            Slug = "old-title",
            Theme = new Theme
            {
                Title = "Old theme"
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            context.Add(theme);
            context.Add(topic);

            await context.SaveChangesAsync();
        }

        var publishingService = new Mock<IPublishingService>(Strict);

        publishingService.Setup(s => s.TaxonomyChanged())
            .ReturnsAsync(Unit.Instance);

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            var service = SetupTopicService(contentContext: context,
                publishingService: publishingService.Object);

            var result = await service.UpdateTopic(
                topic.Id,
                new TopicSaveViewModel
                {
                    Title = "New title",
                    ThemeId = theme.Id
                }
            );

            VerifyAllMocks(publishingService);

            Assert.True(result.IsRight);
            Assert.Equal(topic.Id, result.Right.Id);
            Assert.Equal("New title", result.Right.Title);
            Assert.Equal("new-title", result.Right.Slug);
            Assert.Equal(theme.Id, result.Right.ThemeId);

            var savedTopic = await context.Topics.FindAsync(result.Right.Id);

            Assert.NotNull(savedTopic);
            Assert.Equal("New title", savedTopic.Title);
            Assert.Equal("new-title", savedTopic.Slug);
            Assert.Equal(theme.Id, savedTopic.ThemeId);
        }
    }

    [Fact]
    public async Task UpdateTopic_FailsNonExistingTheme()
    {
        var topic = new Topic
        {
            Title = "Old title",
            Slug = "old-title",
            Theme = new Theme
            {
                Title = "Old theme"
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            context.Add(topic);
            await context.SaveChangesAsync();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            var service = SetupTopicService(context);

            var result = await service.UpdateTopic(
                topic.Id,
                new TopicSaveViewModel
                {
                    Title = "Test topic",
                    ThemeId = Guid.NewGuid()
                }
            );

            result.AssertBadRequest(ThemeDoesNotExist);
        }
    }

    [Fact]
    public async Task UpdateTopic_FailsNonUniqueSlug()
    {
        var theme = new Theme
        {
            Title = "Test theme",
        };
        var topic = new Topic
        {
            Title = "Old title",
            Slug = "old-title",
            Theme = theme
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            context.Add(topic);
            context.Add(
                new Topic
                {
                    Title = "Other topic",
                    Slug = "other-topic",
                    Theme = theme
                }
            );

            await context.SaveChangesAsync();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            var service = SetupTopicService(context);

            var result = await service.UpdateTopic(
                topic.Id,
                new TopicSaveViewModel
                {
                    Title = "Other topic",
                    ThemeId = topic.ThemeId
                }
            );

            result.AssertBadRequest(SlugNotUnique);
        }
    }

    [Fact]
    public async Task GetTopic()
    {
        var topic = new Topic
        {
            Title = "Test topic",
            Slug = "test-topic",
            Theme = new Theme
            {
                Title = "Test theme"
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            context.Add(topic);
            await context.SaveChangesAsync();
        }

        await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
        {
            var service = SetupTopicService(context);

            var result = await service.GetTopic(topic.Id);

            Assert.True(result.IsRight);

            Assert.Equal(topic.Id, result.Right.Id);
            Assert.Equal("Test topic", result.Right.Title);
            Assert.Equal(topic.ThemeId, result.Right.ThemeId);
        }
    }

    [Fact]
    public async Task DeleteTopic()
    {
        var releaseVersionId = Guid.NewGuid();

        var topic = new Topic
        {
            Id = Guid.NewGuid(),
            Title = "UI test topic to delete"
        };

        var dataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestDraftVersion(_fixture
                .DefaultDataBlockVersion()
                .WithReleaseVersionId(releaseVersionId)
                .Generate())
            .WithLatestPublishedVersion(_fixture
                .DefaultDataBlockVersion()
                .WithReleaseVersionId(releaseVersionId)
                .Generate())
            .Generate();

        var releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithId(releaseVersionId)
            .WithPublication(
                _fixture
                    .DefaultPublication()
                    .WithTopic(topic)
                    .Generate())
            .WithDataBlockVersions(ListOf(
                dataBlockParent.LatestDraftVersion!,
                dataBlockParent.LatestPublishedVersion!))
            .Generate();

        var methodology = _fixture
            .DefaultMethodology()
            .WithOwningPublication(releaseVersion.Publication)
            .Generate();

        var statsReleaseVersion = new Data.Model.ReleaseVersion
        {
            Id = releaseVersionId,
            PublicationId = releaseVersion.Publication.Id
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
        await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
        {
            contentContext.ReleaseVersions.Add(releaseVersion);
            contentContext.Methodologies.Add(methodology);
            statisticsContext.ReleaseVersion.Add(statsReleaseVersion);

            await contentContext.SaveChangesAsync();
            await statisticsContext.SaveChangesAsync();
        }

        await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
        await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
        {
            Assert.Equal(1, contentContext.Publications.Count());
            Assert.Equal(1, contentContext.Topics.Count());
            Assert.Equal(1, contentContext.PublicationMethodologies.Count());
            Assert.Equal(1, contentContext.ReleaseVersions.Count());
            Assert.Equal(2, contentContext.DataBlockVersions.Count());
            Assert.Equal(1, contentContext.DataBlockParents.Count());
            Assert.Equal(1, statisticsContext.ReleaseVersion.Count());
        }

        var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
        var releaseFileService = new Mock<IReleaseFileService>(Strict);
        var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);
        var methodologyService = new Mock<IMethodologyService>(Strict);
        var publishingService = new Mock<IPublishingService>(Strict);
        var cacheService = new Mock<IBlobCacheService>(Strict);
        var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

        await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
        await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupTopicService(
                contentContext,
                statisticsContext,
                releaseDataFileService: releaseDataFileService.Object,
                releaseFileService: releaseFileService.Object,
                releaseSubjectRepository: releaseSubjectRepository.Object,
                methodologyService: methodologyService.Object,
                publishingService: publishingService.Object,
                cacheService: cacheService.Object,
                releasePublishingStatusRepository: releasePublishingStatusRepository.Object);

            releaseDataFileService
                .Setup(s => s.DeleteAll(releaseVersionId, true))
                .ReturnsAsync(Unit.Instance);

            releaseFileService
                .Setup(s => s.DeleteAll(releaseVersionId, true))
                .ReturnsAsync(Unit.Instance);

            releaseSubjectRepository
                .Setup(s => s.DeleteAllReleaseSubjects(releaseVersionId, false))
                .Returns(Task.CompletedTask);

            methodologyService
                .Setup(s => s.DeleteMethodology(methodology.Id, true))
                .ReturnsAsync(Unit.Instance);

            publishingService.Setup(s => s.TaxonomyChanged())
                .ReturnsAsync(Unit.Instance);

            cacheService
                .Setup(s =>
                    s.DeleteCacheFolderAsync(
                        ItIs.DeepEqualTo(new PrivateReleaseContentFolderCacheKey(releaseVersionId))))
                .Returns(Task.CompletedTask);

            releasePublishingStatusRepository.Setup(mock =>
                    mock.RemovePublisherReleaseStatuses(new List<Guid> { releaseVersion.Id }))
                .Returns(Task.CompletedTask);

            var result = await service.DeleteTopic(topic.Id);
            VerifyAllMocks(releaseDataFileService,
                releaseFileService,
                releaseSubjectRepository,
                methodologyService,
                publishingService,
                cacheService,
                releasePublishingStatusRepository);

            result.AssertRight();

            Assert.Equal(0, contentContext.Publications.Count());
            Assert.Equal(0, contentContext.Topics.Count());
            Assert.Equal(0, contentContext.ReleaseVersions.Count());
            Assert.Equal(0, contentContext.DataBlockVersions.Count());
            Assert.Equal(0, contentContext.DataBlockParents.Count());
            Assert.Equal(0, statisticsContext.ReleaseVersion.Count());
        }
    }

    [Fact]
    public async Task DeleteTopic_ReleaseVersionsDeletedInCorrectOrder()
    {
        var topicId = Guid.NewGuid();
        var publicationId = Guid.NewGuid();

        var releaseVersion1Id = Guid.NewGuid();
        var releaseVersion2Id = Guid.NewGuid();
        var releaseVersion3Id = Guid.NewGuid();
        var releaseVersion4Id = Guid.NewGuid();

        var releaseVersionIdsInExpectedDeleteOrder =
            AsList(releaseVersion4Id, releaseVersion3Id, releaseVersion2Id, releaseVersion1Id);

        var topic = new Topic
        {
            Id = topicId,
            Title = "UI test topic"
        };

        var publication = new Publication
        {
            Id = publicationId,
            Topic = topic,
            ReleaseVersions = AsList(new ReleaseVersion
                {
                    Id = releaseVersion2Id,
                    PreviousVersionId = releaseVersion1Id
                },
                new ReleaseVersion
                {
                    Id = releaseVersion1Id
                }, new ReleaseVersion
                {
                    Id = releaseVersion4Id,
                    PreviousVersionId = releaseVersion3Id
                }, new ReleaseVersion
                {
                    Id = releaseVersion3Id,
                    PreviousVersionId = releaseVersion2Id
                })
        };

        var statsReleases = AsList(new Data.Model.ReleaseVersion
            {
                Id = releaseVersion1Id,
                PublicationId = publicationId
            },
            new Data.Model.ReleaseVersion
            {
                Id = releaseVersion2Id,
                PublicationId = publicationId
            },
            new Data.Model.ReleaseVersion
            {
                Id = releaseVersion3Id,
                PublicationId = publicationId
            },
            new Data.Model.ReleaseVersion
            {
                Id = releaseVersion4Id,
                PublicationId = publicationId
            });

        var contextId = Guid.NewGuid().ToString();

        await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
        await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
        {
            contentContext.Publications.Add(publication);
            contentContext.Topics.Add(topic);
            statisticsContext.ReleaseVersion.AddRange(statsReleases);

            await contentContext.SaveChangesAsync();
            await statisticsContext.SaveChangesAsync();

            Assert.Equal(1, contentContext.Publications.Count());
            Assert.Equal(1, contentContext.Topics.Count());
            Assert.Equal(4, contentContext.ReleaseVersions.Count());
            Assert.Equal(4, statisticsContext.ReleaseVersion.Count());
        }

        var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
        var releaseFileService = new Mock<IReleaseFileService>(Strict);
        var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);
        var publishingService = new Mock<IPublishingService>(Strict);
        var cacheService = new Mock<IBlobCacheService>(Strict);
        var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

        await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
        await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupTopicService(
                contentContext,
                statisticsContext,
                releaseDataFileService: releaseDataFileService.Object,
                releaseFileService: releaseFileService.Object,
                releaseSubjectRepository: releaseSubjectRepository.Object,
                publishingService: publishingService.Object,
                cacheService: cacheService.Object,
                releasePublishingStatusRepository: releasePublishingStatusRepository.Object);

            var releaseDataFileDeleteSequence = new MockSequence();

            releaseVersionIdsInExpectedDeleteOrder.ForEach(releaseId =>
                releaseDataFileService
                    .InSequence(releaseDataFileDeleteSequence)
                    .Setup(s => s.DeleteAll(releaseId, true))
                    .ReturnsAsync(Unit.Instance));

            var releaseFileDeleteSequence = new MockSequence();

            releaseVersionIdsInExpectedDeleteOrder.ForEach(releaseId =>
                releaseFileService
                    .InSequence(releaseFileDeleteSequence)
                    .Setup(s => s.DeleteAll(releaseId, true))
                    .ReturnsAsync(Unit.Instance));

            var releaseSubjectDeleteSequence = new MockSequence();

            releaseVersionIdsInExpectedDeleteOrder.ForEach(releaseId =>
                releaseSubjectRepository
                    .InSequence(releaseSubjectDeleteSequence)
                    .Setup(s => s.DeleteAllReleaseSubjects(releaseId, false))
                    .Returns(Task.CompletedTask));

            var releaseCacheInvalidationSequence = new MockSequence();

            releaseVersionIdsInExpectedDeleteOrder.ForEach(releaseId =>
                cacheService
                    .InSequence(releaseCacheInvalidationSequence)
                    .Setup(s =>
                        s.DeleteCacheFolderAsync(
                            ItIs.DeepEqualTo(new PrivateReleaseContentFolderCacheKey(releaseId))))
                    .Returns(Task.CompletedTask));

            publishingService.Setup(s => s.TaxonomyChanged())
                .ReturnsAsync(Unit.Instance);

            releasePublishingStatusRepository.Setup(mock =>
                    mock.RemovePublisherReleaseStatuses(releaseVersionIdsInExpectedDeleteOrder))
                .Returns(Task.CompletedTask);

            var result = await service.DeleteTopic(topicId);
            VerifyAllMocks(
                releaseDataFileService,
                releaseFileService,
                releaseSubjectRepository,
                publishingService,
                cacheService,
                releasePublishingStatusRepository);

            result.AssertRight();

            Assert.Equal(0, contentContext.Publications.Count());
            Assert.Equal(0, contentContext.Topics.Count());
            Assert.Equal(0, contentContext.ReleaseVersions.Count());
            Assert.Equal(0, statisticsContext.ReleaseVersion.Count());
        }
    }

    [Fact]
    public async Task DeleteTopic_DisallowedByNamingConvention()
    {
        var topicId = Guid.NewGuid();
        var releaseVersionId = Guid.NewGuid();
        var publicationId = Guid.NewGuid();

        var topic = new Topic
        {
            Id = topicId,
            Title = "Non-confirming title"
        };

        var publication = new Publication
        {
            Id = publicationId,
            Topic = topic,
            ReleaseVersions = AsList(new ReleaseVersion
            {
                Id = releaseVersionId
            })
        };

        var statsReleaseVersion = new Data.Model.ReleaseVersion
        {
            Id = releaseVersionId,
            PublicationId = publicationId
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
        await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
        {
            contentContext.Publications.Add(publication);
            contentContext.Topics.Add(topic);
            statisticsContext.ReleaseVersion.Add(statsReleaseVersion);

            await contentContext.SaveChangesAsync();
            await statisticsContext.SaveChangesAsync();

            Assert.Equal(1, contentContext.Topics.Count());
            Assert.Equal(1, contentContext.Publications.Count());
            Assert.Equal(1, contentContext.ReleaseVersions.Count());
            Assert.Equal(1, statisticsContext.ReleaseVersion.Count());
        }

        await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
        await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupTopicService(contentContext, statisticsContext);

            var result = await service.DeleteTopic(topicId);
            result.AssertForbidden();

            Assert.Equal(1, contentContext.Topics.Count());
            Assert.Equal(1, contentContext.Publications.Count());
            Assert.Equal(1, contentContext.ReleaseVersions.Count());
            Assert.Equal(1, statisticsContext.ReleaseVersion.Count());
        }
    }

    [Fact]
    public async Task DeleteTopic_DisallowedByConfiguration()
    {
        var topicId = Guid.NewGuid();
        var releaseVersionId = Guid.NewGuid();
        var publicationId = Guid.NewGuid();

        var topic = new Topic
        {
            Id = topicId,
            Title = "UI test topic"
        };

        var publication = new Publication
        {
            Id = publicationId,
            Topic = topic,
            ReleaseVersions = AsList(new ReleaseVersion
            {
                Id = releaseVersionId
            })
        };

        var statsReleaseVersion = new Data.Model.ReleaseVersion
        {
            Id = releaseVersionId,
            PublicationId = publicationId
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
        await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
        {
            contentContext.Publications.Add(publication);
            contentContext.Topics.Add(topic);
            statisticsContext.ReleaseVersion.Add(statsReleaseVersion);

            await contentContext.SaveChangesAsync();
            await statisticsContext.SaveChangesAsync();

            Assert.Equal(1, contentContext.Topics.Count());
            Assert.Equal(1, contentContext.Publications.Count());
            Assert.Equal(1, contentContext.ReleaseVersions.Count());
            Assert.Equal(1, statisticsContext.ReleaseVersion.Count());
        }

        await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
        await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupTopicService(
                contentContext,
                statisticsContext,
                enableThemeDeletion: false);

            var result = await service.DeleteTopic(topicId);
            result.AssertForbidden();

            Assert.Equal(1, contentContext.Topics.Count());
            Assert.Equal(1, contentContext.Publications.Count());
            Assert.Equal(1, contentContext.ReleaseVersions.Count());
            Assert.Equal(1, statisticsContext.ReleaseVersion.Count());
        }
    }

    [Fact]
    public async Task DeleteTopic_OtherTopicsUnaffected()
    {
        var releaseVersionId = Guid.NewGuid();

        var topic = new Topic
        {
            Id = Guid.NewGuid(),
            Title = "UI test topic to delete"
        };

        var dataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestDraftVersion(_fixture
                .DefaultDataBlockVersion()
                .WithReleaseVersionId(releaseVersionId)
                .Generate())
            .WithLatestPublishedVersion(_fixture
                .DefaultDataBlockVersion()
                .WithReleaseVersionId(releaseVersionId)
                .Generate())
            .Generate();

        var releaseVersion = _fixture
            .DefaultReleaseVersion()
            .WithId(releaseVersionId)
            .WithPublication(
                _fixture
                    .DefaultPublication()
                    .WithTopic(topic)
                    .Generate())
            .WithDataBlockVersions(ListOf(
                dataBlockParent.LatestDraftVersion!,
                dataBlockParent.LatestPublishedVersion!))
            .Generate();

        var methodology = _fixture
            .DefaultMethodology()
            .WithOwningPublication(releaseVersion.Publication)
            .Generate();

        var statsReleaseVersion = new Data.Model.ReleaseVersion
        {
            Id = releaseVersionId,
            PublicationId = releaseVersion.Publication.Id
        };

        var otherReleaseVersionId = Guid.NewGuid();

        var otherTopic = new Topic
        {
            Id = Guid.NewGuid(),
            Title = "UI test topic to retain"
        };

        var otherDataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestDraftVersion(_fixture
                .DefaultDataBlockVersion()
                .WithReleaseVersionId(otherReleaseVersionId)
                .Generate())
            .WithLatestPublishedVersion(_fixture
                .DefaultDataBlockVersion()
                .WithReleaseVersionId(otherReleaseVersionId)
                .Generate())
            .Generate();

        var otherRelease = _fixture
            .DefaultReleaseVersion()
            .WithId(otherReleaseVersionId)
            .WithPublication(
                _fixture
                    .DefaultPublication()
                    .WithTopic(otherTopic)
                    .Generate())
            .WithDataBlockVersions(ListOf(
                otherDataBlockParent.LatestDraftVersion!,
                otherDataBlockParent.LatestPublishedVersion!))
            .Generate();

        var otherMethodology = _fixture
            .DefaultMethodology()
            .WithOwningPublication(otherRelease.Publication)
            .Generate();

        var otherStatsReleaseVersion = new Data.Model.ReleaseVersion
        {
            Id = otherReleaseVersionId,
            PublicationId = otherRelease.Publication.Id
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
        await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
        {
            contentContext.Methodologies.AddRange(methodology, otherMethodology);
            contentContext.ReleaseVersions.AddRange(releaseVersion, otherRelease);
            contentContext.Topics.AddRange(topic, otherTopic);
            statisticsContext.ReleaseVersion.AddRange(statsReleaseVersion, otherStatsReleaseVersion);

            await contentContext.SaveChangesAsync();
            await statisticsContext.SaveChangesAsync();

            Assert.Equal(2, contentContext.Publications.Count());
            Assert.Equal(2, contentContext.Topics.Count());
            Assert.Equal(2, contentContext.Methodologies.Count());
            Assert.Equal(2, contentContext.PublicationMethodologies.Count());
            Assert.Equal(2, contentContext.ReleaseVersions.Count());
            Assert.Equal(4, contentContext.DataBlockVersions.Count());
            Assert.Equal(2, contentContext.DataBlockParents.Count());
            Assert.Equal(2, statisticsContext.ReleaseVersion.Count());
        }

        var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
        var releaseFileService = new Mock<IReleaseFileService>(Strict);
        var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);
        var methodologyService = new Mock<IMethodologyService>(Strict);
        var publishingService = new Mock<IPublishingService>(Strict);
        var cacheService = new Mock<IBlobCacheService>(Strict);
        var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(Strict);

        await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
        await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = SetupTopicService(
                contentContext,
                statisticsContext,
                releaseDataFileService: releaseDataFileService.Object,
                releaseFileService: releaseFileService.Object,
                releaseSubjectRepository: releaseSubjectRepository.Object,
                methodologyService: methodologyService.Object,
                publishingService: publishingService.Object,
                cacheService: cacheService.Object,
                releasePublishingStatusRepository: releasePublishingStatusRepository.Object);

            releaseDataFileService
                .Setup(s => s.DeleteAll(releaseVersionId, true))
                .ReturnsAsync(Unit.Instance);

            releaseFileService
                .Setup(s => s.DeleteAll(releaseVersionId, true))
                .ReturnsAsync(Unit.Instance);

            releaseSubjectRepository
                .Setup(s => s.DeleteAllReleaseSubjects(releaseVersionId, false))
                .Returns(Task.CompletedTask);

            methodologyService
                .Setup(s => s.DeleteMethodology(methodology.Id, true))
                .ReturnsAsync(Unit.Instance);

            publishingService.Setup(s => s.TaxonomyChanged())
                .ReturnsAsync(Unit.Instance);

            cacheService
                .Setup(s =>
                    s.DeleteCacheFolderAsync(
                        ItIs.DeepEqualTo(new PrivateReleaseContentFolderCacheKey(releaseVersionId))))
                .Returns(Task.CompletedTask);

            releasePublishingStatusRepository.Setup(mock =>
                    mock.RemovePublisherReleaseStatuses(new List<Guid> { releaseVersionId }))
                .Returns(Task.CompletedTask);

            var result = await service.DeleteTopic(topic.Id);
            VerifyAllMocks(releaseDataFileService,
                releaseFileService,
                releaseSubjectRepository,
                methodologyService,
                publishingService,
                cacheService,
                releasePublishingStatusRepository);

            result.AssertRight();

            Assert.Equal(otherRelease.Publication.Id, contentContext.Publications.Single().Id);
            Assert.Equal(otherTopic.Id, contentContext.Topics.Single().Id);
            Assert.Equal(otherRelease.Id, contentContext.ReleaseVersions.Single().Id);
            Assert.Equal(otherRelease.Id, statisticsContext.ReleaseVersion.Single().Id);
            contentContext.DataBlockVersions.ForEach(dataBlockVersion =>
                Assert.Equal(otherReleaseVersionId, dataBlockVersion.ReleaseVersionId));
        }
    }

    private static TopicService SetupTopicService(
        ContentDbContext contentContext,
        StatisticsDbContext? statisticsContext = null,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IMapper? mapper = null,
        IUserService? userService = null,
        IReleaseSubjectRepository? releaseSubjectRepository = null,
        IReleaseDataFileService? releaseDataFileService = null,
        IReleaseFileService? releaseFileService = null,
        IPublishingService? publishingService = null,
        IMethodologyService? methodologyService = null,
        IBlobCacheService? cacheService = null,
        IReleasePublishingStatusRepository? releasePublishingStatusRepository = null,
        bool enableThemeDeletion = true)
    {
        var configuration =
            CreateMockConfiguration(TupleOf("enableThemeDeletion", enableThemeDeletion.ToString()));

        return new TopicService(
            configuration.Object,
            contentContext,
            statisticsContext ?? Mock.Of<StatisticsDbContext>(Strict),
            persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentContext),
            mapper ?? AdminMapper(),
            userService ?? AlwaysTrueUserService().Object,
            releaseSubjectRepository ?? Mock.Of<IReleaseSubjectRepository>(Strict),
            releaseDataFileService ?? Mock.Of<IReleaseDataFileService>(Strict),
            releaseFileService ?? Mock.Of<IReleaseFileService>(Strict),
            publishingService ?? Mock.Of<IPublishingService>(Strict),
            methodologyService ?? Mock.Of<IMethodologyService>(Strict),
            cacheService ?? Mock.Of<IBlobCacheService>(Strict),
            releasePublishingStatusRepository ?? Mock.Of<IReleasePublishingStatusRepository>(Strict)
        );
    }
}
