using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using MockQueryable.Moq;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.SortDirection;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Requests.PublicationsSortBy;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Publications;

public class PublicationsSearchServiceTests
{
    public class GetPublicationsTests : PublicationsSearchServiceTests
    {
        [Fact]
        public async Task Success()
        {
            var publicationA = new Publication
            {
                Slug = "publication-a",
                Title = "Publication A",
                Summary = "Publication A summary",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AccreditedOfficialStatistics,
                    Published = new DateTime(2020, 1, 1),
                    Release = new Release
                    {
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublicationId = Guid.NewGuid(),
                        Year = 2021,
                        Slug = "latest-release-slug A",
                    },
                },
            };

            var publicationB = new Publication
            {
                Slug = "publication-b",
                Title = "Publication B",
                Summary = "Publication B summary",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = OfficialStatistics,
                    Published = new DateTime(2021, 1, 1),
                    Release = new Release
                    {
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublicationId = Guid.NewGuid(),
                        Year = 2021,
                        Slug = "latest-release-slug B",
                    },
                },
            };

            var publicationC = new Publication
            {
                Slug = "publication-c",
                Title = "Publication C",
                Summary = "Publication C summary",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AdHocStatistics,
                    Published = new DateTime(2022, 1, 1),
                    Release = new Release
                    {
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        PublicationId = Guid.NewGuid(),
                        Year = 2021,
                        Slug = "latest-release-slug C",
                    },
                },
            };

            var themes = new List<Theme>
            {
                new() { Title = "Theme 1 title", Publications = [publicationA, publicationB] },
                new() { Title = "Theme 2 title", Publications = [publicationC] },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                var pagedResult = await service.GetPublications();
                var results = pagedResult.Results;

                Assert.Equal(3, results.Count);

                // Expect results sorted by title in ascending order

                Assert.Equal(publicationA.Id, results[0].Id);
                Assert.Equal("publication-a", results[0].Slug);
                Assert.Equal(new DateTime(2020, 1, 1), results[0].Published);
                Assert.Equal("Publication A", results[0].Title);
                Assert.Equal("Publication A summary", results[0].Summary);
                Assert.Equal("latest-release-slug A", results[0].LatestReleaseSlug);
                Assert.Equal("Theme 1 title", results[0].Theme);
                Assert.Equal(AccreditedOfficialStatistics, results[0].Type);

                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal("publication-b", results[1].Slug);
                Assert.Equal(new DateTime(2021, 1, 1), results[1].Published);
                Assert.Equal("Publication B", results[1].Title);
                Assert.Equal("Publication B summary", results[1].Summary);
                Assert.Equal("latest-release-slug B", results[1].LatestReleaseSlug);
                Assert.Equal("Theme 1 title", results[1].Theme);
                Assert.Equal(OfficialStatistics, results[1].Type);

                Assert.Equal(publicationC.Id, results[2].Id);
                Assert.Equal("publication-c", results[2].Slug);
                Assert.Equal(new DateTime(2022, 1, 1), results[2].Published);
                Assert.Equal("Publication C", results[2].Title);
                Assert.Equal("Publication C summary", results[2].Summary);
                Assert.Equal("latest-release-slug C", results[2].LatestReleaseSlug);
                Assert.Equal("Theme 2 title", results[2].Theme);
                Assert.Equal(AdHocStatistics, results[2].Type);
            }
        }

        [Fact]
        public async Task ExcludesUnpublishedPublications()
        {
            // Published
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AccreditedOfficialStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            // Not published (no published release)
            var publicationB = new Publication { Title = "Publication B", LatestPublishedReleaseVersion = null };

            var themes = new List<Theme>
            {
                new() { Title = "Theme 1 title", Publications = [publicationA, publicationB] },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                var pagedResult = await service.GetPublications();
                var results = pagedResult.Results;

                Assert.Equal(publicationA.Id, results[0].Id);
            }
        }

        [Fact]
        public async Task ExcludesSupersededPublications()
        {
            // Published
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AccreditedOfficialStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            // Published
            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AccreditedOfficialStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            // Not published (no published release)
            var publicationC = new Publication { Title = "Publication C", LatestPublishedReleaseVersionId = null };

            // Not published (superseded by publicationB which is published)
            var publicationD = new Publication
            {
                Title = "Publication D",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AccreditedOfficialStatistics,
                    Published = DateTime.UtcNow,
                },
                SupersededBy = publicationB,
            };

            // Published (superseded by publicationC but it's not published yet)
            var publicationE = new Publication
            {
                Title = "Publication E",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AccreditedOfficialStatistics,
                    Published = DateTime.UtcNow,
                },
                SupersededBy = publicationC,
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Publications = [publicationA, publicationB, publicationC, publicationD, publicationE],
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                var pagedResult = await service.GetPublications();
                var results = pagedResult.Results;

                Assert.Equal(publicationA.Id, results[0].Id);
                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal(publicationE.Id, results[2].Id);
            }
        }

        [Fact]
        public async Task FilterByTheme()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AccreditedOfficialStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AccreditedOfficialStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            var themes = new List<Theme>
            {
                new() { Title = "Theme 1 title", Publications = [publicationA, publicationB] },
                new()
                {
                    Title = "Theme 2 title",
                    Publications =
                    [
                        new()
                        {
                            Title = "Publication C",
                            LatestPublishedReleaseVersion = new ReleaseVersion
                            {
                                Type = AdHocStatistics,
                                Published = new DateTime(2022, 1, 1),
                            },
                        },
                    ],
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                var pagedResult = await service.GetPublications(themeId: themes[0].Id);
                var results = pagedResult.Results;

                Assert.Equal(2, results.Count);

                Assert.Equal(publicationA.Id, results[0].Id);
                Assert.Equal("Theme 1 title", results[0].Theme);

                Assert.Equal(publicationB.Id, results[1].Id);
                Assert.Equal("Theme 1 title", results[1].Theme);
            }
        }

        [Fact]
        public async Task FilterByPublicationIds()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AccreditedOfficialStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AccreditedOfficialStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AccreditedOfficialStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            var publicationD = new Publication
            {
                Title = "Publication D",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AccreditedOfficialStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Publications = [publicationA, publicationB, publicationC, publicationD],
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                var pagedResult = await service.GetPublications(publicationIds: [publicationA.Id, publicationB.Id]);
                var results = pagedResult.Results;

                Assert.Equal(2, results.Count);

                Assert.Equal(publicationA.Id, results[0].Id);

                Assert.Equal(publicationB.Id, results[1].Id);
            }
        }

        [Fact]
        public async Task FilterByReleaseType()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AccreditedOfficialStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = OfficialStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AdHocStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            var themes = new List<Theme>
            {
                new() { Title = "Theme 1 title", Publications = [publicationA, publicationB] },
                new() { Title = "Theme 2 title", Publications = [publicationC] },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                var pagedResult = await service.GetPublications(releaseType: OfficialStatistics);
                var results = pagedResult.Results;

                Assert.Single(results);

                Assert.Equal(publicationB.Id, results[0].Id);
                Assert.Equal(OfficialStatistics, results[0].Type);
            }
        }

        [Fact]
        public async Task Search_SortByRelevance_Desc()
        {
            var releaseVersionA = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Type = AccreditedOfficialStatistics,
                Published = DateTime.UtcNow,
                Release = new Release
                {
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    PublicationId = Guid.NewGuid(),
                    Year = 2021,
                    Slug = "latest-release-slug A",
                },
            };

            var releaseVersionB = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Type = AccreditedOfficialStatistics,
                Published = DateTime.UtcNow,
                Release = new Release
                {
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    PublicationId = Guid.NewGuid(),
                    Year = 2021,
                    Slug = "latest-release-slug B",
                },
            };

            var releasedVersionC = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Type = AccreditedOfficialStatistics,
                Published = DateTime.UtcNow,
                Release = new Release
                {
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    PublicationId = Guid.NewGuid(),
                    Year = 2021,
                    Slug = "latest-release-slug C",
                },
            };

            var theme = new Theme { Title = "Theme title" };

            var publications = new List<Publication>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication B",
                    LatestPublishedReleaseVersionId = releaseVersionB.Id,
                    LatestPublishedReleaseVersion = releaseVersionB,
                    Theme = theme,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication C",
                    LatestPublishedReleaseVersionId = releasedVersionC.Id,
                    LatestPublishedReleaseVersion = releasedVersionC,
                    Theme = theme,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication A",
                    LatestPublishedReleaseVersionId = releaseVersionA.Id,
                    LatestPublishedReleaseVersion = releaseVersionA,
                    Theme = theme,
                },
            };

            var freeTextRanks = new List<FreeTextRank>
            {
                new(publications[1].Id, 100),
                new(publications[2].Id, 300),
                new(publications[0].Id, 200),
            };

            var contentDbContext = new Mock<ContentDbContext>();
            contentDbContext
                .Setup(context => context.Publications)
                .Returns(publications.AsQueryable().BuildMockDbSet().Object);
            contentDbContext
                .Setup(context => context.PublicationsFreeTextTable("term"))
                .Returns(freeTextRanks.AsQueryable().BuildMockDbSet().Object);

            var service = BuildService(contentDbContext.Object);

            var pagedResult = await service.GetPublications(
                search: "term",
                sort: null, // Default to relevance
                sortDirection: null // Default to descending
            );
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
        public async Task Search_SortByRelevance_Asc()
        {
            var releaseVersionA = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Type = AccreditedOfficialStatistics,
                Published = DateTime.UtcNow,
                Release = new Release
                {
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    PublicationId = Guid.NewGuid(),
                    Year = 2021,
                    Slug = "latest-release-slug A",
                },
            };

            var releaseVersionB = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Type = AccreditedOfficialStatistics,
                Published = DateTime.UtcNow,
                Release = new Release
                {
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    PublicationId = Guid.NewGuid(),
                    Year = 2021,
                    Slug = "latest-release-slug B",
                },
            };

            var releaseVersionC = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                Type = AccreditedOfficialStatistics,
                Published = DateTime.UtcNow,
                Release = new Release
                {
                    TimePeriodCoverage = TimeIdentifier.AcademicYear,
                    PublicationId = Guid.NewGuid(),
                    Year = 2021,
                    Slug = "latest-release-slug C",
                },
            };

            var theme = new Theme { Title = "Theme title" };

            var publications = new List<Publication>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication B",
                    LatestPublishedReleaseVersionId = releaseVersionB.Id,
                    LatestPublishedReleaseVersion = releaseVersionB,
                    Theme = theme,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication C",
                    LatestPublishedReleaseVersionId = releaseVersionC.Id,
                    LatestPublishedReleaseVersion = releaseVersionC,
                    Theme = theme,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication A",
                    LatestPublishedReleaseVersionId = releaseVersionA.Id,
                    LatestPublishedReleaseVersion = releaseVersionA,
                    Theme = theme,
                },
            };

            var freeTextRanks = new List<FreeTextRank>
            {
                new(publications[1].Id, 100),
                new(publications[2].Id, 300),
                new(publications[0].Id, 200),
            };

            var contentDbContext = new Mock<ContentDbContext>();
            contentDbContext
                .Setup(context => context.Publications)
                .Returns(publications.AsQueryable().BuildMockDbSet().Object);
            contentDbContext
                .Setup(context => context.PublicationsFreeTextTable("term"))
                .Returns(freeTextRanks.AsQueryable().BuildMockDbSet().Object);

            var service = BuildService(contentDbContext.Object);

            var pagedResult = await service.GetPublications(
                search: "term",
                sort: null, // Sort should default to relevance
                sortDirection: Asc
            );
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
        public async Task SortByPublished_Desc()
        {
            var releaseVersionA = new ReleaseVersion
            {
                Type = AccreditedOfficialStatistics,
                Published = new DateTime(2020, 1, 1),
            };

            var releaseVersionB = new ReleaseVersion
            {
                Type = AccreditedOfficialStatistics,
                Published = new DateTime(2021, 1, 1),
            };

            var releaseVersionC = new ReleaseVersion
            {
                Type = AccreditedOfficialStatistics,
                Published = new DateTime(2022, 1, 1),
            };

            // Two releases with the same published date should be ordered by Type
            var releaseVersionD = new ReleaseVersion { Type = AdHocStatistics, Published = new DateTime(2023, 1, 1) };

            var releaseVersionE = new ReleaseVersion
            {
                Type = OfficialStatistics,
                Published = new DateTime(2023, 1, 1),
            };

            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedReleaseVersion = releaseVersionA,
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedReleaseVersion = releaseVersionB,
            };

            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedReleaseVersion = releaseVersionC,
            };

            var publicationD = new Publication
            {
                Title = "Publication D",
                LatestPublishedReleaseVersion = releaseVersionD,
            };

            var publicationE = new Publication
            {
                Title = "Publication E",
                LatestPublishedReleaseVersion = releaseVersionE,
            };

            var themes = new List<Theme>
            {
                new()
                {
                    Title = "Theme 1 title",
                    Publications = [publicationB, publicationC, publicationA, publicationD, publicationE],
                },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                var pagedResult = await service.GetPublications(
                    sort: Published,
                    sortDirection: null // Default to descending
                );
                var results = pagedResult.Results;

                Assert.Equal(5, results.Count);

                // Expect results sorted by published date in descending order

                Assert.Equal(publicationE.Id, results[0].Id);
                Assert.Equal(new DateTime(2023, 1, 1), results[0].Published);

                Assert.Equal(publicationD.Id, results[1].Id);
                Assert.Equal(new DateTime(2023, 1, 1), results[1].Published);

                Assert.Equal(publicationC.Id, results[2].Id);
                Assert.Equal(new DateTime(2022, 1, 1), results[2].Published);

                Assert.Equal(publicationB.Id, results[3].Id);
                Assert.Equal(new DateTime(2021, 1, 1), results[3].Published);

                Assert.Equal(publicationA.Id, results[4].Id);
                Assert.Equal(new DateTime(2020, 1, 1), results[4].Published);
            }
        }

        [Fact]
        public async Task SortByPublished_Asc()
        {
            var releaseVersionA = new ReleaseVersion
            {
                Type = AccreditedOfficialStatistics,
                Published = new DateTime(2020, 1, 1),
            };

            var releaseVersionB = new ReleaseVersion
            {
                Type = AccreditedOfficialStatistics,
                Published = new DateTime(2021, 1, 1),
            };

            var releaseVersionC = new ReleaseVersion
            {
                Type = AccreditedOfficialStatistics,
                Published = new DateTime(2022, 1, 1),
            };

            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedReleaseVersion = releaseVersionA,
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedReleaseVersion = releaseVersionB,
            };

            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedReleaseVersion = releaseVersionC,
            };

            var themes = new List<Theme>
            {
                new() { Title = "Theme 1 title", Publications = [publicationB, publicationC, publicationA] },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                var pagedResult = await service.GetPublications(sort: Published, sortDirection: Asc);
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
        public async Task SortByTitle_Desc()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AccreditedOfficialStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = OfficialStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AdHocStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            var themes = new List<Theme>
            {
                new() { Title = "Theme 1 title", Publications = [publicationB, publicationC, publicationA] },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                var pagedResult = await service.GetPublications(
                    sort: null, // Default to title
                    sortDirection: Desc
                );
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
        public async Task SortByTitle_Asc()
        {
            var publicationA = new Publication
            {
                Title = "Publication A",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AccreditedOfficialStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            var publicationB = new Publication
            {
                Title = "Publication B",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = OfficialStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            var publicationC = new Publication
            {
                Title = "Publication C",
                LatestPublishedReleaseVersion = new ReleaseVersion
                {
                    Type = AdHocStatistics,
                    Published = DateTime.UtcNow,
                },
            };

            var themes = new List<Theme>
            {
                new() { Title = "Theme 1 title", Publications = [publicationB, publicationC, publicationA] },
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Themes.AddRangeAsync(themes);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                var pagedResult = await service.GetPublications(
                    sort: null, // Default to title
                    sortDirection: null // Default to ascending
                );
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
    }

    private static PublicationsSearchService BuildService(ContentDbContext contentDbContext) => new(contentDbContext);
}
