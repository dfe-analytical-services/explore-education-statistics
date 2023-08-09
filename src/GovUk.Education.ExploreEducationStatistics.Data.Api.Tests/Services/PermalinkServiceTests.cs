#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Snapshooter;
using Snapshooter.Xunit;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services
{
    public class PermalinkServiceTests
    {
        private readonly DataFixture _fixture = new();

        private readonly Dictionary<GeographicLevel, List<string>> _regionLocalAuthorityHierarchy = new()
        {
            {
                GeographicLevel.LocalAuthority, ListOf(GeographicLevel.Region.ToString())
            }
        };

        private readonly Guid _publicationId = Guid.NewGuid();

        private readonly PermalinkTableViewModel _frontendTableResponse = new()
        {
            Caption = "Admission Numbers for 'Sample publication' in North East between 2022 and 2023",
            Json = JObject.Parse(
                System.IO.File.ReadAllText(
                    Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                        $"Resources{Path.DirectorySeparatorChar}permalink-table.json")))
        };

        [Fact]
        public async Task CreatePermalink_LatestPublishedReleaseForSubjectNotFound()
        {
            var request = new PermalinkCreateRequest
            {
                Query =
                {
                    SubjectId = Guid.NewGuid()
                }
            };

            var releaseRepository = new Mock<IReleaseRepository>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);

            releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(_publicationId))
                .ReturnsAsync(new NotFoundResult());

            subjectRepository
                .Setup(s => s.FindPublicationIdForSubject(request.Query.SubjectId))
                .ReturnsAsync(_publicationId);

            var service = BuildService(releaseRepository: releaseRepository.Object,
                subjectRepository: subjectRepository.Object);

            var result = await service.CreatePermalink(request);

            MockUtils.VerifyAllMocks(
                releaseRepository,
                subjectRepository);

            result.AssertNotFound();
        }

        [Fact]
        public async Task CreatePermalink_WithoutReleaseId()
        {
            var subject = _fixture
                .DefaultSubject()
                .WithFilters(_fixture.DefaultFilter()
                    .ForIndex(0, s =>
                        s.SetGroupCsvColumn("filter_0_grouping")
                            .SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 1)
                                .ForInstance(s => s.Set(
                                    fg => fg.Label,
                                    (_, _, context) => $"Filter group {context.FixtureTypeIndex}"))
                                .Generate(2)))
                    .ForIndex(1, s =>
                        s.SetGroupCsvColumn("filter_1_grouping")
                            .SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 1)
                                .ForInstance(s => s.Set(
                                    fg => fg.Label,
                                    (_, _, context) => $"Filter group {context.FixtureTypeIndex}"))
                                .Generate(2)))
                    .ForIndex(2, s =>
                        s.SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 2)
                            .Generate(1)))
                    .GenerateList())
                .WithIndicatorGroups(_fixture
                    .DefaultIndicatorGroup(indicatorCount: 1)
                    .Generate(3))
                .Generate();

            var indicators = subject
                .IndicatorGroups
                .SelectMany(ig => ig.Indicators)
                .ToList();

            var filter0Items = subject
                .Filters[0].FilterGroups
                .SelectMany(fg => fg.FilterItems)
                .ToList();

            var filter1Items = subject
                .Filters[1].FilterGroups
                .SelectMany(fg => fg.FilterItems)
                .ToList();

            var filter2Items = subject
                .Filters[2].FilterGroups
                .SelectMany(fg => fg.FilterItems)
                .ToList();

            var locations = _fixture.DefaultLocation()
                .ForRange(..2, l => l
                    .SetPresetRegion()
                    .SetGeographicLevel(GeographicLevel.Region))
                .ForRange(2..4, l => l
                    .SetPresetRegionAndLocalAuthority()
                    .SetGeographicLevel(GeographicLevel.LocalAuthority))
                .GenerateList(4);

            var observations = _fixture
                .DefaultObservation()
                .WithSubject(subject)
                .WithMeasures(indicators)
                .ForRange(..2, o => o
                    .SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0])
                    .SetLocation(locations[0])
                    .SetTimePeriod(2022, AcademicYear))
                .ForRange(2..4, o => o
                    .SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0])
                    .SetLocation(locations[1])
                    .SetTimePeriod(2022, AcademicYear))
                .ForRange(4..6, o => o
                    .SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[1])
                    .SetLocation(locations[2])
                    .SetTimePeriod(2023, AcademicYear))
                .ForRange(6..8, o => o
                    .SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[1])
                    .SetLocation(locations[3])
                    .SetTimePeriod(2023, AcademicYear))
                .GenerateList(8);

            var footnotes = _fixture
                .DefaultFootnote()
                .GenerateList(2);

            var footnoteViewModels = FootnotesViewModelBuilder.BuildFootnotes(footnotes);

            var request = new PermalinkCreateRequest
            {
                Configuration = new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders()
                },
                Query =
                {
                    SubjectId = subject.Id
                }
            };

            var tableResult = new TableBuilderResultViewModel
            {
                SubjectMeta = new SubjectResultMetaViewModel
                {
                    SubjectName = "Test data set",
                    PublicationName = "Test publication",
                    Locations = LocationViewModelBuilder
                        .BuildLocationAttributeViewModels(locations, _regionLocalAuthorityHierarchy)
                        .ToDictionary(
                            level => level.Key.ToString().CamelCase(),
                            level => level.Value),
                    Filters = FiltersMetaViewModelBuilder.BuildFilters(subject.Filters),
                    Indicators = IndicatorsMetaViewModelBuilder.BuildIndicators(indicators),
                    Footnotes = footnoteViewModels,
                    TimePeriodRange = new List<TimePeriodMetaViewModel>
                    {
                        new(2022, AcademicYear)
                        {
                            Label = "2022/23"
                        },
                        new(2023, AcademicYear)
                        {
                            Label = "2023/24"
                        }
                    }
                },
                Results = observations
                    .Select(o =>
                        ObservationViewModelBuilder.BuildObservation(o, indicators.Select(i => i.Id)))
                    .ToList()
            };

            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                ReleaseName = "2000",
                TimePeriodCoverage = AcademicYear,
                Published = DateTime.UtcNow,
            };

            var publication = new Publication
            {
                Id = _publicationId,
                LatestPublishedRelease = release
            };

            var csvMeta = new PermalinkCsvMetaViewModel
            {
                Filters = FiltersMetaViewModelBuilder.BuildCsvFiltersFromFilterItems(
                    filter0Items.Concat(filter1Items).Concat(filter2Items)),
                Indicators = indicators
                    .Select(i => new IndicatorCsvMetaViewModel(i))
                    .ToDictionary(i => i.Name),
                Locations = locations.ToDictionary(l => l.Id, l => l.GetCsvValues()),
                Headers = new List<string>
                {
                    "time_period",
                    "time_identifier",
                    "geographic_level",
                    "country_code",
                    "country_name",
                    "region_code",
                    "region_name",
                    "new_la_code",
                    "old_la_code",
                    "la_name",
                    "filter_0_grouping",
                    "filter_1_grouping",
                    subject.Filters[0].Name,
                    subject.Filters[1].Name,
                    subject.Filters[2].Name,
                    indicators[0].Name,
                    indicators[1].Name,
                    indicators[2].Name
                }
            };

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            Guid expectedPermalinkId;

            string? capturedTableCsvBlobPath = null;
            publicBlobStorageService.Setup(s => s.UploadStream(
                    BlobContainers.PermalinkSnapshots,
                    It.Is<string>(path => path.EndsWith(".csv.zst")),
                    It.IsAny<Stream>(),
                    ContentTypes.Csv,
                    ContentEncodings.Zstd,
                    It.IsAny<CancellationToken>()
                ))
                .Callback<IBlobContainer, string, Stream, string, string, CancellationToken>(
                    (_, path, stream, _, _, _) =>
                    {
                        // Capture the blob path
                        capturedTableCsvBlobPath = path;

                        // Capture the csv from the uploaded stream
                        stream.SeekToBeginning();
                        var csv = stream.ReadToEnd();

                        // Compare the captured csv upload with the expected csv
                        Snapshot.Match(csv, SnapshotNameExtension.Create("csv"));
                    })
                .Returns(Task.CompletedTask);

            string? capturedTableJsonBlobPath = null;
            publicBlobStorageService.Setup(s => s.UploadAsJson(
                    BlobContainers.PermalinkSnapshots,
                    It.Is<string>(path => path.EndsWith(".json.zst")),
                    It.IsAny<PermalinkTableViewModel>(),
                    ContentEncodings.Zstd,
                    null,
                    It.IsAny<CancellationToken>()
                ))
                .Callback<IBlobContainer,
                    string,
                    PermalinkTableViewModel,
                    string,
                    JsonSerializerSettings?,
                    CancellationToken>(
                    (_, path, table, _, _, _) =>
                    {
                        // Capture the blob path
                        capturedTableJsonBlobPath = path;

                        // Compare the captured table upload with the expected json
                        Snapshot.Match(table, SnapshotNameExtension.Create("json"));
                    })
                .Returns(Task.CompletedTask);

            var frontendService = new Mock<IFrontendService>(MockBehavior.Strict);

            frontendService.Setup(s => s.CreateTable(
                tableResult,
                request.Configuration,
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(_frontendTableResponse);

            var permalinkCsvMetaService = new Mock<IPermalinkCsvMetaService>(MockBehavior.Strict);

            permalinkCsvMetaService
                .Setup(s => s
                    .GetCsvMeta(
                        subject.Id,
                        tableResult.SubjectMeta,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(csvMeta);

            var releaseRepository = new Mock<IReleaseRepository>(MockBehavior.Strict);

            releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(_publicationId))
                .ReturnsAsync(release);

            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);

            subjectRepository
                .Setup(s => s.FindPublicationIdForSubject(subject.Id))
                .ReturnsAsync(_publicationId);

            var tableBuilderService = new Mock<ITableBuilderService>(MockBehavior.Strict);

            tableBuilderService
                .Setup(s => s.Query(release.Id, request.Query, CancellationToken.None))
                .ReturnsAsync(tableResult);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(ReleaseDataFile(release, subject.Id));

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object,
                    frontendService: frontendService.Object,
                    permalinkCsvMetaService: permalinkCsvMetaService.Object,
                    releaseRepository: releaseRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    tableBuilderService: tableBuilderService.Object);

                var result = (await service.CreatePermalink(request)).AssertRight();

                MockUtils.VerifyAllMocks(
                    publicBlobStorageService,
                    frontendService,
                    permalinkCsvMetaService,
                    releaseRepository,
                    subjectRepository,
                    tableBuilderService);

                // Expect the uploaded blob paths to be the same apart from the extension
                Assert.NotNull(capturedTableCsvBlobPath);
                Assert.NotNull(capturedTableJsonBlobPath);
                var tableCsvBlobPathWithoutExtension = capturedTableCsvBlobPath.Replace(".csv.zst", string.Empty);
                var tableJsonBlobPathWithoutExtension = capturedTableJsonBlobPath.Replace(".json.zst", string.Empty);
                Assert.Equal(tableJsonBlobPathWithoutExtension, tableCsvBlobPathWithoutExtension);

                // Expect the blob paths to contain a parseable Guid which is the generated permalink Id
                Assert.True(Guid.TryParse(tableCsvBlobPathWithoutExtension, out expectedPermalinkId));

                Assert.Equal(expectedPermalinkId, result.Id);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
                Assert.Equal("Test data set", result.DataSetTitle);
                Assert.Equal("Test publication", result.PublicationTitle);
                Assert.Equal(PermalinkStatus.Current, result.Status);
                Assert.Equal(_frontendTableResponse.Caption, result.Table.Caption);
                Assert.Equal(_frontendTableResponse.Json, result.Table.Json);
                Assert.Equal(footnoteViewModels, result.Table.Footnotes);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                // Verify details of the created permalink have been saved
                var permalink = contentDbContext.Permalinks.Single(p => p.Id == expectedPermalinkId);

                Assert.Equal(expectedPermalinkId, permalink.Id);
                Assert.InRange(DateTime.UtcNow.Subtract(permalink.Created).Milliseconds, 0, 1500);
                Assert.Equal("Test publication", permalink.PublicationTitle);
                Assert.Equal("Test data set", permalink.DataSetTitle);
                Assert.Equal(release.Id, permalink.ReleaseId);
                Assert.Equal(subject.Id, permalink.SubjectId);

                Assert.False(permalink.Legacy);
            }
        }

        [Fact]
        public async Task CreatePermalink_WithReleaseId()
        {
            var subject = _fixture
                .DefaultSubject()
                .WithFilters(_fixture.DefaultFilter()
                    .ForIndex(0, s =>
                        s.SetGroupCsvColumn("filter_0_grouping")
                            .SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 1)
                                .ForInstance(s => s.Set(
                                    fg => fg.Label,
                                    (_, _, context) => $"Filter group {context.FixtureTypeIndex}"))
                                .Generate(2)))
                    .ForIndex(1, s =>
                        s.SetGroupCsvColumn("filter_1_grouping")
                            .SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 1)
                                .ForInstance(s => s.Set(
                                    fg => fg.Label,
                                    (_, _, context) => $"Filter group {context.FixtureTypeIndex}"))
                                .Generate(2)))
                    .ForIndex(2, s =>
                        s.SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 2)
                            .Generate(1)))
                    .GenerateList())
                .WithIndicatorGroups(_fixture
                    .DefaultIndicatorGroup(indicatorCount: 1)
                    .Generate(3))
                .Generate();

            var indicators = subject
                .IndicatorGroups
                .SelectMany(ig => ig.Indicators)
                .ToList();

            var filter0Items = subject
                .Filters[0].FilterGroups
                .SelectMany(fg => fg.FilterItems)
                .ToList();

            var filter1Items = subject
                .Filters[1].FilterGroups
                .SelectMany(fg => fg.FilterItems)
                .ToList();

            var filter2Items = subject
                .Filters[2].FilterGroups
                .SelectMany(fg => fg.FilterItems)
                .ToList();

            var locations = _fixture.DefaultLocation()
                .ForRange(..2, l => l
                    .SetPresetRegion()
                    .SetGeographicLevel(GeographicLevel.Region))
                .ForRange(2..4, l => l
                    .SetPresetRegionAndLocalAuthority()
                    .SetGeographicLevel(GeographicLevel.LocalAuthority))
                .GenerateList(4);

            var observations = _fixture.DefaultObservation()
                .WithSubject(subject)
                .WithMeasures(indicators)
                .ForRange(..2, o => o
                    .SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0])
                    .SetLocation(locations[0])
                    .SetTimePeriod(2022, AcademicYear))
                .ForRange(2..4, o => o
                    .SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0])
                    .SetLocation(locations[1])
                    .SetTimePeriod(2022, AcademicYear))
                .ForRange(4..6, o => o
                    .SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[1])
                    .SetLocation(locations[2])
                    .SetTimePeriod(2023, AcademicYear))
                .ForRange(6..8, o => o
                    .SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[1])
                    .SetLocation(locations[3])
                    .SetTimePeriod(2023, AcademicYear))
                .GenerateList(8);

            var footnotes = _fixture
                .DefaultFootnote()
                .GenerateList(2);

            var footnoteViewModels = FootnotesViewModelBuilder.BuildFootnotes(footnotes);

            var request = new PermalinkCreateRequest
            {
                Configuration = new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders()
                },
                Query =
                {
                    SubjectId = subject.Id
                }
            };

            var tableResult = new TableBuilderResultViewModel
            {
                SubjectMeta = new SubjectResultMetaViewModel
                {
                    SubjectName = "Test data set",
                    PublicationName = "Test publication",
                    Locations = LocationViewModelBuilder
                        .BuildLocationAttributeViewModels(locations, _regionLocalAuthorityHierarchy)
                        .ToDictionary(
                            level => level.Key.ToString().CamelCase(),
                            level => level.Value),
                    Filters = FiltersMetaViewModelBuilder.BuildFilters(subject.Filters),
                    Indicators = IndicatorsMetaViewModelBuilder.BuildIndicators(indicators),
                    Footnotes = footnoteViewModels,
                    TimePeriodRange = new List<TimePeriodMetaViewModel>
                    {
                        new(2022, AcademicYear)
                        {
                            Label = "2022/23"
                        },
                        new(2023, AcademicYear)
                        {
                            Label = "2023/24"
                        }
                    }
                },
                Results = observations
                    .Select(o =>
                        ObservationViewModelBuilder.BuildObservation(o, indicators.Select(i => i.Id)))
                    .ToList()
            };

            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                ReleaseName = "2000",
                TimePeriodCoverage = AcademicYear,
                Published = DateTime.UtcNow,
            };

            var publication = new Publication
            {
                Id = _publicationId,
                LatestPublishedRelease = release
            };

            var csvMeta = new PermalinkCsvMetaViewModel
            {
                Filters = FiltersMetaViewModelBuilder.BuildCsvFiltersFromFilterItems(
                    filter0Items.Concat(filter1Items).Concat(filter2Items)),
                Indicators = indicators
                    .Select(i => new IndicatorCsvMetaViewModel(i))
                    .ToDictionary(i => i.Name),
                Locations = locations.ToDictionary(l => l.Id, l => l.GetCsvValues()),
                Headers = new List<string>
                {
                    "time_period",
                    "time_identifier",
                    "geographic_level",
                    "country_code",
                    "country_name",
                    "region_code",
                    "region_name",
                    "new_la_code",
                    "old_la_code",
                    "la_name",
                    "filter_0_grouping",
                    "filter_1_grouping",
                    subject.Filters[0].Name,
                    subject.Filters[1].Name,
                    subject.Filters[2].Name,
                    indicators[0].Name,
                    indicators[1].Name,
                    indicators[2].Name
                }
            };

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            Guid expectedPermalinkId;

            string? capturedTableCsvBlobPath = null;
            publicBlobStorageService.Setup(s => s.UploadStream(
                    BlobContainers.PermalinkSnapshots,
                    It.Is<string>(path => path.EndsWith(".csv.zst")),
                    It.IsAny<Stream>(),
                    ContentTypes.Csv,
                    ContentEncodings.Zstd,
                    It.IsAny<CancellationToken>()
                ))
                .Callback<IBlobContainer, string, Stream, string, string, CancellationToken>(
                    (_, path, stream, _, _, _) =>
                    {
                        // Capture the blob path
                        capturedTableCsvBlobPath = path;

                        // Capture the csv from the uploaded stream
                        stream.SeekToBeginning();
                        var csv = stream.ReadToEnd();

                        // Compare the captured csv upload with the expected csv
                        Snapshot.Match(csv, SnapshotNameExtension.Create("csv"));
                    })
                .Returns(Task.CompletedTask);

            string? capturedTableJsonBlobPath = null;
            publicBlobStorageService.Setup(s => s.UploadAsJson(
                    BlobContainers.PermalinkSnapshots,
                    It.Is<string>(path => path.EndsWith(".json.zst")),
                    It.IsAny<PermalinkTableViewModel>(),
                    ContentEncodings.Zstd,
                    null,
                    It.IsAny<CancellationToken>()
                ))
                .Callback<IBlobContainer,
                    string,
                    PermalinkTableViewModel,
                    string,
                    JsonSerializerSettings?,
                    CancellationToken>(
                    (_, path, table, _, _, _) =>
                    {
                        // Capture the blob path
                        capturedTableJsonBlobPath = path;

                        // Compare the captured table upload with the expected json
                        Snapshot.Match(table, SnapshotNameExtension.Create("json"));
                    })
                .Returns(Task.CompletedTask);

            var frontendService = new Mock<IFrontendService>(MockBehavior.Strict);

            frontendService.Setup(s => s.CreateTable(
                tableResult,
                request.Configuration,
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(_frontendTableResponse);

            var permalinkCsvMetaService = new Mock<IPermalinkCsvMetaService>(MockBehavior.Strict);

            permalinkCsvMetaService
                .Setup(s => s
                    .GetCsvMeta(
                        subject.Id,
                        tableResult.SubjectMeta,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(csvMeta);

            var tableBuilderService = new Mock<ITableBuilderService>(MockBehavior.Strict);

            tableBuilderService
                .Setup(s => s.Query(release.Id, request.Query, CancellationToken.None))
                .ReturnsAsync(tableResult);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(ReleaseDataFile(release, subject.Id));

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object,
                    frontendService: frontendService.Object,
                    permalinkCsvMetaService: permalinkCsvMetaService.Object,
                    tableBuilderService: tableBuilderService.Object);

                var result = (await service.CreatePermalink(release.Id, request)).AssertRight();

                MockUtils.VerifyAllMocks(
                    publicBlobStorageService,
                    frontendService,
                    permalinkCsvMetaService,
                    tableBuilderService);

                // Expect the uploaded blob paths to be the same apart from the extension
                Assert.NotNull(capturedTableCsvBlobPath);
                Assert.NotNull(capturedTableJsonBlobPath);
                var tableCsvBlobPathWithoutExtension = capturedTableCsvBlobPath.Replace(".csv.zst", string.Empty);
                var tableJsonBlobPathWithoutExtension = capturedTableJsonBlobPath.Replace(".json.zst", string.Empty);
                Assert.Equal(tableJsonBlobPathWithoutExtension, tableCsvBlobPathWithoutExtension);

                // Expect the blob paths to contain a parseable Guid which is the generated permalink Id
                Assert.True(Guid.TryParse(tableCsvBlobPathWithoutExtension, out expectedPermalinkId));

                Assert.Equal(expectedPermalinkId, result.Id);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
                Assert.Equal("Test data set", result.DataSetTitle);
                Assert.Equal("Test publication", result.PublicationTitle);
                Assert.Equal(PermalinkStatus.Current, result.Status);
                Assert.Equal(_frontendTableResponse.Caption, result.Table.Caption);
                Assert.Equal(_frontendTableResponse.Json, result.Table.Json);
                Assert.Equal(footnoteViewModels, result.Table.Footnotes);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                // Verify details of the created permalink have been saved
                var permalink = contentDbContext.Permalinks.Single(p => p.Id == expectedPermalinkId);

                Assert.Equal(expectedPermalinkId, permalink.Id);
                Assert.InRange(DateTime.UtcNow.Subtract(permalink.Created).Milliseconds, 0, 1500);
                Assert.Equal("Test publication", permalink.PublicationTitle);
                Assert.Equal("Test data set", permalink.DataSetTitle);
                Assert.Equal(release.Id, permalink.ReleaseId);
                Assert.Equal(subject.Id, permalink.SubjectId);

                Assert.False(permalink.Legacy);
            }
        }

        [Fact]
        public async Task GetPermalink()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2000",
                PublicationId = _publicationId,
                TimePeriodCoverage = AcademicYear,
                Published = DateTime.UtcNow,
            };

            var publication = new Publication
            {
                Id = _publicationId,
                LatestPublishedRelease = release
            };

            var permalink = new Permalink
            {
                Id = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                DataSetTitle = "Test data set",
                PublicationTitle = "Test publication",
                SubjectId = Guid.NewGuid()
            };

            var table = _frontendTableResponse with
            {
                Footnotes = FootnotesViewModelBuilder.BuildFootnotes(_fixture
                    .DefaultFootnote()
                    .GenerateList(2))
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddAsync(permalink);
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(ReleaseDataFile(release, permalink.SubjectId));

                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            publicBlobStorageService.SetupGetDeserializedJson(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json.zst",
                value: table);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var result = (await service.GetPermalink(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(permalink.Created, result.Created);
                Assert.Equal("Test data set", result.DataSetTitle);
                Assert.Equal("Test publication", result.PublicationTitle);
                Assert.Equal(PermalinkStatus.Current, result.Status);
                Assert.Equal(table.Caption, result.Table.Caption);
                Assert.Equal(table.Json, result.Table.Json);
                Assert.Equal(table.Footnotes, result.Table.Footnotes);
            }
        }

        [Fact]
        public async Task GetPermalink_PermalinkNotFound()
        {
            var service = BuildService();
            var result = await service.GetPermalink(Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task GetPermalink_BlobNotFound()
        {
            var permalink = new Permalink();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddAsync(permalink);
                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            publicBlobStorageService
                .SetupGetDeserializedJsonNotFound<IPublicBlobStorageService, PermalinkTableViewModel>(
                    container: BlobContainers.PermalinkSnapshots,
                    path: $"{permalink.Id}.json.zst");

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var result = await service.GetPermalink(permalink.Id);

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetPermalink_ReleaseWithSubjectNotFound()
        {
            var permalink = new Permalink
            {
                SubjectId = Guid.NewGuid()
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddAsync(permalink);
                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            publicBlobStorageService.SetupGetDeserializedJson(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json.zst",
                value: new PermalinkTableViewModel());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var result = (await service.GetPermalink(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(PermalinkStatus.SubjectRemoved, result.Status);
            }
        }

        [Fact]
        public async Task GetPermalink_SubjectIsForMultipleReleaseVersions()
        {
            var previousVersion = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                ReleaseName = "2000",
                TimePeriodCoverage = AcademicYear,
                Published = DateTime.UtcNow,
                PreviousVersionId = null
            };

            var latestVersion = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                ReleaseName = "2000",
                TimePeriodCoverage = AcademicYear,
                Published = DateTime.UtcNow,
                PreviousVersionId = previousVersion.Id
            };

            var publication = new Publication
            {
                Id = _publicationId,
                LatestPublishedRelease = latestVersion
            };

            var permalink = new Permalink
            {
                SubjectId = Guid.NewGuid()
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddAsync(permalink);
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddRangeAsync(previousVersion, latestVersion);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    ReleaseDataFile(previousVersion, permalink.SubjectId),
                    ReleaseDataFile(latestVersion, permalink.SubjectId)
                );

                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            publicBlobStorageService.SetupGetDeserializedJson(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json.zst",
                value: new PermalinkTableViewModel());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var result = (await service.GetPermalink(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(PermalinkStatus.Current, result.Status);
            }
        }

        [Fact]
        public async Task GetPermalink_SubjectIsNotForLatestRelease_OlderByYear()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                ReleaseName = "2000",
                TimePeriodCoverage = AcademicYear,
                Published = DateTime.UtcNow,
            };

            var latestRelease = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                ReleaseName = "2001",
                TimePeriodCoverage = AcademicYear,
                Published = DateTime.UtcNow,
            };

            var publication = new Publication
            {
                Id = _publicationId,
                LatestPublishedRelease = latestRelease
            };

            var permalink = new Permalink
            {
                SubjectId = Guid.NewGuid()
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddAsync(permalink);
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddRangeAsync(release, latestRelease);
                await contentDbContext.ReleaseFiles.AddRangeAsync(ReleaseDataFile(release, permalink.SubjectId));

                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            publicBlobStorageService.SetupGetDeserializedJson(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json.zst",
                value: new PermalinkTableViewModel());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var result = (await service.GetPermalink(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(PermalinkStatus.NotForLatestRelease, result.Status);
            }
        }

        [Fact]
        public async Task GetPermalink_SubjectIsNotForLatestRelease_OlderByTimePeriod()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                ReleaseName = "2000",
                TimePeriodCoverage = January,
                Published = DateTime.UtcNow,
            };

            var latestRelease = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                ReleaseName = "2000",
                TimePeriodCoverage = February,
                Published = DateTime.UtcNow,
            };

            var publication = new Publication
            {
                Id = _publicationId,
                LatestPublishedRelease = latestRelease
            };

            var permalink = new Permalink
            {
                SubjectId = Guid.NewGuid()
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddAsync(permalink);
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddRangeAsync(release, latestRelease);
                await contentDbContext.ReleaseFiles.AddRangeAsync(ReleaseDataFile(release, permalink.SubjectId));

                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            publicBlobStorageService.SetupGetDeserializedJson(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json.zst",
                value: new PermalinkTableViewModel());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var result = (await service.GetPermalink(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(PermalinkStatus.NotForLatestRelease, result.Status);
            }
        }

        [Fact]
        public async Task GetPermalink_SubjectIsNotForLatestReleaseVersion()
        {
            var previousVersion = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                ReleaseName = "2000",
                TimePeriodCoverage = AcademicYear,
                Published = DateTime.UtcNow,
                PreviousVersionId = null
            };

            var latestVersion = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                ReleaseName = "2000",
                TimePeriodCoverage = AcademicYear,
                Published = DateTime.UtcNow,
                PreviousVersionId = previousVersion.Id
            };

            var publication = new Publication
            {
                Id = _publicationId,
                LatestPublishedRelease = latestVersion
            };

            var permalink = new Permalink
            {
                SubjectId = Guid.NewGuid()
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddAsync(permalink);
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddRangeAsync(previousVersion, latestVersion);
                await contentDbContext.ReleaseFiles.AddRangeAsync(ReleaseDataFile(previousVersion,
                    permalink.SubjectId));

                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            publicBlobStorageService.SetupGetDeserializedJson(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json.zst",
                value: new PermalinkTableViewModel());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var result = (await service.GetPermalink(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(PermalinkStatus.SubjectReplacedOrRemoved, result.Status);
            }
        }

        [Fact]
        public async Task GetPermalink_SubjectIsFromSupersededPublication()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                ReleaseName = "2000",
                TimePeriodCoverage = AcademicYear,
                Published = DateTime.UtcNow,
            };

            var publication = new Publication
            {
                Id = _publicationId,
                LatestPublishedRelease = release,
                SupersededBy = new Publication
                {
                    LatestPublishedReleaseId = Guid.NewGuid()
                }
            };

            var permalink = new Permalink
            {
                SubjectId = Guid.NewGuid()
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddAsync(permalink);
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(ReleaseDataFile(release, permalink.SubjectId));

                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            publicBlobStorageService.SetupGetDeserializedJson(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json.zst",
                value: new PermalinkTableViewModel());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var result = (await service.GetPermalink(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(PermalinkStatus.PublicationSuperseded, result.Status);
            }
        }

        [Fact]
        public async Task DownloadCsvToStream()
        {
            var permalink = new Permalink();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddAsync(permalink);
                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            publicBlobStorageService.SetupDownloadToStream(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.csv.zst",
                content: "Test csv");

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                using var stream = new MemoryStream();

                var service = BuildService(
                    contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object
                );

                var result = await service.DownloadCsvToStream(permalink.Id, stream);

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                result.AssertRight();

                stream.SeekToBeginning();
                var csv = stream.ReadToEnd();

                Assert.Equal("Test csv", csv);
            }
        }

        [Fact]
        public async Task DownloadCsvToStream_PermalinkNotFound()
        {
            var service = BuildService();
            var result = await service.DownloadCsvToStream(Guid.NewGuid(), new MemoryStream());

            result.AssertNotFound();
        }

        [Fact]
        public async Task DownloadCsvToStream_BlobNotFound()
        {
            var permalink = new Permalink();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddAsync(permalink);
                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

            publicBlobStorageService.SetupDownloadToStreamNotFound(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.csv.zst");

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var result = await service.DownloadCsvToStream(permalink.Id, new MemoryStream());

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                result.AssertNotFound();
            }
        }

        private static ReleaseFile ReleaseDataFile(Release release, Guid subjectId)
        {
            return new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    SubjectId = subjectId,
                    Type = FileType.Data
                }
            };
        }

        private static PermalinkService BuildService(
            ContentDbContext? contentDbContext = null,
            ITableBuilderService? tableBuilderService = null,
            IPermalinkCsvMetaService? permalinkCsvMetaService = null,
            IPublicBlobStorageService? publicBlobStorageService = null,
            IFrontendService? frontendService = null,
            IReleaseRepository? releaseRepository = null,
            ISubjectRepository? subjectRepository = null,
            IPublicationRepository? publicationRepository = null)
        {
            contentDbContext ??= InMemoryContentDbContext();

            return new PermalinkService(
                contentDbContext,
                tableBuilderService ?? Mock.Of<ITableBuilderService>(MockBehavior.Strict),
                permalinkCsvMetaService ?? Mock.Of<IPermalinkCsvMetaService>(MockBehavior.Strict),
                publicBlobStorageService ?? Mock.Of<IPublicBlobStorageService>(MockBehavior.Strict),
                frontendService ?? Mock.Of<IFrontendService>(MockBehavior.Strict),
                subjectRepository ?? Mock.Of<ISubjectRepository>(MockBehavior.Strict),
                publicationRepository ?? new PublicationRepository(contentDbContext),
                releaseRepository ?? Mock.Of<IReleaseRepository>(MockBehavior.Strict)
            );
        }
    }
}
