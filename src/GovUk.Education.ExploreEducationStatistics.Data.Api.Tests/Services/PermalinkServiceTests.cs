#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Converters;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Mappings;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using MapperUtils = GovUk.Education.ExploreEducationStatistics.Common.Services.MapperUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services
{
    public class PermalinkServiceTests
    {
        private readonly Guid _publicationId = Guid.NewGuid();

        [Fact]
        public async Task Create_LatestPublishedReleaseForSubjectNotFound()
        {
            var request = new PermalinkCreateViewModel
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

            var result = await service.Create(request);

            MockUtils.VerifyAllMocks(
                releaseRepository,
                subjectRepository);

            result.AssertNotFound();
        }

        [Fact]
        public async Task Create_WithoutReleaseId()
        {
            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var request = new PermalinkCreateViewModel
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
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
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
                SubjectMeta = new ResultSubjectMetaViewModel
                {
                    SubjectName = "Test data set",
                    PublicationName = "Test publication"
                }
            };

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var releaseRepository = new Mock<IReleaseRepository>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);
            var tableBuilderService = new Mock<ITableBuilderService>(MockBehavior.Strict);

            // Permalink id is assigned on creation and used as the blob path
            // Capture it so we can compare it with the view model result
            Guid? expectedPermalinkId = null;
            var blobPathCapture = new CaptureMatch<string>(callback => expectedPermalinkId = Guid.Parse(callback));

            blobStorageService.Setup(s => s.UploadAsJson(
                    Permalinks,
                    Capture.With(blobPathCapture),
                    It.Is<LegacyPermalink>(p =>
                        p.Configuration.Equals(request.Configuration) &&
                        p.FullTable.IsDeepEqualTo(new PermalinkTableBuilderResult(tableResult)) &&
                        p.Query.Equals(request.Query)),
                    It.IsAny<JsonSerializerSettings>()))
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

                var result = (await service.Create(request)).AssertRight();

                MockUtils.VerifyAllMocks(
                    blobStorageService,
                    releaseRepository,
                    subjectRepository,
                    tableBuilderService);

                Assert.Equal(expectedPermalinkId, result.Id);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
                Assert.Equal(PermalinkStatus.Current, result.Status);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var permalink = contentDbContext.Permalinks.Single(permalink => permalink.Id == expectedPermalinkId);
                Assert.InRange(DateTime.UtcNow.Subtract(permalink.Created).Milliseconds, 0, 1500);
                Assert.Equal("Test publication", permalink.PublicationTitle);
                Assert.Equal("Test data set", permalink.DataSetTitle);
                Assert.Equal(release.Id, permalink.ReleaseId);
                Assert.Equal(subject.Id, permalink.SubjectId);
            }
        }

        [Fact]
        public async Task Create_WithReleaseId()
        {
            var subjectId = Guid.NewGuid();

            var request = new PermalinkCreateViewModel
            {
                Query =
                {
                    SubjectId = subjectId
                }
            };

            var tableResult = new TableBuilderResultViewModel
            {
                SubjectMeta = new ResultSubjectMetaViewModel
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
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                Published = DateTime.UtcNow,
            };

            var publication = new Publication
            {
                Id = _publicationId,
                LatestPublishedRelease = release 
            };

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var tableBuilderService = new Mock<ITableBuilderService>(MockBehavior.Strict);

            // Permalink id is assigned on creation and used as the blob path
            // Capture it so we can compare it with the view model result
            Guid? expectedPermalinkId = null;
            var blobPathCapture = new CaptureMatch<string>(callback => expectedPermalinkId = Guid.Parse(callback));

            blobStorageService.Setup(s => s.UploadAsJson(
                    Permalinks,
                    Capture.With(blobPathCapture),
                    It.Is<LegacyPermalink>(p =>
                        p.Configuration.Equals(request.Configuration) &&
                        p.FullTable.IsDeepEqualTo(new PermalinkTableBuilderResult(tableResult)) &&
                        p.Query.Equals(request.Query)),
                    It.IsAny<JsonSerializerSettings>()))
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

                var result = (await service.Create(release.Id, request)).AssertRight();

                MockUtils.VerifyAllMocks(
                    blobStorageService,
                    tableBuilderService);

                Assert.Equal(expectedPermalinkId, result.Id);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
                Assert.Equal(PermalinkStatus.Current, result.Status);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var permalink = contentDbContext.Permalinks.Single(permalink => permalink.Id == expectedPermalinkId);
                Assert.InRange(DateTime.UtcNow.Subtract(permalink.Created).Milliseconds, 0, 1500);
                Assert.Equal("Test publication", permalink.PublicationTitle);
                Assert.Equal("Test data set", permalink.DataSetTitle);
                Assert.Equal(release.Id, permalink.ReleaseId);
                Assert.Equal(subjectId, permalink.SubjectId);
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
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

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

                var result = (await service.Get(permalink.Id)).AssertRight();

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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

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

                var result = (await service.Get(permalink.Id)).AssertRight();

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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobTextNotFound(
                container: Permalinks,
                path: permalinkId.ToString());

            var service = BuildService(blobStorageService: blobStorageService.Object);
            var result = await service.Get(permalinkId);

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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

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

                var result = (await service.Get(permalink.Id)).AssertRight();

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
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                Published = DateTime.UtcNow,
                PreviousVersionId = null
            };

            var latestVersion = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

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

                var result = (await service.Get(permalink.Id)).AssertRight();

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
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                Published = DateTime.UtcNow,
            };

            var latestRelease = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

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

                var result = (await service.Get(permalink.Id)).AssertRight();

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
                TimePeriodCoverage = TimeIdentifier.January,
                Published = DateTime.UtcNow,
            };

            var latestRelease = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.February,
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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

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

                var result = (await service.Get(permalink.Id)).AssertRight();

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
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                Published = DateTime.UtcNow,
                PreviousVersionId = null
            };

            var latestVersion = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = _publicationId,
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

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

                var result = (await service.Get(permalink.Id)).AssertRight();

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
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
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

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

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

                var result = (await service.Get(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(PermalinkStatus.PublicationSuperseded, result.Status);
            }
        }

        private static PermalinkService BuildService(
            ContentDbContext? contentDbContext = null,
            ITableBuilderService? tableBuilderService = null,
            IBlobStorageService? blobStorageService = null,
            IReleaseRepository? releaseRepository = null,
            ISubjectRepository? subjectRepository = null,
            IPublicationRepository? publicationRepository = null)
        {
            contentDbContext ??= InMemoryContentDbContext();

            return new(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                tableBuilderService ?? Mock.Of<ITableBuilderService>(MockBehavior.Strict),
                blobStorageService ?? Mock.Of<IBlobStorageService>(MockBehavior.Strict),
                subjectRepository ?? Mock.Of<ISubjectRepository>(MockBehavior.Strict),
                publicationRepository ?? new PublicationRepository(contentDbContext),
                releaseRepository ?? Mock.Of<IReleaseRepository>(MockBehavior.Strict),
                MapperUtils.MapperForProfile<MappingProfiles>()
            );
        }
    }
}
