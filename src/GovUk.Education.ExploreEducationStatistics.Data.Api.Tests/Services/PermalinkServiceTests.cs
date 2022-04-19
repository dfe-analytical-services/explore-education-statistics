#nullable enable
using System;
using System.Collections.Generic;
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
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using MapperUtils = GovUk.Education.ExploreEducationStatistics.Common.Services.MapperUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;

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
                .Returns((Release?)null);

            subjectRepository
                .Setup(s => s.GetPublicationIdForSubject(request.Query.SubjectId))
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

            var contentRelease = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                Publication = new Publication { Id = _publicationId },
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                ReleaseName = "2000",
                Published = DateTime.UtcNow,
            };

            var tableResult = new TableBuilderResultViewModel
            {
                SubjectMeta = new ResultSubjectMetaViewModel()
            };

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var releaseRepository = new Mock<IReleaseRepository>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);
            var tableBuilderService = new Mock<ITableBuilderService>(MockBehavior.Strict);

            // Permalink id is assigned on creation and used as the blob path
            // Capture it so we can compare it with the view model result
            string blobPath = string.Empty;
            var blobPathCapture = new CaptureMatch<string>(callback => blobPath = callback);

            blobStorageService.Setup(s => s.UploadAsJson(
                    Permalinks,
                    Capture.With(blobPathCapture),
                    It.Is<Permalink>(p =>
                        p.Configuration.Equals(request.Configuration) &&
                        p.FullTable.IsDeepEqualTo(new PermalinkTableBuilderResult(tableResult)) &&
                        p.Query.Equals(request.Query)),
                    It.IsAny<JsonSerializerSettings>()))
                .Returns(Task.CompletedTask);

           releaseRepository
               .Setup(s => s.GetLatestPublishedRelease(_publicationId))
               .Returns(new Release
               {
                   Id = contentRelease.Id,
                   PublicationId = _publicationId,
                   TimeIdentifier = TimeIdentifier.AcademicYear,
                   Year = 2000,
               });


            subjectRepository
                .Setup(s => s.Get(subject.Id))
                .ReturnsAsync(subject);

            subjectRepository
                .Setup(s => s.GetPublicationIdForSubject(subject.Id))
                .ReturnsAsync(_publicationId);

            tableBuilderService
                .Setup(s => s.Query(contentRelease.Id, request.Query, CancellationToken.None))
                .ReturnsAsync(tableResult);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    contentRelease,
                    new ReleaseFile
                    {
                        Release = contentRelease,
                        File = new File
                        {
                            SubjectId = subject.Id,
                            Type = FileType.Data,
                        },
                    });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    releaseRepository: releaseRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    tableBuilderService: tableBuilderService.Object);

                var result = await service.Create(request);
                var permalinkViewModel = result.AssertRight();

                MockUtils.VerifyAllMocks(
                    blobStorageService,
                    releaseRepository,
                    subjectRepository,
                    tableBuilderService);

                Assert.Equal(Guid.Parse(blobPath), permalinkViewModel.Id);
                Assert.InRange(DateTime.UtcNow.Subtract(permalinkViewModel.Created).Milliseconds, 0, 1500);
                Assert.Equal(PermalinkStatus.Current, permalinkViewModel.Status);
            }
        }

        [Fact]
        public async Task Create_WithReleaseId()
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

            var tableResult = new TableBuilderResultViewModel
            {
                SubjectMeta = new ResultSubjectMetaViewModel()
            };

            var release = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                Publication = new Publication { Id = _publicationId },
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                Published = DateTime.UtcNow,
            };

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);
            var tableBuilderService = new Mock<ITableBuilderService>(MockBehavior.Strict);

            // Permalink id is assigned on creation and used as the blob path
            // Capture it so we can compare it with the view model result
            string blobPath = string.Empty;
            var blobPathCapture = new CaptureMatch<string>(callback => blobPath = callback);

            blobStorageService.Setup(s => s.UploadAsJson(
                    Permalinks,
                    Capture.With(blobPathCapture),
                    It.Is<Permalink>(p =>
                        p.Configuration.Equals(request.Configuration) &&
                        p.FullTable.IsDeepEqualTo(new PermalinkTableBuilderResult(tableResult)) &&
                        p.Query.Equals(request.Query)),
                    It.IsAny<JsonSerializerSettings>()))
                .Returns(Task.CompletedTask);

            subjectRepository
                .Setup(s => s.Get(subject.Id))
                .ReturnsAsync(subject);

            tableBuilderService
                .Setup(s => s.Query(release.Id, request.Query, CancellationToken.None))
                .ReturnsAsync(tableResult);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    release,
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

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    subjectRepository: subjectRepository.Object,
                    tableBuilderService: tableBuilderService.Object);

                var result = (await service.Create(release.Id, request)).AssertRight();

                MockUtils.VerifyAllMocks(
                    blobStorageService,
                    subjectRepository,
                    tableBuilderService);

                Assert.Equal(Guid.Parse(blobPath), result.Id);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
                Assert.Equal(PermalinkStatus.Current, result.Status);
            }
        }

        [Fact]
        public async Task Get()
        {
            var release = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                Publication = new Publication { Id = _publicationId },
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                Published = DateTime.UtcNow,
            };

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var permalink = new Permalink(
                new TableBuilderConfiguration(),
                new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta()
                },
                new ObservationQueryContext
                {
                    SubjectId = subject.Id
                });

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobText(
                container: Permalinks,
                path: permalink.Id.ToString(),
                blobText: JsonConvert.SerializeObject(permalink));

            subjectRepository
                .Setup(s => s.Get(subject.Id))
                .ReturnsAsync(subject);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    release,
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

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    subjectRepository: subjectRepository.Object);

                var result = (await service.Get(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(
                    blobStorageService,
                    subjectRepository);

                Assert.Equal(permalink.Id, result.Id);
                Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
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

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

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

            var permalink = new Permalink(
                new TableBuilderConfiguration(),
                new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta()
                },
                new ObservationQueryContext
                {
                    SubjectId = subject.Id
                });

            // Set the legacy locations field on the Permalink table subject meta
            var permalinkJsonObject = JObject.FromObject(permalink);
            var subjectMetaJsonObject = permalinkJsonObject.SelectToken("FullTable.SubjectMeta") as JObject;
            var legacyLocationsJsonArray = JArray.FromObject(legacyLocations);
            subjectMetaJsonObject!.Add("Locations", legacyLocationsJsonArray);

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobText(
                container: Permalinks,
                path: permalink.Id.ToString(),
                blobText: permalinkJsonObject.ToString(Formatting.None));

            subjectRepository
                .Setup(s => s.Get(subject.Id))
                .ReturnsAsync(subject);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    new Content.Model.Release
                    {
                        Id = releaseId,
                        Publication = new Publication { Id = _publicationId },
                        ReleaseName = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = DateTime.UtcNow,
                    },
                    new ReleaseFile
                    {
                        ReleaseId = releaseId,
                        File = new File
                        {
                            SubjectId = subject.Id,
                            Type = FileType.Data,
                        }
                    });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    subjectRepository: subjectRepository.Object);

                var result = (await service.Get(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(
                    blobStorageService,
                    subjectRepository);

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
            var permalink = new Permalink(
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
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobText(
                container: Permalinks,
                path: permalink.Id.ToString(),
                blobText: JsonConvert.SerializeObject(permalink));

            subjectRepository
                .Setup(s => s.Get(permalink.Query.SubjectId))
                .ReturnsAsync((Subject?)null);

            var service = BuildService(blobStorageService: blobStorageService.Object,
                subjectRepository: subjectRepository.Object);

            var result = (await service.Get(permalink.Id)).AssertRight();

            MockUtils.VerifyAllMocks(
                blobStorageService,
                subjectRepository);

            Assert.Equal(permalink.Id, result.Id);
            Assert.Equal(PermalinkStatus.SubjectRemoved, result.Status);
        }

        [Fact]
        public async Task Get_SubjectIsNotFromLatestPublishedRelease()
        {
            var publication = new Publication { Id = _publicationId };
            var release = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                ReleaseName = "2000",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                Published = DateTime.UtcNow,
            };

            var latestRelease = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                ReleaseName = "2001",
                TimePeriodCoverage = TimeIdentifier.AcademicYear,
                Published = DateTime.UtcNow,
            };

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var permalink = new Permalink(
                new TableBuilderConfiguration(),
                new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta()
                },
                new ObservationQueryContext
                {
                    SubjectId = subject.Id
                });

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobText(
                container: Permalinks,
                path: permalink.Id.ToString(),
                blobText: JsonConvert.SerializeObject(permalink));

            subjectRepository
                .Setup(s => s.Get(subject.Id))
                .ReturnsAsync(subject);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    release,
                    latestRelease,
                    new ReleaseFile
                    {
                        Release = release,
                        File = new File
                        {
                            SubjectId = subject.Id,
                            Type = FileType.Data,
                        }
                    });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    subjectRepository: subjectRepository.Object);

                var result = (await service.Get(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(
                    blobStorageService,
                    subjectRepository);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(PermalinkStatus.NotForLatestRelease, result.Status);
            }
        }

        [Fact]
        public async Task Get_SubjectIsFromSupersededPublication()
        {
            var supersededPublicationId = Guid.NewGuid();

            var releaseId = Guid.NewGuid();

            var subject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var permalink = new Permalink(
                new TableBuilderConfiguration(),
                new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta()
                },
                new ObservationQueryContext
                {
                    SubjectId = subject.Id
                });

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var subjectRepository = new Mock<ISubjectRepository>(MockBehavior.Strict);

            blobStorageService.SetupDownloadBlobText(
                container: Permalinks,
                path: permalink.Id.ToString(),
                blobText: JsonConvert.SerializeObject(permalink));

            subjectRepository
                .Setup(s => s.Get(subject.Id))
                .ReturnsAsync(subject);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(
                    new Content.Model.Release
                    {
                        Id = releaseId,
                        Publication = new Publication
                        {
                            Id = _publicationId,
                            SupersededById = supersededPublicationId,
                        },
                        ReleaseName = "2000",
                        TimePeriodCoverage = TimeIdentifier.AcademicYear,
                        Published = DateTime.UtcNow,
                    },
                    new ReleaseFile
                    {
                        ReleaseId = releaseId,
                        File = new File
                        {
                            SubjectId = subject.Id,
                            Type = FileType.Data,
                        }
                    },
                    new Publication
                    {
                        Id = supersededPublicationId,
                        Releases = new List<Content.Model.Release>
                        {
                            new()
                            {
                                Published = DateTime.UtcNow,
                            }
                        }
                    });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    subjectRepository: subjectRepository.Object);

                var result = (await service.Get(permalink.Id)).AssertRight();

                MockUtils.VerifyAllMocks(
                    blobStorageService,
                    subjectRepository);

                Assert.Equal(permalink.Id, result.Id);
                Assert.Equal(PermalinkStatus.NotForLatestRelease, result.Status);
            }
        }

        private static PermalinkService BuildService(
            ContentDbContext? contentDbContext = null,
            ITableBuilderService? tableBuilderService = null,
            IBlobStorageService? blobStorageService = null,
            IReleaseRepository? releaseRepository = null,
            ISubjectRepository? subjectRepository = null)
        {
            return new(
                contentDbContext ?? new Mock<ContentDbContext>(MockBehavior.Strict).Object,
                tableBuilderService ?? new Mock<ITableBuilderService>(MockBehavior.Strict).Object,
                blobStorageService ?? new Mock<IBlobStorageService>(MockBehavior.Strict).Object,
                subjectRepository ?? new Mock<ISubjectRepository>(MockBehavior.Strict).Object,
                releaseRepository ?? new Mock<IReleaseRepository>(MockBehavior.Strict).Object,
                MapperUtils.MapperForProfile<MappingProfiles>()
            );
        }
    }
}
