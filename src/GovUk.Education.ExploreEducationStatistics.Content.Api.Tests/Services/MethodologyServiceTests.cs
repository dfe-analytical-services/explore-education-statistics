#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Mappings;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
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
            var methodologyParent = new MethodologyParent
            {
                Slug = "methodology-slug",
                OwningPublicationTitle = "Methodology title",
                Versions = AsList(
                    new Methodology
                    {
                        Id = Guid.NewGuid(),
                        Annexes = new List<ContentSection>(),
                        Content = new List<ContentSection>(),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        AlternativeTitle = "Alternative title",
                        Version = 0
                    })
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyParents.AddAsync(methodologyParent);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            methodologyRepository.Setup(mock => mock.GetLatestPublishedByMethodologyParent(methodologyParent.Id))
                .ReturnsAsync(methodologyParent.Versions[0]);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = (await service.GetLatestMethodologyBySlug(methodologyParent.Slug)).AssertRight();

                Assert.Equal(methodologyParent.Versions[0].Id, result.Id);
                Assert.Equal("Alternative title", result.Title);

                var annexes = result.Annexes;
                Assert.NotNull(annexes);
                Assert.Empty(annexes);

                var content = result.Content;
                Assert.NotNull(content);
                Assert.Empty(content);
            }

            VerifyAllMocks(methodologyRepository);
        }

        [Fact]
        public async Task GetLatestMethodologyBySlug_MethodologyHasNoPublishedVersion()
        {
            var methodologyParent = new MethodologyParent
            {
                Slug = "methodology-slug",
                OwningPublicationTitle = "Methodology title"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyParents.AddAsync(methodologyParent);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            methodologyRepository.Setup(mock => mock.GetLatestPublishedByMethodologyParent(methodologyParent.Id))
                .ReturnsAsync((Methodology) null!);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetLatestMethodologyBySlug(methodologyParent.Slug);

                result.AssertNotFound();
            }

            VerifyAllMocks(methodologyRepository);
        }

        [Fact]
        public async Task GetLatestMethodologyBySlug_SlugNotFound()
        {
            // Set up a MethodologyParent with a different slug to make sure it's not returned
            var methodologyParent = new MethodologyParent
            {
                Slug = "some-other-slug",
                OwningPublicationTitle = "Methodology title"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyParents.AddAsync(methodologyParent);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            // Expect no call to get the latest published version since no methodology parent will be found

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetLatestMethodologyBySlug("methodology-slug");

                result.AssertNotFound();
            }

            VerifyAllMocks(methodologyRepository);
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

                var result = (await service.GetSummariesByPublication(publication.Id)).AssertRight();

                Assert.Equal(2, result.Count);

                Assert.Equal(methodologies[0].Id, result[0].Id);
                Assert.Equal(methodologyParent.Slug, result[0].Slug);
                Assert.Equal(methodologyParent.OwningPublicationTitle, result[0].Title);

                Assert.Equal(methodologies[1].Id, result[1].Id);
                Assert.Equal(methodologyParent.Slug, result[1].Slug);
                Assert.Equal(methodologies[1].AlternativeTitle, result[1].Title);
            }

            VerifyAllMocks(methodologyRepository);
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

                var result = (await service.GetSummariesByPublication(publication.Id)).AssertRight();

                Assert.Empty(result);
            }

            VerifyAllMocks(methodologyRepository);
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

            VerifyAllMocks(methodologyRepository);
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
                Publications = AsList(publication)
            };

            var theme = new Theme
            {
                Title = "Theme title",
                Topics = AsList(topic)
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

                var themes = (await service.GetTree()).AssertRight();

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

                Assert.Equal(Guid.Parse("7cba2701-b8d0-4d1a-ba9d-0cb71bf56574"), methodologies[0].Id);
                Assert.Equal("methodology-1-slug", methodologies[0].Slug);
                Assert.Equal("Methodology 1 v0 title", methodologies[0].Title);

                Assert.Equal(Guid.Parse("d49b519f-7b50-4bd3-bb87-8464cd434e16"), methodologies[1].Id);
                Assert.Equal("methodology-2-slug", methodologies[1].Slug);
                Assert.Equal("Methodology 2 v0 title", methodologies[1].Title);
            }

            VerifyAllMocks(methodologyRepository);
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

                var result = (await service.GetTree()).AssertRight();

                Assert.Empty(result);
            }

            VerifyAllMocks(methodologyRepository);
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

            VerifyAllMocks(methodologyRepository);
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

            VerifyAllMocks(methodologyRepository);
        }

        private static MethodologyService SetupMethodologyService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IMethodologyRepository? methodologyRepository = null,
            IMapper? mapper = null)
        {
            return new (
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                mapper ?? MapperUtils.MapperForProfile<MappingProfiles>(),
                methodologyRepository ?? new Mock<IMethodologyRepository>().Object
            );
        }
    }
}
