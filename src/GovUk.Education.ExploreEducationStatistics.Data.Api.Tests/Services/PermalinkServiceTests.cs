#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Mappings;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                .WithFilters(_fixture
                    .DefaultFilter(filterGroupCount: 1, filterItemCount: 2)
                    .Generate(2))
                .WithIndicatorGroups(_fixture
                    .DefaultIndicatorGroup(indicatorCount: 1)
                    .Generate(3))
                .Generate();

            var indicators = subject
                .IndicatorGroups
                .SelectMany(ig => ig.Indicators)
                .ToList();
            
            var filter1Items = subject
                .Filters[0].FilterGroups
                .SelectMany(fg => fg.FilterItems)
                .ToList();
            
            var filter2Items = subject
                .Filters[1].FilterGroups
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
                    .SetFilterItems(filter1Items[0], filter2Items[0])
                    .SetLocation(locations[0])
                    .SetTimePeriod(2022, AcademicYear))
                .ForRange(2..4, o => o
                    .SetFilterItems(filter1Items[0], filter2Items[0])
                    .SetLocation(locations[1])
                    .SetTimePeriod(2022, AcademicYear))
                .ForRange(4..6, o => o
                    .SetFilterItems(filter1Items[1], filter2Items[1])
                    .SetLocation(locations[2])
                    .SetTimePeriod(2023, AcademicYear))
                .ForRange(6..8, o => o
                    .SetFilterItems(filter1Items[1], filter2Items[1])
                    .SetLocation(locations[3])
                    .SetTimePeriod(2023, AcademicYear))
                .GenerateList(8);

            var footnotes = _fixture
                .DefaultFootnote()
                .GenerateList(2);

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
                    Footnotes = footnotes.Select(
                        footnote => new FootnoteViewModel(footnote.Id, footnote.Content)
                    ).ToList(),
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
                Filters = FiltersMetaViewModelBuilder
                    .BuildCsvFiltersFromFilterItems(filter1Items.Concat(filter2Items)),
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
                    subject.Filters[0].Name,
                    subject.Filters[1].Name,
                    indicators[0].Name,
                    indicators[1].Name,
                    indicators[2].Name
                }
            };

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            Guid expectedPermalinkId;

            string? capturedTableCsvBlobPath = null;
            blobStorageService.Setup(s => s.UploadStream(
                    BlobContainers.PermalinkSnapshots,
                    It.Is<string>(path => path.EndsWith(".csv")),
                    It.IsAny<Stream>(),
                    "text/csv",
                    It.IsAny<CancellationToken>()
                ))
                .Callback<IBlobContainer, string, Stream, string, CancellationToken>((_, path, stream, _, _) =>
                {
                    // Capture the blob path
                    capturedTableCsvBlobPath = path;

                    // Convert captured stream to string
                    stream.Seek(0L, SeekOrigin.Begin);
                    var csv = stream.ReadToEnd();

                    // Compare the captured csv upload with the expected csv
                    Snapshot.Match(csv);
                })
                .Returns(Task.CompletedTask);

            string? capturedTableJsonBlobPath = null;
            blobStorageService.Setup(s => s.UploadStream(
                    BlobContainers.PermalinkSnapshots,
                    It.Is<string>(path => path.EndsWith(".json")),
                    It.IsAny<Stream>(),
                    MediaTypeNames.Application.Json,
                    It.IsAny<CancellationToken>()
                ))
                .Callback<IBlobContainer, string, Stream, string, CancellationToken>((_, path, stream, _, _) =>
                {
                    // Capture the blob path
                    capturedTableJsonBlobPath = path;

                    // Convert captured stream to string
                    stream.Seek(0L, SeekOrigin.Begin);
                    var tableJson = stream.ReadToEnd();

                    // Compare the captured table json upload with the response set up for the frontend service
                    Assert.Equal("{}", tableJson);
                })
                .Returns(Task.CompletedTask);

            var frontendService = new Mock<IFrontendService>(MockBehavior.Strict);

            frontendService.Setup(s => s.CreateUniversalTable(
                tableResult,
                request.Configuration,
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(new JObject());

            var permalinkCsvMetaService = new Mock<IPermalinkCsvMetaService>(MockBehavior.Strict);

            permalinkCsvMetaService
                .Setup(s => s
                    .GetCsvMeta(
                        subject.Id,
                        tableResult.SubjectMeta.Locations,
                        tableResult.SubjectMeta.Filters,
                        tableResult.SubjectMeta.Indicators,
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
                    blobStorageService: blobStorageService.Object,
                    frontendService: frontendService.Object,
                    permalinkCsvMetaService: permalinkCsvMetaService.Object,
                    releaseRepository: releaseRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    tableBuilderService: tableBuilderService.Object);

                var result = (await service.CreatePermalink(request)).AssertRight();

                MockUtils.VerifyAllMocks(
                    blobStorageService,
                    frontendService,
                    permalinkCsvMetaService,
                    releaseRepository,
                    subjectRepository,
                    tableBuilderService);

                // Expect the uploaded blob paths to be the same apart from the extension
                var tableCsvBlobPathWithoutExtension = Path.GetFileNameWithoutExtension(capturedTableCsvBlobPath);
                var tableJsonBlobPathWithoutExtension = Path.GetFileNameWithoutExtension(capturedTableJsonBlobPath);
                Assert.Equal(tableJsonBlobPathWithoutExtension, tableCsvBlobPathWithoutExtension);

                // Expect the blob paths to contain a parseable Guid which is the generated permalink Id
                Assert.True(Guid.TryParse(tableCsvBlobPathWithoutExtension, out expectedPermalinkId));

                Assert.Equal(expectedPermalinkId, result.Id);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
                Assert.Equal("Test data set", result.DataSetTitle);
                Assert.Equal("Test publication", result.PublicationTitle);
                Assert.Equal(PermalinkStatus.Current, result.Status);
                Assert.Equal("{}", result.Table.ToString());
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

                // Statistics about the permalink should be set
                Assert.Equal(4, permalink.CountFilterItems);
                Assert.Equal(2, permalink.CountFootnotes);
                Assert.Equal(3, permalink.CountIndicators);
                Assert.Equal(4, permalink.CountLocations);
                Assert.Equal(8, permalink.CountObservations);
                Assert.Equal(2, permalink.CountTimePeriods);

                // TODO EES-3755 Remove after Permalink snapshot migration work is complete
                Assert.False(permalink.Legacy);
                Assert.Null(permalink.LegacyContentLength);
                Assert.Null(permalink.LegacyHasConfigurationHeaders);
            }
        }

        [Fact]
        public async Task CreatePermalink_WithReleaseId()
        {
            var subject = _fixture
                .DefaultSubject()
                .WithFilters(_fixture
                    .DefaultFilter(filterGroupCount: 1, filterItemCount: 2)
                    .Generate(2))
                .WithIndicatorGroups(_fixture
                    .DefaultIndicatorGroup(indicatorCount: 1)
                    .Generate(3))
                .Generate();

            var indicators = subject
                .IndicatorGroups
                .SelectMany(ig => ig.Indicators)
                .ToList();
            
            var filter1Items = subject
                .Filters[0].FilterGroups
                .SelectMany(fg => fg.FilterItems)
                .ToList();
            
            var filter2Items = subject
                .Filters[1].FilterGroups
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
                    .SetFilterItems(filter1Items[0], filter2Items[0])
                    .SetLocation(locations[0])
                    .SetTimePeriod(2022, AcademicYear))
                .ForRange(2..4, o => o
                    .SetFilterItems(filter1Items[0], filter2Items[0])
                    .SetLocation(locations[1])
                    .SetTimePeriod(2022, AcademicYear))
                .ForRange(4..6, o => o
                    .SetFilterItems(filter1Items[1], filter2Items[1])
                    .SetLocation(locations[2])
                    .SetTimePeriod(2023, AcademicYear))
                .ForRange(6..8, o => o
                    .SetFilterItems(filter1Items[1], filter2Items[1])
                    .SetLocation(locations[3])
                    .SetTimePeriod(2023, AcademicYear))
                .GenerateList(8);

            var footnotes = _fixture
                .DefaultFootnote()
                .GenerateList(2);

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
                    Footnotes = footnotes.Select(
                        footnote => new FootnoteViewModel(footnote.Id, footnote.Content)
                    ).ToList(),
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
                Filters = FiltersMetaViewModelBuilder
                    .BuildCsvFiltersFromFilterItems(filter1Items.Concat(filter2Items)),
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
                    subject.Filters[0].Name,
                    subject.Filters[1].Name,
                    indicators[0].Name,
                    indicators[1].Name,
                    indicators[2].Name
                }
            };

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            Guid expectedPermalinkId;

            string? capturedTableCsvBlobPath = null;
            blobStorageService.Setup(s => s.UploadStream(
                    BlobContainers.PermalinkSnapshots,
                    It.Is<string>(path => path.EndsWith(".csv")),
                    It.IsAny<Stream>(),
                    "text/csv",
                    It.IsAny<CancellationToken>()
                ))
                .Callback<IBlobContainer, string, Stream, string, CancellationToken>((_, path, stream, _, _) =>
                {
                    // Capture the blob path
                    capturedTableCsvBlobPath = path;

                    // Convert captured stream to string
                    stream.Seek(0L, SeekOrigin.Begin);
                    var csv = stream.ReadToEnd();

                    // Compare the captured csv upload with the expected csv
                    Snapshot.Match(csv);
                })
                .Returns(Task.CompletedTask);

            string? capturedTableJsonBlobPath = null;
            blobStorageService.Setup(s => s.UploadStream(
                    BlobContainers.PermalinkSnapshots,
                    It.Is<string>(path => path.EndsWith(".json")),
                    It.IsAny<Stream>(),
                    MediaTypeNames.Application.Json,
                    It.IsAny<CancellationToken>()
                ))
                .Callback<IBlobContainer, string, Stream, string, CancellationToken>((_, path, stream, _, _) =>
                {
                    // Capture the blob path
                    capturedTableJsonBlobPath = path;

                    // Convert captured stream to string
                    stream.Seek(0L, SeekOrigin.Begin);
                    var tableJson = stream.ReadToEnd();

                    // Compare the captured table json upload with the response set up for the frontend service
                    Assert.Equal("{}", tableJson);
                })
                .Returns(Task.CompletedTask);

            var frontendService = new Mock<IFrontendService>(MockBehavior.Strict);

            frontendService.Setup(s => s.CreateUniversalTable(
                tableResult,
                request.Configuration,
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(new JObject());

            var permalinkCsvMetaService = new Mock<IPermalinkCsvMetaService>(MockBehavior.Strict);

            permalinkCsvMetaService
                .Setup(s => s
                    .GetCsvMeta(
                        subject.Id,
                        tableResult.SubjectMeta.Locations,
                        tableResult.SubjectMeta.Filters,
                        tableResult.SubjectMeta.Indicators,
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
                    blobStorageService: blobStorageService.Object,
                    frontendService: frontendService.Object,
                    permalinkCsvMetaService: permalinkCsvMetaService.Object,
                    tableBuilderService: tableBuilderService.Object);

                var result = (await service.CreatePermalink(release.Id, request)).AssertRight();

                MockUtils.VerifyAllMocks(
                    blobStorageService,
                    frontendService,
                    permalinkCsvMetaService,
                    tableBuilderService);

                // Expect the uploaded blob paths to be the same apart from the extension
                var tableCsvBlobPathWithoutExtension = Path.GetFileNameWithoutExtension(capturedTableCsvBlobPath);
                var tableJsonBlobPathWithoutExtension = Path.GetFileNameWithoutExtension(capturedTableJsonBlobPath);
                Assert.Equal(tableJsonBlobPathWithoutExtension, tableCsvBlobPathWithoutExtension);

                // Expect the blob paths to contain a parseable Guid which is the generated permalink Id
                Assert.True(Guid.TryParse(tableCsvBlobPathWithoutExtension, out expectedPermalinkId));

                Assert.Equal(expectedPermalinkId, result.Id);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
                Assert.Equal("Test data set", result.DataSetTitle);
                Assert.Equal("Test publication", result.PublicationTitle);
                Assert.Equal(PermalinkStatus.Current, result.Status);
                Assert.Equal("{}", result.Table.ToString());
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

                // Statistics about the permalink should be set
                Assert.Equal(4, permalink.CountFilterItems);
                Assert.Equal(2, permalink.CountFootnotes);
                Assert.Equal(3, permalink.CountIndicators);
                Assert.Equal(4, permalink.CountLocations);
                Assert.Equal(8, permalink.CountObservations);
                Assert.Equal(2, permalink.CountTimePeriods);

                // TODO EES-3755 Remove after Permalink snapshot migration work is complete
                Assert.False(permalink.Legacy);
                Assert.Null(permalink.LegacyContentLength);
                Assert.Null(permalink.LegacyHasConfigurationHeaders);
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

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddAsync(permalink);
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(ReleaseDataFile(release, permalink.SubjectId));

                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            // TODO EES-3753 Add some realistic table json here
            blobStorageService.SetupDownloadBlobText(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json",
                blobText: "{}");

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = (await service.GetPermalink(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(permalink.Created, result.Created);
                Assert.Equal("Test data set", result.DataSetTitle);
                Assert.Equal("Test publication", result.PublicationTitle);
                Assert.Equal(PermalinkStatus.Current, result.Status);
                Assert.Equal("{}", result.Table.ToString());
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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobTextNotFound(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json");

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.GetPermalink(permalink.Id);

                MockUtils.VerifyAllMocks(blobStorageService);

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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobText(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json",
                blobText: "{}");

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = (await service.GetPermalink(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);

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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobText(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json",
                blobText: "{}");

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = (await service.GetPermalink(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);

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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobText(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json",
                blobText: "{}");

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = (await service.GetPermalink(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);

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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobText(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json",
                blobText: "{}");

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = (await service.GetPermalink(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);

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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobText(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json",
                blobText: "{}");

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = (await service.GetPermalink(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);

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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobText(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json",
                blobText: "{}");

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = (await service.GetPermalink(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);

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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDownloadToStream(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.csv",
                blobText: "Test csv");

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                using var stream = new MemoryStream();

                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object
                );

                var result = await service.DownloadCsvToStream(permalink.Id, stream);

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertRight();

                stream.Seek(0L, SeekOrigin.Begin);
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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDownloadToStreamNotFound(
                container: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.csv");

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.DownloadCsvToStream(permalink.Id, new MemoryStream());

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertNotFound();
            }
        }

        [Fact]
        // TODO EES-3755 Remove after Permalink snapshot migration work is complete
        // TODO EES-3755 Remove __snapshots__/PermalinkServiceTests.MigratePermalink.snap
        public async Task MigratePermalink()
        {
            var permalink = new Permalink
            {
                Id = Guid.NewGuid(),
                Legacy = true
            };

            var subject = _fixture.DefaultSubject().Generate();

            var filters = _fixture.DefaultFilter().GenerateList(2);

            var filterItems = _fixture.DefaultFilterItem()
                .ForRange(..2, fi => fi
                    .SetFilterGroup(_fixture.DefaultFilterGroup()
                        .WithFilter(filters[0])
                        .Generate()))
                .ForRange(2..4, fi => fi
                    .SetFilterGroup(_fixture.DefaultFilterGroup()
                        .WithFilter(filters[1])
                        .Generate()))
                .GenerateArray();

            var indicators = _fixture.DefaultIndicator()
                .ForRange(..1, i => i
                    .SetIndicatorGroup(_fixture.DefaultIndicatorGroup()
                        .WithSubject(subject))
                )
                .ForRange(1..3, i => i
                    .SetIndicatorGroup(_fixture.DefaultIndicatorGroup()
                        .WithSubject(subject))
                )
                .GenerateList(3);

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
                    .SetFilterItems(filterItems[0], filterItems[2])
                    .SetLocation(locations[0])
                    .SetTimePeriod(2022, AcademicYear))
                .ForRange(2..4, o => o
                    .SetFilterItems(filterItems[0], filterItems[2])
                    .SetLocation(locations[1])
                    .SetTimePeriod(2022, AcademicYear))
                .ForRange(4..6, o => o
                    .SetFilterItems(filterItems[1], filterItems[3])
                    .SetLocation(locations[2])
                    .SetTimePeriod(2023, AcademicYear))
                .ForRange(6..8, o => o
                    .SetFilterItems(filterItems[1], filterItems[3])
                    .SetLocation(locations[3])
                    .SetTimePeriod(2023, AcademicYear))
                .GenerateList(8);

            var legacyPermalink = new LegacyPermalink(
                permalink.Id,
                DateTime.UtcNow,
                new TableBuilderConfiguration(),
                new PermalinkTableBuilderResult
                {
                    Results = observations
                        .Select(o =>
                            ObservationViewModelBuilder.BuildObservation(o, indicators.Select(i => i.Id)))
                        .ToList()
                },
                new ObservationQueryContext());

            var csvMeta = new PermalinkCsvMetaViewModel
            {
                Filters = FiltersMetaViewModelBuilder.BuildCsvFiltersFromFilterItems(filterItems),
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
                    filters[0].Name,
                    filters[1].Name,
                    indicators[0].Name,
                    indicators[1].Name,
                    indicators[2].Name
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddRangeAsync(permalink);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobText(
                container: BlobContainers.Permalinks,
                path: permalink.Id.ToString(),
                blobText: JsonConvert.SerializeObject(legacyPermalink));

            blobStorageService.Setup(s => s.UploadStream(
                    BlobContainers.PermalinkSnapshots,
                    $"{permalink.Id}.csv",
                    It.IsAny<Stream>(),
                    "text/csv",
                    It.IsAny<CancellationToken>()
                ))
                .Callback<IBlobContainer, string, Stream, string, CancellationToken>((_, path, stream, _, _) =>
                {
                    // Convert captured stream to string
                    stream.Seek(0L, SeekOrigin.Begin);
                    var csv = stream.ReadToEnd();

                    // Compare the captured csv upload with the expected csv
                    Snapshot.Match(csv);
                })
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(s => s.UploadStream(
                    BlobContainers.PermalinkSnapshots,
                    $"{permalink.Id}.json",
                    It.IsAny<Stream>(),
                    MediaTypeNames.Application.Json,
                    It.IsAny<CancellationToken>()
                ))
                .Callback<IBlobContainer, string, Stream, string, CancellationToken>((_, path, stream, _, _) =>
                {
                    // Convert captured stream to string
                    stream.Seek(0L, SeekOrigin.Begin);
                    var tableJson = stream.ReadToEnd();

                    // Compare the captured table json upload with the response set up for the frontend service
                    Assert.Equal("{}", tableJson);
                })
                .Returns(Task.CompletedTask);

            var frontendService = new Mock<IFrontendService>(MockBehavior.Strict);

            frontendService.Setup(s => s.CreateUniversalTable(
                ItIs.DeepEqualTo(legacyPermalink),
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(new JObject());

            var permalinkCsvMetaService = new Mock<IPermalinkCsvMetaService>(MockBehavior.Strict);

            permalinkCsvMetaService
                .Setup(s => s
                    .GetCsvMeta(ItIs.DeepEqualTo(legacyPermalink),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(csvMeta);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    frontendService: frontendService.Object,
                    permalinkCsvMetaService: permalinkCsvMetaService.Object);

                var result = await service.MigratePermalink(permalink.Id);

                result.AssertRight();

                MockUtils.VerifyAllMocks(
                    blobStorageService,
                    frontendService,
                    permalinkCsvMetaService);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var saved = contentDbContext.Permalinks.Single(p => p.Id == permalink.Id);
                Assert.True(saved.LegacyHasSnapshot);
            }
        }

        [Fact]
        public async Task MigratePermalink_PermalinkNotFound()
        {
            var permalink = new Permalink
            {
                Legacy = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddRangeAsync(permalink);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext: contentDbContext);

                var result = await service.MigratePermalink(permalinkId: Guid.NewGuid());

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var saved = contentDbContext.Permalinks.Single(p => p.Id == permalink.Id);
                Assert.Null(saved.LegacyHasSnapshot);
            }
        }

        [Fact]
        public async Task MigratePermalink_BlobNotFound()
        {
            var permalink = new Permalink
            {
                Legacy = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddRangeAsync(permalink);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobTextNotFound(
                container: BlobContainers.Permalinks,
                path: permalink.Id.ToString());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.MigratePermalink(permalink.Id);

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task MigratePermalink_NotLegacyPermalink()
        {
            var permalink = new Permalink
            {
                Legacy = false
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddRangeAsync(permalink);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext: contentDbContext);

                var result = await service.MigratePermalink(permalink.Id);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var saved = contentDbContext.Permalinks.Single(p => p.Id == permalink.Id);
                Assert.Null(saved.LegacyHasSnapshot);
            }
        }

        [Fact]
        public async Task MigratePermalink_SnapshotAlreadyExists()
        {
            var permalink = new Permalink
            {
                Legacy = true,
                LegacyHasSnapshot = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddRangeAsync(permalink);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext: contentDbContext);

                var result = await service.MigratePermalink(permalink.Id);

                result.AssertBadRequest(ValidationErrorMessages.PermalinkSnapshotAlreadyExists);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var saved = contentDbContext.Permalinks.Single(p => p.Id == permalink.Id);
                Assert.True(saved.LegacyHasSnapshot);
            }
        }

        [Fact]
        public async Task MigratePermalink_FrontendServiceReturnsNotFound()
        {
            var permalink = new Permalink
            {
                Legacy = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddRangeAsync(permalink);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobText(
                container: BlobContainers.Permalinks,
                path: permalink.Id.ToString(),
                blobText: JsonConvert.SerializeObject(new LegacyPermalink(
                    permalink.Id,
                    DateTime.UtcNow,
                    new TableBuilderConfiguration(),
                    new PermalinkTableBuilderResult(),
                    new ObservationQueryContext())));

            var frontendService = new Mock<IFrontendService>(MockBehavior.Strict);

            frontendService.Setup(s => s.CreateUniversalTable(
                It.IsAny<LegacyPermalink>(),
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(new NotFoundResult());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    frontendService: frontendService.Object);

                var result = await service.MigratePermalink(permalink.Id);

                MockUtils.VerifyAllMocks(blobStorageService,
                    frontendService);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var saved = contentDbContext.Permalinks.Single(p => p.Id == permalink.Id);
                Assert.Null(saved.LegacyHasSnapshot);
            }
        }

        [Fact]
        public async Task MigratePermalink_FrontendServiceReturnsError()
        {
            var permalink = new Permalink
            {
                Legacy = true
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Permalinks.AddRangeAsync(permalink);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobText(
                container: BlobContainers.Permalinks,
                path: permalink.Id.ToString(),
                blobText: JsonConvert.SerializeObject(new LegacyPermalink(
                    permalink.Id,
                    DateTime.UtcNow,
                    new TableBuilderConfiguration(),
                    new PermalinkTableBuilderResult(),
                    new ObservationQueryContext())));

            var frontendService = new Mock<IFrontendService>(MockBehavior.Strict);

            frontendService.Setup(s => s.CreateUniversalTable(
                It.IsAny<LegacyPermalink>(),
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(new StatusCodeResult(StatusCodes.Status500InternalServerError));

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    frontendService: frontendService.Object);

                var result = await service.MigratePermalink(permalink.Id);

                MockUtils.VerifyAllMocks(blobStorageService,
                    frontendService);

                result.AssertInternalServerError();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var saved = contentDbContext.Permalinks.Single(p => p.Id == permalink.Id);
                Assert.Null(saved.LegacyHasSnapshot);
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
            IBlobStorageService? blobStorageService = null,
            IFrontendService? frontendService = null,
            IReleaseRepository? releaseRepository = null,
            ISubjectRepository? subjectRepository = null,
            IPublicationRepository? publicationRepository = null)
        {
            contentDbContext ??= InMemoryContentDbContext();

            return new(
                contentDbContext,
                tableBuilderService ?? Mock.Of<ITableBuilderService>(MockBehavior.Strict),
                permalinkCsvMetaService ?? Mock.Of<IPermalinkCsvMetaService>(MockBehavior.Strict),
                blobStorageService ?? Mock.Of<IBlobStorageService>(MockBehavior.Strict),
                frontendService ?? Mock.Of<IFrontendService>(MockBehavior.Strict),
                subjectRepository ?? Mock.Of<ISubjectRepository>(MockBehavior.Strict),
                publicationRepository ?? new PublicationRepository(contentDbContext),
                releaseRepository ?? Mock.Of<IReleaseRepository>(MockBehavior.Strict),
                MapperUtils.MapperForProfile<MappingProfiles>()
            );
        }
    }
}
