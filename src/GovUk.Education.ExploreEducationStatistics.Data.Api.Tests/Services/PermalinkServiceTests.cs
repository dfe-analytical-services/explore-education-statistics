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
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Converters;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Mappings;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
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
using Snapshooter.Xunit;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using MapperUtils = GovUk.Education.ExploreEducationStatistics.Common.Services.MapperUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services
{
    public class PermalinkServiceTests
    {
        private readonly DataFixture _fixture = new();
        private readonly Guid _publicationId = Guid.NewGuid();

        [Fact]
        public async Task Create_LatestPublishedReleaseForSubjectNotFound()
        {
            var request = new PermalinkCreateRequest
            {
                Query =
                {
                    SubjectId = Guid.NewGuid()
                }
            };

            var releaseRepository = new Mock<IReleaseRepository>(Strict);
            var subjectRepository = new Mock<ISubjectRepository>(Strict);

            releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(_publicationId))
                .ReturnsAsync(new NotFoundResult());

            subjectRepository
                .Setup(s => s.FindPublicationIdForSubject(request.Query.SubjectId))
                .ReturnsAsync(_publicationId);

            var service = BuildService(releaseRepository: releaseRepository.Object,
                subjectRepository: subjectRepository.Object);

            var result = await service.CreateLegacy(request);

            MockUtils.VerifyAllMocks(
                releaseRepository,
                subjectRepository);

            result.AssertNotFound();
        }

        [Fact]
        public async Task Create_UploadsPermalink_WithoutReleaseId()
        {
            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var request = new PermalinkCreateRequest
            {
                Query =
                {
                    SubjectId = subject.Id
                }
            };

            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                TimePeriodCoverage = AcademicYear,
                ReleaseName = "2000",
                Published = DateTime.UtcNow,
            };

            var publication = new Publication
            {
                Id = _publicationId,
                LatestPublishedRelease = release
            };

            var tableResult = new TableBuilderResultViewModel
            {
                SubjectMeta = new SubjectResultMetaViewModel
                {
                    SubjectName = "Test data set",
                    PublicationName = "Test publication"
                }
            };

            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var releaseRepository = new Mock<IReleaseRepository>(Strict);
            var subjectRepository = new Mock<ISubjectRepository>(Strict);
            var tableBuilderService = new Mock<ITableBuilderService>(Strict);

            // Permalink id is assigned on creation and used as the blob path
            // Capture it so we can compare it with the view model result
            Guid? expectedPermalinkId = null;
            var blobPathCapture = new CaptureMatch<string>(callback => expectedPermalinkId = Guid.Parse(callback));

            blobStorageService.Setup(s => s.UploadStream(
                    Permalinks,
                    Capture.With(blobPathCapture),
                    It.IsAny<Stream>(),
                    MediaTypeNames.Application.Json
                ))
                .Callback<IBlobContainer, string, Stream, string>((_, _, stream, _) =>
                {
                    // Convert captured stream to string
                    using var reader = new StreamReader(stream);
                    var content = reader.ReadToEnd();

                    // Deserialize captured JSON and compare with attributes expected of the permalink
                    var uploadedPermalink = JsonConvert.DeserializeObject<LegacyPermalink>(content);
                    Assert.NotNull(uploadedPermalink);
                    request.Configuration.AssertDeepEqualTo(uploadedPermalink.Configuration);
                    new PermalinkTableBuilderResult(tableResult).AssertDeepEqualTo(uploadedPermalink.FullTable);
                    request.Query.AssertDeepEqualTo(uploadedPermalink.Query);
                })
                .Returns(Task.CompletedTask);

            releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(_publicationId))
                .ReturnsAsync(release);

            subjectRepository
                .Setup(s => s.FindPublicationIdForSubject(subject.Id))
                .ReturnsAsync(_publicationId);

            tableBuilderService
                .Setup(s => s.Query(release.Id, request.Query, CancellationToken.None))
                .ReturnsAsync(tableResult);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        File = new File
                        {
                            SubjectId = subject.Id,
                            Type = FileType.Data,
                        },
                    });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    releaseRepository: releaseRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    tableBuilderService: tableBuilderService.Object);

                var result = (await service.CreateLegacy(request)).AssertRight();

                MockUtils.VerifyAllMocks(
                    blobStorageService,
                    releaseRepository,
                    subjectRepository,
                    tableBuilderService);

                Assert.Equal(expectedPermalinkId, result.Id);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
                Assert.Equal(PermalinkStatus.Current, result.Status);
            }
        }

        [Fact]
        public async Task Create_UploadsPermalink_WithReleaseId()
        {
            var subjectId = Guid.NewGuid();

            var request = new PermalinkCreateRequest
            {
                Query =
                {
                    SubjectId = subjectId
                }
            };

            var tableResult = new TableBuilderResultViewModel
            {
                SubjectMeta = new SubjectResultMetaViewModel
                {
                    SubjectName = "Test data set",
                    PublicationName = "Test publication"
                }
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

            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var tableBuilderService = new Mock<ITableBuilderService>(Strict);

            // Permalink id is assigned on creation and used as the blob path
            // Capture it so we can compare it with the view model result
            Guid? expectedPermalinkId = null;
            var blobPathCapture = new CaptureMatch<string>(callback => expectedPermalinkId = Guid.Parse(callback));

            blobStorageService.Setup(s => s.UploadStream(
                    Permalinks,
                    Capture.With(blobPathCapture),
                    It.IsAny<Stream>(),
                    MediaTypeNames.Application.Json
                ))
                .Callback<IBlobContainer, string, Stream, string>((_, _, stream, _) =>
                {
                    // Convert captured stream to string
                    using var reader = new StreamReader(stream);
                    var content = reader.ReadToEnd();

                    // Deserialize captured JSON and compare with attributes expected of the permalink
                    var uploadedPermalink = JsonConvert.DeserializeObject<LegacyPermalink>(content);
                    Assert.NotNull(uploadedPermalink);
                    request.Configuration.AssertDeepEqualTo(uploadedPermalink.Configuration);
                    new PermalinkTableBuilderResult(tableResult).AssertDeepEqualTo(uploadedPermalink.FullTable);
                    request.Query.AssertDeepEqualTo(uploadedPermalink.Query);
                })
                .Returns(Task.CompletedTask);

            tableBuilderService
                .Setup(s => s.Query(release.Id, request.Query, CancellationToken.None))
                .ReturnsAsync(tableResult);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        File = new File
                        {
                            SubjectId = subjectId,
                            Type = FileType.Data,
                        },
                    });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    tableBuilderService: tableBuilderService.Object);

                var result = (await service.CreateLegacy(release.Id, request)).AssertRight();

                MockUtils.VerifyAllMocks(
                    blobStorageService,
                    tableBuilderService);

                Assert.Equal(expectedPermalinkId, result.Id);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
                Assert.Equal(PermalinkStatus.Current, result.Status);
            }
        }

        [Fact]
        public async Task Create_SavesPermalink()
        {
            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var request = new PermalinkCreateRequest
            {
                Configuration =
                {
                    TableHeaders = new TableHeaders()
                },
                Query =
                {
                    SubjectId = subject.Id
                }
            };

            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                TimePeriodCoverage = AcademicYear,
                ReleaseName = "2000",
                Published = DateTime.UtcNow,
            };

            var publication = new Publication
            {
                Id = _publicationId,
                LatestPublishedRelease = release
            };

            var tableResult = new TableBuilderResultViewModel
            {
                SubjectMeta = new SubjectResultMetaViewModel
                {
                    SubjectName = "Test data set",
                    PublicationName = "Test publication",
                    Filters = new Dictionary<string, FilterMetaViewModel>
                    {
                        {
                            "filter1", new FilterMetaViewModel
                            {
                                Options = new Dictionary<string, FilterGroupMetaViewModel>
                                {
                                    {
                                        "option1", new FilterGroupMetaViewModel
                                        {
                                            Options = new List<FilterItemMetaViewModel>
                                            {
                                                new("label", Guid.NewGuid())
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    Footnotes = new List<FootnoteViewModel>
                    {
                        new(Guid.NewGuid(), "footnote 1"),
                        new(Guid.NewGuid(), "footnote 2")
                    },
                    Indicators = new List<IndicatorMetaViewModel>
                    {
                        new()
                        {
                            Label = "A label",
                            Name = "A name",
                            Value = Guid.NewGuid()
                        },
                        new()
                        {
                            Label = "A label",
                            Name = "A name",
                            Value = Guid.NewGuid()
                        },
                        new()
                        {
                        Label = "A label",
                        Name = "A name",
                        Value = Guid.NewGuid()
                        }
                    },
                    Locations = new Dictionary<string, List<LocationAttributeViewModel>>
                    {
                        {
                            "localAuthority", new List<LocationAttributeViewModel>
                            {
                                new()
                                {
                                    Label = "North East",
                                    Value = "E12000001",
                                    Level = GeographicLevel.Region,
                                    Options = new List<LocationAttributeViewModel>
                                    {
                                        new()
                                        {
                                            Id = Guid.NewGuid(),
                                            Label = "Newcastle upon Tyne",
                                            Value = "E08000021"
                                        }
                                    }
                                }
                            }
                        },
                        {
                            "country", new List<LocationAttributeViewModel>
                            {
                                new()
                                {
                                    Label = "England",
                                    Value = "E92000001"
                                }
                            }
                        }
                    },
                    TimePeriodRange = new List<TimePeriodMetaViewModel>
                    {
                        new(2017, AcademicYear)
                        {
                            Label = "2017/18"
                        }
                    }
                },
                Results = new List<ObservationViewModel>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Filters = new List<Guid>
                        {
                            Guid.NewGuid()
                        },
                        LocationId = Guid.NewGuid(),
                        Measures = new Dictionary<Guid, string>
                        {
                            { Guid.NewGuid(), "value" }
                        },
                        GeographicLevel = GeographicLevel.Country,
                        TimePeriod = "2017/18"
                    }
                }
            };

            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var releaseRepository = new Mock<IReleaseRepository>(Strict);
            var subjectRepository = new Mock<ISubjectRepository>(Strict);
            var tableBuilderService = new Mock<ITableBuilderService>(Strict);

            // Permalink id is assigned on creation and used as the blob path
            // Capture it so we can compare it with the view model result
            Guid? expectedPermalinkId = null;
            var blobPathCapture = new CaptureMatch<string>(callback => expectedPermalinkId = Guid.Parse(callback));

            blobStorageService.Setup(s => s.UploadStream(
                    Permalinks,
                    Capture.With(blobPathCapture),
                    It.IsAny<Stream>(),
                    MediaTypeNames.Application.Json
                ))
                .Callback<IBlobContainer, string, Stream, string>((_, _, stream, _) =>
                {
                    // Convert captured stream to string
                    using var reader = new StreamReader(stream);
                    var content = reader.ReadToEnd();

                    // Deserialize captured JSON and compare with attributes expected of the permalink
                    var uploadedPermalink = JsonConvert.DeserializeObject<LegacyPermalink>(content);
                    Assert.NotNull(uploadedPermalink);
                    request.Configuration.AssertDeepEqualTo(uploadedPermalink.Configuration);
                    new PermalinkTableBuilderResult(tableResult).AssertDeepEqualTo(uploadedPermalink.FullTable);
                    request.Query.AssertDeepEqualTo(uploadedPermalink.Query);
                })
                .Returns(Task.CompletedTask);

            releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(_publicationId))
                .ReturnsAsync(release);

            subjectRepository
                .Setup(s => s.FindPublicationIdForSubject(subject.Id))
                .ReturnsAsync(_publicationId);

            tableBuilderService
                .Setup(s => s.Query(release.Id, request.Query, CancellationToken.None))
                .ReturnsAsync(tableResult);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        File = new File
                        {
                            SubjectId = subject.Id,
                            Type = FileType.Data,
                        },
                    });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    releaseRepository: releaseRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    tableBuilderService: tableBuilderService.Object);

                var result = await service.CreateLegacy(request);
                result.AssertRight();

                MockUtils.VerifyAllMocks(
                    blobStorageService,
                    releaseRepository,
                    subjectRepository,
                    tableBuilderService);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                // Verify details of the created permalink have been saved
                var permalink = contentDbContext.Permalinks.Single(permalink => permalink.Id == expectedPermalinkId);
                Assert.InRange(DateTime.UtcNow.Subtract(permalink.Created).Milliseconds, 0, 1500);
                Assert.Equal("Test publication", permalink.PublicationTitle);
                Assert.Equal("Test data set", permalink.DataSetTitle);
                Assert.Equal(release.Id, permalink.ReleaseId);
                Assert.Equal(subject.Id, permalink.SubjectId);

                // Content length should be set
                Assert.True(permalink.LegacyContentLength > 0);

                // Statistics about the permalink should be set
                Assert.Equal(1, permalink.CountFilterItems);
                Assert.Equal(2, permalink.CountFootnotes);
                Assert.Equal(3, permalink.CountIndicators);
                Assert.Equal(2, permalink.CountLocations);
                Assert.Equal(1, permalink.CountObservations);
                Assert.Equal(1, permalink.CountTimePeriods);
                Assert.True(permalink.LegacyHasConfigurationHeaders);
            }
        }

        [Fact]
        public async Task Get()
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

            var subjectId = Guid.NewGuid();

            var permalink = new LegacyPermalink(
                Guid.NewGuid(),
                DateTime.UtcNow,
                new TableBuilderConfiguration(),
                new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta()
                },
                new ObservationQueryContext
                {
                    SubjectId = subjectId
                });

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.SetupDownloadBlobText(
                container: Permalinks,
                path: permalink.Id.ToString(),
                blobText: JsonConvert.SerializeObject(permalink));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        File = new File
                        {
                            SubjectId = subjectId,
                            Type = FileType.Data,
                        },
                    });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = (await service.GetLegacy(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(permalink.Created, result.Created);
                Assert.Equal(PermalinkStatus.Current, result.Status);
            }
        }

        [Fact]
        public async Task Get_LegacyLocationsFieldIsTransformed()
        {
            var releaseId = Guid.NewGuid();

            // Until old Permalinks are migrated to permanently transform their legacy 'Locations' field,
            // test that legacy locations are transformed to 'LocationsHierarchical' and then mapped to 'Locations'
            // in the view model.

            var subjectId = Guid.NewGuid();

            // Setup a list of legacy locations to be added to the Permalink table subject meta during serialization
            var legacyLocations = new List<LegacyLocationAttributeViewModel>
            {
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    Label = "Blackpool",
                    Value = "E06000009",
                    GeoJson = JToken.Parse(@"[{""properties"": {""code"": ""E06000009""}}]")
                },
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    Label = "Derby",
                    Value = "E06000015",
                    GeoJson = JToken.Parse(@"[{""properties"": {""code"": ""E06000015""}}]")
                },
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    Label = "Nottingham",
                    Value = "E06000018",
                    GeoJson = JToken.Parse(@"[{""properties"": {""code"": ""E06000018""}}]")
                }
            };

            var permalink = new LegacyPermalink(
                Guid.NewGuid(),
                DateTime.UtcNow,
                new TableBuilderConfiguration(),
                new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta()
                },
                new ObservationQueryContext
                {
                    SubjectId = subjectId
                });

            // Set the legacy locations field on the Permalink table subject meta
            var permalinkJsonObject = JObject.FromObject(permalink);
            var subjectMetaJsonObject = permalinkJsonObject.SelectToken("FullTable.SubjectMeta") as JObject;
            var legacyLocationsJsonArray = JArray.FromObject(legacyLocations);
            subjectMetaJsonObject!.Add("Locations", legacyLocationsJsonArray);

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.SetupDownloadBlobText(
                container: Permalinks,
                path: permalink.Id.ToString(),
                blobText: permalinkJsonObject.ToString(Formatting.None));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    new ReleaseFile
                    {
                        ReleaseId = releaseId,
                        File = new File
                        {
                            SubjectId = subjectId,
                            Type = FileType.Data,
                        }
                    });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = (await service.GetLegacy(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.Equal(permalink.Id, result.Id);

                var subjectMeta = result.FullTable.SubjectMeta;

                // Expect Locations to have been transformed
                Assert.Single(subjectMeta.Locations);
                Assert.True(subjectMeta.Locations.ContainsKey("localAuthority"));

                var localAuthorities = subjectMeta.Locations["localAuthority"];
                Assert.Equal(3, localAuthorities.Count);

                Assert.Equal(legacyLocations[0].Label, localAuthorities[0].Label);
                Assert.Equal(legacyLocations[0].Value, localAuthorities[0].Value);
                Assert.Equal(legacyLocations[0].GeoJson, localAuthorities[0].GeoJson);

                Assert.Equal(legacyLocations[1].Label, localAuthorities[1].Label);
                Assert.Equal(legacyLocations[1].Value, localAuthorities[1].Value);
                Assert.Equal(legacyLocations[1].GeoJson, localAuthorities[1].GeoJson);

                Assert.Equal(legacyLocations[2].Label, localAuthorities[2].Label);
                Assert.Equal(legacyLocations[2].Value, localAuthorities[2].Value);
                Assert.Equal(legacyLocations[2].GeoJson, localAuthorities[2].GeoJson);
            }
        }

        [Fact]
        public async Task Get_PermalinkNotFound()
        {
            var permalinkId = Guid.NewGuid();

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.SetupDownloadBlobTextNotFound(
                container: Permalinks,
                path: permalinkId.ToString());

            var service = BuildService(blobStorageService: blobStorageService.Object);
            var result = await service.GetLegacy(permalinkId);

            MockUtils.VerifyAllMocks(
                blobStorageService);

            result.AssertNotFound();
        }

        [Fact]
        public async Task Get_SubjectNotFound()
        {
            var permalink = new LegacyPermalink(
                Guid.NewGuid(),
                DateTime.UtcNow,
                new TableBuilderConfiguration(),
                new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta()
                },
                new ObservationQueryContext
                {
                    SubjectId = Guid.NewGuid()
                });

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.SetupDownloadBlobText(
                container: Permalinks,
                path: permalink.Id.ToString(),
                blobText: JsonConvert.SerializeObject(permalink));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = (await service.GetLegacy(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(PermalinkStatus.SubjectRemoved, result.Status);
            }
        }

        [Fact]
        public async Task Get_SubjectIsForMultipleVersions()
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

            var subjectId = Guid.NewGuid();

            var permalink = new LegacyPermalink(
                Guid.NewGuid(),
                DateTime.UtcNow,
                new TableBuilderConfiguration(),
                new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta()
                },
                new ObservationQueryContext
                {
                    SubjectId = subjectId
                });

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.SetupDownloadBlobText(
                container: Permalinks,
                path: permalink.Id.ToString(),
                blobText: JsonConvert.SerializeObject(permalink));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddRangeAsync(previousVersion, latestVersion);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = previousVersion,
                        File = new File
                        {
                            SubjectId = subjectId,
                            Type = FileType.Data,
                        }
                    },
                    new ReleaseFile
                    {
                        Release = latestVersion,
                        File = new File
                        {
                            SubjectId = subjectId,
                            Type = FileType.Data,
                        }
                    });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = (await service.GetLegacy(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(PermalinkStatus.Current, result.Status);
            }
        }

        [Fact]
        public async Task Get_SubjectIsNotForLatestRelease_OlderByYear()
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

            var subjectId = Guid.NewGuid();

            var permalink = new LegacyPermalink(
                Guid.NewGuid(),
                DateTime.UtcNow,
                new TableBuilderConfiguration(),
                new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta()
                },
                new ObservationQueryContext
                {
                    SubjectId = subjectId
                });

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.SetupDownloadBlobText(
                container: Permalinks,
                path: permalink.Id.ToString(),
                blobText: JsonConvert.SerializeObject(permalink));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddRangeAsync(release, latestRelease);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        File = new File
                        {
                            SubjectId = subjectId,
                            Type = FileType.Data,
                        }
                    });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = (await service.GetLegacy(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(PermalinkStatus.NotForLatestRelease, result.Status);
            }
        }

        [Fact]
        public async Task Get_SubjectIsNotForLatestRelease_OlderByTimePeriod()
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

            var subjectId = Guid.NewGuid();

            var permalink = new LegacyPermalink(
                Guid.NewGuid(),
                DateTime.UtcNow,
                new TableBuilderConfiguration(),
                new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta()
                },
                new ObservationQueryContext
                {
                    SubjectId = subjectId
                });

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.SetupDownloadBlobText(
                container: Permalinks,
                path: permalink.Id.ToString(),
                blobText: JsonConvert.SerializeObject(permalink));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddRangeAsync(release, latestRelease);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        File = new File
                        {
                            SubjectId = subjectId,
                            Type = FileType.Data,
                        }
                    });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = (await service.GetLegacy(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(PermalinkStatus.NotForLatestRelease, result.Status);
            }
        }

        [Fact]
        public async Task Get_SubjectIsNotForLatestVersion()
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

            var subjectId = Guid.NewGuid();

            var permalink = new LegacyPermalink(
                Guid.NewGuid(),
                DateTime.UtcNow,
                new TableBuilderConfiguration(),
                new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta()
                },
                new ObservationQueryContext
                {
                    SubjectId = subjectId
                });

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.SetupDownloadBlobText(
                container: Permalinks,
                path: permalink.Id.ToString(),
                blobText: JsonConvert.SerializeObject(permalink));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddRangeAsync(previousVersion, latestVersion);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = previousVersion,
                        File = new File
                        {
                            SubjectId = subjectId,
                            Type = FileType.Data,
                        }
                    });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = (await service.GetLegacy(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(PermalinkStatus.SubjectReplacedOrRemoved, result.Status);
            }
        }

        [Fact]
        public async Task Get_SubjectIsFromSupersededPublication()
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

            var subjectId = Guid.NewGuid();

            var permalink = new LegacyPermalink(
                Guid.NewGuid(),
                DateTime.UtcNow,
                new TableBuilderConfiguration(),
                new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta()
                },
                new ObservationQueryContext
                {
                    SubjectId = subjectId
                });

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.SetupDownloadBlobText(
                container: Permalinks,
                path: permalink.Id.ToString(),
                blobText: JsonConvert.SerializeObject(permalink));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(
                    new ReleaseFile
                    {
                        Release = release,
                        File = new File
                        {
                            SubjectId = subjectId,
                            Type = FileType.Data,
                        }
                    });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = (await service.GetLegacy(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(PermalinkStatus.PublicationSuperseded, result.Status);
            }
        }

        [Fact]
        public async Task DownloadCsvToStream()
        {
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
                .GenerateArray(4);

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
                .ForRange(..2, l => l.SetPresetRegion())
                .ForRange(2..4, l => l.SetPresetRegionAndLocalAuthority())
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

            var permalink = new LegacyPermalink
            {
                Id = Guid.NewGuid(),
                Query = new ObservationQueryContext
                {
                    SubjectId = subject.Id
                },
                FullTable = new PermalinkTableBuilderResult
                {
                    Results = observations
                        .Select(o =>
                            ObservationViewModelBuilder.BuildObservation(o, indicators.Select(i => i.Id)))
                        .ToList()
                }
            };

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

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.SetupDownloadBlobText(
                container: Permalinks,
                path: permalink.Id.ToString(),
                blobText: JsonConvert.SerializeObject(permalink));

            var permalinkCsvMetaService = new Mock<IPermalinkCsvMetaService>(Strict);

            permalinkCsvMetaService
                .Setup(s => s
                    .GetCsvMeta(
                        It.Is<LegacyPermalink>(p => p.IsDeepEqualTo(permalink)),
                        default))
                .ReturnsAsync(csvMeta);

            var service = BuildService(
                blobStorageService: blobStorageService.Object,
                permalinkCsvMetaService: permalinkCsvMetaService.Object
            );

            using var stream = new MemoryStream();

            var result = await service.LegacyDownloadCsvToStream(permalink.Id, stream);

            MockUtils.VerifyAllMocks(blobStorageService, permalinkCsvMetaService);

            result.AssertRight();

            stream.Seek(0L, SeekOrigin.Begin);
            var csv = stream.ReadToEnd();

            Snapshot.Match(csv);
        }

        [Fact]
        public async Task DownloadCsvToStream_BlobNotFound()
        {
            var permalinkId = Guid.NewGuid();

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.SetupDownloadBlobTextNotFound(
                container: Permalinks,
                path: permalinkId.ToString());

            var service = BuildService(blobStorageService: blobStorageService.Object);
            var result = await service.LegacyDownloadCsvToStream(permalinkId, new MemoryStream());

            MockUtils.VerifyAllMocks(blobStorageService);

            result.AssertNotFound();
        }

        private static PermalinkService BuildService(
            ContentDbContext? contentDbContext = null,
            ITableBuilderService? tableBuilderService = null,
            IPermalinkCsvMetaService? permalinkCsvMetaService = null,
            IBlobStorageService? blobStorageService = null,
            IReleaseRepository? releaseRepository = null,
            ISubjectRepository? subjectRepository = null,
            IPublicationRepository? publicationRepository = null)
        {
            contentDbContext ??= InMemoryContentDbContext();

            return new(
                contentDbContext,
                tableBuilderService ?? Mock.Of<ITableBuilderService>(Strict),
                permalinkCsvMetaService ?? Mock.Of<IPermalinkCsvMetaService>(Strict),
                blobStorageService ?? Mock.Of<IBlobStorageService>(Strict),
                subjectRepository ?? Mock.Of<ISubjectRepository>(Strict),
                publicationRepository ?? new PublicationRepository(contentDbContext),
                releaseRepository ?? Mock.Of<IReleaseRepository>(Strict),
                MapperUtils.MapperForProfile<MappingProfiles>()
            );
        }
    }
}
