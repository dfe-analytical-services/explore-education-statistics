#nullable enable
using System;
using System.Collections.Generic;
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
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;
 
[Collection(BlobCacheServiceTests)]
public class MethodologyServiceCachingTests : BlobCacheServiceTestFixture
{
    [Fact]
    public async Task GetCachedSummariesByPublication_NoCachedMethodologyThemeTreeExists()
    {
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
        
        CacheService
            .Setup(s => s.GetItem(
                It.IsAny<AllMethodologiesCacheKey>(), typeof(List<AllMethodologiesThemeViewModel>)))
            .ReturnsAsync(null);
        
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

        methodologyVersionRepository
            .Setup(mock => mock.GetLatestPublishedVersionByPublication(publication.Id))
            .ReturnsAsync(publicationLatestVersions);
        
        methodologyVersionRepository
            .Setup(mock => mock.GetLatestPublishedVersionByPublication(publication.Id))
            .ReturnsAsync(publicationLatestVersions);

        CacheService
            .Setup(s => s.SetItem<object>(
                It.IsAny<AllMethodologiesCacheKey>(), It.IsAny<List<AllMethodologiesThemeViewModel>>()))
            .Returns(Task.CompletedTask);

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
    public async Task GetCachedSummariesByPublication_CachedMethodologyThemeTreeExists()
    {
        var publicationId = Guid.NewGuid();
        
        var cachedResult = ListOf(
            new AllMethodologiesThemeViewModel
            {
                Title = "Theme 1",
                Topics = ListOf(
                    new AllMethodologiesTopicViewModel
                    {
                        Title = "Theme 1 Topic 1",
                        Publications = ListOf(
                            new AllMethodologiesPublicationViewModel
                            {
                                Id = Guid.NewGuid(),
                                Title = "Theme 1 Topic 1 Publication 1",
                                Methodologies = ListOf(
                                    new MethodologyVersionSummaryViewModel
                                    {
                                        Id = Guid.NewGuid(),
                                        Title = "Theme 1 Topic 1 Publication 1 Methodology 1",
                                        Slug = "Theme 1 Topic 1 Publication 1 Methodology 1 Slug"
                                    })
                            },
                            // This is the list of Methodologies that we expect to have returned.
                            new AllMethodologiesPublicationViewModel
                            {
                                Id = publicationId,
                                Title = "Theme 1 Topic 1 Publication 2",
                                Methodologies = ListOf(
                                    new MethodologyVersionSummaryViewModel
                                    {
                                        Id = Guid.NewGuid(),
                                        Title = "Theme 1 Topic 1 Publication 2 Methodology 1",
                                        Slug = "Theme 1 Topic 1 Publication 2 Methodology 1 Slug"
                                    },
                                    new MethodologyVersionSummaryViewModel
                                    {
                                        Id = Guid.NewGuid(),
                                        Title = "Theme 1 Topic 1 Publication 2 Methodology 2",
                                        Slug = "Theme 1 Topic 1 Publication 2 Methodology 2 Slug"
                                    })
                            })
                    },
                    new AllMethodologiesTopicViewModel
                    {
                        Title = "Theme 1 Topic 2",
                        Publications = ListOf(
                            new AllMethodologiesPublicationViewModel
                            {
                                Id = Guid.NewGuid(),
                                Title = "Theme 1 Topic 2 Publication 1",
                                Methodologies = ListOf(
                                    new MethodologyVersionSummaryViewModel
                                    {
                                        Id = Guid.NewGuid(),
                                        Title = "Theme 1 Topic 2 Publication 1 Methodology 1",
                                        Slug = "Theme 1 Topic 2 Publication 1 Methodology 1 Slug"
                                    }
                                )
                            })
                    })
            },
            new AllMethodologiesThemeViewModel
            {
                Title = "Theme 2",
                Topics = ListOf(
                    new AllMethodologiesTopicViewModel
                    {
                        Title = "Theme 2 Topic 1",
                        Publications = ListOf(
                            new AllMethodologiesPublicationViewModel
                            {
                                Id = Guid.NewGuid(),
                                Title = "Theme 2 Topic 1 Publication 1",
                                Methodologies = ListOf(
                                    new MethodologyVersionSummaryViewModel
                                    {
                                        Id = Guid.NewGuid(),
                                        Title = "Theme 2 Topic 1 Publication 1 Methodology 1",
                                        Slug = "Theme 2 Topic 1 Publication 1 Methodology 1 Slug"
                                    })
                            })
                    })
            }
        );
        
        var publication2 = new Publication
        {
            Id = publicationId,
            Title = "Publication 2"
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Publications.AddAsync(publication2);
            await contentDbContext.SaveChangesAsync();
        }

        CacheService
            .Setup(s => s.GetItem(
                It.IsAny<AllMethodologiesCacheKey>(), typeof(List<AllMethodologiesThemeViewModel>)))
            .ReturnsAsync(cachedResult);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(contentDbContext);

            var result = await service.GetCachedSummariesByPublication(publicationId);

            VerifyAllMocks(CacheService);

            // Assert that the list of Methodologies for "Publication 2" are picked from the cached result.
            result.AssertRight(cachedResult[0].Topics[0].Publications[1].Methodologies);
        }
    }
    
    [Fact]
    public async Task GetCachedSummariesByPublication_CachedMethodologyThemeTreeExistsButNoMatchingPublicationRecord()
    {
        var publicationId = Guid.NewGuid();
        
        var cachedResult = ListOf(
            new AllMethodologiesThemeViewModel
            {
                Title = "Theme 1",
                Topics = ListOf(
                    new AllMethodologiesTopicViewModel
                    {
                        Title = "Theme 1 Topic 1",
                        Publications = ListOf(
                            new AllMethodologiesPublicationViewModel
                            {
                                Id = Guid.NewGuid(),
                                Title = "Theme 1 Topic 1 Publication 1",
                                Methodologies = ListOf(
                                    new MethodologyVersionSummaryViewModel
                                    {
                                        Id = Guid.NewGuid(),
                                        Title = "Theme 1 Topic 1 Publication 1 Methodology 1",
                                        Slug = "Theme 1 Topic 1 Publication 1 Methodology 1 Slug"
                                    })
                            })
                    }
                )
            }
        );
        
        var publicationWithNoCachedRecord = new Publication
        {
            Id = publicationId
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Publications.AddAsync(publicationWithNoCachedRecord);
            await contentDbContext.SaveChangesAsync();
        }

        CacheService
            .Setup(s => s.GetItem(
                It.IsAny<AllMethodologiesCacheKey>(), typeof(List<AllMethodologiesThemeViewModel>)))
            .ReturnsAsync(cachedResult);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(contentDbContext);

            var result = await service.GetCachedSummariesByPublication(publicationId);

            VerifyAllMocks(CacheService);

            // Assert that we get an empty list of Methodology Summaries back, as there was no matching
            // Publication in the cached Theme Tree.
            result.AssertRight(new List<MethodologyVersionSummaryViewModel>());
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
    public async Task GetCachedSummariesTree_NoCachedMethodologyThemeTreeExists()
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
                    Slug = "methodology-1-slug",
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
        
        CacheService
            .Setup(s => s.GetItem(
                It.IsAny<AllMethodologiesCacheKey>(), typeof(List<AllMethodologiesThemeViewModel>)))
            .ReturnsAsync(null);

        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

        methodologyVersionRepository.Setup(mock => mock.GetLatestPublishedVersionByPublication(publication.Id))
            .ReturnsAsync(latestVersions);

        var capturedCachedResults = new List<List<AllMethodologiesThemeViewModel>>();
        
        CacheService
            .Setup(s => s.SetItem<object>(
                It.IsAny<AllMethodologiesCacheKey>(), Capture.In(capturedCachedResults)))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(contentDbContext,
                methodologyVersionRepository: methodologyVersionRepository.Object);

            var result = await service.GetCachedSummariesTree();
            VerifyAllMocks(CacheService, methodologyVersionRepository);

            // Assert that the CacheService.SetItem() result was captured successfully.
            var capturedCachedResult = Assert.Single(capturedCachedResults);
            
            // Assert that the cached result is what was returned by the method.
            var themes = result.AssertRight(capturedCachedResult);

            // Assert the details of the result are correct.
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

            Assert.Equal(latestVersions[0].Id, methodologies[0].Id);
            Assert.Equal("methodology-1-slug", methodologies[0].Slug);
            Assert.Equal("Methodology 1 v0 title", methodologies[0].Title);

            Assert.Equal(latestVersions[1].Id, methodologies[1].Id);
            Assert.Equal("methodology-2-slug", methodologies[1].Slug);
            Assert.Equal("Methodology 2 v0 title", methodologies[1].Title);
        }
    }
    
    [Fact]
    public async Task GetCachedSummariesTree_CachedMethodologyThemeTreeExists()
    {
        var cachedResult = ListOf(
            new AllMethodologiesThemeViewModel
            {
                Title = "Theme 1",
                Topics = ListOf(
                    new AllMethodologiesTopicViewModel
                    {
                        Title = "Theme 1 Topic 1",
                        Publications = ListOf(
                            new AllMethodologiesPublicationViewModel
                            {
                                Id = Guid.NewGuid(),
                                Title = "Theme 1 Topic 1 Publication 1",
                                Methodologies = ListOf(
                                    new MethodologyVersionSummaryViewModel
                                    {
                                        Id = Guid.NewGuid(),
                                        Title = "Theme 1 Topic 1 Publication 1 Methodology 1",
                                        Slug = "Theme 1 Topic 1 Publication 1 Methodology 1 Slug"
                                    })
                            })
                    }
                )
            }
        );
        
        await using var contentDbContext = InMemoryContentDbContext();

        CacheService
            .Setup(s => s.GetItem(
                It.IsAny<AllMethodologiesCacheKey>(), typeof(List<AllMethodologiesThemeViewModel>)))
            .ReturnsAsync(cachedResult);
        
        var service = SetupMethodologyService(contentDbContext);

        var result = await service.GetCachedSummariesTree();
        VerifyAllMocks(CacheService);

        result.AssertRight(cachedResult);
    }

    [Fact]
    public async Task GetCachedSummariesTree_NoCachedMethodologyThemeTreeExists_ThemeWithoutTopicsIsNotIncluded()
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
        
        CacheService
            .Setup(s => s.GetItem(
                It.IsAny<AllMethodologiesCacheKey>(), typeof(List<AllMethodologiesThemeViewModel>)))
            .ReturnsAsync(null);

        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

        CacheService
            .Setup(s => s.SetItem<object>(
                It.IsAny<AllMethodologiesCacheKey>(), It.IsAny<List<AllMethodologiesThemeViewModel>>()))
            .Returns(Task.CompletedTask);
        
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(contentDbContext,
                methodologyVersionRepository: methodologyVersionRepository.Object);

            var result = await service.GetCachedSummariesTree();
            VerifyAllMocks(methodologyVersionRepository);

            result.AssertRight(new List<AllMethodologiesThemeViewModel>());
        }
    }

    [Fact]
    public async Task GetCachedSummariesTree_NoCachedMethodologyThemeTreeExists_ThemeWithoutPublicationsIsNotIncluded()
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
        
        CacheService
            .Setup(s => s.GetItem(
                It.IsAny<AllMethodologiesCacheKey>(), typeof(List<AllMethodologiesThemeViewModel>)))
            .ReturnsAsync(null);

        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

        CacheService
            .Setup(s => s.SetItem<object>(
                It.IsAny<AllMethodologiesCacheKey>(), It.IsAny<List<AllMethodologiesThemeViewModel>>()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupMethodologyService(contentDbContext,
                methodologyVersionRepository: methodologyVersionRepository.Object);

            var result = await service.GetCachedSummariesTree();

            VerifyAllMocks(methodologyVersionRepository);

            result.AssertRight();

            Assert.Empty(result.Right);
        }
    }

    [Fact]
    public async Task GetCachedSummariesTree_NoCachedMethodologyThemeTreeExists_ThemeWithoutPublishedMethodologiesIsNotIncluded()
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
        
        CacheService
            .Setup(s => s.GetItem(
                It.IsAny<AllMethodologiesCacheKey>(), typeof(List<AllMethodologiesThemeViewModel>)))
            .ReturnsAsync(null);

        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

        CacheService
            .Setup(s => s.SetItem<object>(
                It.IsAny<AllMethodologiesCacheKey>(), It.IsAny<List<AllMethodologiesThemeViewModel>>()))
            .Returns(Task.CompletedTask);

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