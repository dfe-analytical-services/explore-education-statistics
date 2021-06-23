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
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
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

            // Set up a methodology with a published version
            var methodology1 = new MethodologyParent
            {
                Slug = slug,
                Versions = AsList(
                    new Methodology
                    {
                        Id = Guid.Parse("7a2179a3-16a2-4eff-9be4-5a281d901213"),
                        Annexes = new List<ContentSection>(),
                        Content = new List<ContentSection>(),
                        PreviousVersionId = null,
                        PublishingStrategy = Immediately,
                        Slug = slug,
                        Status = Approved,
                        Title = "Methodology 1 title",
                        Version = 0
                    },
                    new Methodology
                    {
                        Id = Guid.Parse("926750dc-b079-4acb-a6a2-71b550920e81"),
                        Annexes = new List<ContentSection>(),
                        Content = new List<ContentSection>(),
                        PreviousVersionId = Guid.Parse("7a2179a3-16a2-4eff-9be4-5a281d901213"),
                        PublishingStrategy = Immediately,
                        Slug = slug,
                        Status = Approved,
                        Title = "Methodology 1 title updated",
                        Version = 1
                    },
                    // Latest version is a draft that should not be returned
                    new Methodology
                    {
                        Id = Guid.Parse("ad13999c-4caf-4f82-8df0-94db22acbcbc"),
                        Annexes = new List<ContentSection>(),
                        Content = new List<ContentSection>(),
                        PreviousVersionId = Guid.Parse("926750dc-b079-4acb-a6a2-71b550920e81"),
                        PublishingStrategy = Immediately,
                        Slug = slug,
                        Status = Draft,
                        Title = "Methodology 1 title draft",
                        Version = 2
                    }
                )
            };

            // Set up a different methodology with a different slug to make sure it's not returned
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
                        Slug = "methodology-2",
                        Status = Approved,
                        Title = "Methodology 2 title",
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext);

                var result = await service.GetLatestMethodologyBySlug(slug);

                result.AssertRight();

                Assert.Equal(Guid.Parse("926750dc-b079-4acb-a6a2-71b550920e81"), result.Right.Id);
                Assert.Equal("Methodology 1 title updated", result.Right.Title);

                var annexes = result.Right.Annexes;
                Assert.NotNull(annexes);
                Assert.Empty(annexes);

                var content = result.Right.Content;
                Assert.NotNull(content);
                Assert.Empty(content);
            }
        }

        [Fact]
        public async Task GetLatestMethodologyBySlug_MethodologyHasNoPublishedVersion()
        {
            const string slug = "methodology-1";

            // Set up a methodology with a draft version that should not be returned
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
                        Slug = slug,
                        Status = Draft,
                        Title = "Methodology 1 title",
                        Version = 0
                    }
                )
            };

            // Set up a different approved methodology with a different slug to make sure it's not returned
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
                        Slug = "methodology-2",
                        Status = Approved,
                        Title = "Methodology 2 title",
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext);

                var result = await service.GetLatestMethodologyBySlug(slug);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetLatestMethodologyBySlug_SlugNotFound()
        {
            // Set up a different methodology with a different slug to make sure it's not returned
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
                        Slug = "some-other-slug",
                        Status = Approved,
                        Title = "Methodology title",
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext);

                var result = await service.GetLatestMethodologyBySlug("methodology-slug");

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetSummariesByPublication()
        {
            var publication = new Publication();

            var methodologies = AsList(
                new Methodology
                {
                    Id = Guid.NewGuid(),
                    Slug = "methodology-1",
                    Title = "Methodology 1 title",
                },
                new Methodology
                {
                    Id = Guid.NewGuid(),
                    Slug = "methodology-2",
                    Title = "Methodology 2 title",
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
                Assert.Equal(methodologies[0].Slug, result.Right[0].Slug);
                Assert.Equal(methodologies[0].Title, result.Right[0].Title);

                Assert.Equal(methodologies[1].Id, result.Right[1].Id);
                Assert.Equal(methodologies[1].Slug, result.Right[1].Slug);
                Assert.Equal(methodologies[1].Title, result.Right[1].Title);
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
            // Publication with a published Release
            var publication = new Publication
            {
                Title = "Publication title",
                Slug = "publication-slug",
                Summary = "Publication summary",
                Releases = AsList(
                    new Release
                    {
                        ReleaseName = "2018",
                        TimePeriodCoverage = AcademicYearQ1,
                        Published = new DateTime(2019, 1, 01),
                        ApprovalStatus = ReleaseApprovalStatus.Approved
                    }
                )
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

            var publicationMethodologies = AsList(
                new PublicationMethodology
                {
                    Publication = publication,
                    MethodologyParent = new MethodologyParent
                    {
                        Slug = "methodology-1-slug",
                        Versions = AsList(
                            new Methodology
                            {
                                Id = Guid.Parse("7cba2701-b8d0-4d1a-ba9d-0cb71bf56574"),
                                Annexes = new List<ContentSection>(),
                                Content = new List<ContentSection>(),
                                PreviousVersionId = null,
                                PublishingStrategy = Immediately,
                                Slug = "methodology-1-slug",
                                Status = Approved,
                                Title = "Methodology 1 v0 title",
                                Version = 0
                            },
                            new Methodology
                            {
                                Id = Guid.Parse("481bb64c-3b87-44fe-aa62-b3ff6f1ab6d7"),
                                Annexes = new List<ContentSection>(),
                                Content = new List<ContentSection>(),
                                PreviousVersionId = Guid.Parse("7cba2701-b8d0-4d1a-ba9d-0cb71bf56574"),
                                PublishingStrategy = Immediately,
                                Slug = "methodology-1-slug",
                                Status = Draft,
                                Title = "Methodology 1 v1 title",
                                Version = 1
                            }
                        )
                    },
                    Owner = true
                },
                new PublicationMethodology
                {
                    Publication = publication,
                    MethodologyParent = new MethodologyParent
                    {
                        Slug = "methodology-2-slug",
                        Versions = AsList(
                            new Methodology
                            {
                                Id = Guid.Parse("d49b519f-7b50-4bd3-bb87-8464cd434e16"),
                                Annexes = new List<ContentSection>(),
                                Content = new List<ContentSection>(),
                                PreviousVersionId = null,
                                PublishingStrategy = Immediately,
                                Slug = "methodology-2-slug",
                                Status = Approved,
                                Title = "Methodology 2 v0 title",
                                Version = 0
                            },
                            new Methodology
                            {
                                Id = Guid.Parse("8f02ec7b-fa52-4d8f-84e9-f04fa096d4d2"),
                                Annexes = new List<ContentSection>(),
                                Content = new List<ContentSection>(),
                                PreviousVersionId = Guid.Parse("d49b519f-7b50-4bd3-bb87-8464cd434e16"),
                                PublishingStrategy = Immediately,
                                Slug = "methodology-2-slug",
                                Status = Draft,
                                Title = "Methodology 2 v1 title",
                                Version = 1
                            }
                        )
                    },
                    Owner = false
                }
            );

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddAsync(theme);
                await contentDbContext.PublicationMethodologies.AddRangeAsync(publicationMethodologies);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyRepository = new Mock<IMethodologyRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyRepository: methodologyRepository.Object);

                var result = await service.GetTree();

                result.AssertRight();

                var themes = result.Right;
                Assert.Single(themes);

                Assert.Equal(theme.Id, themes[0].Id);
                Assert.Null(themes[0].Summary);
                Assert.Equal("Theme title", themes[0].Title);

                var topics = themes[0].Topics;
                Assert.Single(topics);

                Assert.Equal(theme.Topics[0].Id, topics[0].Id);
                Assert.Null(topics[0].Summary);
                Assert.Equal("Topic title", topics[0].Title);

                var publications = topics[0].Publications;
                Assert.Single(publications);

                Assert.Equal(publication.Id, publications[0].Id);
                Assert.Equal("publication-slug", publications[0].Slug);
                Assert.Equal("Publication summary", publications[0].Summary);
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
        public async Task GetTree_ThemeWithoutReleasesIsNotIncluded()
        {
            var publication = new Publication
            {
                Title = "Publication title",
                Slug = "publication-slug",
                Summary = "Publication summary",
                Releases = new List<Release>()
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

            var publicationMethodology = new PublicationMethodology
            {
                Publication = publication,
                MethodologyParent = new MethodologyParent
                {
                    Slug = "methodology-slug",
                    Versions = AsList(
                        new Methodology
                        {
                            Annexes = new List<ContentSection>(),
                            Content = new List<ContentSection>(),
                            PreviousVersionId = null,
                            PublishingStrategy = Immediately,
                            Slug = "methodology-slug",
                            Status = Approved,
                            Title = "Methodology title",
                            Version = 0
                        }
                    )
                },
                Owner = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddAsync(theme);
                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
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
        public async Task GetTree_ThemeWithoutMethodologiesIsNotIncluded()
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
                        Publications = AsList(
                            new Publication
                            {
                                Title = "Publication title",
                                Slug = "publication-slug",
                                Summary = "Publication summary",
                                Releases = AsList(
                                    new Release
                                    {
                                        ReleaseName = "2018",
                                        TimePeriodCoverage = AcademicYearQ1,
                                        Published = new DateTime(2019, 1, 01),
                                        ApprovalStatus = ReleaseApprovalStatus.Approved
                                    }
                                )
                            }
                        )
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
        public async Task GetTree_ThemeWithoutApprovedReleasesIsNotIncluded()
        {
            var publication = new Publication
            {
                Title = "Publication title",
                Slug = "publication-slug",
                Summary = "Publication summary",
                Releases = AsList(
                    new Release
                    {
                        ReleaseName = "2018",
                        TimePeriodCoverage = AcademicYearQ1,
                        Published = null,
                        ApprovalStatus = ReleaseApprovalStatus.Draft
                    }
                )
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

            var publicationMethodology = new PublicationMethodology
            {
                Publication = publication,
                MethodologyParent = new MethodologyParent
                {
                    Slug = "methodology-slug",
                    Versions = AsList(
                        new Methodology
                        {
                            Annexes = new List<ContentSection>(),
                            Content = new List<ContentSection>(),
                            PreviousVersionId = null,
                            PublishingStrategy = Immediately,
                            Slug = "methodology-slug",
                            Status = Approved,
                            Title = "Methodology title",
                            Version = 0
                        }
                    )
                },
                Owner = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddAsync(theme);
                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
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
        public async Task GetTree_ThemeWithoutApprovedMethodologiesIsNotIncluded()
        {
            var publication = new Publication
            {
                Title = "Publication title",
                Slug = "publication-slug",
                Summary = "Publication summary",
                Releases = AsList(
                    new Release
                    {
                        ReleaseName = "2018",
                        TimePeriodCoverage = AcademicYearQ1,
                        Published = null,
                        ApprovalStatus = ReleaseApprovalStatus.Approved
                    }
                )
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

            var publicationMethodology = new PublicationMethodology
            {
                Publication = publication,
                MethodologyParent = new MethodologyParent
                {
                    Slug = "methodology-slug",
                    Versions = AsList(
                        new Methodology
                        {
                            Annexes = new List<ContentSection>(),
                            Content = new List<ContentSection>(),
                            PreviousVersionId = null,
                            PublishingStrategy = Immediately,
                            Slug = "methodology-slug",
                            Status = Draft,
                            Title = "Methodology title",
                            Version = 0
                        }
                    )
                },
                Owner = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddAsync(theme);
                await contentDbContext.PublicationMethodologies.AddAsync(publicationMethodology);
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
