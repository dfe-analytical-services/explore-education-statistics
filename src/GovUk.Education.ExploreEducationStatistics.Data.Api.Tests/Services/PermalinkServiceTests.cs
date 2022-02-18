#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
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
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

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
                .Returns((Release?) null);

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
                Id = new Guid()
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
                Id = Guid.NewGuid()
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
                .Returns(release);

            subjectRepository
                .Setup(s => s.Get(subject.Id))
                .ReturnsAsync(subject);

            subjectRepository
                .Setup(s => s.GetPublicationIdForSubject(subject.Id))
                .ReturnsAsync(_publicationId);

            subjectRepository
                .Setup(s => s.FindPublicationIdForSubject(subject.Id))
                .ReturnsAsync(_publicationId);

            subjectRepository
                .Setup(s => s.IsSubjectForLatestPublishedRelease(subject.Id))
                .ReturnsAsync(true);

            tableBuilderService
                .Setup(s => s.Query(release.Id, request.Query, CancellationToken.None))
                .ReturnsAsync(tableResult);

            var service = BuildService(blobStorageService: blobStorageService.Object,
                releaseRepository: releaseRepository.Object,
                subjectRepository: subjectRepository.Object,
                tableBuilderService: tableBuilderService.Object);

            var result = (await service.Create(request)).AssertRight();

            MockUtils.VerifyAllMocks(
                blobStorageService,
                releaseRepository,
                subjectRepository,
                tableBuilderService);

            Assert.Equal(Guid.Parse(blobPath), result.Id);
            Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
            Assert.False(result.Invalidated);
            Assert.Equal(_publicationId, result.Query.PublicationId);
        }

        [Fact]
        public async Task Create_WithReleaseId()
        {
            var subject = new Subject
            {
                Id = new Guid()
            };

            var request = new PermalinkCreateViewModel
            {
                Query =
                {
                    SubjectId = subject.Id
                }
            };

            var releaseId = Guid.NewGuid();

            var tableResult = new TableBuilderResultViewModel
            {
                SubjectMeta = new ResultSubjectMetaViewModel()
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

            subjectRepository
                .Setup(s => s.FindPublicationIdForSubject(subject.Id))
                .ReturnsAsync(_publicationId);

            subjectRepository
                .Setup(s => s.IsSubjectForLatestPublishedRelease(subject.Id))
                .ReturnsAsync(true);

            tableBuilderService
                .Setup(s => s.Query(releaseId, request.Query, CancellationToken.None))
                .ReturnsAsync(tableResult);

            var service = BuildService(blobStorageService: blobStorageService.Object,
                subjectRepository: subjectRepository.Object,
                tableBuilderService: tableBuilderService.Object);

            var result = (await service.Create(releaseId, request)).AssertRight();

            MockUtils.VerifyAllMocks(
                blobStorageService,
                subjectRepository,
                tableBuilderService);

            Assert.Equal(Guid.Parse(blobPath), result.Id);
            Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
            Assert.False(result.Invalidated);
            Assert.Equal(_publicationId, result.Query.PublicationId);
        }

        [Fact]
        public async Task Get()
        {
            var subject = new Subject
            {
                Id = new Guid()
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

            subjectRepository
                .Setup(s => s.IsSubjectForLatestPublishedRelease(subject.Id))
                .ReturnsAsync(true);

            subjectRepository
                .Setup(s => s.FindPublicationIdForSubject(subject.Id))
                .ReturnsAsync(_publicationId);

            var service = BuildService(blobStorageService: blobStorageService.Object,
                subjectRepository: subjectRepository.Object);

            var result = (await service.Get(permalink.Id)).AssertRight();

            MockUtils.VerifyAllMocks(
                blobStorageService,
                subjectRepository);

            Assert.Equal(permalink.Id, result.Id);
            Assert.InRange(DateTime.UtcNow.Subtract(result.Created).Milliseconds, 0, 1500);
            Assert.False(result.Invalidated);
            Assert.Equal(_publicationId, result.Query.PublicationId);
        }

        [Fact]
        public async Task Get_LegacyLocationsFieldIsTransformed()
        {
            // Until old Permalinks are migrated to permanently transform their legacy 'Locations' field,
            // test that legacy locations are transformed to 'LocationsHierarchical' and then mapped to 'Locations'
            // in the view model.

            var subject = new Subject
            {
                Id = new Guid()
            };

            // Setup a list of legacy locations to be added to the Permalink table subject meta during serialization
            var legacyLocations = new List<ObservationalUnitMetaViewModel>
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

            subjectRepository
                .Setup(s => s.IsSubjectForLatestPublishedRelease(subject.Id))
                .ReturnsAsync(true);

            subjectRepository
                .Setup(s => s.FindPublicationIdForSubject(subject.Id))
                .ReturnsAsync(_publicationId);

            var service = BuildService(blobStorageService: blobStorageService.Object,
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
                .ReturnsAsync((Subject?) null);

            subjectRepository
                .Setup(s => s.FindPublicationIdForSubject(permalink.Query.SubjectId))
                .ReturnsAsync((Guid?) null);

            var service = BuildService(blobStorageService: blobStorageService.Object,
                subjectRepository: subjectRepository.Object);

            var result = (await service.Get(permalink.Id)).AssertRight();

            MockUtils.VerifyAllMocks(
                blobStorageService,
                subjectRepository);

            Assert.Equal(permalink.Id, result.Id);
            // Expect invalidated Permalink and missing Publication Id
            Assert.True(result.Invalidated);
            Assert.Null(result.Query.PublicationId);
        }

        [Fact]
        public async Task Get_SubjectIsNotFromLatestPublishedRelease()
        {
            var subject = new Subject
            {
                Id = new Guid()
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

            subjectRepository
                .Setup(s => s.IsSubjectForLatestPublishedRelease(subject.Id))
                .ReturnsAsync(false);

            subjectRepository
                .Setup(s => s.FindPublicationIdForSubject(subject.Id))
                .ReturnsAsync(_publicationId);

            var service = BuildService(blobStorageService: blobStorageService.Object,
                subjectRepository: subjectRepository.Object);

            var result = (await service.Get(permalink.Id)).AssertRight();

            MockUtils.VerifyAllMocks(
                blobStorageService,
                subjectRepository);

            Assert.Equal(permalink.Id, result.Id);
            // Expect invalidated Permalink
            Assert.True(result.Invalidated);
            Assert.Equal(_publicationId, result.Query.PublicationId);
        }

        private static PermalinkService BuildService(
            ITableBuilderService? tableBuilderService = null,
            IBlobStorageService? blobStorageService = null,
            IReleaseRepository? releaseRepository = null,
            ISubjectRepository? subjectRepository = null)
        {
            return new(
                tableBuilderService ?? new Mock<ITableBuilderService>(MockBehavior.Strict).Object,
                blobStorageService ?? new Mock<IBlobStorageService>(MockBehavior.Strict).Object,
                subjectRepository ?? new Mock<ISubjectRepository>(MockBehavior.Strict).Object,
                releaseRepository ?? new Mock<IReleaseRepository>(MockBehavior.Strict).Object,
                MapperUtils.MapperForProfile<MappingProfiles>()
            );
        }
    }
}
