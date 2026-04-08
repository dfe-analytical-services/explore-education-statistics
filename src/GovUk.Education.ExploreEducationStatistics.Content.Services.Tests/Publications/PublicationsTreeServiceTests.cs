using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Publications;

public abstract class PublicationsTreeServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetPublicationsTreeTests : PublicationsTreeServiceTests
    {
        [Fact]
        public async Task WhenNoThemesAndPublicationsExist_ReturnsEmptyTree()
        {
            // Arrange
            await using var context = InMemoryContentDbContext();
            var sut = BuildService(context);

            // Act
            var result = await sut.GetPublicationsTree();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task WhenMultipleThemesAndPublicationsExist_ReturnsThemesWithTheirPublications()
        {
            // Arrange
            var (publication1, publication2, publication3) = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .GenerateTuple3();

            var (theme1, theme2) = _dataFixture
                .DefaultTheme()
                .ForIndex(0, s => s.SetPublications([publication1, publication2]))
                .ForIndex(1, s => s.SetPublications([publication3]))
                .GenerateTuple2();

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Themes.AddRange(theme1, theme2);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var result = await sut.GetPublicationsTree();

                // Assert
                Assert.Equal(2, result.Length);
                Assert.Equal(theme1.Title, result[0].Title);
                Assert.Equal(theme1.Summary, result[0].Summary);
                Assert.Equal(theme2.Title, result[1].Title);
                Assert.Equal(theme2.Summary, result[1].Summary);

                var theme1Publications = result[0].Publications;
                Assert.Equal(2, theme1Publications.Length);
                Assert.Equal(theme1.Publications[0].Title, theme1Publications[0].Title);
                Assert.Equal(theme1.Publications[1].Title, theme1Publications[1].Title);
                Assert.Equal(publication1.Slug, theme1Publications[0].Slug);
                Assert.Equal(publication1.Title, theme1Publications[0].Title);
                Assert.Equal(publication2.Slug, theme1Publications[1].Slug);
                Assert.Equal(publication2.Title, theme1Publications[1].Title);
                Assert.False(theme1Publications[0].AnyLiveReleaseHasData);
                Assert.False(theme1Publications[0].LatestReleaseHasData);
                Assert.False(theme1Publications[1].AnyLiveReleaseHasData);
                Assert.False(theme1Publications[1].LatestReleaseHasData);

                var theme2Publication = Assert.Single(result[1].Publications);
                Assert.Equal(theme2.Publications[0].Title, theme2Publication.Title);
                Assert.Equal(publication3.Slug, theme2Publication.Slug);
                Assert.Equal(publication3.Title, theme2Publication.Title);
                Assert.False(theme2Publication.AnyLiveReleaseHasData);
                Assert.False(theme2Publication.LatestReleaseHasData);
            }
        }

        [Fact]
        public async Task WhenThemeHasNoPublications_ThemeIsExcluded()
        {
            // Arrange
            var (theme1, theme2) = _dataFixture
                .DefaultTheme()
                // Index 0 has no publications,
                // Index 1 has one publication
                .ForIndex(
                    1,
                    s =>
                        s.SetPublications(_ =>
                            [
                                _dataFixture
                                    .DefaultPublication()
                                    .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]),
                            ]
                        )
                )
                .GenerateTuple2();

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Themes.AddRange(theme1, theme2);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var result = await sut.GetPublicationsTree();

                // Assert
                var resultTheme = Assert.Single(result);
                Assert.Equal(theme2.Title, resultTheme.Title);

                var resultPublication = Assert.Single(resultTheme.Publications);
                Assert.Equal(theme2.Publications[0].Title, resultPublication.Title);
            }
        }

        [Fact]
        public async Task WhenThemeHasNoVisiblePublications_ThemeIsExcluded()
        {
            // Arrange
            var themes = _dataFixture
                .DefaultTheme()
                // Index 0 has a publication with a published release,
                // Index 1 has a publication with a published and unpublished release
                // Index 2 has a publication with no releases
                // Index 3 has a publication with an unpublished release
                .ForIndex(
                    0,
                    s =>
                        s.SetPublications(_ =>
                            [
                                _dataFixture
                                    .DefaultPublication()
                                    .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]),
                            ]
                        )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetPublications(_ =>
                            [
                                _dataFixture
                                    .DefaultPublication()
                                    .WithReleases(_ =>
                                        [
                                            _dataFixture.DefaultRelease(publishedVersions: 1),
                                            _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true),
                                        ]
                                    ),
                            ]
                        )
                )
                .ForIndex(2, s => s.SetPublications(_ => [_dataFixture.DefaultPublication()]))
                .ForIndex(
                    3,
                    s =>
                        s.SetPublications(_ =>
                            [
                                _dataFixture
                                    .DefaultPublication()
                                    .WithReleases(_ =>
                                        [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]
                                    ),
                            ]
                        )
                )
                .GenerateList(4);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Themes.AddRange(themes);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var result = await sut.GetPublicationsTree();

                // Assert
                // Expect only the first two themes to be included
                Assert.Equal(2, result.Length);
                Assert.Equal(themes[0].Title, result[0].Title);
                Assert.Equal(themes[1].Title, result[1].Title);

                Assert.Single(result[0].Publications);
                Assert.Equal(themes[0].Publications[0].Title, result[0].Publications[0].Title);

                Assert.Single(result[1].Publications);
                Assert.Equal(themes[1].Publications[0].Title, result[1].Publications[0].Title);
            }
        }

        [Fact]
        public async Task WhenLatestReleaseHasData_LatestReleaseHasDataAndAnyLiveReleaseHasDataAreTrue()
        {
            // Arrange
            Theme theme = _dataFixture
                .DefaultTheme()
                .WithPublications(_ =>
                    [
                        _dataFixture
                            .DefaultPublication()
                            .WithReleases(_ =>
                                [
                                    _dataFixture.DefaultRelease(publishedVersions: 1, year: 2024),
                                    _dataFixture.DefaultRelease(publishedVersions: 1, year: 2025),
                                ]
                            ),
                    ]
                );

            var publication = theme.Publications.Single();
            var nonLatestRelease = publication.Releases.Single(r => r.Year == 2024);
            var latestRelease = publication.Releases.Single(r => r.Year == 2025);

            var releaseFiles = _dataFixture
                .DefaultReleaseFile()
                // Non-latest release has no data
                .ForIndex(
                    0,
                    s =>
                        s.SetFile(() => _dataFixture.DefaultFile(FileType.Ancillary))
                            .SetReleaseVersion(nonLatestRelease.Versions.Single())
                )
                // Latest release has data
                .ForIndex(
                    1,
                    s =>
                        s.SetFile(() => _dataFixture.DefaultFile(FileType.Data))
                            .SetReleaseVersion(latestRelease.Versions.Single())
                )
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Themes.Add(theme);
                context.ReleaseFiles.AddRange(releaseFiles);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var result = await sut.GetPublicationsTree();

                // Assert
                var resultTheme = Assert.Single(result);
                Assert.Equal(theme.Title, resultTheme.Title);

                var resultPublication = Assert.Single(resultTheme.Publications);
                Assert.Equal(publication.Title, resultPublication.Title);
                Assert.Equal(publication.Id, resultPublication.Id);
                Assert.True(resultPublication.AnyLiveReleaseHasData);
                Assert.True(resultPublication.LatestReleaseHasData);
            }
        }

        [Fact]
        public async Task WhenOnlyAPreviousReleaseHasData_AnyLiveReleaseHasDataIsTrueButLatestReleaseHasDataIsFalse()
        {
            // Arrange
            Theme theme = _dataFixture
                .DefaultTheme()
                .WithPublications(_ =>
                    [
                        _dataFixture
                            .DefaultPublication()
                            .WithReleases(_ =>
                                [
                                    _dataFixture.DefaultRelease(publishedVersions: 1, year: 2024),
                                    _dataFixture.DefaultRelease(publishedVersions: 1, year: 2025),
                                ]
                            ),
                    ]
                );

            var publication = theme.Publications.Single();
            var nonLatestRelease = publication.Releases.Single(r => r.Year == 2024);
            var latestRelease = publication.Releases.Single(r => r.Year == 2025);

            var releaseFiles = _dataFixture
                .DefaultReleaseFile()
                // Non-latest release has data
                .ForIndex(
                    0,
                    s =>
                        s.SetFile(() => _dataFixture.DefaultFile(FileType.Data))
                            .SetReleaseVersion(nonLatestRelease.Versions.Single())
                )
                // Latest release has no data
                .ForIndex(
                    1,
                    s =>
                        s.SetFile(() => _dataFixture.DefaultFile(FileType.Ancillary))
                            .SetReleaseVersion(latestRelease.Versions.Single())
                )
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Themes.Add(theme);
                context.ReleaseFiles.AddRange(releaseFiles);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var result = await sut.GetPublicationsTree();

                // Assert
                var resultTheme = Assert.Single(result);
                Assert.Equal(theme.Title, resultTheme.Title);

                var resultPublication = Assert.Single(resultTheme.Publications);
                Assert.Equal(publication.Title, resultPublication.Title);
                Assert.Equal(publication.Id, resultPublication.Id);
                Assert.True(resultPublication.AnyLiveReleaseHasData);
                Assert.False(resultPublication.LatestReleaseHasData);
            }
        }

        [Fact]
        public async Task WhenOnlyAnUnpublishedReleaseHasData_AnyLiveReleaseHasDataIsFalse()
        {
            // Arrange
            Theme theme = _dataFixture
                .DefaultTheme()
                .WithPublications(_ =>
                    [
                        _dataFixture
                            .DefaultPublication()
                            .WithReleases(_ =>
                                [
                                    _dataFixture.DefaultRelease(publishedVersions: 1, year: 2024),
                                    _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2025),
                                ]
                            ),
                    ]
                );

            var publication = theme.Publications.Single();
            var publishedRelease = publication.Releases.Single(r => r.Year == 2024);
            var unpublishedRelease = publication.Releases.Single(r => r.Year == 2025);

            var releaseFiles = _dataFixture
                .DefaultReleaseFile()
                // Published release has no data
                .ForIndex(
                    0,
                    s =>
                        s.SetFile(() => _dataFixture.DefaultFile(FileType.Ancillary))
                            .SetReleaseVersion(publishedRelease.Versions.Single())
                )
                // Unpublished release has data
                .ForIndex(
                    1,
                    s =>
                        s.SetFile(() => _dataFixture.DefaultFile(FileType.Data))
                            .SetReleaseVersion(unpublishedRelease.Versions.Single())
                )
                .GenerateList(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Themes.Add(theme);
                context.ReleaseFiles.AddRange(releaseFiles);

                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var result = await sut.GetPublicationsTree();

                // Assert
                var resultTheme = Assert.Single(result);
                Assert.Equal(theme.Id, resultTheme.Id);

                var resultPublication = Assert.Single(resultTheme.Publications);
                Assert.Equal(publication.Id, resultPublication.Id);
                Assert.False(resultPublication.AnyLiveReleaseHasData);
                Assert.False(resultPublication.LatestReleaseHasData);
            }
        }

        [Fact]
        public async Task WhenPublicationIsSuperseded_IsSupersededIsTrueAndSupersededByIsSet()
        {
            // Arrange
            Publication supersedingPublication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var (publication1, publication2) = _dataFixture
                .DefaultPublication()
                // Both publications have published releases
                // Index 1 is superseded
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .ForIndex(1, s => s.SetSupersededBy(supersedingPublication))
                .GenerateTuple2();

            Theme theme = _dataFixture
                .DefaultTheme()
                // Publications are in random order
                // to check that ordering is done by title
                .WithPublications([publication2, publication1]);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Themes.Add(theme);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var result = await sut.GetPublicationsTree();

                // Assert
                var resultTheme = Assert.Single(result);
                Assert.Equal(theme.Title, resultTheme.Title);

                var resultPublications = resultTheme.Publications;
                Assert.Equal(2, resultPublications.Length);

                Assert.Equal(publication1.Title, resultPublications[0].Title);
                Assert.False(resultPublications[0].IsSuperseded);

                Assert.Equal(publication2.Title, resultPublications[1].Title);
                Assert.True(resultPublications[1].IsSuperseded);

                Assert.NotNull(resultPublications[1].SupersededBy);
                Assert.Equal(supersedingPublication.Id, resultPublications[1].SupersededBy!.Id);
                Assert.Equal(supersedingPublication.Title, resultPublications[1].SupersededBy!.Title);
                Assert.Equal(supersedingPublication.Slug, resultPublications[1].SupersededBy!.Slug);
            }
        }
    }

    public class GetPublicationsTreeCachedTests : PublicationsTreeServiceTests
    {
        [Fact]
        public async Task WhenCacheMiss_ReturnsBuiltAndCachedTree()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithFile(() => _dataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.Add(releaseFile);

                await context.SaveChangesAsync();
            }

            PublicationsTreeThemeDto[] expectedTree =
            [
                new()
                {
                    Id = publication.ThemeId,
                    Summary = publication.Theme.Summary,
                    Title = publication.Theme.Title,
                    Publications =
                    [
                        new PublicationsTreePublicationDto
                        {
                            Id = publication.Id,
                            Slug = publication.Slug,
                            Title = publication.Title,
                            SupersededBy = null,
                            AnyLiveReleaseHasData = true,
                            LatestReleaseHasData = true,
                        },
                    ],
                },
            ];

            var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);

            publicBlobCacheService
                .Setup(s => s.GetItemAsync(new PublicationsTreeCacheKey(), typeof(PublicationsTreeThemeDto[])))
                .ReturnsAsync((object?)null);

            publicBlobCacheService
                .Setup(s => s.SetItemAsync<object>(new PublicationsTreeCacheKey(), expectedTree))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context, publicBlobCacheService: publicBlobCacheService.Object);

                // Act
                var result = await sut.GetPublicationsTreeCached();

                // Assert
                VerifyAllMocks(publicBlobCacheService);

                Assert.Equal(expectedTree, result);
            }
        }

        [Fact]
        public async Task WhenCacheHit_ReturnsCachedTree()
        {
            // Arrange
            PublicationsTreeThemeDto[] expectedTree =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Summary = "",
                    Title = "",
                    Publications = [],
                },
            ];

            var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);

            publicBlobCacheService
                .Setup(s => s.GetItemAsync(new PublicationsTreeCacheKey(), typeof(PublicationsTreeThemeDto[])))
                .ReturnsAsync(expectedTree);

            var sut = BuildService(publicBlobCacheService: publicBlobCacheService.Object);

            // Act
            var result = await sut.GetPublicationsTreeCached();

            // Assert
            VerifyAllMocks(publicBlobCacheService);

            Assert.Equal(expectedTree, result);
        }
    }

    public class GetPublicationsTreeFilteredTests : PublicationsTreeServiceTests
    {
        [Theory]
        [InlineData(PublicationsTreeFilter.DataCatalogue)]
        [InlineData(PublicationsTreeFilter.FastTrack)]
        public async Task WhenFilterIsDataCatalogueOrFastTrack_IncludesPublicationWhenAnyLiveReleaseHasData(
            PublicationsTreeFilter filter
        )
        {
            // Arrange
            PublicationsTreeThemeDto[] tree =
            [
                new()
                {
                    Id = Guid.Empty,
                    Summary = "",
                    Title = "Theme A",
                    Publications =
                    [
                        new PublicationsTreePublicationDto
                        {
                            Id = Guid.Empty,
                            Slug = "",
                            Title = "Publication A",
                            SupersededBy = null,
                            AnyLiveReleaseHasData = true,
                            LatestReleaseHasData = false,
                        },
                    ],
                },
            ];

            var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);
            publicBlobCacheService
                .Setup(s => s.GetItemAsync(new PublicationsTreeCacheKey(), typeof(PublicationsTreeThemeDto[])))
                .ReturnsAsync(tree);

            var sut = BuildService(publicBlobCacheService: publicBlobCacheService.Object);

            // Act
            var result = await sut.GetPublicationsTreeFiltered(filter);

            // Assert
            VerifyAllMocks(publicBlobCacheService);

            var resultTheme = Assert.Single(result);
            Assert.Equal("Theme A", resultTheme.Title);

            var resultPublication = Assert.Single(resultTheme.Publications);
            Assert.Equal("Publication A", resultPublication.Title);
        }

        [Theory]
        [InlineData(PublicationsTreeFilter.DataCatalogue)]
        [InlineData(PublicationsTreeFilter.FastTrack)]
        public async Task WhenFilterIsDataCatalogueOrFastTrack_ExcludesPublicationWhenReleaseHasNoData(
            PublicationsTreeFilter filter
        )
        {
            // Arrange
            PublicationsTreeThemeDto[] tree =
            [
                new()
                {
                    Id = Guid.Empty,
                    Summary = "",
                    Title = "Theme A",
                    Publications =
                    [
                        new PublicationsTreePublicationDto
                        {
                            Id = Guid.Empty,
                            Slug = "",
                            Title = "Publication A",
                            SupersededBy = null,
                            AnyLiveReleaseHasData = false,
                            LatestReleaseHasData = false,
                        },
                    ],
                },
            ];

            var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);
            publicBlobCacheService
                .Setup(s => s.GetItemAsync(new PublicationsTreeCacheKey(), typeof(PublicationsTreeThemeDto[])))
                .ReturnsAsync(tree);

            var sut = BuildService(publicBlobCacheService: publicBlobCacheService.Object);

            // Act
            var result = await sut.GetPublicationsTreeFiltered(filter);

            // Assert
            VerifyAllMocks(publicBlobCacheService);

            Assert.Empty(result);
        }

        [Fact]
        public async Task WhenFilterIsDataTables_IncludesPublicationWhenLatestReleaseHasData()
        {
            // Arrange
            PublicationsTreeThemeDto[] tree =
            [
                new()
                {
                    Id = Guid.Empty,
                    Summary = "",
                    Title = "Theme A",
                    Publications =
                    [
                        new PublicationsTreePublicationDto
                        {
                            Id = Guid.Empty,
                            Slug = "",
                            Title = "Publication A",
                            SupersededBy = null,
                            AnyLiveReleaseHasData = false,
                            LatestReleaseHasData = true,
                        },
                    ],
                },
            ];

            var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);
            publicBlobCacheService
                .Setup(s => s.GetItemAsync(new PublicationsTreeCacheKey(), typeof(PublicationsTreeThemeDto[])))
                .ReturnsAsync(tree);

            var sut = BuildService(publicBlobCacheService: publicBlobCacheService.Object);

            // Act
            var result = await sut.GetPublicationsTreeFiltered(PublicationsTreeFilter.DataTables);

            // Assert
            VerifyAllMocks(publicBlobCacheService);

            var resultTheme = Assert.Single(result);
            Assert.Equal("Theme A", resultTheme.Title);

            var resultPublication = Assert.Single(resultTheme.Publications);
            Assert.Equal("Publication A", resultPublication.Title);
        }

        [Fact]
        public async Task WhenFilterIsDataTables_ExcludesPublicationWhenLatestReleaseHasNoData()
        {
            // Arrange
            PublicationsTreeThemeDto[] tree =
            [
                new()
                {
                    Id = Guid.Empty,
                    Summary = "",
                    Title = "Theme A",
                    Publications =
                    [
                        new PublicationsTreePublicationDto
                        {
                            Id = Guid.Empty,
                            Slug = "",
                            Title = "Publication A",
                            SupersededBy = null,
                            AnyLiveReleaseHasData = true,
                            LatestReleaseHasData = false,
                        },
                    ],
                },
            ];

            var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);
            publicBlobCacheService
                .Setup(s => s.GetItemAsync(new PublicationsTreeCacheKey(), typeof(PublicationsTreeThemeDto[])))
                .ReturnsAsync(tree);

            var sut = BuildService(publicBlobCacheService: publicBlobCacheService.Object);

            // Act
            var result = await sut.GetPublicationsTreeFiltered(PublicationsTreeFilter.DataTables);

            // Assert
            VerifyAllMocks(publicBlobCacheService);

            Assert.Empty(result);
        }

        [Theory]
        [InlineData(PublicationsTreeFilter.DataCatalogue)]
        [InlineData(PublicationsTreeFilter.FastTrack)]
        [InlineData(PublicationsTreeFilter.DataTables)]
        public async Task WhenThemesExistWithNoPublicationsRemainingAfterFiltering_ThemesAreRemoved(
            PublicationsTreeFilter filter
        )
        {
            // Arrange
            PublicationsTreeThemeDto[] tree =
            [
                // Theme A: Its only publication has no data and matches no filters
                new()
                {
                    Id = Guid.NewGuid(),
                    Summary = "",
                    Title = "Theme A",
                    Publications =
                    [
                        new PublicationsTreePublicationDto
                        {
                            Id = Guid.NewGuid(),
                            Slug = "",
                            Title = "Publication with no data",
                            SupersededBy = null,
                            AnyLiveReleaseHasData = false,
                            LatestReleaseHasData = false,
                        },
                    ],
                },
                // Theme B: Has a publication with data that matches all filters
                new()
                {
                    Id = Guid.NewGuid(),
                    Summary = "",
                    Title = "Theme B",
                    Publications =
                    [
                        new PublicationsTreePublicationDto
                        {
                            Id = Guid.NewGuid(),
                            Slug = "",
                            Title = "Publication with data",
                            SupersededBy = null,
                            AnyLiveReleaseHasData = true,
                            LatestReleaseHasData = true,
                        },
                    ],
                },
            ];

            var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);
            publicBlobCacheService
                .Setup(s => s.GetItemAsync(new PublicationsTreeCacheKey(), typeof(PublicationsTreeThemeDto[])))
                .ReturnsAsync(tree);

            var sut = BuildService(publicBlobCacheService: publicBlobCacheService.Object);

            // Act
            var result = await sut.GetPublicationsTreeFiltered(filter);

            // Assert
            VerifyAllMocks(publicBlobCacheService);

            var resultTheme = Assert.Single(result);
            Assert.Equal("Theme B", resultTheme.Title);

            var resultPublication = Assert.Single(resultTheme.Publications);
            Assert.Equal("Publication with data", resultPublication.Title);
        }
    }

    public class UpdateCachedPublicationsTreeTests : PublicationsTreeServiceTests
    {
        [Fact]
        public async Task WhenPublicationExists_CachesTree()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithFile(() => _dataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.Add(releaseFile);

                await context.SaveChangesAsync();
            }

            PublicationsTreeThemeDto[] expectedTree =
            [
                new()
                {
                    Id = publication.ThemeId,
                    Summary = publication.Theme.Summary,
                    Title = publication.Theme.Title,
                    Publications =
                    [
                        new PublicationsTreePublicationDto
                        {
                            Id = publication.Id,
                            Slug = publication.Slug,
                            Title = publication.Title,
                            SupersededBy = null,
                            AnyLiveReleaseHasData = true,
                            LatestReleaseHasData = true,
                        },
                    ],
                },
            ];

            var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);

            publicBlobCacheService
                .Setup(s => s.SetItemAsync<object>(new PublicationsTreeCacheKey(), expectedTree))
                .Returns(Task.CompletedTask);

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context, publicBlobCacheService: publicBlobCacheService.Object);

                // Act
                var result = await sut.UpdateCachedPublicationsTree();

                // Assert
                VerifyAllMocks(publicBlobCacheService);

                Assert.Equal(expectedTree, result);
            }
        }

        [Fact]
        public async Task WhenNoPublicationsExist_CachesEmptyTree()
        {
            // Arrange
            PublicationsTreeThemeDto[] expectedTree = [];

            var publicBlobCacheService = new Mock<IPublicBlobCacheService>(Strict);

            publicBlobCacheService
                .Setup(s => s.SetItemAsync<object>(new PublicationsTreeCacheKey(), expectedTree))
                .Returns(Task.CompletedTask);

            await using var context = InMemoryContentDbContext();
            var sut = BuildService(context, publicBlobCacheService: publicBlobCacheService.Object);

            // Act
            var result = await sut.UpdateCachedPublicationsTree();

            // Assert
            VerifyAllMocks(publicBlobCacheService);

            Assert.Equal(expectedTree, result);
        }
    }

    private static PublicationsTreeService BuildService(
        ContentDbContext? contentDbContext = null,
        IPublicationRepository? publicationRepository = null,
        IPublicBlobCacheService? publicBlobCacheService = null,
        IReleaseVersionRepository? releaseVersionRepository = null
    )
    {
        contentDbContext ??= InMemoryContentDbContext();
        return new PublicationsTreeService(
            contentDbContext,
            publicationRepository ?? new PublicationRepository(contentDbContext),
            publicBlobCacheService ?? Mock.Of<IPublicBlobCacheService>(Strict),
            releaseVersionRepository ?? new ReleaseVersionRepository(contentDbContext),
            NullLogger<PublicationsTreeService>.Instance
        );
    }
}
