using GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Releases;

public abstract class ReleaseDataContentServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetReleaseDataContentTests : ReleaseDataContentServiceTests
    {
        [Fact]
        public async Task WhenPublicationAndReleaseExist_ReturnsExpectedDataContent()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            releaseVersion.FeaturedTables = _dataFixture.DefaultFeaturedTable().GenerateList(2);

            releaseVersion.RelatedDashboardsSection = _dataFixture
                .DefaultContentSection()
                .WithType(ContentSectionType.RelatedDashboards)
                .WithContentBlocks([_dataFixture.DefaultHtmlBlock().WithBody("<p>Data dashboards</p>")]);

            var dataSets = _dataFixture
                .DefaultReleaseFile()
                .ForIndex(
                    0,
                    s =>
                        s.SetFile(
                            _dataFixture
                                .DefaultFile(FileType.Data)
                                .WithDataSetFileMeta(_dataFixture.DefaultDataSetFileMeta().WithNumDataFileRows(1000))
                                .WithDataSetFileVersionGeographicLevels(
                                    [GeographicLevel.Country, GeographicLevel.LocalAuthority]
                                )
                        )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetFile(
                            _dataFixture
                                .DefaultFile(FileType.Data)
                                .WithDataSetFileMeta(_dataFixture.DefaultDataSetFileMeta().WithNumDataFileRows(2000))
                                .WithDataSetFileVersionGeographicLevels(
                                    [GeographicLevel.Region, GeographicLevel.School]
                                )
                        )
                )
                .WithReleaseVersion(releaseVersion)
                .GenerateArray(2);

            var supportingFiles = _dataFixture
                .DefaultReleaseFile()
                .WithFile(() => _dataFixture.DefaultFile(FileType.Ancillary))
                .WithReleaseVersion(releaseVersion)
                .GenerateArray(2);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.AddRange(dataSets);
                context.ReleaseFiles.AddRange(supportingFiles);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetReleaseDataContent(releaseVersion.Id);

                // Assert
                var result = outcome.AssertRight();

                Assert.Equal(release.Id, result.ReleaseId);
                Assert.Equal(releaseVersion.Id, result.ReleaseVersionId);
                Assert.Equal("<p>Data dashboards</p>", result.DataDashboards);
                Assert.Equal(releaseVersion.DataGuidance, result.DataGuidance);

                Assert.Equal(dataSets.Length, result.DataSets.Length);
                Assert.All(dataSets, (dataSet, index) => AssertDataSetEqual(dataSet, result.DataSets[index]));

                Assert.Equal(releaseVersion.FeaturedTables.Count, result.FeaturedTables.Length);
                Assert.All(
                    releaseVersion.FeaturedTables,
                    (featuredTable, index) => AssertFeaturedTableEqual(featuredTable, result.FeaturedTables[index])
                );

                Assert.Equal(supportingFiles.Length, result.SupportingFiles.Length);
                Assert.All(
                    supportingFiles,
                    (supportingFile, index) => AssertSupportingFileEqual(supportingFile, result.SupportingFiles[index])
                );
            }
        }

        [Fact]
        public async Task WhenDataSetHasNoFilterSequenceDefined_ReturnsFiltersInLabelOrder()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            // Define filters in non-alphabetical order
            FilterMeta[] filters =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Filter B",
                    ColumnName = "",
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Filter C",
                    ColumnName = "",
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Filter A",
                    ColumnName = "",
                },
            ];

            ReleaseFile dataSet = _dataFixture
                .DefaultReleaseFile()
                .WithFile(
                    _dataFixture
                        .DefaultFile(FileType.Data)
                        .WithDataSetFileMeta(_dataFixture.DefaultDataSetFileMeta().WithFilters([.. filters]))
                )
                .WithReleaseVersion(releaseVersion);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.Add(dataSet);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetReleaseDataContent(releaseVersion.Id);

                // Assert
                var result = outcome.AssertRight();

                Assert.Equal(["Filter A", "Filter B", "Filter C"], result.DataSets[0].Meta.Filters);
            }
        }

        [Fact]
        public async Task WhenDataSetHasFilterSequenceDefined_ReturnsFiltersInSequenceOrder()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            FilterMeta[] filters =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Filter A",
                    ColumnName = "",
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Filter B",
                    ColumnName = "",
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Filter C",
                    ColumnName = "",
                },
            ];

            // Define a custom filter sequence
            FilterSequenceEntry[] filterSequence =
            [
                new(filters[1].Id, null!), // Filter B
                new(filters[2].Id, null!), // Filter C
                new(filters[0].Id, null!), // Filter A
            ];

            ReleaseFile dataSet = _dataFixture
                .DefaultReleaseFile()
                .WithFile(
                    _dataFixture
                        .DefaultFile(FileType.Data)
                        .WithDataSetFileMeta(_dataFixture.DefaultDataSetFileMeta().WithFilters([.. filters]))
                )
                .WithFilterSequence([.. filterSequence])
                .WithReleaseVersion(releaseVersion);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.Add(dataSet);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetReleaseDataContent(releaseVersion.Id);

                // Assert
                var result = outcome.AssertRight();

                Assert.Equal(["Filter B", "Filter C", "Filter A"], result.DataSets[0].Meta.Filters);
            }
        }

        [Fact]
        public async Task WhenDataSetHasNoIndicatorSequenceDefined_ReturnsIndicatorsInLabelOrder()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            // Define indicators in non-alphabetical order
            IndicatorMeta[] indicators =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Indicator B",
                    ColumnName = "",
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Indicator C",
                    ColumnName = "",
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Indicator A",
                    ColumnName = "",
                },
            ];

            ReleaseFile dataSet = _dataFixture
                .DefaultReleaseFile()
                .WithFile(
                    _dataFixture
                        .DefaultFile(FileType.Data)
                        .WithDataSetFileMeta(_dataFixture.DefaultDataSetFileMeta().WithIndicators([.. indicators]))
                )
                .WithReleaseVersion(releaseVersion);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.Add(dataSet);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetReleaseDataContent(releaseVersion.Id);

                // Assert
                var result = outcome.AssertRight();

                Assert.Equal(["Indicator A", "Indicator B", "Indicator C"], result.DataSets[0].Meta.Indicators);
            }
        }

        [Fact]
        public async Task WhenDataSetHasIndicatorSequenceDefined_ReturnsIndicatorsInSequenceOrder()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            IndicatorMeta[] indicators =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Indicator A",
                    ColumnName = "",
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Indicator B",
                    ColumnName = "",
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Indicator C",
                    ColumnName = "",
                },
            ];

            // Define a custom indicator sequence
            IndicatorGroupSequenceEntry[] indicatorSequence =
            [
                new(
                    Guid.Empty,
                    [
                        indicators[1].Id, // Indicator B
                        indicators[2].Id, // Indicator C
                        indicators[0].Id, // Indicator A
                    ]
                ),
            ];

            ReleaseFile dataSet = _dataFixture
                .DefaultReleaseFile()
                .WithFile(
                    _dataFixture
                        .DefaultFile(FileType.Data)
                        .WithDataSetFileMeta(_dataFixture.DefaultDataSetFileMeta().WithIndicators([.. indicators]))
                )
                .WithIndicatorSequence([.. indicatorSequence])
                .WithReleaseVersion(releaseVersion);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.Add(dataSet);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetReleaseDataContent(releaseVersion.Id);

                // Assert
                var result = outcome.AssertRight();

                Assert.Equal(["Indicator B", "Indicator C", "Indicator A"], result.DataSets[0].Meta.Indicators);
            }
        }

        [Fact]
        public async Task WhenDataSetHasMultipleGeographicLevels_ReturnsGeographicLevelsInLabelOrder()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            ReleaseFile dataSet = _dataFixture
                .DefaultReleaseFile()
                .WithFile(
                    _dataFixture
                        .DefaultFile(FileType.Data)
                        .WithDataSetFileVersionGeographicLevels(
                            [GeographicLevel.Ward, GeographicLevel.Country, GeographicLevel.LocalAuthority]
                        )
                )
                .WithReleaseVersion(releaseVersion);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.Add(dataSet);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetReleaseDataContent(releaseVersion.Id);

                // Assert
                var result = outcome.AssertRight();

                // Expect the geographic levels to be returned in alphabetical order
                Assert.Equal(["Local authority", "National", "Ward"], result.DataSets[0].Meta.GeographicLevels);
            }
        }

        [Fact]
        public async Task WhenDataSetSummaryIsHtml_ReturnsPlainTextSummary()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            ReleaseFile dataSet = _dataFixture
                .DefaultReleaseFile()
                .WithFile(() => _dataFixture.DefaultFile(FileType.Data))
                .WithReleaseVersion(releaseVersion)
                .WithSummary(
                    "<div><p>Test paragraph with <strong>bold text</strong> and <em>italic text</em></p></div>"
                );

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                context.ReleaseFiles.Add(dataSet);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetReleaseDataContent(releaseVersion.Id);

                // Assert
                var result = outcome.AssertRight();

                Assert.Equal("Test paragraph with bold text and italic text", result.DataSets[0].Summary);
            }
        }

        [Fact]
        public async Task WhenReleaseVersionHasNoContent_ReturnsEmptyDataContent()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            // Release version has no content assigned to test that empty/optional properties are handled correctly

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.GetReleaseDataContent(releaseVersion.Id);

                // Assert
                var result = outcome.AssertRight();

                Assert.Equal(release.Id, result.ReleaseId);
                Assert.Equal(release.Versions[0].Id, result.ReleaseVersionId);
                Assert.Null(result.DataDashboards);
                Assert.Equal(releaseVersion.DataGuidance, result.DataGuidance);
                Assert.Empty(result.DataSets);
                Assert.Empty(result.FeaturedTables);
                Assert.Empty(result.SupportingFiles);
            }
        }

        [Fact]
        public async Task WhenReleaseVersionDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var releaseVersionId = Guid.NewGuid();

            await using var context = InMemoryContentDbContext();
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetReleaseDataContent(releaseVersionId);

            // Assert
            outcome.AssertNotFound();
        }

        private static void AssertDataSetEqual(ReleaseFile expected, ReleaseDataContentDataSetDto actual)
        {
            Assert.Equal(expected.File.DataSetFileId, actual.DataSetFileId);
            Assert.Equal(expected.File.Id, actual.FileId);
            Assert.Equal(expected.File.SubjectId, actual.SubjectId);
            AssertDataSetFileMetaEqual(expected, actual.Meta);
            Assert.Equal(expected.Summary, actual.Summary);
            Assert.Equal(expected.Name, actual.Title);
        }

        private static void AssertDataSetFileMetaEqual(ReleaseFile expected, ReleaseDataContentDataSetMetaDto actual)
        {
            var expectedGeographicLevels = expected
                .File.DataSetFileVersionGeographicLevels.Select(gl => gl.GeographicLevel.GetEnumLabel())
                .Order()
                .ToArray();
            Assert.Equal(expectedGeographicLevels, actual.GeographicLevels);

            var expectedMeta = expected.File.DataSetFileMeta!;
            var expectedFilters = expectedMeta.Filters.Select(f => f.Label);
            Assert.Equal(expectedFilters.Order(), actual.Filters.Order());
            var expectedIndicators = expectedMeta.Indicators.Select(i => i.Label);
            Assert.Equal(expectedIndicators.Order(), actual.Indicators.Order());
            Assert.Equal(expectedMeta.NumDataFileRows, actual.NumDataFileRows);
            Assert.Equal(expectedMeta.TimePeriodRange.Start.ToLabel(), actual.TimePeriodRange.Start);
            Assert.Equal(expectedMeta.TimePeriodRange.End.ToLabel(), actual.TimePeriodRange.End);
        }

        private static void AssertFeaturedTableEqual(FeaturedTable expected, ReleaseDataContentFeaturedTableDto actual)
        {
            Assert.Equal(expected.Id, actual.FeaturedTableId);
            Assert.Equal(expected.DataBlockParentId, actual.DataBlockParentId);
            Assert.Equal(expected.Description, actual.Summary);
            Assert.Equal(expected.Name, actual.Title);
        }

        private static void AssertSupportingFileEqual(ReleaseFile expected, ReleaseDataContentSupportingFileDto actual)
        {
            Assert.Equal(expected.File.Id, actual.FileId);
            Assert.Equal(expected.File.Extension, actual.Extension);
            Assert.Equal(expected.File.Filename, actual.Filename);
            Assert.Equal(expected.File.DisplaySize(), actual.Size);
            Assert.Equal(expected.Summary, actual.Summary);
            Assert.Equal(expected.Name, actual.Title);
        }
    }

    private static ReleaseDataContentService BuildService(ContentDbContext contentDbContext) => new(contentDbContext);
}
