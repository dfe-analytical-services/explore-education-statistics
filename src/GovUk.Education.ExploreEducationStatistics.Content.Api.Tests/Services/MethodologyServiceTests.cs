using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Mappings;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Services
{
    public class MethodologyServiceTests
    {
        [Fact]
        public async Task GetLatestMethodologyBySlug()
        {
            const string slug = "methodology-1";

            // TODO SOW4 Won't need to add 'Versions' to db when Slug is moved to MethodologyParent
            // since the service will lookup a Methodology parent by slug rather than a Methodology
            // Also do this for the other variants of this unit test

            var methodology1 = new MethodologyParent
            {
                Slug = slug,
                Versions = AsList(
                    new Methodology
                    {
                        Annexes = new List<ContentSection>(),
                        Content = new List<ContentSection>(),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        AlternativeTitle = "Methodology 1 title",
                        Version = 0
                    }
                )
            };

            // Set up a MethodologyParent with a different slug to make sure it's not returned
            var methodology2 = new MethodologyParent
            {
                Slug = "methodology-2",
                Versions = AsList(
                    new Methodology
                    {
                        Annexes = new List<ContentSection>(),
                        Content = new List<ContentSection>(),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        AlternativeTitle = "Methodology 2 title",
                        Version = 0
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyParents.AddRangeAsync(methodology1, methodology2);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            methodologyRepository.Setup(mock => mock.GetLatestPublishedByMethodologyParent(methodology1.Id))
                .ReturnsAsync(methodology1.Versions[0]);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetLatestMethodologyBySlug(slug);

                result.AssertRight();

                Assert.Equal(methodology1.Versions[0].Id, result.Right.Id);
                Assert.Equal("Methodology 1 title", result.Right.Title);

                var annexes = result.Right.Annexes;
                Assert.NotNull(annexes);
                Assert.Empty(annexes);

                var content = result.Right.Content;
                Assert.NotNull(content);
                Assert.Empty(content);
            }

            MockUtils.VerifyAllMocks(methodologyRepository);
        }

        [Fact]
        public async Task GetLatestMethodologyBySlug_MethodologyHasNoPublishedVersion()
        {
            const string slug = "methodology-1";

            var methodology1 = new MethodologyParent
            {
                Slug = slug,
                Versions = AsList(
                    new Methodology
                    {
                        Annexes = new List<ContentSection>(),
                        Content = new List<ContentSection>(),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Status = Draft,
                        AlternativeTitle = "Methodology 1 title",
                        Version = 0
                    }
                )
            };

            // Set up a MethodologyParent with a different slug to make sure it's not returned
            var methodology2 = new MethodologyParent
            {
                Slug = "methodology-2",
                Versions = AsList(
                    new Methodology
                    {
                        Annexes = new List<ContentSection>(),
                        Content = new List<ContentSection>(),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        AlternativeTitle = "Methodology 2 title",
                        Version = 0
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyParents.AddRangeAsync(methodology1, methodology2);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            methodologyRepository.Setup(mock => mock.GetLatestPublishedByMethodologyParent(methodology1.Id))
                .ReturnsAsync((Methodology) null);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetLatestMethodologyBySlug(slug);

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(methodologyRepository);
        }

        [Fact]
        public async Task GetLatestMethodologyBySlug_SlugNotFound()
        {
            // Set up a MethodologyParent with a different slug to make sure it's not returned
            var methodology = new MethodologyParent
            {
                Slug = "some-other-slug",
                Versions = AsList(
                    new Methodology
                    {
                        Annexes = new List<ContentSection>(),
                        Content = new List<ContentSection>(),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        AlternativeTitle = "Methodology title",
                        Version = 0
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyParents.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetLatestMethodologyBySlug("methodology-slug");

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(methodologyRepository);
        }

        [Fact]
        public async Task GetSummariesByPublication()
        {
            var publication = new Publication();

            var methodologyParent = new MethodologyParent
            {
                Slug = "methodology-1",
                OwningPublicationTitle = "Methodology 1 title"
            };
            
            var methodologies = AsList(
                new Methodology
                {
                    Id = Guid.NewGuid(),
                    MethodologyParent = methodologyParent
                },
                new Methodology
                {
                    Id = Guid.NewGuid(),
                    MethodologyParent = methodologyParent,
                    AlternativeTitle = "Methodology 2 title",
                }
            );

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            methodologyRepository.Setup(mock => mock.GetLatestPublishedByPublication(publication.Id))
                .ReturnsAsync(methodologies);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetSummariesByPublication(publication.Id);

                result.AssertRight();

                Assert.Equal(2, result.Right.Count);

                Assert.Equal(methodologies[0].Id, result.Right[0].Id);
                Assert.Equal(methodologyParent.Slug, result.Right[0].Slug);
                Assert.Equal(methodologyParent.OwningPublicationTitle, result.Right[0].Title);

                Assert.Equal(methodologies[1].Id, result.Right[1].Id);
                Assert.Equal(methodologyParent.Slug, result.Right[1].Slug);
                Assert.Equal(methodologies[1].AlternativeTitle, result.Right[1].Title);
            }

            MockUtils.VerifyAllMocks(methodologyRepository);
        }

        [Fact]
        public async Task GetSummariesByPublication_PublicationHasNoMethodologies()
        {
            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            methodologyRepository.Setup(mock => mock.GetLatestPublishedByPublication(publication.Id))
                .ReturnsAsync(new List<Methodology>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetSummariesByPublication(publication.Id);

                result.AssertRight();

                Assert.Empty(result.Right);
            }

            MockUtils.VerifyAllMocks(methodologyRepository);
        }

        [Fact]
        public async Task GetSummariesByPublication_PublicationNotFound()
        {
            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext())
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetSummariesByPublication(Guid.NewGuid());

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(methodologyRepository);
        }

        [Fact]
        public async Task GetTree()
        {
            var publication = new Publication
            {
                Title = "Publication title"
            };

            var theme = new Theme
            {
                Title = "Theme title",
                Topics = AsList(
                    new Topic
                    {
                        Title = "Topic title",
                        Publications = AsList(publication)
                    }
                )
            };

            var latestMethodologies = AsList(
                new Methodology
                {
                    Id = Guid.Parse("7cba2701-b8d0-4d1a-ba9d-0cb71bf56574"),
                    Annexes = new List<ContentSection>(),
                    Content = new List<ContentSection>(),
                    PreviousVersionId = null,
                    PublishingStrategy = Immediately,
                    Status = Approved,
                    AlternativeTitle = "Methodology 1 v0 title",
                    Version = 0,
                    MethodologyParent = new MethodologyParent 
                    {
                        Slug = "methodology-1-slug"
                    }
                },
                new Methodology
                {
                    Id = Guid.Parse("d49b519f-7b50-4bd3-bb87-8464cd434e16"),
                    Annexes = new List<ContentSection>(),
                    Content = new List<ContentSection>(),
                    PreviousVersionId = null,
                    PublishingStrategy = Immediately,
                    Status = Approved,
                    AlternativeTitle = "Methodology 2 v0 title",
                    Version = 0,
                    MethodologyParent = new MethodologyParent 
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

            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            methodologyRepository.Setup(mock => mock.GetLatestPublishedByPublication(publication.Id))
                .ReturnsAsync(latestMethodologies);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetTree();

                result.AssertRight();

                var themes = result.Right;
                Assert.Single(themes);

                Assert.Equal(theme.Id, themes[0].Id);
                Assert.Equal("Theme title", themes[0].Title);

                var topics = themes[0].Topics;
                Assert.Single(topics);

                Assert.Equal(theme.Topics[0].Id, topics[0].Id);
                Assert.Equal("Topic title", topics[0].Title);

                var publications = topics[0].Publications;
                Assert.Single(publications);

                Assert.Equal(publication.Id, publications[0].Id);
                Assert.Equal("Publication title", publications[0].Title);

                var methodologies = publications[0].Methodologies;
                Assert.Equal(2, methodologies.Count);

                Assert.Equal(Guid.Parse("7cba2701-b8d0-4d1a-ba9d-0cb71bf56574"), methodologies[0].Id);
                Assert.Equal("methodology-1-slug", methodologies[0].Slug);
                Assert.Equal("Methodology 1 v0 title", methodologies[0].Title);
                
                Assert.Equal(Guid.Parse("d49b519f-7b50-4bd3-bb87-8464cd434e16"), methodologies[1].Id);
                Assert.Equal("methodology-2-slug", methodologies[1].Slug);
                Assert.Equal("Methodology 2 v0 title", methodologies[1].Title);
            }

            MockUtils.VerifyAllMocks(methodologyRepository);
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

            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetTree();

                result.AssertRight();

                Assert.Empty(result.Right);
            }

            MockUtils.VerifyAllMocks(methodologyRepository);
        }

        [Fact]
        public async Task GetTree_ThemeWithoutPublicationsIsNotIncluded()
        {
            var theme = new Theme
            {
                Title = "Theme title",
                Slug = "theme-slug",
                Summary = "Theme summary",
                Topics = AsList(
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

            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetTree();

                result.AssertRight();

                Assert.Empty(result.Right);
            }

            MockUtils.VerifyAllMocks(methodologyRepository);
        }

        [Fact]
        public async Task GetTree_ThemeWithoutPublishedMethodologiesIsNotIncluded()
        {
            var publication = new Publication
            {
                Title = "Publication title",
                Slug = "publication-slug",
                Summary = "Publication summary"
            };
            
            var theme = new Theme
            {
                Title = "Theme title",
                Slug = "theme-slug",
                Summary = "Theme summary",
                Topics = AsList(
                    new Topic
                    {
                        Title = "Topic title",
                        Slug = "topic-slug",
                        Publications = AsList(publication)
                    }
                )
            };

            // This test sets up returning an empty list of the latest publicly accessible methodologies for a publication.
            // The theme/topic/publication shouldn't be visible because it has no methodology leaf nodes.
            // This would be the case if:
            // * There are no approved methodologies yet.
            // * All of the approved methodologies depend on other releases which aren't published yet.
            // * If the publication doesn't have at least one published release yet.
            var latestMethodologies = new List<Methodology>();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddAsync(theme);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            methodologyRepository.Setup(mock => mock.GetLatestPublishedByPublication(publication.Id))
                .ReturnsAsync(latestMethodologies);
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetTree();

                result.AssertRight();

                Assert.Empty(result.Right);
            }

            MockUtils.VerifyAllMocks(methodologyRepository);
        }

        private static MethodologyService SetupMethodologyService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IMethodologyRepository methodologyRepository = null,
            IMapper mapper = null)
        {
            return new MethodologyService(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                mapper ?? MapperUtils.MapperForProfile<MappingProfiles>(),
                methodologyRepository ?? new Mock<IMethodologyRepository>().Object
            );
        }
    }
}
