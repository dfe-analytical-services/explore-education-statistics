using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using Footnote = GovUk.Education.ExploreEducationStatistics.Data.Model.Footnote;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class DataSetFileServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class ListDataSetFilesTests : DataSetFileServiceTests
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithTheme(_dataFixture.DefaultTheme());

            var releaseFiles = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(publication.Releases[0].Versions[0])
                .WithFiles(_dataFixture.DefaultFile(FileType.Data).GenerateList(2))
                .GenerateList();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.AddRange(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext);

                var result = await ListDataSetFiles(service);

                var pagedResult = result.AssertRight();

                Assert.Equal(2, pagedResult.Paging.TotalResults);
                Assert.Equal(2, pagedResult.Results.Count);

                foreach (var releaseFile in releaseFiles)
                {
                    var viewModel = Assert.Single(pagedResult.Results, r => r.FileId == releaseFile.FileId);

                    Assert.Equal(releaseFile.File.DataSetFileId, viewModel.Id);
                    Assert.Equal(releaseFile.File.Filename, viewModel.Filename);
                    Assert.Equal(releaseFile.File.DisplaySize(), viewModel.FileSize);
                    Assert.Equal(releaseFile.Name, viewModel.Title);
                    Assert.Equal(releaseFile.Summary, viewModel.Content);

                    Assert.Equal(publication.ThemeId, viewModel.Theme.Id);
                    Assert.Equal(publication.Theme.Title, viewModel.Theme.Title);

                    Assert.Equal(publication.Id, viewModel.Publication.Id);
                    Assert.Equal(publication.Title, viewModel.Publication.Title);
                    Assert.Equal(publication.Slug, viewModel.Publication.Slug);

                    Assert.Equal(releaseFile.ReleaseVersion.Id, viewModel.Release.Id);
                    Assert.Equal(releaseFile.ReleaseVersion.Release.Title, viewModel.Release.Title);
                    Assert.Equal(releaseFile.ReleaseVersion.Release.Slug, viewModel.Release.Slug);

                    Assert.True(viewModel.LatestData);
                    Assert.False(viewModel.IsSuperseded);
                    Assert.Equal(releaseFile.ReleaseVersion.PublishedDisplayDate, viewModel.Published);
                    Assert.Equal(releaseFile.Published, viewModel.LastUpdated);

                    Assert.Null(viewModel.Api);

                    Assert.Equal(releaseFile.File.DataSetFileMeta!.NumDataFileRows, viewModel.Meta.NumDataFileRows);
                    Assert.Equal(
                        releaseFile
                            .File.DataSetFileVersionGeographicLevels.Select(gl => gl.GeographicLevel.GetEnumLabel())
                            .Order()
                            .ToList(),
                        viewModel.Meta.GeographicLevels
                    );
                }
            }
        }

        [Fact]
        public async Task CsvOnlyGeographicLevels_ExcludedFromViewModel()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithTheme(_dataFixture.DefaultTheme());

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(publication.Releases[0].Versions[0])
                .WithFile(
                    _dataFixture
                        .DefaultFile(FileType.Data)
                        .WithDataSetFileVersionGeographicLevels([GeographicLevel.Country])
                        .WithCsvOnlyGeographicLevels([GeographicLevel.Institution])
                );

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext);

                var result = await ListDataSetFiles(service);

                var pagedResult = result.AssertRight();

                var viewModel = Assert.Single(pagedResult.Results);
                Assert.Equal([GeographicLevel.Country.GetEnumLabel()], viewModel.Meta.GeographicLevels);
            }
        }

        [Fact]
        public async Task FilterByGeographicLevel_CsvOnlyLevelsDoNotMatchWhenMultipleGeogLvls()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithTheme(_dataFixture.DefaultTheme());

            var releaseFiles = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(publication.Releases[0].Versions[0])
                .WithFiles([
                    _dataFixture
                        .DefaultFile(FileType.Data)
                        .WithDataSetFileVersionGeographicLevels([GeographicLevel.Institution]),
                    _dataFixture
                        .DefaultFile(FileType.Data)
                        .WithDataSetFileVersionGeographicLevels([GeographicLevel.Country])
                        .WithCsvOnlyGeographicLevels([GeographicLevel.Institution]),
                ])
                .GenerateList();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.AddRange(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext);

                var result = await ListDataSetFiles(service, geographicLevel: GeographicLevel.Institution);

                var pagedResult = result.AssertRight();

                var viewModel = Assert.Single(pagedResult.Results);
                Assert.Equal(releaseFiles[0].FileId, viewModel.FileId);
            }
        }

        [Fact]
        public async Task LatestOnlyDefaultsToTrue_ReturnsFilesOfLatestPublishedReleaseOnly()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ =>
                    [
                        _dataFixture.DefaultRelease(publishedVersions: 1, year: 2023),
                        _dataFixture.DefaultRelease(publishedVersions: 1, year: 2024),
                    ]
                )
                .WithTheme(_dataFixture.DefaultTheme());

            var releaseFiles = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersions(publication.Releases.Select(r => r.Versions[0]))
                .WithFiles(_dataFixture.DefaultFile(FileType.Data).GenerateList(2))
                .GenerateList();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.AddRange(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext);

                var result = await ListDataSetFiles(service);

                var pagedResult = result.AssertRight();

                var viewModel = Assert.Single(pagedResult.Results);
                Assert.Equal(publication.LatestPublishedReleaseVersionId, viewModel.Release.Id);
            }
        }

        [Fact]
        public async Task LatestOnlyFalse_ReturnsFilesOfAllLatestPublishedReleaseVersions()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ =>
                    [
                        _dataFixture.DefaultRelease(publishedVersions: 1, year: 2023),
                        _dataFixture.DefaultRelease(publishedVersions: 1, year: 2024),
                    ]
                )
                .WithTheme(_dataFixture.DefaultTheme());

            var releaseFiles = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersions(publication.Releases.Select(r => r.Versions[0]))
                .WithFiles(_dataFixture.DefaultFile(FileType.Data).GenerateList(2))
                .GenerateList();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.AddRange(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext);

                var result = await ListDataSetFiles(service, latestOnly: false);

                var pagedResult = result.AssertRight();

                Assert.Equal(
                    releaseFiles.Select(rf => rf.FileId).Order().ToList(),
                    pagedResult.Results.Select(vm => vm.FileId).Order().ToList()
                );
            }
        }

        [Fact]
        public async Task AmendedRelease_ReturnsFilesOfLatestVersionOnly()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 2)])
                .WithTheme(_dataFixture.DefaultTheme());

            var dataSetFileId = Guid.NewGuid();

            var releaseFiles = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersions(publication.Releases[0].Versions)
                .WithFiles(_dataFixture.DefaultFile(FileType.Data).WithDataSetFileId(dataSetFileId).GenerateList(2))
                .GenerateList();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.AddRange(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext);

                var result = await ListDataSetFiles(service, latestOnly: false);

                var pagedResult = result.AssertRight();

                var latestVersion = publication.Releases[0].Versions.Single(rv => rv.Version == 1);
                var viewModel = Assert.Single(pagedResult.Results);
                Assert.Equal(latestVersion.Id, viewModel.Release.Id);
            }
        }

        [Fact]
        public async Task FileHasDataReplacementInProgress_Excluded()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithTheme(_dataFixture.DefaultTheme());

            var releaseFiles = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(publication.Releases[0].Versions[0])
                .WithFiles([
                    _dataFixture.DefaultFile(FileType.Data),
                    _dataFixture.DefaultFile(FileType.Data).WithReplacingId(Guid.NewGuid()),
                ])
                .GenerateList();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.AddRange(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext);

                var result = await ListDataSetFiles(service);

                var pagedResult = result.AssertRight();

                var viewModel = Assert.Single(pagedResult.Results);
                Assert.Equal(releaseFiles[0].FileId, viewModel.FileId);
            }
        }

        [Fact]
        public async Task Pagination_ReturnsRequestedPage()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithTheme(_dataFixture.DefaultTheme());

            var releaseFiles = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(publication.Releases[0].Versions[0])
                .WithFiles(_dataFixture.DefaultFile(FileType.Data).GenerateList(3))
                .GenerateList();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.AddRange(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext);

                var page1Result = await ListDataSetFiles(service, page: 1, pageSize: 2);
                var page2Result = await ListDataSetFiles(service, page: 2, pageSize: 2);

                var page1 = page1Result.AssertRight();
                var page2 = page2Result.AssertRight();

                Assert.Equal(3, page1.Paging.TotalResults);
                Assert.Equal(2, page1.Paging.TotalPages);
                Assert.Equal(2, page1.Results.Count);

                Assert.Equal(2, page2.Paging.Page);
                Assert.Single(page2.Results);
            }
        }

        private static async Task<
            Either<ActionResult, PaginatedListViewModel<DataSetFileSummaryViewModel>>
        > ListDataSetFiles(
            DataSetFileService service,
            Guid? themeId = null,
            Guid? publicationId = null,
            Guid? releaseVersionId = null,
            GeographicLevel? geographicLevel = null,
            bool? latestOnly = null,
            DataSetType? dataSetType = null,
            string? searchTerm = null,
            DataSetsListRequestSortBy? sort = null,
            SortDirection? sortDirection = null,
            int page = 1,
            int pageSize = 10
        )
        {
            return await service.ListDataSetFiles(
                themeId: themeId,
                publicationId: publicationId,
                releaseVersionId: releaseVersionId,
                geographicLevel: geographicLevel,
                latestOnly: latestOnly,
                dataSetType: dataSetType,
                searchTerm: searchTerm,
                sort: sort,
                sortDirection: sortDirection,
                page: page,
                pageSize: pageSize
            );
        }
    }

    public class GetDataSetFileTests : DataSetFileServiceTests
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithTheme(_dataFixture.DefaultTheme());

            var releaseVersion = publication.Releases[0].Versions[0];

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithPublicApiDataSetId(Guid.NewGuid())
                .WithPublicApiDataSetVersion(major: 2, minor: 1)
                .WithFile(
                    _dataFixture
                        .DefaultFile(FileType.Data)
                        .WithDataSetFileVersionGeographicLevels([GeographicLevel.Country])
                        .WithCsvOnlyGeographicLevels([GeographicLevel.Institution])
                );

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(s => s.IsLatestPublishedReleaseVersion(releaseVersion.Id, CancellationToken.None))
                .ReturnsAsync(true);

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(MockBehavior.Strict);
            releaseFileBlobService
                .Setup(s =>
                    s.GetDownloadStream(It.Is<ReleaseFile>(rf => rf.Id == releaseFile.Id), CancellationToken.None)
                )
                .ReturnsAsync(
                    """
                    col_1,col_2
                    1,2
                    3,4
                    """.ToStream()
                );

            var footnotes = new List<Footnote>
            {
                new() { Id = Guid.NewGuid(), Content = "Footnote 1" },
                new() { Id = Guid.NewGuid(), Content = "Footnote 2" },
            };

            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);
            footnoteRepository
                .Setup(s => s.GetFootnotes(releaseVersion.Id, releaseFile.File.SubjectId))
                .ReturnsAsync(footnotes);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext,
                    releaseVersionRepository: releaseVersionRepository.Object,
                    releaseFileBlobService: releaseFileBlobService.Object,
                    footnoteRepository: footnoteRepository.Object
                );

                var result = await service.GetDataSetFile(
                    releaseFile.File.DataSetFileId!.Value,
                    CancellationToken.None
                );

                var viewModel = result.AssertRight();

                Assert.Equal(releaseFile.File.DataSetFileId, viewModel.Id);
                Assert.Equal(releaseFile.Name, viewModel.Title);
                Assert.Equal(releaseFile.Summary, viewModel.Summary);

                Assert.Equal(releaseVersion.Id, viewModel.Release.Id);
                Assert.Equal(releaseVersion.Release.Title, viewModel.Release.Title);
                Assert.Equal(releaseVersion.Release.Slug, viewModel.Release.Slug);
                Assert.Equal(releaseVersion.Type, viewModel.Release.Type);
                Assert.True(viewModel.Release.IsLatestPublishedRelease);
                Assert.False(viewModel.Release.IsSuperseded);
                Assert.Equal(releaseVersion.PublishedDisplayDate, viewModel.Release.Published);
                Assert.Equal(releaseFile.Published, viewModel.Release.LastUpdated);

                Assert.Equal(publication.Id, viewModel.Release.Publication.Id);
                Assert.Equal(publication.Title, viewModel.Release.Publication.Title);
                Assert.Equal(publication.Slug, viewModel.Release.Publication.Slug);
                Assert.Equal(publication.Theme.Title, viewModel.Release.Publication.ThemeTitle);

                Assert.Equal(releaseFile.FileId, viewModel.File.Id);
                Assert.Equal(releaseFile.File.Filename, viewModel.File.Name);
                Assert.Equal(releaseFile.File.DisplaySize(), viewModel.File.Size);
                Assert.Equal(releaseFile.File.SubjectId, viewModel.File.SubjectId);

                var meta = releaseFile.File.DataSetFileMeta!;

                Assert.Equal(meta.NumDataFileRows, viewModel.File.Meta.NumDataFileRows);
                // Csv-only geographic levels should be excluded
                Assert.Equal([GeographicLevel.Country.GetEnumLabel()], viewModel.File.Meta.GeographicLevels);
                Assert.Equal(
                    TimePeriodLabelFormatter.Format(
                        meta.TimePeriodRange.Start.Period,
                        meta.TimePeriodRange.Start.TimeIdentifier
                    ),
                    viewModel.File.Meta.TimePeriodRange.From
                );
                Assert.Equal(
                    TimePeriodLabelFormatter.Format(
                        meta.TimePeriodRange.End.Period,
                        meta.TimePeriodRange.End.TimeIdentifier
                    ),
                    viewModel.File.Meta.TimePeriodRange.To
                );
                Assert.Equal(meta.Filters.Select(f => f.Label).ToList(), viewModel.File.Meta.Filters);
                Assert.Equal(meta.Indicators.Select(i => i.Label).ToList(), viewModel.File.Meta.Indicators);

                Assert.Equal(
                    meta.Filters.Select(f => new LabelValue(f.Label, f.ColumnName))
                        .Concat(meta.Indicators.Select(i => new LabelValue(i.Label, i.ColumnName)))
                        .OrderBy(variable => variable.Value)
                        .ToList(),
                    viewModel.File.Variables
                );

                Assert.Equal(["col_1", "col_2"], viewModel.File.DataCsvPreview.Headers);
                Assert.Equal(2, viewModel.File.DataCsvPreview.Rows.Count);
                Assert.Equal(["1", "2"], viewModel.File.DataCsvPreview.Rows[0]);
                Assert.Equal(["3", "4"], viewModel.File.DataCsvPreview.Rows[1]);

                Assert.Equal(2, viewModel.Footnotes.Count);
                Assert.Equal(footnotes[0].Id, viewModel.Footnotes[0].Id);
                Assert.Equal(footnotes[0].Content, viewModel.Footnotes[0].Label);
                Assert.Equal(footnotes[1].Id, viewModel.Footnotes[1].Id);
                Assert.Equal(footnotes[1].Content, viewModel.Footnotes[1].Label);

                Assert.NotNull(viewModel.Api);
                Assert.Equal(releaseFile.PublicApiDataSetId, viewModel.Api.Id);
                Assert.Equal("2.1", viewModel.Api.Version);
            }
        }

        [Fact]
        public async Task AmendedRelease_ReturnsFileOfLatestVersion()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 2)])
                .WithTheme(_dataFixture.DefaultTheme());

            var previousVersion = publication.Releases[0].Versions.Single(rv => rv.Version == 0);
            var latestVersion = publication.Releases[0].Versions.Single(rv => rv.Version == 1);

            var dataSetFileId = Guid.NewGuid();

            var releaseFiles = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersions([previousVersion, latestVersion])
                .WithFiles(_dataFixture.DefaultFile(FileType.Data).WithDataSetFileId(dataSetFileId).GenerateList(2))
                .GenerateList();

            var latestReleaseFile = releaseFiles.Single(rf => rf.ReleaseVersionId == latestVersion.Id);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.AddRange(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(s => s.IsLatestPublishedReleaseVersion(latestVersion.Id, CancellationToken.None))
                .ReturnsAsync(true);

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(MockBehavior.Strict);
            releaseFileBlobService
                .Setup(s =>
                    s.GetDownloadStream(It.Is<ReleaseFile>(rf => rf.Id == latestReleaseFile.Id), CancellationToken.None)
                )
                .ReturnsAsync("col_1\n1".ToStream());

            var footnoteRepository = new Mock<IFootnoteRepository>(MockBehavior.Strict);
            footnoteRepository
                .Setup(s => s.GetFootnotes(latestVersion.Id, latestReleaseFile.File.SubjectId))
                .ReturnsAsync([]);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext,
                    releaseVersionRepository: releaseVersionRepository.Object,
                    releaseFileBlobService: releaseFileBlobService.Object,
                    footnoteRepository: footnoteRepository.Object
                );

                var result = await service.GetDataSetFile(dataSetFileId, CancellationToken.None);

                var viewModel = result.AssertRight();

                Assert.Equal(latestVersion.Id, viewModel.Release.Id);
                Assert.Equal(latestReleaseFile.FileId, viewModel.File.Id);
            }
        }

        [Fact]
        public async Task NoReleaseFile_ReturnsNotFound()
        {
            await using var contentDbContext = InMemoryContentDbContext();

            var service = SetupService(contentDbContext);

            var result = await service.GetDataSetFile(Guid.NewGuid(), CancellationToken.None);

            result.AssertNotFound();
        }

        [Fact]
        public async Task ReleaseVersionNotPublished_ReturnsNotFound()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)])
                .WithTheme(_dataFixture.DefaultTheme());

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(publication.Releases[0].Versions[0])
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext);

                var result = await service.GetDataSetFile(
                    releaseFile.File.DataSetFileId!.Value,
                    CancellationToken.None
                );

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task ReleaseVersionPublishedInFuture_ReturnsNotFound()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ =>
                    [
                        _dataFixture
                            .DefaultRelease()
                            .WithVersions([
                                _dataFixture.DefaultReleaseVersion().WithPublished(DateTimeOffset.UtcNow.AddDays(1)),
                            ]),
                    ]
                )
                .WithTheme(_dataFixture.DefaultTheme());

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(publication.Releases[0].Versions[0])
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext);

                var result = await service.GetDataSetFile(
                    releaseFile.File.DataSetFileId!.Value,
                    CancellationToken.None
                );

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task ReleaseVersionNotLatestPublished_ReturnsNotFound()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 2)])
                .WithTheme(_dataFixture.DefaultTheme());

            var previousVersion = publication.Releases[0].Versions.Single(rv => rv.Version == 0);

            // The data set file only exists on the previous version, i.e. it was removed on amendment
            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(previousVersion)
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(s => s.IsLatestPublishedReleaseVersion(previousVersion.Id, CancellationToken.None))
                .ReturnsAsync(false);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext, releaseVersionRepository: releaseVersionRepository.Object);

                var result = await service.GetDataSetFile(
                    releaseFile.File.DataSetFileId!.Value,
                    CancellationToken.None
                );

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task BlobStreamUnavailable_ReturnsError()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithTheme(_dataFixture.DefaultTheme());

            var releaseVersion = publication.Releases[0].Versions[0];

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(s => s.IsLatestPublishedReleaseVersion(releaseVersion.Id, CancellationToken.None))
                .ReturnsAsync(true);

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(MockBehavior.Strict);
            releaseFileBlobService
                .Setup(s =>
                    s.GetDownloadStream(It.Is<ReleaseFile>(rf => rf.Id == releaseFile.Id), CancellationToken.None)
                )
                .ReturnsAsync(new NotFoundResult());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext,
                    releaseVersionRepository: releaseVersionRepository.Object,
                    releaseFileBlobService: releaseFileBlobService.Object
                );

                var result = await service.GetDataSetFile(
                    releaseFile.File.DataSetFileId!.Value,
                    CancellationToken.None
                );

                result.AssertNotFound();
            }
        }
    }

    public class DownloadDataSetFileTests : DataSetFileServiceTests
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithTheme(_dataFixture.DefaultTheme());

            var releaseVersion = publication.Releases[0].Versions[0];

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(s => s.IsLatestPublishedReleaseVersion(releaseVersion.Id, CancellationToken.None))
                .ReturnsAsync(true);

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(MockBehavior.Strict);
            releaseFileBlobService
                .Setup(s =>
                    s.GetDownloadStream(It.Is<ReleaseFile>(rf => rf.Id == releaseFile.Id), CancellationToken.None)
                )
                .ReturnsAsync("Test csv".ToStream());

            IAnalyticsCaptureRequest? capturedRequest = null;
            var analyticsManager = new Mock<IAnalyticsManager>(MockBehavior.Strict);
            analyticsManager
                .Setup(s => s.Add(It.IsAny<IAnalyticsCaptureRequest>(), CancellationToken.None))
                .Callback((IAnalyticsCaptureRequest request, CancellationToken _) => capturedRequest = request)
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext,
                    releaseVersionRepository: releaseVersionRepository.Object,
                    releaseFileBlobService: releaseFileBlobService.Object,
                    analyticsManager: analyticsManager.Object
                );

                var result = await service.DownloadDataSetFile(
                    releaseFile.File.DataSetFileId!.Value,
                    CancellationToken.None
                );

                var fileStreamResult = Assert.IsType<FileStreamResult>(result);
                Assert.Equal("text/csv", fileStreamResult.ContentType);
                Assert.Equal(releaseFile.File.Filename, fileStreamResult.FileDownloadName);
                Assert.Equal("Test csv", fileStreamResult.FileStream.ReadToEnd());

                var captureRequest = Assert.IsType<CaptureCsvDownloadRequest>(capturedRequest);
                Assert.Equal(publication.Title, captureRequest.PublicationName);
                Assert.Equal(releaseVersion.Id, captureRequest.ReleaseVersionId);
                Assert.Equal(releaseVersion.Release.Title, captureRequest.ReleaseName);
                Assert.Equal(releaseVersion.Release.Label, captureRequest.ReleaseLabel);
                Assert.Equal(releaseFile.File.SubjectId, captureRequest.SubjectId);
                Assert.Equal(releaseFile.Name, captureRequest.DataSetTitle);
            }
        }

        [Fact]
        public async Task NoReleaseFile_ReturnsNotFound()
        {
            await using var contentDbContext = InMemoryContentDbContext();

            var service = SetupService(contentDbContext);

            var result = await service.DownloadDataSetFile(Guid.NewGuid(), CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ReleaseVersionNotLatestPublished_ReturnsNotFound()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 2)])
                .WithTheme(_dataFixture.DefaultTheme());

            var previousVersion = publication.Releases[0].Versions.Single(rv => rv.Version == 0);

            // The data set file only exists on the previous version, i.e. it was removed on amendment
            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(previousVersion)
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(s => s.IsLatestPublishedReleaseVersion(previousVersion.Id, CancellationToken.None))
                .ReturnsAsync(false);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext, releaseVersionRepository: releaseVersionRepository.Object);

                var result = await service.DownloadDataSetFile(
                    releaseFile.File.DataSetFileId!.Value,
                    CancellationToken.None
                );

                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async Task AnalyticsErrors_StillReturnsFile()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithTheme(_dataFixture.DefaultTheme());

            var releaseVersion = publication.Releases[0].Versions[0];

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(s => s.IsLatestPublishedReleaseVersion(releaseVersion.Id, CancellationToken.None))
                .ReturnsAsync(true);

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(MockBehavior.Strict);
            releaseFileBlobService
                .Setup(s =>
                    s.GetDownloadStream(It.Is<ReleaseFile>(rf => rf.Id == releaseFile.Id), CancellationToken.None)
                )
                .ReturnsAsync("Test csv".ToStream());

            var analyticsManager = new Mock<IAnalyticsManager>(MockBehavior.Strict);
            analyticsManager
                .Setup(s => s.Add(It.IsAny<IAnalyticsCaptureRequest>(), CancellationToken.None))
                .ThrowsAsync(new Exception("Analytics error"));

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext,
                    releaseVersionRepository: releaseVersionRepository.Object,
                    releaseFileBlobService: releaseFileBlobService.Object,
                    analyticsManager: analyticsManager.Object
                );

                var result = await service.DownloadDataSetFile(
                    releaseFile.File.DataSetFileId!.Value,
                    CancellationToken.None
                );

                var fileStreamResult = Assert.IsType<FileStreamResult>(result);
                Assert.Equal("Test csv", fileStreamResult.FileStream.ReadToEnd());
            }
        }

        [Fact]
        public async Task FileHasNoSubjectId_SkipsAnalytics()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithTheme(_dataFixture.DefaultTheme());

            var releaseVersion = publication.Releases[0].Versions[0];

            File file = _dataFixture.DefaultFile(FileType.Data);
            file.SubjectId = null;

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(file);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(s => s.IsLatestPublishedReleaseVersion(releaseVersion.Id, CancellationToken.None))
                .ReturnsAsync(true);

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(MockBehavior.Strict);
            releaseFileBlobService
                .Setup(s =>
                    s.GetDownloadStream(It.Is<ReleaseFile>(rf => rf.Id == releaseFile.Id), CancellationToken.None)
                )
                .ReturnsAsync("Test csv".ToStream());

            var analyticsManager = new Mock<IAnalyticsManager>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext,
                    releaseVersionRepository: releaseVersionRepository.Object,
                    releaseFileBlobService: releaseFileBlobService.Object,
                    analyticsManager: analyticsManager.Object
                );

                var result = await service.DownloadDataSetFile(
                    releaseFile.File.DataSetFileId!.Value,
                    CancellationToken.None
                );

                Assert.IsType<FileStreamResult>(result);
                analyticsManager.VerifyNoOtherCalls();
            }
        }

        [Fact]
        public async Task BlobStreamUnavailable_ReturnsError()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithTheme(_dataFixture.DefaultTheme());

            var releaseVersion = publication.Releases[0].Versions[0];

            ReleaseFile releaseFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.Add(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(MockBehavior.Strict);
            releaseVersionRepository
                .Setup(s => s.IsLatestPublishedReleaseVersion(releaseVersion.Id, CancellationToken.None))
                .ReturnsAsync(true);

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(MockBehavior.Strict);
            releaseFileBlobService
                .Setup(s =>
                    s.GetDownloadStream(It.Is<ReleaseFile>(rf => rf.Id == releaseFile.Id), CancellationToken.None)
                )
                .ReturnsAsync(new NotFoundResult());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(
                    contentDbContext,
                    releaseVersionRepository: releaseVersionRepository.Object,
                    releaseFileBlobService: releaseFileBlobService.Object,
                    analyticsManager: new Mock<IAnalyticsManager>().Object
                );

                var result = await service.DownloadDataSetFile(
                    releaseFile.File.DataSetFileId!.Value,
                    CancellationToken.None
                );

                Assert.IsType<NotFoundResult>(result);
            }
        }
    }

    public class ListSitemapItemsTests : DataSetFileServiceTests
    {
        [Fact]
        public async Task Success()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 2)])
                .WithTheme(_dataFixture.DefaultTheme());

            var previousVersion = publication.Releases[0].Versions.Single(rv => rv.Version == 0);
            var latestVersion = publication.Releases[0].Versions.Single(rv => rv.Version == 1);

            ReleaseFile previousVersionFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(previousVersion)
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            ReleaseFile latestVersionFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(latestVersion)
                .WithFile(_dataFixture.DefaultFile(FileType.Data));

            ReleaseFile ancillaryFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(latestVersion)
                .WithFile(_dataFixture.DefaultFile(FileType.Ancillary));

            ReleaseFile replacementInProgressFile = _dataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(latestVersion)
                .WithFile(_dataFixture.DefaultFile(FileType.Data).WithReplacingId(Guid.NewGuid()));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseFiles.AddRange(
                    previousVersionFile,
                    latestVersionFile,
                    ancillaryFile,
                    replacementInProgressFile
                );
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext);

                var result = await service.ListSitemapItems();

                var sitemapItems = result.AssertRight();

                var sitemapItem = Assert.Single(sitemapItems); // only on latest version with FileType.Data
                Assert.Equal(latestVersionFile.File.DataSetFileId!.Value.ToString(), sitemapItem.Id);
                Assert.Equal(latestVersionFile.Published, sitemapItem.LastModified);
            }
        }
    }

    private static DataSetFileService SetupService(
        ContentDbContext contentDbContext,
        IReleaseVersionRepository? releaseVersionRepository = null,
        IReleaseFileBlobService? releaseFileBlobService = null,
        IFootnoteRepository? footnoteRepository = null,
        IAnalyticsManager? analyticsManager = null,
        ILogger<DataSetFileService>? logger = null
    )
    {
        return new DataSetFileService(
            contentDbContext,
            releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(MockBehavior.Strict),
            releaseFileBlobService ?? Mock.Of<IReleaseFileBlobService>(MockBehavior.Strict),
            footnoteRepository ?? Mock.Of<IFootnoteRepository>(MockBehavior.Strict),
            analyticsManager ?? Mock.Of<IAnalyticsManager>(),
            logger ?? Mock.Of<ILogger<DataSetFileService>>()
        );
    }
}
