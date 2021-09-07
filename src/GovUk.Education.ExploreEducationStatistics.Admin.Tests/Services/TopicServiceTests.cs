using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.StatisticsDbUtils;
using static Moq.MockBehavior;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;
using ContentRelease = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Theme = GovUk.Education.ExploreEducationStatistics.Content.Model.Theme;
using Topic = GovUk.Education.ExploreEducationStatistics.Content.Model.Topic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class TopicServiceTests
    {
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

                Assert.True(result.IsRight);
                Assert.Equal("Test topic", result.Right.Title);
                Assert.Equal("test-topic", result.Right.Slug);
                Assert.Equal(theme.Id, result.Right.ThemeId);

                var savedTopic = await context.Topics.FindAsync(result.Right.Id);

                Assert.Equal("Test topic", savedTopic.Title);
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

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                var service = SetupTopicService(context);

                var result = await service.UpdateTopic(
                    topic.Id,
                    new TopicSaveViewModel
                    {
                        Title = "New title",
                        ThemeId = theme.Id
                    }
                );

                Assert.True(result.IsRight);
                Assert.Equal(topic.Id, result.Right.Id);
                Assert.Equal("New title", result.Right.Title);
                Assert.Equal("new-title", result.Right.Slug);
                Assert.Equal(theme.Id, result.Right.ThemeId);

                var savedTopic = await context.Topics.FindAsync(result.Right.Id);

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
            var topicId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();
            var publicationId = Guid.NewGuid();
            var methodologyVersionId = Guid.NewGuid();

            var publicationMethodology = new PublicationMethodology
            {
                PublicationId = publicationId,
                Methodology = new Methodology
                {
                    Versions = new List<MethodologyVersion>
                    {
                        new()
                        {
                            Id = methodologyVersionId
                        }
                    }
                }
            };

            var topic = new Topic
            {
                Id = topicId,
                Title = "UI test topic"
            };

            var publication = new Publication
            {
                Id = publicationId,
                Topic = topic,
                Releases = AsList(new ContentRelease
                {
                    Id = releaseId
                })
            };

            var statsRelease = new Release
            {
                Id = releaseId,
                PublicationId = publicationId
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                await contentContext.Publications.AddAsync(publication);
                await contentContext.Topics.AddAsync(topic);
                await contentContext.PublicationMethodologies.AddAsync(publicationMethodology);
                await statisticsContext.Release.AddAsync(statsRelease);

                await contentContext.SaveChangesAsync();
                await statisticsContext.SaveChangesAsync();

                Assert.Equal(1, contentContext.Publications.Count());
                Assert.Equal(1, contentContext.Topics.Count());
                Assert.Equal(1, contentContext.MethodologyVersions.Count());
                Assert.Equal(1, contentContext.PublicationMethodologies.Count());
                Assert.Equal(1, contentContext.Releases.Count());
                Assert.Equal(1, statisticsContext.Release.Count());
            }

            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);
            var methodologyService = new Mock<IMethodologyService>(Strict);

            await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupTopicService(
                    contentContext,
                    statisticsContext,
                    releaseDataFileService: releaseDataFileService.Object,
                    releaseFileService: releaseFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    methodologyService: methodologyService.Object);

                releaseDataFileService
                    .Setup(s => s.DeleteAll(releaseId, true))
                    .ReturnsAsync(Unit.Instance);

                releaseFileService
                    .Setup(s => s.DeleteAll(releaseId, true))
                    .ReturnsAsync(Unit.Instance);

                releaseSubjectRepository
                    .Setup(s => s.DeleteAllReleaseSubjects(releaseId, false))
                    .Returns(Task.CompletedTask);

                methodologyService
                    .Setup(s => s.DeleteMethodologyVersion(methodologyVersionId, true))
                    .ReturnsAsync(Unit.Instance);

                var result = await service.DeleteTopic(topicId);
                VerifyAllMocks(releaseDataFileService, releaseFileService, releaseSubjectRepository,
                    methodologyService);
                result.AssertRight();

                Assert.Equal(0, contentContext.Publications.Count());
                Assert.Equal(0, contentContext.Topics.Count());
                Assert.Equal(0, contentContext.Releases.Count());
                Assert.Equal(0, statisticsContext.Release.Count());
            }
        }

        [Fact]
        public async Task DeleteTopic_ReleaseAndMethodologyVersionsDeletedInCorrectOrder()
        {
            var topicId = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            var releaseVersion1Id = Guid.NewGuid();
            var releaseVersion2Id = Guid.NewGuid();
            var releaseVersion3Id = Guid.NewGuid();
            var releaseVersion4Id = Guid.NewGuid();

            var methodologyVersion1Id = Guid.NewGuid();
            var methodologyVersion2Id = Guid.NewGuid();
            var methodologyVersion3Id = Guid.NewGuid();
            var methodologyVersion4Id = Guid.NewGuid();

            var publicationMethodology = new PublicationMethodology
            {
                PublicationId = publicationId,
                Methodology = new Methodology
                {
                    Versions = new List<MethodologyVersion>
                    {
                        new()
                        {
                            Id = methodologyVersion2Id,
                            PreviousVersionId = methodologyVersion1Id
                        },
                        new()
                        {
                            Id = methodologyVersion1Id
                        },
                        new()
                        {
                            Id = methodologyVersion4Id,
                            PreviousVersionId = methodologyVersion3Id
                        },
                        new()
                        {
                            Id = methodologyVersion3Id,
                            PreviousVersionId = methodologyVersion2Id,
                        }
                    }
                }
            };

            var topic = new Topic
            {
                Id = topicId,
                Title = "UI test topic"
            };

            var publication = new Publication
            {
                Id = publicationId,
                Topic = topic,
                Releases = AsList(new ContentRelease
                {
                    Id = releaseVersion2Id,
                    PreviousVersionId = releaseVersion1Id
                },
                new ContentRelease
                {
                    Id = releaseVersion1Id
                }, new ContentRelease
                {
                    Id = releaseVersion4Id,
                    PreviousVersionId = releaseVersion3Id
                }, new ContentRelease
                {
                    Id = releaseVersion3Id,
                    PreviousVersionId = releaseVersion2Id
                })
            };

            var statsReleases = AsList(new Release
            {
                Id = releaseVersion1Id,
                PublicationId = publicationId
            },
            new Release
            {
                Id = releaseVersion2Id,
                PreviousVersionId = releaseVersion1Id,
                PublicationId = publicationId
            },
            new Release
            {
                Id = releaseVersion3Id,
                PreviousVersionId = releaseVersion2Id,
                PublicationId = publicationId
            },
            new Release
            {
                Id = releaseVersion4Id,
                PreviousVersionId = releaseVersion3Id,
                PublicationId = publicationId
            });
            
            var contextId = Guid.NewGuid().ToString();

            await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                await contentContext.Publications.AddAsync(publication);
                await contentContext.Topics.AddAsync(topic);
                await contentContext.PublicationMethodologies.AddAsync(publicationMethodology);
                await statisticsContext.Release.AddRangeAsync(statsReleases);

                await contentContext.SaveChangesAsync();
                await statisticsContext.SaveChangesAsync();

                Assert.Equal(1, contentContext.Publications.Count());
                Assert.Equal(1, contentContext.Topics.Count());
                Assert.Equal(4, contentContext.MethodologyVersions.Count());
                Assert.Equal(1, contentContext.PublicationMethodologies.Count());
                Assert.Equal(4, contentContext.Releases.Count());
                Assert.Equal(4, statisticsContext.Release.Count());
            }

            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);
            var methodologyService = new Mock<IMethodologyService>(Strict);

            await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupTopicService(
                    contentContext,
                    statisticsContext,
                    releaseDataFileService: releaseDataFileService.Object,
                    releaseFileService: releaseFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    methodologyService: methodologyService.Object);

                var releaseDataFileDeleteSequence = new MockSequence();

                releaseDataFileService
                    .InSequence(releaseDataFileDeleteSequence)
                    .Setup(s => s.DeleteAll(releaseVersion4Id, true))
                    .ReturnsAsync(Unit.Instance);

                releaseDataFileService
                    .InSequence(releaseDataFileDeleteSequence)
                    .Setup(s => s.DeleteAll(releaseVersion3Id, true))
                    .ReturnsAsync(Unit.Instance);

                releaseDataFileService
                    .InSequence(releaseDataFileDeleteSequence)
                    .Setup(s => s.DeleteAll(releaseVersion2Id, true))
                    .ReturnsAsync(Unit.Instance);

                releaseDataFileService
                    .InSequence(releaseDataFileDeleteSequence)
                    .Setup(s => s.DeleteAll(releaseVersion1Id, true))
                    .ReturnsAsync(Unit.Instance);

                var releaseFileDeleteSequence = new MockSequence();

                releaseFileService
                    .InSequence(releaseFileDeleteSequence)
                    .Setup(s => s.DeleteAll(releaseVersion4Id, true))
                    .ReturnsAsync(Unit.Instance);

                releaseFileService
                    .InSequence(releaseFileDeleteSequence)
                    .Setup(s => s.DeleteAll(releaseVersion3Id, true))
                    .ReturnsAsync(Unit.Instance);

                releaseFileService
                    .InSequence(releaseFileDeleteSequence)
                    .Setup(s => s.DeleteAll(releaseVersion2Id, true))
                    .ReturnsAsync(Unit.Instance);

                releaseFileService
                    .InSequence(releaseFileDeleteSequence)
                    .Setup(s => s.DeleteAll(releaseVersion1Id, true))
                    .ReturnsAsync(Unit.Instance);

                var releaseSubjectDeleteSequence = new MockSequence();

                releaseSubjectRepository
                    .InSequence(releaseSubjectDeleteSequence)
                    .Setup(s => s.DeleteAllReleaseSubjects(releaseVersion4Id, false))
                    .Returns(Task.CompletedTask);

                releaseSubjectRepository
                    .InSequence(releaseSubjectDeleteSequence)
                    .Setup(s => s.DeleteAllReleaseSubjects(releaseVersion3Id, false))
                    .Returns(Task.CompletedTask);

                releaseSubjectRepository
                    .InSequence(releaseSubjectDeleteSequence)
                    .Setup(s => s.DeleteAllReleaseSubjects(releaseVersion2Id, false))
                    .Returns(Task.CompletedTask);

                releaseSubjectRepository
                    .InSequence(releaseSubjectDeleteSequence)
                    .Setup(s => s.DeleteAllReleaseSubjects(releaseVersion1Id, false))
                    .Returns(Task.CompletedTask);

                var methodologyDeleteSequence = new MockSequence();

                methodologyService
                    .InSequence(methodologyDeleteSequence)
                    .Setup(s => s.DeleteMethodologyVersion(methodologyVersion4Id, true))
                    .ReturnsAsync(Unit.Instance);

                methodologyService
                    .InSequence(methodologyDeleteSequence)
                    .Setup(s => s.DeleteMethodologyVersion(methodologyVersion3Id, true))
                    .ReturnsAsync(Unit.Instance);

                methodologyService
                    .InSequence(methodologyDeleteSequence)
                    .Setup(s => s.DeleteMethodologyVersion(methodologyVersion2Id, true))
                    .ReturnsAsync(Unit.Instance);

                methodologyService
                    .InSequence(methodologyDeleteSequence)
                    .Setup(s => s.DeleteMethodologyVersion(methodologyVersion1Id, true))
                    .ReturnsAsync(Unit.Instance);

                var result = await service.DeleteTopic(topicId);
                VerifyAllMocks(releaseDataFileService, releaseFileService, releaseSubjectRepository, methodologyService);
                result.AssertRight();

                Assert.Equal(0, contentContext.Publications.Count());
                Assert.Equal(0, contentContext.Topics.Count());
                Assert.Equal(0, contentContext.Releases.Count());
                Assert.Equal(0, statisticsContext.Release.Count());
            }
        }

        [Fact]
        public async Task DeleteTopic_DisallowedByNamingConvention()
        {
            var topicId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();
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
                Releases = AsList(new ContentRelease
                {
                    Id = releaseId
                })
            };

            var statsRelease = new Release
            {
                Id = releaseId,
                PublicationId = publicationId
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                await contentContext.Publications.AddAsync(publication);
                await contentContext.Topics.AddAsync(topic);
                await statisticsContext.Release.AddAsync(statsRelease);

                await contentContext.SaveChangesAsync();
                await statisticsContext.SaveChangesAsync();

                Assert.Equal(1, contentContext.Topics.Count());
                Assert.Equal(1, contentContext.Publications.Count());
                Assert.Equal(1, contentContext.Releases.Count());
                Assert.Equal(1, statisticsContext.Release.Count());
            }

            await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupTopicService(contentContext, statisticsContext);

                var result = await service.DeleteTopic(topicId);
                result.AssertForbidden();

                Assert.Equal(1, contentContext.Topics.Count());
                Assert.Equal(1, contentContext.Publications.Count());
                Assert.Equal(1, contentContext.Releases.Count());
                Assert.Equal(1, statisticsContext.Release.Count());
            }
        }

        [Fact]
        public async Task DeleteTopic_DisallowedByConfiguration()
        {
            var topicId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();
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
                Releases = AsList(new ContentRelease
                {
                    Id = releaseId
                })
            };

            var statsRelease = new Release
            {
                Id = releaseId,
                PublicationId = publicationId
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                await contentContext.Publications.AddAsync(publication);
                await contentContext.Topics.AddAsync(topic);
                await statisticsContext.Release.AddAsync(statsRelease);

                await contentContext.SaveChangesAsync();
                await statisticsContext.SaveChangesAsync();

                Assert.Equal(1, contentContext.Topics.Count());
                Assert.Equal(1, contentContext.Publications.Count());
                Assert.Equal(1, contentContext.Releases.Count());
                Assert.Equal(1, statisticsContext.Release.Count());
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
                Assert.Equal(1, contentContext.Releases.Count());
                Assert.Equal(1, statisticsContext.Release.Count());
            }
        }

        [Fact]
        public async Task DeleteTopic_OtherTopicsUnaffected()
        {
            var topicId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();
            var publicationId = Guid.NewGuid();
            var methodologyId = Guid.NewGuid();

            var otherTopicId = Guid.NewGuid();
            var otherReleaseId = Guid.NewGuid();
            var otherPublicationId = Guid.NewGuid();
            var otherMethodologyId = Guid.NewGuid();

            var publicationMethodology = new PublicationMethodology
            {
                PublicationId = publicationId,
                Methodology = new Methodology
                {
                    Versions = new List<MethodologyVersion>
                    {
                        new()
                        {
                            Id = methodologyId
                        }
                    }
                }
            };

            var topic = new Topic
            {
                Id = topicId,
                Title = "UI test topic"
            };

            var publication = new Publication
            {
                Id = publicationId,
                Topic = topic,
                Releases = AsList(new ContentRelease
                {
                    Id = releaseId
                })
            };

            var statsRelease = new Release
            {
                Id = releaseId,
                PublicationId = publicationId
            };

            var otherPublicationMethodology = new PublicationMethodology
            {
                PublicationId = otherPublicationId,
                Methodology = new Methodology
                {
                    Versions = new List<MethodologyVersion>
                    {
                        new()
                        {
                            Id = otherMethodologyId
                        }
                    }
                }
            };

            var otherTopic = new Topic
            {
                Id = otherTopicId,
                Title = "UI test topic"
            };

            var otherPublication = new Publication
            {
                Id = otherPublicationId,
                Topic = otherTopic,
                Releases = AsList(new ContentRelease
                {
                    Id = otherReleaseId
                })
            };

            var otherStatsRelease = new Release
            {
                Id = otherReleaseId,
                PublicationId = otherPublicationId
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                await contentContext.Publications.AddRangeAsync(publication, otherPublication);
                await contentContext.Topics.AddRangeAsync(topic, otherTopic);
                await contentContext.PublicationMethodologies.AddRangeAsync(publicationMethodology, otherPublicationMethodology);
                await statisticsContext.Release.AddRangeAsync(statsRelease, otherStatsRelease);

                await contentContext.SaveChangesAsync();
                await statisticsContext.SaveChangesAsync();

                Assert.Equal(2, contentContext.Publications.Count());
                Assert.Equal(2, contentContext.Topics.Count());
                Assert.Equal(2, contentContext.MethodologyVersions.Count());
                Assert.Equal(2, contentContext.PublicationMethodologies.Count());
                Assert.Equal(2, contentContext.Releases.Count());
                Assert.Equal(2, statisticsContext.Release.Count());
            }

            var releaseDataFileService = new Mock<IReleaseDataFileService>(Strict);
            var releaseFileService = new Mock<IReleaseFileService>(Strict);
            var releaseSubjectRepository = new Mock<IReleaseSubjectRepository>(Strict);
            var methodologyService = new Mock<IMethodologyService>(Strict);

            await using (var contentContext = DbUtils.InMemoryApplicationDbContext(contextId))
            await using (var statisticsContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupTopicService(
                    contentContext,
                    statisticsContext,
                    releaseDataFileService: releaseDataFileService.Object,
                    releaseFileService: releaseFileService.Object,
                    releaseSubjectRepository: releaseSubjectRepository.Object,
                    methodologyService: methodologyService.Object);

                releaseDataFileService
                    .Setup(s => s.DeleteAll(releaseId, true))
                    .ReturnsAsync(Unit.Instance);

                releaseFileService
                    .Setup(s => s.DeleteAll(releaseId, true))
                    .ReturnsAsync(Unit.Instance);

                releaseSubjectRepository
                    .Setup(s => s.DeleteAllReleaseSubjects(releaseId, false))
                    .Returns(Task.CompletedTask);

                methodologyService
                    .Setup(s => s.DeleteMethodologyVersion(methodologyId, true))
                    .ReturnsAsync(Unit.Instance);

                var result = await service.DeleteTopic(topicId);
                VerifyAllMocks(releaseDataFileService, releaseFileService, releaseSubjectRepository, methodologyService);
                result.AssertRight();

                Assert.Equal(otherPublicationId, contentContext.Publications.Select(p => p.Id).Single());
                Assert.Equal(otherTopicId, contentContext.Topics.Select(t => t.Id).Single());
                Assert.Equal(otherReleaseId, contentContext.Releases.Select(r => r.Id).Single());
                Assert.Equal(otherReleaseId, statisticsContext.Release.Select(r => r.Id).Single());
            }
        }

        [Fact]
        public void VersionedEntityDeletionOrderComparer()
        {
            var version1Id = Guid.NewGuid();
            var version2Id = Guid.NewGuid();
            var version3Id = Guid.NewGuid();

            var version1 = new TopicService.IdAndPreviousVersionIdPair(version1Id, null);
            var version2 = new TopicService.IdAndPreviousVersionIdPair(version2Id, version1Id);
            var version3 = new TopicService.IdAndPreviousVersionIdPair(version3Id, version2Id);

            var comparer = new TopicService.VersionedEntityDeletionOrderComparer();

            Assert.Equal(-1, comparer.Compare(version2, version1));
            Assert.Equal(-1, comparer.Compare(version3, version2));

            Assert.Equal(1, comparer.Compare(version1, version2));
            Assert.Equal(1, comparer.Compare(version2, version3));

            Assert.Equal(1, comparer.Compare(version1, version3));
            Assert.Equal(-1, comparer.Compare(version3, version1));
        }

        [Fact]
        public void VersionedEntityDeletionOrderComparer_WithSequence()
        {
            var version1Id = Guid.NewGuid();
            var version4Id = Guid.NewGuid();
            var version3Id = Guid.NewGuid();
            var version2Id = Guid.NewGuid();
            var version5Id = Guid.NewGuid();

            var versions = AsList(
                new TopicService.IdAndPreviousVersionIdPair(version2Id, version1Id),
                new TopicService.IdAndPreviousVersionIdPair(version1Id, null),
                new TopicService.IdAndPreviousVersionIdPair(version5Id, version4Id),
                new TopicService.IdAndPreviousVersionIdPair(version4Id, version3Id),
                new TopicService.IdAndPreviousVersionIdPair(version3Id, version2Id));

            var orderedByLatestVersionsFirst = versions
                .OrderBy(version => version, new TopicService.VersionedEntityDeletionOrderComparer())
                .Select(version => version.Id)
                .ToList();

            var expectedVersionOrder = AsList(version5Id, version4Id, version3Id, version2Id, version1Id);
            Assert.Equal(expectedVersionOrder, orderedByLatestVersionsFirst);
        }

        private TopicService SetupTopicService(
            ContentDbContext contentContext,
            StatisticsDbContext statisticsContext = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IMapper mapper = null,
            IUserService userService = null,
            IReleaseSubjectRepository releaseSubjectRepository = null,
            IReleaseDataFileService releaseDataFileService = null,
            IReleaseFileService releaseFileService = null,
            IPublishingService publishingService = null,
            IMethodologyService methodologyService = null,
            bool enableThemeDeletion = true)
        {
            var configuration =
                CreateMockConfiguration(new Tuple<string, string>("enableThemeDeletion", enableThemeDeletion.ToString()));
            
            return new TopicService(
                configuration.Object,
                contentContext,
                statisticsContext ?? Mock.Of<StatisticsDbContext>(),
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentContext),
                mapper ?? AdminMapper(),
                userService ?? AlwaysTrueUserService().Object,
                releaseSubjectRepository ?? Mock.Of<IReleaseSubjectRepository>(),
                releaseDataFileService ?? Mock.Of<IReleaseDataFileService>(),
                releaseFileService ?? Mock.Of<IReleaseFileService>(),
                publishingService ?? Mock.Of<IPublishingService>(),
                methodologyService ?? Mock.Of<IMethodologyService>()
            );
        }
    }
}
