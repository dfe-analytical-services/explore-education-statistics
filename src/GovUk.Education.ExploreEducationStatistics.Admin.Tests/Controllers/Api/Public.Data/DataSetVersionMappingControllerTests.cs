#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

public abstract class DataSetVersionMappingControllerTests(
    TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/public-data/data-set-versions";

    public class GetLocationMappingsTests(
        TestApplicationFactory testApp) : DataSetVersionMappingControllerTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(currentDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithLocationMappingPlan(DataFixture
                    .DefaultLocationMappingPlan()
                    .AddLevel(
                        level: GeographicLevel.LocalAuthority,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            .AddMapping(
                                sourceKey: "source-location-1-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithAutoMapped("target-location-1-key"))
                            .AddMapping(
                                sourceKey: "source-location-2-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping()
                            )
                            .AddCandidate(
                                targetKey: "target-location-1-key",
                                candidate: DataFixture.DefaultMappableLocationOption()))
                    .AddLevel(
                        level: GeographicLevel.Country,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            .AddMapping(
                                sourceKey: "source-location-1-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithManualNone())
                            .AddMapping(
                                sourceKey: "source-location-2-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithManualMapped("target-location-1-key")
                            )
                            .AddCandidate(
                                targetKey: "target-location-1-key",
                                candidate: DataFixture.DefaultMappableLocationOption())));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            var client = BuildApp().CreateClient();

            var response = await GetLocationMappings(
                nextDataSetVersionId: nextDataSetVersion.Id,
                client);

            var retrievedMappings = response.AssertOk<LocationMappingPlan>();

            // Test that the mappings from the Controller are identical to the mappings saved in the database
            retrievedMappings.AssertDeepEqualTo(
                mappings.LocationMappingPlan,
                ignoreCollectionOrders: true);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = BuildApp(user: AuthenticatedUser()).CreateClient();

            var response = await GetLocationMappings(
                Guid.NewGuid(),
                client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task DataSetVersionMappingDoesNotExist_Returns404()
        {
            var client = BuildApp().CreateClient();

            var response = await GetLocationMappings(
                Guid.NewGuid(),
                client);

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> GetLocationMappings(
            Guid nextDataSetVersionId,
            HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            var uri = new Uri($"{BaseUrl}/{nextDataSetVersionId}/mapping/locations", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    public class ApplyBatchLocationMappingUpdatesTests(
        TestApplicationFactory testApp) : DataSetVersionMappingControllerTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(currentDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithLocationMappingPlan(DataFixture
                    .DefaultLocationMappingPlan()
                    .AddLevel(
                        level: GeographicLevel.LocalAuthority,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            .AddMapping(
                                sourceKey: "source-location-1-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithAutoNone())
                            .AddMapping(
                                sourceKey: "source-location-2-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithAutoNone()
                            )
                            .AddCandidate(
                                targetKey: "target-location-1-key",
                                candidate: DataFixture.DefaultMappableLocationOption()))
                    .AddLevel(
                        level: GeographicLevel.Country,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            .AddMapping(
                                sourceKey: "source-location-1-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithAutoNone())
                            .AddMapping(
                                sourceKey: "source-location-3-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithAutoMapped("target-location-3-key"))
                            .AddCandidate(
                                targetKey: "target-location-1-key",
                                candidate: DataFixture.DefaultMappableLocationOption())));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextDataSetVersion.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithPublicApiDataSetId(nextDataSetVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextDataSetVersion.SemVersion());

            await TestApp.AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            var client = BuildApp().CreateClient();

            List<LocationMappingUpdateRequest> updates =
            [
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-location-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-location-1-key"
                },
                new()
                {
                    Level = GeographicLevel.Country,
                    SourceKey = "source-location-3-key",
                    Type = MappingType.ManualNone
                }
            ];

            var response = await ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client);

            var viewModel = response.AssertOk<BatchLocationMappingUpdatesResponseViewModel>();

            var originalLocalAuthorityMappingToUpdate = mappings
                .GetLocationOptionMapping(GeographicLevel.LocalAuthority, "source-location-1-key");

            var originalLocalAuthorityMappingNotUpdated = mappings
                .GetLocationOptionMapping(GeographicLevel.LocalAuthority, "source-location-2-key");

            var originalCountryMappingNotUpdated = mappings
                .GetLocationOptionMapping(GeographicLevel.Country, "source-location-1-key");

            var originalCountryMappingToUpdate = mappings
                .GetLocationOptionMapping(GeographicLevel.Country, "source-location-3-key");

            var expectedUpdateResponse = new BatchLocationMappingUpdatesResponseViewModel
            {
                Updates =
                [
                    new LocationMappingUpdateResponseViewModel
                    {
                        Level = GeographicLevel.LocalAuthority,
                        SourceKey = "source-location-1-key",
                        Mapping = originalLocalAuthorityMappingToUpdate with
                        {
                            Type = MappingType.ManualMapped,
                            CandidateKey = "target-location-1-key"
                        }
                    },
                    new LocationMappingUpdateResponseViewModel
                    {
                        Level = GeographicLevel.Country,
                        SourceKey = "source-location-3-key",
                        Mapping = originalCountryMappingToUpdate with
                        {
                            Type = MappingType.ManualNone,
                            CandidateKey = null
                        }
                    }
                ]
            };

            // Test that the response from the Controller contains details of all the mappings
            // that were updated.
            viewModel.AssertDeepEqualTo(expectedUpdateResponse, ignoreCollectionOrders: true);

            var updatedMappings = TestApp.GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Include(m => m.TargetDataSetVersion)
                .Single(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            var expectedFullMappings = new Dictionary<GeographicLevel, LocationLevelMappings>
            {
                {
                    GeographicLevel.LocalAuthority, new LocationLevelMappings
                    {
                        Mappings = new Dictionary<string, LocationOptionMapping>
                        {
                            // We expect this mapping's type ot be set to ManualMapped and
                            // its CandidateKey set.
                            {
                                "source-location-1-key",
                                originalLocalAuthorityMappingToUpdate with
                                {
                                    Type = MappingType.ManualMapped,
                                    CandidateKey = "target-location-1-key"
                                }
                            },
                            { "source-location-2-key", originalLocalAuthorityMappingNotUpdated }
                        },
                        Candidates = mappings
                            .LocationMappingPlan
                            .Levels[GeographicLevel.LocalAuthority]
                            .Candidates
                    }
                },
                {
                    GeographicLevel.Country, new LocationLevelMappings
                    {
                        Mappings = new Dictionary<string, LocationOptionMapping>
                        {
                            { "source-location-1-key", originalCountryMappingNotUpdated },
                            {
                                // We expect this mapping's type to be set to ManualNone and
                                // its CandidateKey unset.
                                "source-location-3-key",
                                originalCountryMappingToUpdate with
                                {
                                    Type = MappingType.ManualNone,
                                    CandidateKey = null
                                }
                            }
                        },
                        Candidates = mappings
                            .LocationMappingPlan
                            .Levels[GeographicLevel.Country]
                            .Candidates
                    }
                }
            };

            // Test that the updated mappings retrieved from the database reflect the updates
            // that were requested.
            updatedMappings.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedFullMappings,
                ignoreCollectionOrders: true);

            // Assert that the batch saves still show the location mappings as incomplete, as there
            // are still mappings with type "AutoNone" in the plan.
            Assert.False(updatedMappings.LocationMappingsComplete);

            // Assert that this update constitutes a major version update, as some locations options
            // are 'ManualNone', indicating that some of the source location options may have been
            // removed thus creating a breaking change.
            Assert.Equal("2.0.0", updatedMappings.TargetDataSetVersion.SemVersion());
        }

        [Theory]
        [InlineData(MappingType.ManualMapped, MappingType.AutoNone, false)]
        [InlineData(MappingType.ManualMapped, MappingType.AutoMapped, true)]
        [InlineData(MappingType.ManualMapped, MappingType.ManualMapped, true)]
        [InlineData(MappingType.ManualMapped, MappingType.ManualNone, true)]
        [InlineData(MappingType.ManualNone, MappingType.AutoNone, false)]
        [InlineData(MappingType.ManualNone, MappingType.AutoMapped, true)]
        [InlineData(MappingType.ManualNone, MappingType.ManualMapped, true)]
        [InlineData(MappingType.ManualNone, MappingType.ManualNone, true)]
        public async Task Success_MappingsComplete(
            MappingType updatedMappingType,
            MappingType unchangedMappingType,
            bool expectedMappingsComplete)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(currentDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithLocationMappingPlan(DataFixture
                    .DefaultLocationMappingPlan()
                    .AddLevel(
                        level: GeographicLevel.LocalAuthority,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            .AddMapping(
                                sourceKey: "source-location-1-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping())
                            .AddCandidate(
                                targetKey: "target-location-1-key",
                                candidate: DataFixture.DefaultMappableLocationOption()))
                    .AddLevel(
                        level: GeographicLevel.Country,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            .AddMapping(
                                sourceKey: "source-location-1-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithType(unchangedMappingType)
                                    .WithCandidateKey(unchangedMappingType switch
                                    {
                                        MappingType.ManualMapped or MappingType.AutoMapped => "target-location-1-key",
                                        _ => null
                                    }))
                            .AddCandidate(
                                targetKey: "target-location-1-key",
                                candidate: DataFixture.DefaultMappableLocationOption())));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextDataSetVersion.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile())
                .WithPublicApiDataSetId(nextDataSetVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextDataSetVersion.SemVersion());

            await TestApp.AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            var client = BuildApp().CreateClient();

            var mappingCandidateKey = updatedMappingType == MappingType.ManualMapped
                ? "target-location-1-key"
                : null;

            List<LocationMappingUpdateRequest> updates =
            [
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-location-1-key",
                    Type = updatedMappingType,
                    CandidateKey = mappingCandidateKey
                }
            ];

            var response = await ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client);

            response.AssertOk<BatchLocationMappingUpdatesResponseViewModel>();

            var updatedMappings = TestApp.GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Single(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            // Assert that the batch save calculates the LocationMappingsComplete flag as expected given
            // the combination of the requested mapping update and the existing mapping that is untouched. 
            Assert.Equal(expectedMappingsComplete, updatedMappings.LocationMappingsComplete);
        }

        [Theory]
        [InlineData(MappingType.ManualMapped, MappingType.AutoNone, "2.0.0")]
        [InlineData(MappingType.ManualMapped, MappingType.AutoMapped, "1.1.0")]
        [InlineData(MappingType.ManualMapped, MappingType.ManualMapped, "1.1.0")]
        [InlineData(MappingType.ManualMapped, MappingType.ManualNone, "2.0.0")]
        [InlineData(MappingType.ManualNone, MappingType.AutoNone, "2.0.0")]
        [InlineData(MappingType.ManualNone, MappingType.AutoMapped, "2.0.0")]
        [InlineData(MappingType.ManualNone, MappingType.ManualMapped, "2.0.0")]
        [InlineData(MappingType.ManualNone, MappingType.ManualNone, "2.0.0")]
        public async Task Success_VersionUpdate(
            MappingType updatedMappingType,
            MappingType unchangedMappingType,
            string expectedVersion)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(currentDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithLocationMappingPlan(DataFixture
                    .DefaultLocationMappingPlan()
                    .AddLevel(
                        level: GeographicLevel.LocalAuthority,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            .AddMapping(
                                sourceKey: "source-location-1-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping())
                            .AddCandidate(
                                targetKey: "target-location-1-key",
                                candidate: DataFixture.DefaultMappableLocationOption()))
                    .AddLevel(
                        level: GeographicLevel.Country,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            .AddMapping(
                                sourceKey: "source-location-1-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithType(unchangedMappingType)
                                    .WithCandidateKey(unchangedMappingType switch
                                    {
                                        MappingType.ManualMapped or MappingType.AutoMapped => "target-location-1-key",
                                        _ => null
                                    }))
                            .AddCandidate(
                                targetKey: "target-location-1-key",
                                candidate: DataFixture.DefaultMappableLocationOption())));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextDataSetVersion.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithPublicApiDataSetId(nextDataSetVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextDataSetVersion.SemVersion());

            await TestApp.AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            var client = BuildApp().CreateClient();

            var mappingCandidateKey = updatedMappingType == MappingType.ManualMapped
                ? "target-location-1-key"
                : null;

            List<LocationMappingUpdateRequest> updates =
            [
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-location-1-key",
                    Type = updatedMappingType,
                    CandidateKey = mappingCandidateKey
                }
            ];

            var response = await ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client);

            response.AssertOk<BatchLocationMappingUpdatesResponseViewModel>();

            var updatedMappings = TestApp.GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Include(m => m.TargetDataSetVersion)
                .Single(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            // Assert that the batch save calculates the next data set version correctly. 
            Assert.Equal(expectedVersion, updatedMappings.TargetDataSetVersion.SemVersion());

            var updatedReleaseFile = await TestApp.GetDbContext<ContentDbContext>()
                .ReleaseFiles
                .SingleAsync(rf => rf.PublicApiDataSetId == updatedMappings.TargetDataSetVersion.DataSetId);

            Assert.Equal(expectedVersion, updatedReleaseFile.PublicApiDataSetVersion);
        }

        [Fact]
        public async Task SourceKeyDoesNotExist_Returns400_AndRollsBackTransaction()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(currentDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithLocationMappingPlan(DataFixture
                    .DefaultLocationMappingPlan()
                    .AddLevel(
                        level: GeographicLevel.LocalAuthority,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            .AddMapping(
                                sourceKey: "source-la-location-1-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping())
                            .AddMapping(
                                sourceKey: "source-la-location-2-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping()
                            )
                            .AddCandidate(
                                targetKey: "target-la-location-1-key",
                                candidate: DataFixture.DefaultMappableLocationOption())
                            .AddCandidate(
                                targetKey: "target-la-location-2-key",
                                candidate: DataFixture.DefaultMappableLocationOption()))
                    .AddLevel(
                        level: GeographicLevel.Country,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            .AddCandidate(
                                targetKey: "target-country-location-1-key",
                                candidate: DataFixture.DefaultMappableLocationOption())));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextDataSetVersion.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithPublicApiDataSetId(nextDataSetVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextDataSetVersion.SemVersion());

            await TestApp.AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            var client = BuildApp().CreateClient();

            List<LocationMappingUpdateRequest> updates =
            [
                // This mapping exists.
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-la-location-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-la-location-1-key"
                },
                // This mapping does not exist.
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-la-location-3-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-la-location-2-key"
                },
                // This mapping does not exist.
                new()
                {
                    Level = GeographicLevel.Country,
                    SourceKey = "source-la-location-2-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-country-location-1-key"
                }
            ];

            var response = await ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client);

            var validationProblem = response.AssertValidationProblem();

            // The 2 non-existent mappings in the batch update request should have failure messages
            // indicating that the mappings they were attempting to update do not exist.
            Assert.Equal(2, validationProblem.Errors.Count);

            validationProblem.AssertHasError(
                expectedPath: "updates[1].sourceKey",
                expectedCode: nameof(ValidationMessages.DataSetVersionMappingSourcePathDoesNotExist));

            validationProblem.AssertHasError(
                expectedPath: "updates[2].sourceKey",
                expectedCode: nameof(ValidationMessages.DataSetVersionMappingSourcePathDoesNotExist));

            var retrievedMappings = TestApp.GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Single(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            // Test that the mappings are not updated due to the failures of some of the update requests.
            retrievedMappings.LocationMappingPlan.Levels.AssertDeepEqualTo(
                mappings.LocationMappingPlan.Levels,
                ignoreCollectionOrders: true);
        }

        [Fact]
        public async Task CandidateKeyDoesNotExist_Returns400()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(currentDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithLocationMappingPlan(DataFixture
                    .DefaultLocationMappingPlan()
                    .AddLevel(
                        level: GeographicLevel.LocalAuthority,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            .AddMapping(
                                sourceKey: "source-la-location-1-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping())
                            .AddMapping(
                                sourceKey: "source-la-location-2-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping()
                            )
                            .AddMapping(
                                sourceKey: "source-la-location-3-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping()
                            )
                            .AddCandidate(
                                targetKey: "target-la-location-1-key",
                                candidate: DataFixture.DefaultMappableLocationOption()))
                    .AddLevel(
                        level: GeographicLevel.Country,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            .AddCandidate(
                                targetKey: "target-country-location-1-key",
                                candidate: DataFixture.DefaultMappableLocationOption())));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            var client = BuildApp().CreateClient();

            List<LocationMappingUpdateRequest> updates =
            [
                // This candidate exists.
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-la-location-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-la-location-1-key"
                },
                // This candidate does not exist as there is no candidate with the key
                // "target-la-location-2-key" under the "LocalAuthority" level. This tests
                // the simple case where a candidate simply doesn't exist at all with the
                // given key.
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-la-location-2-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-la-location-2-key"
                },
                // This candidate does not exist as there is no candidate with the key
                // "target-la-location-1-key" under the "Country" level, despite it existing
                // under the "LocalAuthority" level.  This tests the more complex case
                // whereby a candidate *does* exist with the given key, but it's under a
                // different level than that of the source location and thus is not a valid
                // candidate.
                new()
                {
                    Level = GeographicLevel.Country,
                    SourceKey = "source-la-location-3-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-la-location-1-key"
                }
            ];

            var response = await ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client);

            var validationProblem = response.AssertValidationProblem();

            // The 2 non-existent mappings in the batch update request should have failure messages
            // indicating that the mappings they were attempting to update do not exist.
            validationProblem.AssertHasError(
                expectedPath: "updates[1].candidateKey",
                expectedCode: nameof(ValidationMessages.DataSetVersionMappingCandidatePathDoesNotExist));

            validationProblem.AssertHasError(
                expectedPath: "updates[2].candidateKey",
                expectedCode: nameof(ValidationMessages.DataSetVersionMappingCandidatePathDoesNotExist));

            Assert.Equal(2, validationProblem.Errors.Count);

            var retrievedMappings = TestApp.GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Single(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            // Test that the mappings are not updated due to the failures of some of the update requests.
            retrievedMappings.LocationMappingPlan.Levels.AssertDeepEqualTo(
                mappings.LocationMappingPlan.Levels,
                ignoreCollectionOrders: true);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = BuildApp(user: AuthenticatedUser()).CreateClient();

            var response = await ApplyBatchLocationMappingUpdates(
                Guid.NewGuid(),
                new List<LocationMappingUpdateRequest>(),
                client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task DataSetVersionMappingDoesNotExist_Returns404()
        {
            var client = BuildApp().CreateClient();

            var response = await ApplyBatchLocationMappingUpdates(
                Guid.NewGuid(),
                new List<LocationMappingUpdateRequest>(),
                client);

            response.AssertNotFound();
        }

        [Fact]
        public async Task EmptyRequiredFields_Return400()
        {
            var client = BuildApp().CreateClient();

            var response = await ApplyBatchLocationMappingUpdates(
                Guid.NewGuid(),
                [
                    new LocationMappingUpdateRequest()
                ],
                client);

            var validationProblem = response.AssertValidationProblem();
            Assert.Equal(3, validationProblem.Errors.Count);
            validationProblem.AssertHasNotNullError("updates[0].level");
            validationProblem.AssertHasNotNullError("updates[0].type");
            validationProblem.AssertHasNotEmptyError("updates[0].sourceKey");
        }

        [Theory]
        [InlineData(MappingType.ManualMapped, null)]
        [InlineData(MappingType.ManualMapped, "")]
        public async Task MappingTypeExpectsCandidateKey_Returns400(MappingType type, string? candidateKeyValue)
        {
            var client = BuildApp().CreateClient();

            var response = await ApplyBatchLocationMappingUpdates(
                Guid.NewGuid(),
                [
                    new LocationMappingUpdateRequest
                    {
                        Level = GeographicLevel.LocalAuthority,
                        SourceKey = "location-1",
                        Type = type,
                        CandidateKey = candidateKeyValue
                    }
                ],
                client);

            var validationProblem = response.AssertValidationProblem();
            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasError(
                expectedPath: "updates[0].candidateKey",
                expectedCode: ValidationMessages.CandidateKeyMustBeSpecifiedWithMappedMappingType.Code,
                expectedMessage: ValidationMessages.CandidateKeyMustBeSpecifiedWithMappedMappingType.Message);
        }

        [Theory]
        [InlineData(MappingType.ManualNone)]
        public async Task MappingTypeDoeNotExpectCandidateKey_Returns400(MappingType type)
        {
            var client = BuildApp().CreateClient();

            var response = await ApplyBatchLocationMappingUpdates(
                Guid.NewGuid(),
                [
                    new LocationMappingUpdateRequest
                    {
                        Level = GeographicLevel.LocalAuthority,
                        SourceKey = "location-1",
                        Type = type,
                        CandidateKey = "target-location-1"
                    }
                ],
                client);

            var validationProblem = response.AssertValidationProblem();
            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasError(
                expectedPath: "updates[0].candidateKey",
                expectedCode: ValidationMessages.CandidateKeyMustBeEmptyWithNoneMappingType.Code,
                expectedMessage: ValidationMessages.CandidateKeyMustBeEmptyWithNoneMappingType.Message);
        }

        [Theory]
        [InlineData(MappingType.None)]
        [InlineData(MappingType.AutoMapped)]
        [InlineData(MappingType.AutoNone)]
        public async Task InvalidMappingTypeForManualInteraction_Returns400(MappingType type)
        {
            var client = BuildApp().CreateClient();

            var response = await ApplyBatchLocationMappingUpdates(
                Guid.NewGuid(),
                [
                    new LocationMappingUpdateRequest
                    {
                        Level = GeographicLevel.LocalAuthority,
                        SourceKey = "location-1",
                        Type = type,
                        CandidateKey = null
                    }
                ],
                client);

            var validationProblem = response.AssertValidationProblem();
            Assert.Single(validationProblem.Errors);
            validationProblem.AssertHasError(
                expectedPath: "updates[0].type",
                expectedCode: ValidationMessages.ManualMappingTypeInvalid.Code,
                expectedMessage: "Type must be one of the following values: ManualMapped, ManualNone");
        }

        private async Task<HttpResponseMessage> ApplyBatchLocationMappingUpdates(
            Guid nextDataSetVersionId,
            List<LocationMappingUpdateRequest> updates,
            HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            var uri = new Uri($"{BaseUrl}/{nextDataSetVersionId}/mapping/locations", UriKind.Relative);

            return await client.PatchAsync(uri,
                new JsonNetContent(new BatchLocationMappingUpdatesRequest { Updates = updates }));
        }
    }

    public class GetFilterMappingsTests(
        TestApplicationFactory testApp) : DataSetVersionMappingControllerTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(currentDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithFilterMappingPlan(DataFixture
                    .DefaultFilterMappingPlan()
                    .AddFilterMapping("filter-1-key", DataFixture
                        .DefaultFilterMapping()
                        .WithAutoMapped("filter-1-key")
                        .AddOptionMapping("filter-1-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithAutoMapped("filter-1-option-1-key"))
                        .AddOptionMapping("filter-1-option-2-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithManualNone()))
                    .AddFilterMapping("filter-2-key", DataFixture
                        .DefaultFilterMapping()
                        .WithAutoMapped("filter-2-key")
                        .AddOptionMapping("filter-2-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithAutoNone()))
                    .AddFilterCandidate("filter-1-key", DataFixture
                        .DefaultFilterMappingCandidate()
                        .AddOptionCandidate("filter-1-option-1-key", DataFixture
                            .DefaultMappableFilterOption())
                        .AddOptionCandidate("filter-1-option-3-key", DataFixture
                            .DefaultMappableFilterOption())));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            var client = BuildApp().CreateClient();

            var response = await GetFilterMappings(
                nextDataSetVersionId: nextDataSetVersion.Id,
                client);

            var retrievedMappings = response.AssertOk<FilterMappingPlan>();

            // Test that the mappings from the Controller are identical to the mappings saved in the database
            retrievedMappings.AssertDeepEqualTo(
                mappings.FilterMappingPlan,
                ignoreCollectionOrders: true);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = BuildApp(user: AuthenticatedUser()).CreateClient();

            var response = await GetFilterMappings(
                Guid.NewGuid(),
                client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task DataSetVersionMappingDoesNotExist_Returns404()
        {
            var client = BuildApp().CreateClient();

            var response = await GetFilterMappings(
                Guid.NewGuid(),
                client);

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> GetFilterMappings(
            Guid nextDataSetVersionId,
            HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            var uri = new Uri($"{BaseUrl}/{nextDataSetVersionId}/mapping/filters", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    public class ApplyBatchFilterOptionMappingUpdatesTests(
        TestApplicationFactory testApp) : DataSetVersionMappingControllerTests(testApp)
    {
        [Fact]
        public async Task Success()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(currentDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithFilterMappingPlan(DataFixture
                    .DefaultFilterMappingPlan()
                    .AddFilterMapping("filter-1-key", DataFixture
                        .DefaultFilterMapping()
                        .WithAutoMapped("filter-1-key")
                        .AddOptionMapping("filter-1-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithAutoMapped("filter-1-option-1-key"))
                        .AddOptionMapping("filter-1-option-2-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithManualNone()))
                    .AddFilterMapping("filter-2-key", DataFixture
                        .DefaultFilterMapping()
                        .WithAutoMapped("filter-2-key")
                        .AddOptionMapping("filter-2-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithAutoMapped("filter-2-option-1-key")))
                    .AddFilterCandidate("filter-1-key", DataFixture
                        .DefaultFilterMappingCandidate()
                        .AddOptionCandidate("filter-1-option-1-key", DataFixture
                            .DefaultMappableFilterOption()))
                    .AddFilterCandidate("filter-2-key", DataFixture
                        .DefaultFilterMappingCandidate()
                        .AddOptionCandidate("filter-2-option-1-key", DataFixture
                            .DefaultMappableFilterOption())));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextDataSetVersion.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithPublicApiDataSetId(nextDataSetVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextDataSetVersion.SemVersion());

            await TestApp.AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            var client = BuildApp().CreateClient();

            List<FilterOptionMappingUpdateRequest> updates =
            [
                new()
                {
                    FilterKey = "filter-1-key",
                    SourceKey = "filter-1-option-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "filter-1-option-1-key"
                },
                new()
                {
                    FilterKey = "filter-2-key",
                    SourceKey = "filter-2-option-1-key",
                    Type = MappingType.ManualNone
                }
            ];

            var response = await ApplyBatchFilterOptionMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client);

            var viewModel = response.AssertOk<BatchFilterOptionMappingUpdatesResponseViewModel>();

            var expectedUpdateResponse = new BatchFilterOptionMappingUpdatesResponseViewModel
            {
                Updates =
                [
                    new FilterOptionMappingUpdateResponseViewModel
                    {
                        FilterKey = "filter-1-key",
                        SourceKey = "filter-1-option-1-key",
                        Mapping = mappings.GetFilterOptionMapping(
                                filterKey: "filter-1-key",
                                filterOptionKey: "filter-1-option-1-key") with
                            {
                                Type = MappingType.ManualMapped,
                                CandidateKey = "filter-1-option-1-key"
                            }
                    },
                    new FilterOptionMappingUpdateResponseViewModel
                    {
                        FilterKey = "filter-2-key",
                        SourceKey = "filter-2-option-1-key",
                        Mapping = mappings.GetFilterOptionMapping(
                                filterKey: "filter-2-key",
                                filterOptionKey: "filter-2-option-1-key") with
                            {
                                Type = MappingType.ManualNone,
                                CandidateKey = null
                            }
                    },
                ]
            };

            // Test that the response from the Controller contains details of all the mappings
            // that were updated.
            viewModel.AssertDeepEqualTo(expectedUpdateResponse, ignoreCollectionOrders: true);

            var updatedMappings = TestApp.GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Include(mapping => mapping.TargetDataSetVersion)
                .Single(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            var expectedFullMappings = new Dictionary<string, FilterMapping>
            {
                {
                    "filter-1-key",
                    mappings.GetFilterMapping("filter-1-key") with
                    {
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            {
                                "filter-1-option-1-key",
                                mappings.GetFilterOptionMapping("filter-1-key", "filter-1-option-1-key") with
                                {
                                    Type = MappingType.ManualMapped,
                                    CandidateKey = "filter-1-option-1-key"
                                }
                            },
                            {
                                "filter-1-option-2-key",
                                mappings.GetFilterOptionMapping("filter-1-key", "filter-1-option-2-key")
                            }
                        }
                    }
                },
                {
                    "filter-2-key",
                    mappings.GetFilterMapping("filter-2-key") with
                    {
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            {
                                "filter-2-option-1-key",
                                mappings.GetFilterOptionMapping("filter-2-key", "filter-2-option-1-key") with
                                {
                                    Type = MappingType.ManualNone,
                                    CandidateKey = null
                                }
                            }
                        }
                    }
                }
            };

            // Test that the updated mappings retrieved from the database reflect the updates
            // that were requested.
            updatedMappings.FilterMappingPlan.Mappings.AssertDeepEqualTo(
                expectedFullMappings,
                ignoreCollectionOrders: true);

            // Assert that the batch saves show the filter mappings as complete, as there
            // are no remaining mappings with type "None" or "AutoNone" in the plan.
            Assert.True(updatedMappings.FilterMappingsComplete);

            // Assert that this update constitutes a major version update, as some filter options
            // belonging to mapped filters have a mapping type of "ManualNone", indicating that 
            // some of the source filter options are no longer available in the target data set
            // version, thus creating a breaking change. 
            Assert.Equal("2.0.0", updatedMappings.TargetDataSetVersion.SemVersion());
        }

        [Theory]
        [InlineData(MappingType.ManualMapped, MappingType.AutoNone, false)]
        [InlineData(MappingType.ManualMapped, MappingType.AutoMapped, true)]
        [InlineData(MappingType.ManualMapped, MappingType.ManualMapped, true)]
        [InlineData(MappingType.ManualMapped, MappingType.ManualNone, true)]
        [InlineData(MappingType.ManualNone, MappingType.AutoNone, false)]
        [InlineData(MappingType.ManualNone, MappingType.AutoMapped, true)]
        [InlineData(MappingType.ManualNone, MappingType.ManualMapped, true)]
        [InlineData(MappingType.ManualNone, MappingType.ManualNone, true)]
        public async Task Success_MappingsComplete(
            MappingType updatedMappingType,
            MappingType unchangedMappingType,
            bool expectedMappingsComplete)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(currentDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithFilterMappingPlan(DataFixture
                    .DefaultFilterMappingPlan()
                    .AddFilterMapping("filter-1-key", DataFixture
                        .DefaultFilterMapping()
                        .WithAutoMapped("filter-1-key")
                        .AddOptionMapping("filter-1-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithNoMapping()))
                    .AddFilterMapping("filter-2-key", DataFixture
                        .DefaultFilterMapping()
                        .WithAutoMapped("filter-2-key")
                        .AddOptionMapping("filter-2-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithType(unchangedMappingType)
                            .WithCandidateKey(unchangedMappingType switch
                            {
                                MappingType.ManualMapped or MappingType.AutoMapped => "filter-2-option-1-key",
                                _ => null
                            })))
                    // Add an unmappable filter and filter options. Because we don't currently allow the 
                    // users to update mappings for filters, this should not count against the calculation
                    // of the FilterMappingsComplete flag.
                    .AddFilterMapping("filter-3-key", DataFixture
                        .DefaultFilterMapping()
                        .WithAutoNone()
                        .AddOptionMapping("filter-3-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithAutoNone()))
                    .AddFilterCandidate("filter-1-key", DataFixture
                        .DefaultFilterMappingCandidate()
                        .AddOptionCandidate("filter-1-option-1-key", DataFixture
                            .DefaultMappableFilterOption()))
                    .AddFilterCandidate("filter-2-key", DataFixture
                        .DefaultFilterMappingCandidate()
                        .AddOptionCandidate("filter-2-option-1-key", DataFixture
                            .DefaultMappableFilterOption())));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextDataSetVersion.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithPublicApiDataSetId(nextDataSetVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextDataSetVersion.SemVersion());

            await TestApp.AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            var client = BuildApp().CreateClient();

            var mappingCandidateKey = updatedMappingType == MappingType.ManualMapped
                ? "filter-1-option-1-key"
                : null;

            List<FilterOptionMappingUpdateRequest> updates =
            [
                new()
                {
                    FilterKey = "filter-1-key",
                    SourceKey = "filter-1-option-1-key",
                    Type = updatedMappingType,
                    CandidateKey = mappingCandidateKey
                }
            ];

            var response = await ApplyBatchFilterOptionMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client);

            response.AssertOk<BatchFilterOptionMappingUpdatesResponseViewModel>();

            var updatedMappings = TestApp.GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Single(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            // Assert that the batch save calculates the LocationMappingsComplete flag as expected given the
            // combination of the requested mapping update and the existing mapping that is untouched. 
            Assert.Equal(expectedMappingsComplete, updatedMappings.FilterMappingsComplete);
        }

        [Theory]
        [InlineData(MappingType.ManualMapped, MappingType.AutoMapped, "1.1.0")]
        [InlineData(MappingType.ManualMapped, MappingType.AutoNone, "2.0.0")]
        [InlineData(MappingType.ManualMapped, MappingType.ManualMapped, "1.1.0")]
        [InlineData(MappingType.ManualMapped, MappingType.ManualNone, "2.0.0")]
        [InlineData(MappingType.ManualNone, MappingType.AutoMapped, "2.0.0")]
        [InlineData(MappingType.ManualNone, MappingType.AutoNone, "2.0.0")]
        [InlineData(MappingType.ManualNone, MappingType.ManualMapped, "2.0.0")]
        [InlineData(MappingType.ManualNone, MappingType.ManualNone, "2.0.0")]
        public async Task Success_VersionUpdate(
            MappingType updatedMappingType,
            MappingType unchangedMappingType,
            string expectedVersion)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(currentDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithFilterMappingPlan(DataFixture
                    .DefaultFilterMappingPlan()
                    .AddFilterMapping("filter-1-key", DataFixture
                        .DefaultFilterMapping()
                        .WithAutoMapped("filter-1-key")
                        .AddOptionMapping("filter-1-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithNoMapping()))
                    .AddFilterMapping("filter-2-key", DataFixture
                        .DefaultFilterMapping()
                        .WithAutoMapped("filter-2-key")
                        .AddOptionMapping("filter-2-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithType(unchangedMappingType)
                            .WithCandidateKey(unchangedMappingType switch
                            {
                                MappingType.ManualMapped or MappingType.AutoMapped => "filter-2-option-1-key",
                                _ => null
                            })))
                    .AddFilterCandidate("filter-1-key", DataFixture
                        .DefaultFilterMappingCandidate()
                        .AddOptionCandidate("filter-1-option-1-key", DataFixture
                            .DefaultMappableFilterOption()))
                    .AddFilterCandidate("filter-2-key", DataFixture
                        .DefaultFilterMappingCandidate()
                        .AddOptionCandidate("filter-2-option-1-key", DataFixture
                            .DefaultMappableFilterOption())));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextDataSetVersion.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithPublicApiDataSetId(nextDataSetVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextDataSetVersion.SemVersion());

            await TestApp.AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            var client = BuildApp().CreateClient();

            var mappingCandidateKey = updatedMappingType == MappingType.ManualMapped
                ? "filter-1-option-1-key"
                : null;

            List<FilterOptionMappingUpdateRequest> updates =
            [
                new()
                {
                    FilterKey = "filter-1-key",
                    SourceKey = "filter-1-option-1-key",
                    Type = updatedMappingType,
                    CandidateKey = mappingCandidateKey
                }
            ];

            var response = await ApplyBatchFilterOptionMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client);

            response.AssertOk<BatchFilterOptionMappingUpdatesResponseViewModel>();

            var updatedMappings = TestApp.GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Include(m => m.TargetDataSetVersion)
                .Single(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            // Assert that the batch save calculates the next version number as expected. 
            Assert.Equal(expectedVersion, updatedMappings.TargetDataSetVersion.SemVersion());

            var updatedReleaseFile = await TestApp.GetDbContext<ContentDbContext>()
                .ReleaseFiles
                .SingleAsync(rf => rf.PublicApiDataSetId == updatedMappings.TargetDataSetVersion.DataSetId);

            Assert.Equal(expectedVersion, updatedReleaseFile.PublicApiDataSetVersion);
        }

        [Theory]
        [InlineData(MappingType.ManualMapped)]
        [InlineData(MappingType.ManualNone)]
        public async Task Success_VersionUpdates_UnmappableFilter(MappingType updatedMappingType)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(currentDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithFilterMappingPlan(DataFixture
                    .DefaultFilterMappingPlan()
                    .AddFilterMapping("filter-1-key", DataFixture
                        .DefaultFilterMapping()
                        .WithAutoMapped("filter-1-key")
                        .AddOptionMapping("filter-1-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithNoMapping()))
                    // Add an unmappable filter and filter options. Unlike the calculation of the
                    // "FilterMappingsComplete" flag, this counts towards the version number having
                    // to be a major update, as a filter from the source data set version no longer
                    // appears in the next version.
                    .AddFilterMapping("filter-3-key", DataFixture
                        .DefaultFilterMapping()
                        .WithAutoNone()
                        .AddOptionMapping("filter-3-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithAutoNone()))
                    .AddFilterCandidate("filter-1-key", DataFixture
                        .DefaultFilterMappingCandidate()
                        .AddOptionCandidate("filter-1-option-1-key", DataFixture
                            .DefaultMappableFilterOption()))
                    .AddFilterCandidate("filter-2-key", DataFixture
                        .DefaultFilterMappingCandidate()
                        .AddOptionCandidate("filter-2-option-1-key", DataFixture
                            .DefaultMappableFilterOption())));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextDataSetVersion.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile(FileType.Data))
                .WithPublicApiDataSetId(nextDataSetVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextDataSetVersion.SemVersion());

            await TestApp.AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            var client = BuildApp().CreateClient();

            var mappingCandidateKey = updatedMappingType == MappingType.ManualMapped
                ? "filter-1-option-1-key"
                : null;

            List<FilterOptionMappingUpdateRequest> updates =
            [
                new()
                {
                    FilterKey = "filter-1-key",
                    SourceKey = "filter-1-option-1-key",
                    Type = updatedMappingType,
                    CandidateKey = mappingCandidateKey
                }
            ];

            var response = await ApplyBatchFilterOptionMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client);

            response.AssertOk<BatchFilterOptionMappingUpdatesResponseViewModel>();

            var updatedMappings = TestApp.GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Include(m => m.TargetDataSetVersion)
                .Single(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            // Assert that the batch save calculates the next version number as a major change,
            // as filter options that were in the source data set version no longer appear in the
            // next version. 
            Assert.Equal("2.0.0", updatedMappings.TargetDataSetVersion.SemVersion());
        }

        [Fact]
        public async Task SourceKeyDoesNotExist_Returns400_AndRollsBackTransaction()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(currentDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithFilterMappingPlan(DataFixture
                    .DefaultFilterMappingPlan()
                    .AddFilterMapping("filter-1-key", DataFixture
                        .DefaultFilterMapping()
                        .WithAutoMapped("filter-1-key")
                        .AddOptionMapping("filter-1-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithAutoMapped("filter-1-option-1-key"))
                        .AddOptionMapping("filter-1-option-2-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithManualNone()))
                    .AddFilterCandidate("filter-1-key", DataFixture
                        .DefaultFilterMappingCandidate()
                        .AddOptionCandidate("filter-1-option-1-key", DataFixture
                            .DefaultMappableFilterOption())
                        .AddOptionCandidate("filter-1-option-2-key", DataFixture
                            .DefaultMappableFilterOption())
                        .AddOptionCandidate("filter-1-option-3-key", DataFixture
                            .DefaultMappableFilterOption())));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            var client = BuildApp().CreateClient();

            List<FilterOptionMappingUpdateRequest> updates =
            [
                // This mapping exists.
                new()
                {
                    FilterKey = "filter-1-key",
                    SourceKey = "filter-1-option-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "filter-1-option-1-key"
                },
                // This mapping does not exist.
                new()
                {
                    FilterKey = "filter-1-key",
                    SourceKey = "filter-1-option-3-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "filter-1-option-2-key"
                }
            ];

            var response = await ApplyBatchFilterOptionMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client);

            var validationProblem = response.AssertValidationProblem();

            // The non-existent mapping in the batch update request should have failure messages
            // indicating that the mappings they were attempting to update do not exist.
            validationProblem.AssertHasError(
                expectedPath: "updates[1].sourceKey",
                expectedCode: nameof(ValidationMessages.DataSetVersionMappingSourcePathDoesNotExist));

            Assert.Single(validationProblem.Errors);

            var retrievedMappings = TestApp.GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Single(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            // Test that the mappings are not updated due to the failures of some of the update requests.
            retrievedMappings.FilterMappingPlan.Mappings.AssertDeepEqualTo(
                mappings.FilterMappingPlan.Mappings,
                ignoreCollectionOrders: true);
        }

        [Fact]
        public async Task CandidateKeyDoesNotExist_Returns400()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(currentDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithFilterMappingPlan(DataFixture
                    .DefaultFilterMappingPlan()
                    .AddFilterMapping("filter-1-key", DataFixture
                        .DefaultFilterMapping()
                        .WithAutoMapped("filter-1-key")
                        .AddOptionMapping("filter-1-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithAutoMapped("filter-1-option-1-key"))
                        .AddOptionMapping("filter-1-option-2-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithManualNone()))
                    .AddFilterMapping("filter-2-key", DataFixture
                        .DefaultFilterMapping()
                        .WithAutoMapped("filter-2-key")
                        .AddOptionMapping("filter-2-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithAutoMapped("filter-2-option-1-key")))
                    .AddFilterCandidate("filter-1-key", DataFixture
                        .DefaultFilterMappingCandidate()
                        .AddOptionCandidate("filter-1-option-1-key", DataFixture
                            .DefaultMappableFilterOption())
                        .AddOptionCandidate("filter-1-option-2-key", DataFixture
                            .DefaultMappableFilterOption()))
                    .AddFilterCandidate("filter-2-key", DataFixture
                        .DefaultFilterMappingCandidate()
                        .AddOptionCandidate("filter-2-option-1-key", DataFixture
                            .DefaultMappableFilterOption())));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            var client = BuildApp().CreateClient();

            List<FilterOptionMappingUpdateRequest> updates =
            [
                // This candidate exists.
                new()
                {
                    FilterKey = "filter-1-key",
                    SourceKey = "filter-1-option-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "filter-1-option-1-key"
                },
                // This candidate does not exist as there is no candidate with the key
                // "Non existent candidate key".  This tests the simple case where a candidate
                // doesn't exist at all with the given key.
                new()
                {
                    FilterKey = "filter-1-key",
                    SourceKey = "filter-1-option-2-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "Non existent candidate key"
                },
                // This candidate does not exist as there is no candidate with the key
                // "Non existent candidate key" under the filter that "filter-2-key" is
                // mapped to, despite there being a filter option candidate that exists
                // under a different filter with the key "filter-1-option-1-key".
                // This tests the more complex case whereby only filter option candidates
                // that exist under the filter that the owning filter is mapped to are
                // considered valid candidates.
                new()
                {
                    FilterKey = "filter-2-key",
                    SourceKey = "filter-2-option-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "filter-1-option-1-key"
                }
            ];

            var response = await ApplyBatchFilterOptionMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client);

            var validationProblem = response.AssertValidationProblem();

            // The 2 non-existent mappings in the batch update request should have failure messages
            // indicating that the mappings they were attempting to update do not exist.
            validationProblem.AssertHasError(
                expectedPath: "updates[1].candidateKey",
                expectedCode: nameof(ValidationMessages.DataSetVersionMappingCandidatePathDoesNotExist));

            validationProblem.AssertHasError(
                expectedPath: "updates[2].candidateKey",
                expectedCode: nameof(ValidationMessages.DataSetVersionMappingCandidatePathDoesNotExist));

            Assert.Equal(2, validationProblem.Errors.Count);

            var retrievedMappings = TestApp.GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Single(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            // Test that the mappings are not updated due to the failures of some of the update requests.
            retrievedMappings.FilterMappingPlan.Mappings.AssertDeepEqualTo(
                mappings.FilterMappingPlan.Mappings,
                ignoreCollectionOrders: true);
        }

        [Fact]
        public async Task OwningFilterNotMapped_Returns400()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion currentDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 0)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

            DataSetVersion nextDataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithVersionNumber(major: 1, minor: 1)
                .WithStatusDraft()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(currentDataSetVersion, nextDataSetVersion);
                context.DataSets.Update(dataSet);
            });

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(currentDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithFilterMappingPlan(DataFixture
                    .DefaultFilterMappingPlan()
                    .AddFilterMapping("filter-1-key", DataFixture
                        .DefaultFilterMapping()
                        .WithManualNone()
                        .AddOptionMapping("filter-1-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithManualNone())
                        .AddOptionMapping("filter-1-option-2-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithManualNone()))
                    .AddFilterCandidate("filter-1-key", DataFixture
                        .DefaultFilterMappingCandidate()
                        .AddOptionCandidate("filter-1-option-1-key", DataFixture
                            .DefaultMappableFilterOption())));

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            var client = BuildApp().CreateClient();

            List<FilterOptionMappingUpdateRequest> updates =
            [
                // This candidate exists, but the filter that owns "filter-1-option-1-key" has not itself
                // been mapped, so it cannot supply any valid candidate filter options.
                new()
                {
                    FilterKey = "filter-1-key",
                    SourceKey = "filter-1-option-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "filter-1-option-1-key"
                }
            ];

            var response = await ApplyBatchFilterOptionMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client);

            var validationProblem = response.AssertValidationProblem();

            // The mappings in the batch update request should have failure messages
            // indicating that the owning filter has not been mapped.
            validationProblem.AssertHasError(
                expectedPath: "updates[0].filterKey",
                expectedCode: nameof(ValidationMessages.OwningFilterNotMapped));

            Assert.Single(validationProblem.Errors);

            var retrievedMappings = TestApp.GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Single(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            // Test that the mappings are not updated due to the failures of some of the update requests.
            retrievedMappings.FilterMappingPlan.Mappings.AssertDeepEqualTo(
                mappings.FilterMappingPlan.Mappings,
                ignoreCollectionOrders: true);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var client = BuildApp(user: AuthenticatedUser()).CreateClient();

            var response = await ApplyBatchFilterOptionMappingUpdates(
                Guid.NewGuid(),
                new List<FilterOptionMappingUpdateRequest>(),
                client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task DataSetVersionMappingDoesNotExist_Returns404()
        {
            var client = BuildApp().CreateClient();

            var response = await ApplyBatchFilterOptionMappingUpdates(
                Guid.NewGuid(),
                new List<FilterOptionMappingUpdateRequest>(),
                client);

            response.AssertNotFound();
        }

        [Fact]
        public async Task EmptyRequiredFields_Return400()
        {
            var client = BuildApp().CreateClient();

            var response = await ApplyBatchFilterOptionMappingUpdates(
                Guid.NewGuid(),
                [
                    new FilterOptionMappingUpdateRequest()
                ],
                client);

            var validationProblem = response.AssertValidationProblem();
            Assert.Equal(3, validationProblem.Errors.Count);
            validationProblem.AssertHasNotEmptyError("updates[0].filterKey");
            validationProblem.AssertHasNotNullError("updates[0].type");
            validationProblem.AssertHasNotEmptyError("updates[0].sourceKey");
        }

        [Theory]
        [InlineData(MappingType.ManualMapped, null)]
        [InlineData(MappingType.ManualMapped, "")]
        public async Task MappingTypeExpectsCandidateKey_Returns400(MappingType type, string? candidateKeyValue)
        {
            var client = BuildApp().CreateClient();

            var response = await ApplyBatchFilterOptionMappingUpdates(
                Guid.NewGuid(),
                [
                    new FilterOptionMappingUpdateRequest
                    {
                        FilterKey = "filter-1",
                        SourceKey = "location-1",
                        Type = type,
                        CandidateKey = candidateKeyValue
                    }
                ],
                client);

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "updates[0].candidateKey",
                expectedCode: ValidationMessages.CandidateKeyMustBeSpecifiedWithMappedMappingType.Code,
                expectedMessage: ValidationMessages.CandidateKeyMustBeSpecifiedWithMappedMappingType.Message);

            Assert.Single(validationProblem.Errors);
        }

        [Theory]
        [InlineData(MappingType.ManualNone)]
        public async Task MappingTypeDoeNotExpectCandidateKey_Returns400(MappingType type)
        {
            var client = BuildApp().CreateClient();

            var response = await ApplyBatchFilterOptionMappingUpdates(
                Guid.NewGuid(),
                [
                    new FilterOptionMappingUpdateRequest
                    {
                        FilterKey = "filter-1",
                        SourceKey = "location-1",
                        Type = type,
                        CandidateKey = "target-location-1"
                    }
                ],
                client);

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "updates[0].candidateKey",
                expectedCode: ValidationMessages.CandidateKeyMustBeEmptyWithNoneMappingType.Code,
                expectedMessage: ValidationMessages.CandidateKeyMustBeEmptyWithNoneMappingType.Message);

            Assert.Single(validationProblem.Errors);
        }

        [Theory]
        [InlineData(MappingType.None)]
        [InlineData(MappingType.AutoMapped)]
        [InlineData(MappingType.AutoNone)]
        public async Task InvalidMappingTypeForManualInteraction_Returns400(MappingType type)
        {
            var client = BuildApp().CreateClient();

            var response = await ApplyBatchFilterOptionMappingUpdates(
                Guid.NewGuid(),
                [
                    new FilterOptionMappingUpdateRequest
                    {
                        FilterKey = "filter-1",
                        SourceKey = "location-1",
                        Type = type,
                        CandidateKey = null
                    }
                ],
                client);

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "updates[0].type",
                expectedCode: ValidationMessages.ManualMappingTypeInvalid.Code,
                expectedMessage: "Type must be one of the following values: ManualMapped, ManualNone");

            Assert.Single(validationProblem.Errors);
        }

        private async Task<HttpResponseMessage> ApplyBatchFilterOptionMappingUpdates(
            Guid nextDataSetVersionId,
            List<FilterOptionMappingUpdateRequest> updates,
            HttpClient? client = null)
        {
            client ??= BuildApp().CreateClient();

            var uri = new Uri($"{BaseUrl}/{nextDataSetVersionId}/mapping/filters/options", UriKind.Relative);

            return await client.PatchAsync(uri,
                new JsonNetContent(new BatchFilterOptionMappingUpdatesRequest { Updates = updates }));
        }
    }

    private WebApplicationFactory<TestStartup> BuildApp(ClaimsPrincipal? user = null)
    {
        return TestApp.SetUser(user ?? BauUser());
    }
}
