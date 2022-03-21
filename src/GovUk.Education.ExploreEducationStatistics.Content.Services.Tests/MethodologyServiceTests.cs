﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
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
    public class MethodologyServiceTests
    {
        [Fact]
        public async Task GetLatestMethodologyBySlug()
        {
            var methodology = new Methodology
            {
                Slug = "methodology-slug",
                OwningPublicationTitle = "Methodology title",
                Versions = new List<MethodologyVersion>
                {
                    new()
                    {
                        PublishingStrategy = Immediately,
                        Published = DateTime.UtcNow,
                        Status = Approved,
                        AlternativeTitle = "Alternative title",
                    }
                }
            };

            var publication = new Publication
            {
                Slug = "publication-slug",
                Title = "Publication title",
                Methodologies = new List<PublicationMethodology>
                {
                    new()
                    {
                        Methodology = methodology,
                        Owner = true
                    }
                },
                Releases = new List<Release>
                {
                    new()
                    {
                        Published = new DateTime(2021, 1, 1)
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            methodologyVersionRepository.Setup(mock => mock.GetLatestPublishedVersion(methodology.Id))
                .ReturnsAsync(methodology.Versions[0]);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Attach(methodology.Versions[0]);

                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var result = (await service.GetLatestMethodologyBySlug(methodology.Slug)).AssertRight();

                VerifyAllMocks(methodologyVersionRepository);

                Assert.Equal(methodology.Versions[0].Id, result.Id);
                Assert.Equal(methodology.Versions[0].Published, result.Published);
                Assert.Equal(methodology.Slug, result.Slug);
                Assert.Equal("Alternative title", result.Title);
                Assert.Empty(result.Annexes);
                Assert.Empty(result.Content);
                Assert.Empty(result.Notes);

                Assert.Single(result.Publications);
                Assert.Equal(publication.Id, result.Publications[0].Id);
                Assert.Equal(publication.Slug, result.Publications[0].Slug);
                Assert.Equal(publication.Title, result.Publications[0].Title);
            }
        }

        [Fact]
        public async Task GetLatestMethodologyBySlug_FiltersPublicationsWithNoPublishedReleases()
        {
            var methodology = new Methodology
            {
                Versions = new List<MethodologyVersion>
                {
                    new()
                    {
                        PublishingStrategy = Immediately,
                        Published = DateTime.UtcNow,
                        Status = Approved
                    }
                }
            };

            // Publication has a published release and is visible
            var publicationA = new Publication
            {
                Slug = "publication-a",
                Title = "Publication A",
                Methodologies = new List<PublicationMethodology>
                {
                    new()
                    {
                        Methodology = methodology,
                        Owner = true
                    }
                },
                Releases = new List<Release>
                {
                    new()
                    {
                        Published = new DateTime(2021, 1, 1)
                    }
                }
            };

            // Publication has published and unpublished releases and is visible
            var publicationB = new Publication
            {
                Slug = "publication-b",
                Title = "Publication B",
                Methodologies = new List<PublicationMethodology>
                {
                    new()
                    {
                        Methodology = methodology,
                        Owner = false
                    }
                },
                Releases = new List<Release>
                {
                    new()
                    {
                        Published = new DateTime(2021, 1, 1)
                    },
                    new()
                }
            };

            // Publication has no releases and is not visible
            var publicationC = new Publication
            {
                Slug = "publication-c",
                Title = "Publication C",
                Methodologies = new List<PublicationMethodology>
                {
                    new()
                    {
                        Methodology = methodology,
                        Owner = false
                    }
                },
                Releases = new List<Release>()
            };

            // Publication has no published releases and is not visible
            var publicationD = new Publication
            {
                Slug = "publication-d",
                Title = "Publication D",
                Methodologies = new List<PublicationMethodology>
                {
                    new()
                    {
                        Methodology = methodology,
                        Owner = false
                    }
                },
                Releases = new List<Release>
                {
                    // Release is not published
                    new()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.Publications.AddRangeAsync(publicationA, publicationB, publicationC,
                    publicationD);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            methodologyVersionRepository.Setup(mock => mock.GetLatestPublishedVersion(methodology.Id))
                .ReturnsAsync(methodology.Versions[0]);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Attach(methodology.Versions[0]);

                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var result = (await service.GetLatestMethodologyBySlug(methodology.Slug)).AssertRight();

                VerifyAllMocks(methodologyVersionRepository);

                Assert.Equal(2, result.Publications.Count);
                Assert.Equal(publicationA.Id, result.Publications[0].Id);
                Assert.Equal(publicationA.Slug, result.Publications[0].Slug);
                Assert.Equal(publicationA.Title, result.Publications[0].Title);
                Assert.Equal(publicationB.Id, result.Publications[1].Id);
                Assert.Equal(publicationB.Slug, result.Publications[1].Slug);
                Assert.Equal(publicationB.Title, result.Publications[1].Title);
            }
        }

        [Fact]
        public async Task GetLatestMethodologyBySlug_TestContentSections()
        {
            var methodology = new Methodology
            {
                Slug = "methodology-slug",
                OwningPublicationTitle = "Methodology title",
                Versions = new List<MethodologyVersion>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Annexes = new List<ContentSection>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Order = 2,
                                Heading = "Annex 3 heading",
                                Caption = "Annex 3 caption"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Order = 0,
                                Heading = "Annex 1 heading",
                                Caption = "Annex 1 caption"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Order = 1,
                                Heading = "Annex 2 heading",
                                Caption = "Annex 2 caption"
                            }
                        },
                        Content = new List<ContentSection>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Order = 2,
                                Heading = "Section 3 heading",
                                Caption = "Section 3 caption"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Order = 0,
                                Heading = "Section 1 heading",
                                Caption = "Section 1 caption"
                            },
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Order = 1,
                                Heading = "Section 2 heading",
                                Caption = "Section 2 caption"
                            }
                        },
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        AlternativeTitle = "Alternative title"
                    }
                }
            };
            var methodologyVersion = methodology.Versions[0];

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            methodologyVersionRepository.Setup(mock => mock.GetLatestPublishedVersion(methodology.Id))
                .ReturnsAsync(methodology.Versions[0]);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Attach(methodology.Versions[0]);

                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var result = (await service.GetLatestMethodologyBySlug(methodology.Slug)).AssertRight();

                VerifyAllMocks(methodologyVersionRepository);

                Assert.Equal(3, result.Annexes.Count);
                Assert.Equal(3, result.Content.Count);

                var expectedAnnex1 = methodologyVersion.Annexes.Single(section => section.Order == 0);
                var expectedAnnex2 = methodologyVersion.Annexes.Single(section => section.Order == 1);
                var expectedAnnex3 = methodologyVersion.Annexes.Single(section => section.Order == 2);

                AssertContentSectionAndViewModelEqual(expectedAnnex1, result.Annexes[0]);
                AssertContentSectionAndViewModelEqual(expectedAnnex2, result.Annexes[1]);
                AssertContentSectionAndViewModelEqual(expectedAnnex3, result.Annexes[2]);

                var expectedContent1 = methodologyVersion.Content.Single(section => section.Order == 0);
                var expectedContent2 = methodologyVersion.Content.Single(section => section.Order == 1);
                var expectedContent3 = methodologyVersion.Content.Single(section => section.Order == 2);

                AssertContentSectionAndViewModelEqual(expectedContent1, result.Content[0]);
                AssertContentSectionAndViewModelEqual(expectedContent2, result.Content[1]);
                AssertContentSectionAndViewModelEqual(expectedContent3, result.Content[2]);
            }
        }

        private static void AssertContentSectionAndViewModelEqual(
            ContentSection expected,
            ContentSectionViewModel actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Caption, actual.Caption);
            Assert.Equal(expected.Heading, actual.Heading);
            Assert.Equal(expected.Order, actual.Order);
        }

        [Fact]
        public async Task GetLatestMethodologyBySlug_TestNotes()
        {
            var methodology = new Methodology
            {
                Slug = "methodology-slug",
                OwningPublicationTitle = "Methodology title",
                Versions = new List<MethodologyVersion>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        PublishingStrategy = Immediately,
                        Status = Approved,
                        AlternativeTitle = "Alternative title"
                    }
                }
            };
            var methodologyVersion = methodology.Versions[0];

            var otherNote = new MethodologyNote
            {
                DisplayDate = DateTime.Today.AddDays(-2).ToUniversalTime(),
                Content = "Other note",
                MethodologyVersion = methodologyVersion
            };

            var earliestNote = new MethodologyNote
            {
                DisplayDate = DateTime.Today.AddDays(-3).ToUniversalTime(),
                Content = "Earliest note",
                MethodologyVersion = methodologyVersion
            };

            var latestNote = new MethodologyNote
            {
                DisplayDate = DateTime.Today.AddDays(-1).ToUniversalTime(),
                Content = "Latest note",
                MethodologyVersion = methodologyVersion
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.MethodologyNotes.AddRangeAsync(otherNote, earliestNote, latestNote);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            methodologyVersionRepository.Setup(mock => mock.GetLatestPublishedVersion(methodology.Id))
                .ReturnsAsync(methodology.Versions[0]);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Attach(methodology.Versions[0]);

                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var result = (await service.GetLatestMethodologyBySlug(methodology.Slug)).AssertRight();

                VerifyAllMocks(methodologyVersionRepository);

                Assert.Equal(3, result.Notes.Count);

                AssertMethodologyNoteAndViewModelEqual(latestNote, result.Notes[0]);
                AssertMethodologyNoteAndViewModelEqual(otherNote, result.Notes[1]);
                AssertMethodologyNoteAndViewModelEqual(earliestNote, result.Notes[2]);
            }
        }

        private static void AssertMethodologyNoteAndViewModelEqual(
            MethodologyNote expected,
            MethodologyNoteViewModel actual)
        {
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Content, actual.Content);
            Assert.Equal(expected.DisplayDate, actual.DisplayDate);
        }

        [Fact]
        public async Task GetLatestMethodologyBySlug_MethodologyHasNoPublishedVersion()
        {
            var methodology = new Methodology
            {
                Slug = "methodology-slug",
                OwningPublicationTitle = "Methodology title"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            methodologyVersionRepository.Setup(mock => mock.GetLatestPublishedVersion(methodology.Id))
                .ReturnsAsync((MethodologyVersion?) null);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var result = await service.GetLatestMethodologyBySlug(methodology.Slug);

                VerifyAllMocks(methodologyVersionRepository);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetLatestMethodologyBySlug_SlugNotFound()
        {
            // Set up a methodology with a different slug to make sure it's not returned
            var methodology = new Methodology
            {
                Slug = "some-other-slug",
                OwningPublicationTitle = "Methodology title"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Methodologies.AddAsync(methodology);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            // Expect no call to get the latest published version since no methodology will be found

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var result = await service.GetLatestMethodologyBySlug("methodology-slug");

                VerifyAllMocks(methodologyVersionRepository);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetSummariesByPublication()
        {
            var publication = new Publication();

            var methodology = new Methodology
            {
                Slug = "methodology-1",
                OwningPublicationTitle = "Methodology 1 title"
            };

            var versions = AsList(
                new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    Methodology = methodology
                },
                new MethodologyVersion
                {
                    Id = Guid.NewGuid(),
                    Methodology = methodology,
                    AlternativeTitle = "Methodology 2 title"
                }
            );

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            methodologyVersionRepository.Setup(mock => mock.GetLatestPublishedVersionByPublication(publication.Id))
                .ReturnsAsync(versions);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var result = (await service.GetSummariesByPublication(publication.Id)).AssertRight();

                Assert.Equal(2, result.Count);

                Assert.Equal(versions[0].Id, result[0].Id);
                Assert.Equal(methodology.Slug, result[0].Slug);
                Assert.Equal(methodology.OwningPublicationTitle, result[0].Title);

                Assert.Equal(versions[1].Id, result[1].Id);
                Assert.Equal(methodology.Slug, result[1].Slug);
                Assert.Equal(versions[1].AlternativeTitle, result[1].Title);
            }

            VerifyAllMocks(methodologyVersionRepository);
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

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            methodologyVersionRepository.Setup(mock => mock.GetLatestPublishedVersionByPublication(publication.Id))
                .ReturnsAsync(new List<MethodologyVersion>());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var result = (await service.GetSummariesByPublication(publication.Id)).AssertRight();

                Assert.Empty(result);
            }

            VerifyAllMocks(methodologyVersionRepository);
        }

        [Fact]
        public async Task GetSummariesByPublication_PublicationNotFound()
        {
            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext())
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var result = await service.GetSummariesByPublication(Guid.NewGuid());

                result.AssertNotFound();
            }

            VerifyAllMocks(methodologyVersionRepository);
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

            var latestVersions = AsList(
                new MethodologyVersion
                {
                    Id = Guid.Parse("7cba2701-b8d0-4d1a-ba9d-0cb71bf56574"),
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
                    Id = Guid.Parse("d49b519f-7b50-4bd3-bb87-8464cd434e16"),
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
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

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
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var result = (await service.GetTree()).AssertRight();

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

            var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var result = await service.GetTree();

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
                var service = SetupMethodologyService(contentDbContext: contentDbContext,
                    methodologyVersionRepository: methodologyVersionRepository.Object);

                var result = await service.GetTree();

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
