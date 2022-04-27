#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Mappings;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    [Collection(BlobCacheServiceTests)]
    public class MethodologyServiceCachingTests : BlobCacheServiceTestFixture
    {
        [Fact]
        public async Task GetCachedSummariesByPublication()
        {
            CacheService
                .Setup(s => s.GetItem(
                    It.IsAny<AllMethodologiesCacheKey>(), typeof(List<AllMethodologiesThemeViewModel>)))
                .ReturnsAsync(null);
            
            CacheService
                .Setup(s => s.SetItem<object>(
                    It.IsAny<AllMethodologiesCacheKey>(), It.IsAny<List<AllMethodologiesThemeViewModel>>()))
                .Returns(Task.CompletedTask);
            
            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                Title = "Publication title 1"
            };
            
            var topic = new Topic
            {
                Title = "Topic title",
                Publications = ListOf(publication)
            };

            var theme = new Theme
            {
                Title = "Theme title",
                Topics = ListOf(topic)
            };

            var methodology = new Methodology
            {
                Slug = "methodology-1-slug",
            };

            var publicationLatestVersions = ListOf(
                new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    Version = 0,
                    Methodology = methodology
                });

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddAsync(theme);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            methodologyVersionRepository
                .Setup(mock => mock.GetLatestPublishedVersionByPublication(publication.Id))
                .ReturnsAsync(publicationLatestVersions);
            
            methodologyVersionRepository
                .Setup(mock => mock.GetLatestPublishedVersionByPublication(publication.Id))
                .ReturnsAsync(publicationLatestVersions);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var result = (await service.GetCachedSummariesByPublication(publication.Id)).AssertRight();

                VerifyAllMocks(methodologyVersionRepository, CacheService);

                var methodologySummary = Assert.Single(result);

                Assert.Equal(publicationLatestVersions[0].Id, methodologySummary.Id);
                Assert.Equal(methodology.Slug, methodologySummary.Slug);
                Assert.Equal(methodology.OwningPublicationTitle, methodologySummary.Title);
            }
        }
        
        [Fact]
        public async Task GetCachedSummariesByPublication_PublicationNotFound()
        {
            await using var contentDbContext = InMemoryContentDbContext();
            var service = SetupMethodologyService(contentDbContext);
            var result = await service.GetCachedSummariesByPublication(Guid.NewGuid());
            result.AssertNotFound();
        }

        [Fact]
        public async Task GetTree()
        {
            var publication = new Publication
            {
                Title = "Publication title"
            };

            var topic = new Topic
            {
                Title = "Topic title",
                Publications = ListOf(publication)
            };

            var theme = new Theme
            {
                Title = "Theme title",
                Topics = ListOf(topic)
            };

            var latestVersions = ListOf(
                new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    Annexes = new List<ContentSection>(),
                    Content = new List<ContentSection>(),
                    PreviousVersionId = null,
                    PublishingStrategy = Immediately,
                    Status = Approved,
                    AlternativeTitle = "Methodology 1 v0 title",
                    Version = 0,
                    Methodology = new Methodology
                    {
                        Slug = "methodology-1-slug"
                    }
                },
                new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    Annexes = new List<ContentSection>(),
                    Content = new List<ContentSection>(),
                    PreviousVersionId = null,
                    PublishingStrategy = Immediately,
                    Status = Approved,
                    AlternativeTitle = "Methodology 2 v0 title",
                    Version = 0,
                    Methodology = new Methodology
                    {
                        Slug = "methodology-2-slug"
                    }
                });

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddAsync(theme);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            methodologyVersionRepository.Setup(mock => mock.GetLatestPublishedVersionByPublication(publication.Id))
                .ReturnsAsync(latestVersions);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var themes = (await service.GetCachedSummariesTree()).AssertRight();

                Assert.Single(themes);

                Assert.Equal(theme.Id, themes[0].Id);
                Assert.Equal("Theme title", themes[0].Title);

                var topics = themes[0].Topics;
                Assert.Single(topics);

                Assert.Equal(topic.Id, topics[0].Id);
                Assert.Equal("Topic title", topics[0].Title);

                var publications = topics[0].Publications;
                Assert.Single(publications);

                Assert.Equal(publication.Id, publications[0].Id);
                Assert.Equal("Publication title", publications[0].Title);

                var methodologies = publications[0].Methodologies;
                Assert.Equal(2, methodologies.Count);

                Assert.Equal(latestVersions[0].MethodologyId, methodologies[0].Id);
                Assert.Equal("methodology-1-slug", methodologies[0].Slug);
                Assert.Equal("Methodology 1 v0 title", methodologies[0].Title);

                Assert.Equal(latestVersions[1].MethodologyId, methodologies[1].Id);
                Assert.Equal("methodology-2-slug", methodologies[1].Slug);
                Assert.Equal("Methodology 2 v0 title", methodologies[1].Title);
            }

            VerifyAllMocks(methodologyVersionRepository);
        }

        [Fact]
        public async Task GetTree_ThemeWithoutTopicsIsNotIncluded()
        {
            var theme = new Theme
            {
                Title = "Theme title",
                Slug = "theme-slug",
                Summary = "Theme summary",
                Topics = new List<Topic>()
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddAsync(theme);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var result = (await service.GetCachedSummariesTree()).AssertRight();

                Assert.Empty(result);
            }

            VerifyAllMocks(methodologyVersionRepository);
        }

        [Fact]
        public async Task GetTree_ThemeWithoutPublicationsIsNotIncluded()
        {
            var theme = new Theme
            {
                Title = "Theme title",
                Slug = "theme-slug",
                Summary = "Theme summary",
                Topics = ListOf(
                    new Topic
                    {
                        Title = "Topic title",
                        Slug = "topic-slug",
                        Publications = new List<Publication>()
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddAsync(theme);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var result = await service.GetCachedSummariesTree();

                result.AssertRight();

                Assert.Empty(result.Right);
            }

            VerifyAllMocks(methodologyVersionRepository);
        }

        [Fact]
        public async Task GetTree_ThemeWithoutPublishedMethodologiesIsNotIncluded()
        {
            var publication = new Publication
            {
                Title = "Publication title",
                Slug = "publication-slug",
            };

            var theme = new Theme
            {
                Title = "Theme title",
                Slug = "theme-slug",
                Summary = "Theme summary",
                Topics = ListOf(
                    new Topic
                    {
                        Title = "Topic title",
                        Slug = "topic-slug",
                        Publications = ListOf(publication)
                    }
                )
            };

            // This test sets up returning an empty list of the latest publicly accessible methodologies for a publication.
            // The theme/topic/publication shouldn't be visible because it has no methodology leaf nodes.
            // This would be the case if:
            // * There are no approved methodologies yet.
            // * All of the approved methodologies depend on other releases which aren't published yet.
            // * If the publication doesn't have at least one published release yet.
            var latestMethodologies = new List<MethodologyVersion>();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddAsync(theme);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            methodologyVersionRepository.Setup(mock => mock.GetLatestPublishedVersionByPublication(publication.Id))
                .ReturnsAsync(latestMethodologies);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var result = await service.GetCachedSummariesTree();

                result.AssertRight();

                Assert.Empty(result.Right);
            }

            VerifyAllMocks(methodologyVersionRepository);
        }

        private static MethodologyService SetupMethodologyService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IMethodologyVersionRepository? methodologyVersionRepository = null,
            IMapper? mapper = null)
        {
            return new(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                mapper ?? MapperUtils.MapperForProfile<MappingProfiles>(),
                methodologyVersionRepository ?? Mock.Of<IMethodologyVersionRepository>(MockBehavior.Strict)
            );
        }
    }
}
