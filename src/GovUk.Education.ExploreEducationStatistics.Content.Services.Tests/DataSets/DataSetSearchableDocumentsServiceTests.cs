using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.DataSets;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.DataSets;

public abstract class DataSetSearchableDocumentsServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetReleaseDataSetSearchableDocumentsTests : DataSetSearchableDocumentsServiceTests
    {
        [Fact]
        public async Task WhenReleaseHasDataSet_ReturnsExpectedSearchableDocument()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];
            releaseVersion.Type = ReleaseType.AccreditedOfficialStatistics;
            releaseVersion.PublishingOrganisations =
            [
                _dataFixture.DefaultOrganisation().WithTitle("Organisation B"),
                _dataFixture.DefaultOrganisation().WithTitle(Organisation.DepartmentForEducationTitle),
                _dataFixture.DefaultOrganisation().WithTitle("Organisation A"),
            ];

            var filter1Id = Guid.NewGuid();
            var filter2Id = Guid.NewGuid();
            var indicator1Id = Guid.NewGuid();
            var indicator2Id = Guid.NewGuid();

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithName("Data set title")
                .WithSummary("Data set summary")
                .WithPublicApiDataSetId(Guid.NewGuid())
                .WithPublicApiDataSetVersion(major: 2, minor: 1, patch: 3)
                .WithFilterSequence([new FilterSequenceEntry(filter2Id, []), new FilterSequenceEntry(filter1Id, [])])
                .WithIndicatorSequence([new IndicatorGroupSequenceEntry(Guid.NewGuid(), [indicator2Id, indicator1Id])])
                .WithFile(
                    _dataFixture
                        .DefaultFile(FileType.Data)
                        .WithFilename("data.csv")
                        .WithContentLength(10240)
                        .WithDataSetFileMeta(
                            _dataFixture
                                .DefaultDataSetFileMeta()
                                .WithNumDataFileRows(1234)
                                .WithTimePeriodRange(
                                    new TimePeriodRangeMeta
                                    {
                                        Start = new TimePeriodRangeBoundMeta
                                        {
                                            TimeIdentifier = TimeIdentifier.AcademicYear,
                                            Period = "2020",
                                        },
                                        End = new TimePeriodRangeBoundMeta
                                        {
                                            TimeIdentifier = TimeIdentifier.AcademicYear,
                                            Period = "2024",
                                        },
                                    }
                                )
                                .WithFilters([
                                    new FilterMeta
                                    {
                                        Id = filter1Id,
                                        Label = "Filter 1",
                                        ColumnName = "filter_1",
                                    },
                                    new FilterMeta
                                    {
                                        Id = filter2Id,
                                        Label = "Filter 2",
                                        ColumnName = "filter_2",
                                    },
                                ])
                                .WithIndicators([
                                    new IndicatorMeta
                                    {
                                        Id = indicator1Id,
                                        Label = "Indicator 1",
                                        ColumnName = "indicator_1",
                                    },
                                    new IndicatorMeta
                                    {
                                        Id = indicator2Id,
                                        Label = "Indicator 2",
                                        ColumnName = "indicator_2",
                                    },
                                ])
                        )
                        .WithDataSetFileVersionGeographicLevels([GeographicLevel.Region, GeographicLevel.Country])
                        .WithCsvOnlyGeographicLevels([GeographicLevel.LocalAuthority])
                );

            var contextId = await SeedDatabase([publication], [releaseFile]);

            await using var context = InMemoryContentDbContext(contextId);
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetReleaseDataSetSearchableDocuments(publication.Slug, release.Slug);

            // Assert
            var actual = Assert.Single(outcome.AssertRight());
            var file = releaseFile.File;

            Assert.Multiple(
                () => Assert.Equal(releaseFile.FileId, actual.FileId),
                () => Assert.Equal(file.DataSetFileId, actual.DataSetFileId),
                () => Assert.Equal(file.SubjectId, actual.SubjectId),
                () => Assert.Equal("data.csv", actual.Filename),
                () => Assert.Equal("csv", actual.FileExtension),
                () => Assert.Equal("10 Kb", actual.FileSize),
                () => Assert.Equal("Data set title", actual.Title),
                () => Assert.Equal("Data set summary", actual.Content),
                () => Assert.Equal(publication.ThemeId, actual.ThemeId),
                () => Assert.Equal(publication.Theme.Title, actual.ThemeTitle),
                () => Assert.Equal(publication.Id, actual.PublicationId),
                () => Assert.Equal(publication.Title, actual.PublicationTitle),
                () => Assert.Equal(publication.Slug, actual.PublicationSlug),
                () => Assert.Equal(release.Id, actual.ReleaseId),
                () => Assert.Equal(releaseVersion.Id, actual.ReleaseVersionId),
                () => Assert.Equal(release.Title, actual.ReleaseTitle),
                () => Assert.Equal(release.Slug, actual.ReleaseSlug),
                () => Assert.Equal(ReleaseType.AccreditedOfficialStatistics.ToString(), actual.ReleaseType),
                () => Assert.True(actual.LatestData),
                () => Assert.False(actual.IsSuperseded),
                () => Assert.Equal(releaseVersion.PublishedDisplayDate, actual.Published),
                () => Assert.Equal(releaseFile.Published!.Value, actual.LastUpdated.UtcDateTime),
                () => Assert.Equal(TimeSpan.Zero, actual.LastUpdated.Offset),
                () => Assert.Equal(releaseFile.PublicApiDataSetId, actual.Api?.Id),
                () => Assert.Equal("2.1.3", actual.Api?.Version),
                () => Assert.Equal(1234, actual.NumDataFileRows),
                () => Assert.Equal("2020/21", actual.TimePeriodRange.From),
                () => Assert.Equal("2024/25", actual.TimePeriodRange.To),
                () => Assert.Equal(["Filter 2", "Filter 1"], actual.Filters),
                () => Assert.Equal(["Indicator 2", "Indicator 1"], actual.Indicators),
                () =>
                    Assert.Equal(
                        [("Local authority", "LA", true), ("National", "NAT", false), ("Regional", "REG", false)],
                        actual.GeographicLevels.Select(level => (level.Label, level.Code, level.CsvOnly))
                    ),
                () =>
                    Assert.Equal(
                        [Organisation.DepartmentForEducationTitle, "Organisation A", "Organisation B"],
                        actual.PublishingOrganisations.Select(organisation => organisation.Title)
                    )
            );
        }

        [Fact]
        public async Task WhenDataSetIsNotAnApiDataSet_ReturnsNullApi()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];

            var releaseFiles = GenerateDataSetReleaseFiles(release.Versions[0], numberOfDataSets: 1);

            var contextId = await SeedDatabase([publication], releaseFiles);

            await using var context = InMemoryContentDbContext(contextId);
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetReleaseDataSetSearchableDocuments(publication.Slug, release.Slug);

            // Assert
            var actual = Assert.Single(outcome.AssertRight());
            Assert.Null(actual.Api);
        }

        [Fact]
        public async Task WhenReleaseHasDraftAmendment_ReturnsDataSetsOfLatestPublishedVersion()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 2, draftVersion: true)]);
            var release = publication.Releases[0];

            var previousVersionReleaseFiles = GenerateDataSetReleaseFiles(release.Versions[0]);
            var latestPublishedVersionReleaseFiles = GenerateDataSetReleaseFiles(release.Versions[1]);
            var draftVersionReleaseFiles = GenerateDataSetReleaseFiles(release.Versions[2]);

            var contextId = await SeedDatabase(
                [publication],
                [.. previousVersionReleaseFiles, .. latestPublishedVersionReleaseFiles, .. draftVersionReleaseFiles]
            );

            await using var context = InMemoryContentDbContext(contextId);
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetReleaseDataSetSearchableDocuments(publication.Slug, release.Slug);

            // Assert
            var actual = outcome.AssertRight();
            Assert.Equal(
                latestPublishedVersionReleaseFiles.Select(rf => rf.FileId).Order(),
                actual.Select(document => document.FileId).Order()
            );
            Assert.All(actual, document => Assert.Equal(release.Versions[1].Id, document.ReleaseVersionId));
        }

        [Fact]
        public async Task WhenReleaseHasNonDataFilesAndReplacementInProgress_ReturnsOnlyDataSets()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            ReleaseFile dataSetReleaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            ReleaseFile replacementReleaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_dataFixture.DefaultFile(FileType.Data).WithReplacing(dataSetReleaseFile.File));

            ReleaseFile ancillaryReleaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_dataFixture.DefaultFile(FileType.Ancillary));

            ReleaseFile metaReleaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_dataFixture.DefaultFile(FileType.Metadata));

            var contextId = await SeedDatabase(
                [publication],
                [dataSetReleaseFile, replacementReleaseFile, ancillaryReleaseFile, metaReleaseFile]
            );

            await using var context = InMemoryContentDbContext(contextId);
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetReleaseDataSetSearchableDocuments(publication.Slug, release.Slug);

            // Assert
            var actual = Assert.Single(outcome.AssertRight());
            Assert.Equal(dataSetReleaseFile.FileId, actual.FileId);
        }

        [Fact]
        public async Task WhenReleaseIsNotTheLatestRelease_ReturnsLatestDataFalse()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture.DefaultRelease(publishedVersions: 1, year: 2020),
                        _dataFixture.DefaultRelease(publishedVersions: 1, year: 2021),
                    ]
                );
            var olderRelease = publication.Releases.Single(r => r.Year == 2020);
            var latestRelease = publication.Releases.Single(r => r.Year == 2021);
            publication.LatestPublishedReleaseVersion = latestRelease.Versions[0];

            var contextId = await SeedDatabase(
                [publication],
                [
                    .. GenerateDataSetReleaseFiles(olderRelease.Versions[0]),
                    .. GenerateDataSetReleaseFiles(latestRelease.Versions[0]),
                ]
            );

            await using var context = InMemoryContentDbContext(contextId);
            var sut = BuildService(context);

            // Act
            var olderReleaseOutcome = await sut.GetReleaseDataSetSearchableDocuments(
                publication.Slug,
                olderRelease.Slug
            );
            var latestReleaseOutcome = await sut.GetReleaseDataSetSearchableDocuments(
                publication.Slug,
                latestRelease.Slug
            );

            // Assert
            var olderReleaseDocuments = olderReleaseOutcome.AssertRight();
            var latestReleaseDocuments = latestReleaseOutcome.AssertRight();
            Assert.Equal(2, olderReleaseDocuments.Length);
            Assert.Equal(2, latestReleaseDocuments.Length);
            Assert.All(olderReleaseDocuments, document => Assert.False(document.LatestData));
            Assert.All(latestReleaseDocuments, document => Assert.True(document.LatestData));
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task WhenPublicationIsSupersededBy_ReturnsIsSupersededOnlyWhenSupersedingPublicationIsPublished(
            bool supersedingPublicationIsPublished,
            bool expectedIsSuperseded
        )
        {
            // Arrange
            Publication supersedingPublication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    supersedingPublicationIsPublished
                        ? [_dataFixture.DefaultRelease(publishedVersions: 1)]
                        : [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]
                );

            Publication supersededPublication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithSupersededBy(supersedingPublication)
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = supersededPublication.Releases[0];

            var contextId = await SeedDatabase(
                [supersedingPublication, supersededPublication],
                GenerateDataSetReleaseFiles(release.Versions[0])
            );

            await using var context = InMemoryContentDbContext(contextId);
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetReleaseDataSetSearchableDocuments(supersededPublication.Slug, release.Slug);

            // Assert
            var actual = outcome.AssertRight();
            Assert.Equal(2, actual.Length);
            Assert.All(actual, document => Assert.Equal(expectedIsSuperseded, document.IsSuperseded));
        }

        [Fact]
        public async Task WhenDataSetHasNoGeographicLevels_ReturnsDataSet()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(release.Versions[0])
                .WithFile(_dataFixture.DefaultFile(FileType.Data).WithDataSetFileVersionGeographicLevels([]));

            var contextId = await SeedDatabase([publication], [releaseFile]);

            await using var context = InMemoryContentDbContext(contextId);
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetReleaseDataSetSearchableDocuments(publication.Slug, release.Slug);

            // Assert
            var actual = Assert.Single(outcome.AssertRight());
            Assert.Empty(actual.GeographicLevels);
        }

        [Fact]
        public async Task WhenReleaseHasNoDataSets_ReturnsEmpty()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];

            var contextId = await SeedDatabase([publication], []);

            await using var context = InMemoryContentDbContext(contextId);
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetReleaseDataSetSearchableDocuments(publication.Slug, release.Slug);

            // Assert
            Assert.Empty(outcome.AssertRight());
        }

        [Fact]
        public async Task WhenPublicationDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var contextId = await SeedDatabase([], []);

            await using var context = InMemoryContentDbContext(contextId);
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetReleaseDataSetSearchableDocuments("unknown-publication", "unknown-release");

            // Assert
            outcome.AssertNotFound();
        }

        [Fact]
        public async Task WhenReleaseDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var contextId = await SeedDatabase(
                [publication],
                GenerateDataSetReleaseFiles(publication.Releases[0].Versions[0])
            );

            await using var context = InMemoryContentDbContext(contextId);
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetReleaseDataSetSearchableDocuments(publication.Slug, "unknown-release");

            // Assert
            outcome.AssertNotFound();
        }

        [Fact]
        public async Task WhenReleaseHasNoPublishedVersion_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture.DefaultRelease(publishedVersions: 1, year: 2020),
                        _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2021),
                    ]
                );
            var unpublishedRelease = publication.Releases.Single(r => r.Year == 2021);

            var contextId = await SeedDatabase(
                [publication],
                GenerateDataSetReleaseFiles(unpublishedRelease.Versions[0])
            );

            await using var context = InMemoryContentDbContext(contextId);
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetReleaseDataSetSearchableDocuments(publication.Slug, unpublishedRelease.Slug);

            // Assert
            outcome.AssertNotFound();
        }
    }

    public class GetPublicationDataSetSearchableDocumentsTests : DataSetSearchableDocumentsServiceTests
    {
        [Fact]
        public async Task WhenPublicationHasReleases_ReturnsDataSetsOfLatestPublishedVersionOfEachRelease()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture.DefaultRelease(publishedVersions: 2, year: 2020),
                        _dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: true, year: 2021),
                        _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2022),
                    ]
                );
            var release2020 = publication.Releases.Single(r => r.Year == 2020);
            var release2021 = publication.Releases.Single(r => r.Year == 2021);
            var release2022 = publication.Releases.Single(r => r.Year == 2022);
            publication.LatestPublishedReleaseVersion = release2021.Versions[0];

            Publication otherPublication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release2020Version0ReleaseFiles = GenerateDataSetReleaseFiles(release2020.Versions[0]);
            var release2020Version1ReleaseFiles = GenerateDataSetReleaseFiles(release2020.Versions[1]);
            var release2021Version0ReleaseFiles = GenerateDataSetReleaseFiles(release2021.Versions[0]);
            var release2021DraftReleaseFiles = GenerateDataSetReleaseFiles(release2021.Versions[1]);
            var release2022DraftReleaseFiles = GenerateDataSetReleaseFiles(release2022.Versions[0]);
            var otherPublicationReleaseFiles = GenerateDataSetReleaseFiles(otherPublication.Releases[0].Versions[0]);

            var contextId = await SeedDatabase(
                [publication, otherPublication],
                [
                    .. release2020Version0ReleaseFiles,
                    .. release2020Version1ReleaseFiles,
                    .. release2021Version0ReleaseFiles,
                    .. release2021DraftReleaseFiles,
                    .. release2022DraftReleaseFiles,
                    .. otherPublicationReleaseFiles,
                ]
            );

            await using var context = InMemoryContentDbContext(contextId);
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetPublicationDataSetSearchableDocuments(publication.Slug);

            // Assert
            var actual = outcome.AssertRight();
            Assert.Equal(
                release2020Version1ReleaseFiles.Concat(release2021Version0ReleaseFiles).Select(rf => rf.FileId).Order(),
                actual.Select(document => document.FileId).Order()
            );
            Assert.All(
                actual.Where(document => document.ReleaseId == release2020.Id),
                document => Assert.False(document.LatestData)
            );
            Assert.All(
                actual.Where(document => document.ReleaseId == release2021.Id),
                document => Assert.True(document.LatestData)
            );
        }

        [Fact]
        public async Task WhenPublicationHasNoDataSets_ReturnsEmpty()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var contextId = await SeedDatabase([publication], []);

            await using var context = InMemoryContentDbContext(contextId);
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetPublicationDataSetSearchableDocuments(publication.Slug);

            // Assert
            Assert.Empty(outcome.AssertRight());
        }

        [Fact]
        public async Task WhenPublicationDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var contextId = await SeedDatabase([], []);

            await using var context = InMemoryContentDbContext(contextId);
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetPublicationDataSetSearchableDocuments("unknown-publication");

            // Assert
            outcome.AssertNotFound();
        }

        [Fact]
        public async Task WhenPublicationHasNoPublishedRelease_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);

            var contextId = await SeedDatabase(
                [publication],
                GenerateDataSetReleaseFiles(publication.Releases[0].Versions[0])
            );

            await using var context = InMemoryContentDbContext(contextId);
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetPublicationDataSetSearchableDocuments(publication.Slug);

            // Assert
            outcome.AssertNotFound();
        }
    }

    private List<ReleaseFile> GenerateDataSetReleaseFiles(ReleaseVersion releaseVersion, int numberOfDataSets = 2) =>
        _dataFixture
            .DefaultReleaseFile()
            .WithReleaseVersion(releaseVersion)
            .WithFiles(_dataFixture.DefaultFile(FileType.Data).GenerateList(numberOfDataSets))
            .GenerateList();

    private static async Task<string> SeedDatabase(
        IEnumerable<Publication> publications,
        IEnumerable<ReleaseFile> releaseFiles
    )
    {
        var contextId = Guid.NewGuid().ToString();
        await using var context = InMemoryContentDbContext(contextId);
        context.Publications.AddRange(publications);
        context.ReleaseFiles.AddRange(releaseFiles);
        await context.SaveChangesAsync();
        return contextId;
    }

    private static DataSetSearchableDocumentsService BuildService(ContentDbContext contentDbContext) =>
        new(contentDbContext);
}
