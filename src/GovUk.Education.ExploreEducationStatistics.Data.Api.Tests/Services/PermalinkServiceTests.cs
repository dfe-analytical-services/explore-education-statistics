#nullable enable
using System;
using System.Collections.Generic;
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
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.Extensions.Options;
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
        public async Task Get()
        {
            var subject = new Subject
            {
                Id = new Guid()
            };

            var permalink = new Permalink(
                new TableBuilderConfiguration(),
                new TableBuilderResultViewModel
                {
                    SubjectMeta = new ResultSubjectMetaViewModel()
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
            // test that legacy locations are transformed to 'LocationsHierarchical',
            // ensuring that consumers are aware of them when accessing the new field.

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
                new TableBuilderResultViewModel
                {
                    SubjectMeta = new ResultSubjectMetaViewModel()
                },
                new ObservationQueryContext
                {
                    SubjectId = subject.Id
                });

            // Set the legacy locations field on the Permalink table subject meta
            var permalinkJsonObject = JObject.FromObject(permalink);
            var subjectMetaJsonObject = permalinkJsonObject.SelectToken("FullTable.SubjectMeta") as JObject;
            var legacyLocationsJsonArray = JArray.FromObject(legacyLocations);
            if (subjectMetaJsonObject!.SelectToken("Locations") is JArray existingLegacyLocationsJsonArray)
            {
                // Handle case where there is an existing legacy locations field in the JSON object
                // I.e. when field 'Locations' is still defined in ResultSubjectMetaViewModel
                // TODO EES-2902 No need to do this after the Locations field is dropped from the type.
                existingLegacyLocationsJsonArray.Replace(legacyLocationsJsonArray);
            }
            else
            {
                subjectMetaJsonObject!.Add("Locations", legacyLocationsJsonArray);
            }

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

            // Expect Locations to be empty
            Assert.Empty(subjectMeta.Locations);

            // Expect Locations to have been transformed to LocationsHierarchical
            Assert.Single(subjectMeta.LocationsHierarchical);
            Assert.True(subjectMeta.LocationsHierarchical.ContainsKey("localAuthority"));

            var localAuthorities = subjectMeta.LocationsHierarchical["localAuthority"];
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
        [Obsolete("TODO EES-2902 - Remove with SOW8 after EES-2777", false)]
        public async Task Get_LocationHierarchiesFeatureDisabled_LegacyLocationsFieldIsNotTranslated()
        {
            // Until the feature to add hierarchical locations in Table Result Subject Metadata becomes permanent,
            // test that ResultSubjectMetaViewModel legacy 'Locations' field remains untouched when the feature is off.

            var subject = new Subject
            {
                Id = new Guid()
            };

            var permalink = new Permalink(
                new TableBuilderConfiguration(),
                new TableBuilderResultViewModel
                {
                    SubjectMeta = new ResultSubjectMetaViewModel
                    {
                        Locations = new List<ObservationalUnitMetaViewModel>
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
                        }
                    }
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
                subjectRepository: subjectRepository.Object,
                // Setup the location hierarchies featured as disabled
                options: DisabledLocationHierarchiesOptions());

            var result = (await service.Get(permalink.Id)).AssertRight();

            MockUtils.VerifyAllMocks(
                blobStorageService,
                subjectRepository);

            Assert.Equal(permalink.Id, result.Id);

            var subjectMeta = result.FullTable.SubjectMeta;

            // Expect Locations to be untouched
            Assert.Equal(permalink.FullTable.SubjectMeta.Locations[0].Level, subjectMeta.Locations[0].Level);
            Assert.Equal(permalink.FullTable.SubjectMeta.Locations[0].Label, subjectMeta.Locations[0].Label);
            Assert.Equal(permalink.FullTable.SubjectMeta.Locations[0].Value, subjectMeta.Locations[0].Value);
            Assert.Equal(permalink.FullTable.SubjectMeta.Locations[0].GeoJson, subjectMeta.Locations[0].GeoJson);

            Assert.Equal(permalink.FullTable.SubjectMeta.Locations[1].Level, subjectMeta.Locations[1].Level);
            Assert.Equal(permalink.FullTable.SubjectMeta.Locations[1].Label, subjectMeta.Locations[1].Label);
            Assert.Equal(permalink.FullTable.SubjectMeta.Locations[1].Value, subjectMeta.Locations[1].Value);
            Assert.Equal(permalink.FullTable.SubjectMeta.Locations[1].GeoJson, subjectMeta.Locations[1].GeoJson);

            Assert.Equal(permalink.FullTable.SubjectMeta.Locations[2].Level, subjectMeta.Locations[2].Level);
            Assert.Equal(permalink.FullTable.SubjectMeta.Locations[2].Label, subjectMeta.Locations[2].Label);
            Assert.Equal(permalink.FullTable.SubjectMeta.Locations[2].Value, subjectMeta.Locations[2].Value);
            Assert.Equal(permalink.FullTable.SubjectMeta.Locations[2].GeoJson, subjectMeta.Locations[2].GeoJson);

            // Expect LocationsHierarchical to be empty
            Assert.Empty(subjectMeta.LocationsHierarchical);
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
                new TableBuilderResultViewModel
                {
                    SubjectMeta = new ResultSubjectMetaViewModel()
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
                new TableBuilderResultViewModel
                {
                    SubjectMeta = new ResultSubjectMetaViewModel()
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

        private static IOptions<LocationsOptions> DefaultLocationOptions()
        {
            return Options.Create(new LocationsOptions
            {
                TableResultLocationHierarchiesEnabled = true
            });
        }

        private static IOptions<LocationsOptions> DisabledLocationHierarchiesOptions()
        {
            return Options.Create(new LocationsOptions
            {
                TableResultLocationHierarchiesEnabled = false
            });
        }

        private static PermalinkService BuildService(
            ITableBuilderService? tableBuilderService = null,
            IBlobStorageService? blobStorageService = null,
            IReleaseRepository? releaseRepository = null,
            ISubjectRepository? subjectRepository = null,
            IOptions<LocationsOptions>? options = null)
        {
            return new(
                tableBuilderService ?? new Mock<ITableBuilderService>(MockBehavior.Strict).Object,
                blobStorageService ?? new Mock<IBlobStorageService>(MockBehavior.Strict).Object,
                subjectRepository ?? new Mock<ISubjectRepository>(MockBehavior.Strict).Object,
                releaseRepository ?? new Mock<IReleaseRepository>(MockBehavior.Strict).Object,
                options ?? DefaultLocationOptions(),
                MapperUtils.MapperForProfile<MappingProfiles>()
            );
        }
    }
}
