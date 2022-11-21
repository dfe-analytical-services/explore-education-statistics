#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using MockQueryable.Moq;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.SortOrder;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IPublicationService.
    PublicationsSortBy;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public class PublicationServiceTests
    {
        private const string PublicationSlug = "publication-slug";

        [Fact]
        public async Task Get()
        {
            var release2000Version0Id = Guid.NewGuid();
            var release2000Version1Id = Guid.NewGuid();
            var publication = new Publication
            {
                Title = "Publication Title",
                Slug = PublicationSlug,
                LatestPublishedReleaseId = release2000Version1Id,
                Releases = new List<Release>
                {
                    new() // latest published release
                    {
                        Id = release2000Version1Id,
                        ReleaseName = "2000",
                        Slug = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = DateTime.UtcNow,
                        Version = 1,
                        PreviousVersionId = release2000Version0Id,
                    },
                    new() // previous version
                    {
                        Id = release2000Version0Id,
                        ReleaseName = "2000",
                        Slug = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = DateTime.UtcNow,
                        Version = 0,
                    },
                    new() // not published
                    {
                        ReleaseName = "2001",
                        Slug = "2001",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = null,
                        Version = 0,
                    },
                    new() // published so appears in ListPublishedReleases result
                    {
                        ReleaseName = "1999",
                        Slug = "1999",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = DateTime.UtcNow,
                        Version = 0,
                    },
                },
                LegacyReleases = new List<LegacyRelease>
                {
                    new()
                    {
                        Description = "Legacy release description",
                        Url = "https://legacy.release.com",
                        Order = 0,
                    }
                },
                Topic = new Topic
                {
                    Title = "Test topic",
                    Slug = "test-topic",
                    Theme = new Theme
                    {
                        Title = "Test theme",
                        Slug = "test-theme",
                    }
                },
                Contact = new Contact
                {
                    TeamName = "Team name",
                    TeamEmail = "team@email.com",
                    ContactName = "Contact name",
                    ContactTelNo = "1234",
                },
                ExternalMethodology = new ExternalMethodology
                {
                    Title = "External methodology title",
                    Url = "https://external.methodology.com",
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var result = await service.Get(publication.Slug);

                var publicationViewModel = result.AssertRight();

                Assert.Equal(publication.Id, publicationViewModel.Id);
                Assert.Equal(publication.Title, publicationViewModel.Title);
                Assert.Equal(publication.Slug, publicationViewModel.Slug);
                Assert.False(publicationViewModel.IsSuperseded);

                Assert.Equal(2, publicationViewModel.Releases.Count);
                Assert.Equal(publication.Releases[0].Id, publicationViewModel.LatestReleaseId);

                Assert.Equal(publication.Releases[0].Id, publicationViewModel.Releases[0].Id);
                Assert.Equal(publication.Releases[0].Slug, publicationViewModel.Releases[0].Slug);
                Assert.Equal(publication.Releases[0].Title, publicationViewModel.Releases[0].Title);

                Assert.Equal(publication.Releases[3].Id, publicationViewModel.Releases[1].Id);
                Assert.Equal(publication.Releases[3].Slug, publicationViewModel.Releases[1].Slug);
                Assert.Equal(publication.Releases[3].Title, publicationViewModel.Releases[1].Title);

                Assert.Single(publication.LegacyReleases);
                Assert.Equal(publication.LegacyReleases[0].Id, publicationViewModel.LegacyReleases[0].Id);
                Assert.Equal(publication.LegacyReleases[0].Description,
                    publicationViewModel.LegacyReleases[0].Description);
                Assert.Equal(publication.LegacyReleases[0].Url, publicationViewModel.LegacyReleases[0].Url);

                Assert.Equal(publication.Topic.Theme.Title, publicationViewModel.Topic.Theme.Title);

                Assert.Equal(publication.Contact.TeamName, publicationViewModel.Contact.TeamName);
                Assert.Equal(publication.Contact.TeamEmail, publicationViewModel.Contact.TeamEmail);
                Assert.Equal(publication.Contact.ContactName, publicationViewModel.Contact.ContactName);
                Assert.Equal(publication.Contact.ContactTelNo, publicationViewModel.Contact.ContactTelNo);

                Assert.NotNull(publicationViewModel.ExternalMethodology);
                Assert.Equal(publication.ExternalMethodology.Title, publicationViewModel.ExternalMethodology!.Title);
                Assert.Equal(publication.ExternalMethodology.Url, publicationViewModel.ExternalMethodology.Url);
            }
        }

        [Fact]
        public async Task Get_IsSuperseded_SupersedingPublicationHasPublishedRelease()
        {
            var releaseId = Guid.NewGuid();

            var publication = new Publication
            {
                Title = "Publication Title",
                Slug = PublicationSlug,
                LatestPublishedReleaseId = releaseId,
                SupersededBy = new Publication
                {
                    LatestPublishedReleaseId = Guid.NewGuid()
                },
                Releases = new List<Release>
                {
                    new()
                    {
                        Id = releaseId,
                        ReleaseName = "2000",
                        Slug = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = DateTime.UtcNow
                    },
                },
                Topic = new Topic
                {
                    Title = "Test topic",
                    Slug = "test-topic",
                    Theme = new Theme
                    {
                        Title = "Test theme",
                        Slug = "test-theme",
                    }
                },
                Contact = new Contact
                {
                    TeamName = "Team name",
                    TeamEmail = "team@email.com",
                    ContactName = "Contact name",
                    ContactTelNo = "1234",
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var result = await service.Get(publication.Slug);

                var publicationViewModel = result.AssertRight();

                Assert.Equal(publication.Id, publicationViewModel.Id);
                Assert.True(publicationViewModel.IsSuperseded);
            }
        }

        [Fact]
        public async Task Get_IsSuperseded_SupersedingPublicationHasNoPublishedRelease()
        {
            var releaseId = Guid.NewGuid();

            var publication = new Publication
            {
                Title = "Publication Title",
                Slug = PublicationSlug,
                LatestPublishedReleaseId = releaseId,
                SupersededBy = new Publication
                {
                    LatestPublishedReleaseId = null
                },
                Releases = new List<Release>
                {
                    new()
                    {
                        Id = releaseId,
                        ReleaseName = "2000",
                        Slug = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear
                    }
                },
                Topic = new Topic
                {
                    Title = "Test topic",
                    Slug = "test-topic",
                    Theme = new Theme
                    {
                        Title = "Test theme",
                        Slug = "test-theme",
                    }
                },
                Contact = new Contact
                {
                    TeamName = "Team name",
                    TeamEmail = "team@email.com",
                    ContactName = "Contact name",
                    ContactTelNo = "1234",
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var result = await service.Get(publication.Slug);

                var publicationViewModel = result.AssertRight();

                Assert.Equal(publication.Id, publicationViewModel.Id);
                Assert.False(publicationViewModel.IsSuperseded);
            }
        }

        [Fact]
        public async Task Get_NoPublication()
        {
            var service = SetupPublicationService();

            var result = await service.Get("nonexistent-publication");

            result.AssertNotFound();
        }

        [Fact]
        public async Task ListPublications()
        {
            var publicationA = new Publication
            {
                Slug = "publication-a",
                Title = "Publication A",
                Summary = "Publication A summary",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = new DateTime(2020, 1, 1)
                }
            };

            var publicationB = new Publication
            {
                Slug = "publication-b",
                Title = "Publication B",
                Summary = "Publication B summary",
                LatestPublishedRelease = new Release
                {
                    Type = OfficialStatistics,
                    Published = new DateTime(2021, 1, 1)
                }
            };

            var publicationC = new Publication
            {
                Slug = "publication-c",
                Title = "Publication C",
                Summary = "Publication C summary",
                LatestPublishedRelease = new Release
                {
                    Type = AdHocStatistics,
                    Published = new DateTime(2022, 1, 1)
                }
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationA,
                                publicationB
                            }
                        }
                    },
                },
                new()
                {
                    Title = "Theme 2 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationC
                            }
                        }
                    },
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications()).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(3, results.Count);

                // Expect results sorted by title in ascending order

                Assert.Equal(publicationA.Id, results[0].Id);
                Assert.Equal("publication-a", results[0].Slug);
                Assert.Equal(new DateTime(2020, 1, 1), results[0].Published);
                Assert.Equal("Publication A", results[0].Title);
                Assert.Equal("Publication A summary", results[0].Summary);
                Assert.Equal("Theme 1 title", results[0].Theme);
                Assert.Equal(NationalStatistics, results[0].Type);

                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal("publication-b", results[1].Slug);
                Assert.Equal(new DateTime(2021, 1, 1), results[1].Published);
                Assert.Equal("Publication B", results[1].Title);
                Assert.Equal("Publication B summary", results[1].Summary);
                Assert.Equal("Theme 1 title", results[1].Theme);
                Assert.Equal(OfficialStatistics, results[1].Type);

                Assert.Equal(publicationC.Id, results[2].Id);
                Assert.Equal("publication-c", results[2].Slug);
                Assert.Equal(new DateTime(2022, 1, 1), results[2].Published);
                Assert.Equal("Publication C", results[2].Title);
                Assert.Equal("Publication C summary", results[2].Summary);
                Assert.Equal("Theme 2 title", results[2].Theme);
                Assert.Equal(AdHocStatistics, results[2].Type);
            }
        }

        [Fact]
        public async Task ListPublications_ExcludesUnpublishedPublications()
        {
            // Published
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                }
            };

            // Not published (no published release)
            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = null
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationA,
                                publicationB
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications()).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(publicationA.Id, results[0].Id);
            }
        }

        [Fact]
        public async Task ListPublications_ExcludesSupersededPublications()
        {
            // Published
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                }
            };

            // Published
            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                }
            };

            // Not published (no published release)
            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedReleaseId = null
            };

            // Not published (superseded by publicationB which is published)
            var publicationD = new Publication
            {
                Title = "Publication D",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                },
                SupersededBy = publicationB
            };

            // Published (superseded by publicationC but it's not published yet)
            var publicationE = new Publication
            {
                Title = "Publication E",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                },
                SupersededBy = publicationC
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationA,
                                publicationB,
                                publicationC,
                                publicationD,
                                publicationE
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications()).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(publicationA.Id, results[0].Id);
                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal(publicationE.Id, results[2].Id);
            }
        }

        [Fact]
        public async Task ListPublications_FilterByTheme()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationA,
                                publicationB
                            }
                        }
                    },
                },
                new()
                {
                    Title = "Theme 2 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                new()
                                {
                                    Title = "Publication C",
                                    LatestPublishedRelease = new Release
                                    {
                                        Type = AdHocStatistics,
                                        Published = new DateTime(2022, 1, 1)
                                    }
                                }
                            }
                        }
                    },
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications(
                    themeId: themes[0].Id
                )).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(2, results.Count);

                Assert.Equal(publicationA.Id, results[0].Id);
                Assert.Equal("Theme 1 title", results[0].Theme);

                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal("Theme 1 title", results[1].Theme);
            }
        }

        [Fact]
        public async Task ListPublications_FilterByReleaseType()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = new Release
                {
                    Type = OfficialStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedRelease = new Release
                {
                    Type = AdHocStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationA,
                                publicationB
                            }
                        }
                    },
                },
                new()
                {
                    Title = "Theme 2 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationC
                            }
                        }
                    },
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications(
                    releaseType: OfficialStatistics
                )).AssertRight();
                var results = pagedResult.Results;

                Assert.Single(results);

                Assert.Equal(publicationB.Id, results[0].Id);
                Assert.Equal(OfficialStatistics, results[0].Type);
            }
        }

        [Fact]
        public async Task ListPublications_Search_SortByRelevance_Desc()
        {
            var releaseA = new Release
            {
                Id = Guid.NewGuid(),
                Type = NationalStatistics,
                Published = DateTime.UtcNow
            };

            var releaseB = new Release
            {
                Id = Guid.NewGuid(),
                Type = NationalStatistics,
                Published = DateTime.UtcNow
            };

            var releaseC = new Release
            {
                Id = Guid.NewGuid(),
                Type = NationalStatistics,
                Published = DateTime.UtcNow
            };

            var topic = new Topic
            {
                Theme = new Theme
                {
                    Title = "Theme title"
                }
            };

            var publications = new List<Publication>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication B",
                    LatestPublishedReleaseId = releaseB.Id,
                    LatestPublishedRelease = releaseB,
                    Topic = topic
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication C",
                    LatestPublishedReleaseId = releaseC.Id,
                    LatestPublishedRelease = releaseC,
                    Topic = topic
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication A",
                    LatestPublishedReleaseId = releaseA.Id,
                    LatestPublishedRelease = releaseA,
                    Topic = topic
                },
            };

            var freeTextRanks = new List<FreeTextRank>
            {
                new(publications[1].Id, 100),
                new(publications[2].Id, 300),
                new(publications[0].Id, 200)
            };

            var contentDbContext = new Mock<ContentDbContext>();
            contentDbContext.Setup(context => context.Publications)
                .Returns(publications.AsQueryable().BuildMockDbSet().Object);
            contentDbContext.Setup(context => context.PublicationsFreeTextTable("term"))
                .Returns(freeTextRanks.AsQueryable().BuildMockDbSet().Object);

            var service = SetupPublicationService(contentDbContext.Object);

            var pagedResult = (await service.ListPublications(
                search: "term",
                sort: null, // Sort should default to relevance
                order: null // Order should default to descending
            )).AssertRight();
            var results = pagedResult.Results;

            Assert.Equal(3, results.Count);

            // Expect results sorted by relevance in descending order

            Assert.Equal(publications[2].Id, results[0].Id);
            Assert.Equal(300, results[0].Rank);

            Assert.Equal(publications[0].Id, results[1].Id);
            Assert.Equal(200, results[1].Rank);

            Assert.Equal(publications[1].Id, results[2].Id);
            Assert.Equal(100, results[2].Rank);
        }

        [Fact]
        public async Task ListPublications_Search_SortByRelevance_Asc()
        {
            var releaseA = new Release
            {
                Id = Guid.NewGuid(),
                Type = NationalStatistics,
                Published = DateTime.UtcNow
            };

            var releaseB = new Release
            {
                Id = Guid.NewGuid(),
                Type = NationalStatistics,
                Published = DateTime.UtcNow
            };

            var releaseC = new Release
            {
                Id = Guid.NewGuid(),
                Type = NationalStatistics,
                Published = DateTime.UtcNow
            };

            var topic = new Topic
            {
                Theme = new Theme
                {
                    Title = "Theme title"
                }
            };

            var publications = new List<Publication>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication B",
                    LatestPublishedReleaseId = releaseB.Id,
                    LatestPublishedRelease = releaseB,
                    Topic = topic
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication C",
                    LatestPublishedReleaseId = releaseC.Id,
                    LatestPublishedRelease = releaseC,
                    Topic = topic
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication A",
                    LatestPublishedReleaseId = releaseA.Id,
                    LatestPublishedRelease = releaseA,
                    Topic = topic
                },
            };

            var freeTextRanks = new List<FreeTextRank>
            {
                new(publications[1].Id, 100),
                new(publications[2].Id, 300),
                new(publications[0].Id, 200)
            };

            var contentDbContext = new Mock<ContentDbContext>();
            contentDbContext.Setup(context => context.Publications)
                .Returns(publications.AsQueryable().BuildMockDbSet().Object);
            contentDbContext.Setup(context => context.PublicationsFreeTextTable("term"))
                .Returns(freeTextRanks.AsQueryable().BuildMockDbSet().Object);

            var service = SetupPublicationService(contentDbContext.Object);

            var pagedResult = (await service.ListPublications(
                search: "term",
                sort: null, // Sort should default to relevance
                order: Asc
            )).AssertRight();
            var results = pagedResult.Results;

            Assert.Equal(3, results.Count);

            // Expect results sorted by relevance in ascending order

            Assert.Equal(publications[1].Id, results[0].Id);
            Assert.Equal(100, results[0].Rank);

            Assert.Equal(publications[0].Id, results[1].Id);
            Assert.Equal(200, results[1].Rank);

            Assert.Equal(publications[2].Id, results[2].Id);
            Assert.Equal(300, results[2].Rank);
        }

        [Fact]
        public async Task ListPublications_SortByPublished_Desc()
        {
            var releaseA = new Release
            {
                Type = NationalStatistics,
                Published = new DateTime(2020, 1, 1)
            };

            var releaseB = new Release
            {
                Type = NationalStatistics,
                Published = new DateTime(2021, 1, 1)
            };

            var releaseC = new Release
            {
                Type = NationalStatistics,
                Published = new DateTime(2022, 1, 1)
            };

            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = releaseA
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = releaseB
            };

            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedRelease = releaseC
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationB,
                                publicationC,
                                publicationA
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications(
                    sort: Published,
                    order: null // Order should default to descending
                )).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(3, results.Count);

                // Expect results sorted by published date in descending order

                Assert.Equal(publicationC.Id, results[0].Id);
                Assert.Equal(new DateTime(2022, 1, 1), results[0].Published);

                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal(new DateTime(2021, 1, 1), results[1].Published);

                Assert.Equal(publicationA.Id, results[2].Id);
                Assert.Equal(new DateTime(2020, 1, 1), results[2].Published);
            }
        }

        [Fact]
        public async Task ListPublications_SortByPublished_Asc()
        {
            var releaseA = new Release
            {
                Type = NationalStatistics,
                Published = new DateTime(2020, 1, 1)
            };

            var releaseB = new Release
            {
                Type = NationalStatistics,
                Published = new DateTime(2021, 1, 1)
            };

            var releaseC = new Release
            {
                Type = NationalStatistics,
                Published = new DateTime(2022, 1, 1)
            };

            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = releaseA
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = releaseB
            };

            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedRelease = releaseC
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationB,
                                publicationC,
                                publicationA
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications(
                    sort: Published,
                    order: Asc
                )).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(3, results.Count);

                // Expect results sorted by published date in ascending order

                Assert.Equal(publicationA.Id, results[0].Id);
                Assert.Equal(new DateTime(2020, 1, 1), results[0].Published);

                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal(new DateTime(2021, 1, 1), results[1].Published);

                Assert.Equal(publicationC.Id, results[2].Id);
                Assert.Equal(new DateTime(2022, 1, 1), results[2].Published);
            }
        }

        [Fact]
        public async Task ListPublications_SortByTitle_Desc()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = new Release
                {
                    Type = OfficialStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedRelease = new Release
                {
                    Type = AdHocStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationB,
                                publicationC,
                                publicationA
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications(
                    sort: null, // Sort should default to title
                    order: Desc
                )).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(3, results.Count);

                // Expect results sorted by title in descending order

                Assert.Equal(publicationC.Id, results[0].Id);
                Assert.Equal("Publication C", results[0].Title);

                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal("Publication B", results[1].Title);

                Assert.Equal(publicationA.Id, results[2].Id);
                Assert.Equal("Publication A", results[2].Title);
            }
        }

        [Fact]
        public async Task ListPublications_SortByTitle_Asc()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedRelease = new Release
                {
                    Type = NationalStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedRelease = new Release
                {
                    Type = OfficialStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedRelease = new Release
                {
                    Type = AdHocStatistics,
                    Published = DateTime.UtcNow
                }
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Topics = new List<Topic>
                    {
                        new()
                        {
                            Publications = new List<Publication>
                            {
                                publicationB,
                                publicationC,
                                publicationA
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupPublicationService(contentDbContext);

                var pagedResult = (await service.ListPublications(
                    sort: null, // Sort should default to title
                    order: null // Order should default to ascending
                )).AssertRight();
                var results = pagedResult.Results;

                Assert.Equal(3, results.Count);

                // Expect results sorted by title in ascending order

                Assert.Equal(publicationA.Id, results[0].Id);
                Assert.Equal("Publication A", results[0].Title);

                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal("Publication B", results[1].Title);

                Assert.Equal(publicationC.Id, results[2].Id);
                Assert.Equal("Publication C", results[2].Title);
            }
        }

        private static PublicationService SetupPublicationService(
            ContentDbContext? contentDbContext = null,
            IPublicationRepository? publicationRepository = null,
            IUserService? userService = null)
        {
            contentDbContext ??= InMemoryContentDbContext();

            return new(
                contentDbContext,
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                publicationRepository ?? new PublicationRepository(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}
