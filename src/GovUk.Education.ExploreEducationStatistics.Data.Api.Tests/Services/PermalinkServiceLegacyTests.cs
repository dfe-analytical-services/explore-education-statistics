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
using Snapshooter.Xunit;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using MapperUtils = GovUk.Education.ExploreEducationStatistics.Common.Services.MapperUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services;

/// <summary>
/// TODO EES-3755 Remove after Permalink snapshot migration work is complete
/// and remove __snapshots__/PermalinkServiceLegacyTests.*.snap files
/// </summary>
public class PermalinkServiceLegacyTests
{
    private readonly DataFixture _fixture = new();
    private readonly Guid _publicationId = Guid.NewGuid();

    [Fact]
    public async Task CreateLegacy_LatestPublishedReleaseForSubjectNotFound()
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
    public async Task CreateLegacy_UploadsPermalink_WithoutReleaseId()
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

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);
        var releaseRepository = new Mock<IReleaseRepository>(Strict);
        var subjectRepository = new Mock<ISubjectRepository>(Strict);
        var tableBuilderService = new Mock<ITableBuilderService>(Strict);

        // Permalink id is assigned on creation and used as the blob path
        // Capture it so we can compare it with the view model result
        Guid? expectedPermalinkId = null;
        var blobPathCapture = new CaptureMatch<string>(callback => expectedPermalinkId = Guid.Parse(callback));

        publicBlobStorageService.Setup(s => s.UploadStream(
                Permalinks,
                Capture.With(blobPathCapture),
                It.IsAny<Stream>(),
                MediaTypeNames.Application.Json,
                null,
                It.IsAny<CancellationToken>()
            ))
            .Callback<IBlobContainer, string, Stream, string, string, CancellationToken>((_, _, stream, _, _, _) =>
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
                publicBlobStorageService: publicBlobStorageService.Object,
                releaseRepository: releaseRepository.Object,
                subjectRepository: subjectRepository.Object,
                tableBuilderService: tableBuilderService.Object);

            var result = (await service.CreateLegacy(request)).AssertRight();

            MockUtils.VerifyAllMocks(
                publicBlobStorageService,
                releaseRepository,
                subjectRepository,
                tableBuilderService);

            Assert.Equal(expectedPermalinkId, result.Id);
            Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
            Assert.Equal(PermalinkStatus.Current, result.Status);
        }
    }

    [Fact]
    public async Task CreateLegacy_UploadsPermalink_WithReleaseId()
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

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);
        var tableBuilderService = new Mock<ITableBuilderService>(Strict);

        // Permalink id is assigned on creation and used as the blob path
        // Capture it so we can compare it with the view model result
        Guid? expectedPermalinkId = null;
        var blobPathCapture = new CaptureMatch<string>(callback => expectedPermalinkId = Guid.Parse(callback));

        publicBlobStorageService.Setup(s => s.UploadStream(
                Permalinks,
                Capture.With(blobPathCapture),
                It.IsAny<Stream>(),
                MediaTypeNames.Application.Json,
                null,
                It.IsAny<CancellationToken>()
            ))
            .Callback<IBlobContainer, string, Stream, string, string, CancellationToken>((_, _, stream, _, _, _) =>
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
                publicBlobStorageService: publicBlobStorageService.Object,
                tableBuilderService: tableBuilderService.Object);

            var result = (await service.CreateLegacy(release.Id, request)).AssertRight();

            MockUtils.VerifyAllMocks(
                publicBlobStorageService,
                tableBuilderService);

            Assert.Equal(expectedPermalinkId, result.Id);
            Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
            Assert.Equal(PermalinkStatus.Current, result.Status);
        }
    }

    [Fact]
    public async Task CreateLegacy_SavesPermalink()
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

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);
        var releaseRepository = new Mock<IReleaseRepository>(Strict);
        var subjectRepository = new Mock<ISubjectRepository>(Strict);
        var tableBuilderService = new Mock<ITableBuilderService>(Strict);

        // Permalink id is assigned on creation and used as the blob path
        // Capture it so we can compare it with the view model result
        Guid? expectedPermalinkId = null;
        var blobPathCapture = new CaptureMatch<string>(callback => expectedPermalinkId = Guid.Parse(callback));

        publicBlobStorageService.Setup(s => s.UploadStream(
                Permalinks,
                Capture.With(blobPathCapture),
                It.IsAny<Stream>(),
                MediaTypeNames.Application.Json,
                null,
                It.IsAny<CancellationToken>()
            ))
            .Callback<IBlobContainer, string, Stream, string, string, CancellationToken>((_, _, stream, _, _, _) =>
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
                publicBlobStorageService: publicBlobStorageService.Object,
                releaseRepository: releaseRepository.Object,
                subjectRepository: subjectRepository.Object,
                tableBuilderService: tableBuilderService.Object);

            var result = await service.CreateLegacy(request);
            result.AssertRight();

            MockUtils.VerifyAllMocks(
                publicBlobStorageService,
                releaseRepository,
                subjectRepository,
                tableBuilderService);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            // Verify details of the created permalink have been saved
            var permalink = contentDbContext.Permalinks.Single(permalink => permalink.Id == expectedPermalinkId);
            Assert.True(permalink.Legacy);
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
    public async Task GetLegacy()
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

        var permalink = new Permalink
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Legacy = true
        };

        var permalinkForSerialization = new LegacyPermalink(
            permalink.Id,
            permalink.Created,
            new TableBuilderConfiguration(),
            new PermalinkTableBuilderResult
            {
                SubjectMeta = new PermalinkResultSubjectMeta()
            },
            new ObservationQueryContext
            {
                SubjectId = subjectId
            });

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        publicBlobStorageService.SetupGetDeserializedJson(
            container: Permalinks,
            path: permalink.Id.ToString(),
            value: permalinkForSerialization,
            settings: PermalinkService.LegacyPermalinkSerializerSettings);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(permalink);
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
                publicBlobStorageService: publicBlobStorageService.Object);

            var result = (await service.GetLegacy(permalink.Id)).AssertRight();

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            Assert.Equal(permalink.Id, result.Id);
            Assert.Equal(permalink.Created, result.Created);
            Assert.Equal(PermalinkStatus.Current, result.Status);
        }
    }

    [Fact]
    public async Task GetLegacy_PermalinkNotFound()
    {
        var service = BuildService();
        var result = await service.GetLegacy(permalinkId: Guid.NewGuid());

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetLegacy_BlobNotFound()
    {
        var permalink = new Permalink
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Legacy = true
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(permalink);

            await contentDbContext.SaveChangesAsync();
        }

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        publicBlobStorageService.SetupGetDeserializedJsonNotFound<IPublicBlobStorageService, LegacyPermalink>(
            container: Permalinks,
            path: permalink.Id.ToString(),
            settings: PermalinkService.LegacyPermalinkSerializerSettings);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildService(contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object);

            var result = await service.GetLegacy(permalink.Id);

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task GetLegacy_SubjectNotFound()
    {
        var permalink = new Permalink
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Legacy = true
        };

        var permalinkForSerialization = new LegacyPermalink(
            permalink.Id,
            permalink.Created,
            new TableBuilderConfiguration(),
            new PermalinkTableBuilderResult
            {
                SubjectMeta = new PermalinkResultSubjectMeta()
            },
            new ObservationQueryContext
            {
                SubjectId = Guid.NewGuid()
            });

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        publicBlobStorageService.SetupGetDeserializedJson(
            container: Permalinks,
            path: permalink.Id.ToString(),
            value: permalinkForSerialization,
            settings: PermalinkService.LegacyPermalinkSerializerSettings);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(permalink);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildService(
                contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object);

            var result = (await service.GetLegacy(permalink.Id)).AssertRight();

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            Assert.Equal(permalink.Id, result.Id);
            Assert.Equal(PermalinkStatus.SubjectRemoved, result.Status);
        }
    }

    [Fact]
    public async Task GetLegacy_SubjectIsForMultipleVersions()
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

        var permalink = new Permalink
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Legacy = true
        };

        var permalinkForSerialization = new LegacyPermalink(
            permalink.Id,
            permalink.Created,
            new TableBuilderConfiguration(),
            new PermalinkTableBuilderResult
            {
                SubjectMeta = new PermalinkResultSubjectMeta()
            },
            new ObservationQueryContext
            {
                SubjectId = subjectId
            });

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        publicBlobStorageService.SetupGetDeserializedJson(
            container: Permalinks,
            path: permalink.Id.ToString(),
            value: permalinkForSerialization,
            settings: PermalinkService.LegacyPermalinkSerializerSettings);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(permalink);
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
                publicBlobStorageService: publicBlobStorageService.Object);

            var result = (await service.GetLegacy(permalink.Id)).AssertRight();

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            Assert.Equal(permalink.Id, result.Id);
            Assert.Equal(PermalinkStatus.Current, result.Status);
        }
    }

    [Fact]
    public async Task GetLegacy_SubjectIsNotForLatestRelease_OlderByYear()
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

        var permalink = new Permalink
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Legacy = true
        };

        var permalinkForSerialization = new LegacyPermalink(
            permalink.Id,
            permalink.Created,
            new TableBuilderConfiguration(),
            new PermalinkTableBuilderResult
            {
                SubjectMeta = new PermalinkResultSubjectMeta()
            },
            new ObservationQueryContext
            {
                SubjectId = subjectId
            });

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        publicBlobStorageService.SetupGetDeserializedJson(
            container: Permalinks,
            path: permalink.Id.ToString(),
            value: permalinkForSerialization,
            settings: PermalinkService.LegacyPermalinkSerializerSettings);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(permalink);
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
                publicBlobStorageService: publicBlobStorageService.Object);

            var result = (await service.GetLegacy(permalink.Id)).AssertRight();

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            Assert.Equal(permalink.Id, result.Id);
            Assert.Equal(PermalinkStatus.NotForLatestRelease, result.Status);
        }
    }

    [Fact]
    public async Task GetLegacy_SubjectIsNotForLatestRelease_OlderByTimePeriod()
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

        var permalink = new Permalink
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Legacy = true
        };

        var permalinkForSerialization = new LegacyPermalink(
            permalink.Id,
            permalink.Created,
            new TableBuilderConfiguration(),
            new PermalinkTableBuilderResult
            {
                SubjectMeta = new PermalinkResultSubjectMeta()
            },
            new ObservationQueryContext
            {
                SubjectId = subjectId
            });

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        publicBlobStorageService.SetupGetDeserializedJson(
            container: Permalinks,
            path: permalink.Id.ToString(),
            value: permalinkForSerialization,
            settings: PermalinkService.LegacyPermalinkSerializerSettings);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(permalink);
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
                publicBlobStorageService: publicBlobStorageService.Object);

            var result = (await service.GetLegacy(permalink.Id)).AssertRight();

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            Assert.Equal(permalink.Id, result.Id);
            Assert.Equal(PermalinkStatus.NotForLatestRelease, result.Status);
        }
    }

    [Fact]
    public async Task GetLegacy_SubjectIsNotForLatestVersion()
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

        var permalink = new Permalink
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Legacy = true
        };

        var permalinkForSerialization = new LegacyPermalink(
            permalink.Id,
            permalink.Created,
            new TableBuilderConfiguration(),
            new PermalinkTableBuilderResult
            {
                SubjectMeta = new PermalinkResultSubjectMeta()
            },
            new ObservationQueryContext
            {
                SubjectId = subjectId
            });

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        publicBlobStorageService.SetupGetDeserializedJson(
            container: Permalinks,
            path: permalink.Id.ToString(),
            value: permalinkForSerialization,
            settings: PermalinkService.LegacyPermalinkSerializerSettings);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(permalink);
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
                publicBlobStorageService: publicBlobStorageService.Object);

            var result = (await service.GetLegacy(permalink.Id)).AssertRight();

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            Assert.Equal(permalink.Id, result.Id);
            Assert.Equal(PermalinkStatus.SubjectReplacedOrRemoved, result.Status);
        }
    }

    [Fact]
    public async Task GetLegacy_SubjectIsFromSupersededPublication()
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

        var permalink = new Permalink
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Legacy = true
        };

        var permalinkForSerialization = new LegacyPermalink(
            permalink.Id,
            permalink.Created,
            new TableBuilderConfiguration(),
            new PermalinkTableBuilderResult
            {
                SubjectMeta = new PermalinkResultSubjectMeta()
            },
            new ObservationQueryContext
            {
                SubjectId = subjectId
            });

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        publicBlobStorageService.SetupGetDeserializedJson(
            container: Permalinks,
            path: permalink.Id.ToString(),
            value: permalinkForSerialization,
            settings: PermalinkService.LegacyPermalinkSerializerSettings);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(permalink);
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
                publicBlobStorageService: publicBlobStorageService.Object);

            var result = (await service.GetLegacy(permalink.Id)).AssertRight();

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            Assert.Equal(permalink.Id, result.Id);
            Assert.Equal(PermalinkStatus.PublicationSuperseded, result.Status);
        }
    }

    [Fact]
    public async Task LegacyDownloadCsvToStream()
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

        var permalink = new Permalink
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Legacy = true
        };

        var permalinkForSerialization = new LegacyPermalink
        {
            Id = permalink.Id,
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

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        publicBlobStorageService.SetupGetDeserializedJson(
            container: Permalinks,
            path: permalink.Id.ToString(),
            value: permalinkForSerialization,
            settings: PermalinkService.LegacyPermalinkSerializerSettings);

        var permalinkCsvMetaService = new Mock<IPermalinkCsvMetaService>(Strict);

        permalinkCsvMetaService
            .Setup(s => s
                .GetCsvMeta(
                    ItIs.DeepEqualTo(permalinkForSerialization),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(csvMeta);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(permalink);

            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            using var stream = new MemoryStream();

            var service = BuildService(
                contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object,
                permalinkCsvMetaService: permalinkCsvMetaService.Object
            );

            var result = await service.LegacyDownloadCsvToStream(permalink.Id, stream);

            MockUtils.VerifyAllMocks(publicBlobStorageService, permalinkCsvMetaService);

            result.AssertRight();

            stream.SeekToBeginning();
            var csv = stream.ReadToEnd();

            Snapshot.Match(csv);
        }
    }

    [Fact]
    public async Task LegacyDownloadCsvToStream_PermalinkHasNoLocationIds()
    {
        // Setup test data with observations that have no location id's
        // but have location objects instead to represent a historical permalink
        // with observations that had a location object prior to location id being added.

        // Setup the PermalinkCsvMetaService to return csv meta without locations
        // since rows should be written using the location object to get the location values instead

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

        var permalink = new Permalink
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Legacy = true
        };

        var permalinkForSerialization = new LegacyPermalink
        {
            Id = permalink.Id,
            Query = new ObservationQueryContext
            {
                SubjectId = subject.Id
            },
            FullTable = new PermalinkTableBuilderResult
            {
                // Build the observations WITHOUT location id's here and include locations instead
                Results = observations
                    .Select(observation =>
                        ObservationViewModelBuilderTestUtils.BuildObservationViewModelWithoutLocationId(
                            observation,
                            indicators))
                    .ToList()
            }
        };

        var csvMeta = new PermalinkCsvMetaViewModel
        {
            Filters = FiltersMetaViewModelBuilder.BuildCsvFiltersFromFilterItems(filterItems),
            Indicators = indicators
                .Select(i => new IndicatorCsvMetaViewModel(i))
                .ToDictionary(i => i.Name),
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
                "la_name",
                filters[0].Name,
                filters[1].Name,
                indicators[0].Name,
                indicators[1].Name,
                indicators[2].Name
            },
            // Locations are not included in the csv meta here as we expect them to come from the observations
            // instead
        };

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        publicBlobStorageService.SetupGetDeserializedJson(
            container: Permalinks,
            path: permalink.Id.ToString(),
            value: permalinkForSerialization,
            settings: PermalinkService.LegacyPermalinkSerializerSettings);

        var permalinkCsvMetaService = new Mock<IPermalinkCsvMetaService>(Strict);

        permalinkCsvMetaService
            .Setup(s => s
                .GetCsvMeta(
                    ItIs.DeepEqualTo(permalinkForSerialization),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(csvMeta);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(permalink);

            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            using var stream = new MemoryStream();

            var service = BuildService(
                contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object,
                permalinkCsvMetaService: permalinkCsvMetaService.Object
            );

            var result = await service.LegacyDownloadCsvToStream(permalink.Id, stream);

            MockUtils.VerifyAllMocks(publicBlobStorageService, permalinkCsvMetaService);

            result.AssertRight();

            stream.SeekToBeginning();
            var csv = stream.ReadToEnd();

            Snapshot.Match(csv);
        }
    }

    [Fact]
    public async Task LegacyDownloadCsvToStream_PermalinkNotFound()
    {
        var service = BuildService();
        var result = await service.LegacyDownloadCsvToStream(permalinkId: Guid.NewGuid(), new MemoryStream());

        result.AssertNotFound();
    }

    [Fact]
    public async Task LegacyDownloadCsvToStream_BlobNotFound()
    {
        var permalink = new Permalink
        {
            Id = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Legacy = true
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(permalink);

            await contentDbContext.SaveChangesAsync();
        }

        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

        publicBlobStorageService.SetupGetDeserializedJsonNotFound<IPublicBlobStorageService, LegacyPermalink>(
            container: Permalinks,
            path: permalink.Id.ToString(),
            settings: PermalinkService.LegacyPermalinkSerializerSettings);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildService(contentDbContext: contentDbContext,
                publicBlobStorageService: publicBlobStorageService.Object);

            var result = await service.LegacyDownloadCsvToStream(permalink.Id, new MemoryStream());

            MockUtils.VerifyAllMocks(publicBlobStorageService);

            result.AssertNotFound();
        }
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

        return new(
            contentDbContext,
            tableBuilderService ?? Mock.Of<ITableBuilderService>(Strict),
            permalinkCsvMetaService ?? Mock.Of<IPermalinkCsvMetaService>(Strict),
            publicBlobStorageService ?? Mock.Of<IPublicBlobStorageService>(Strict),
            frontendService ?? Mock.Of<IFrontendService>(Strict),
            subjectRepository ?? Mock.Of<ISubjectRepository>(Strict),
            publicationRepository ?? new PublicationRepository(contentDbContext),
            releaseRepository ?? Mock.Of<IReleaseRepository>(Strict),
            MapperUtils.MapperForProfile<MappingProfiles>()
        );
    }
}
