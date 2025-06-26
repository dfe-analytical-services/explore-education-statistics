#nullable enable
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

public abstract class OptimisedDataSetVersionMappingControllerTests :
    IClassFixture<OptimisedHttpClientWithPsqlFixture>
{
    private const string BaseUrl = "api/public-data/data-set-versions";

    private readonly DataFixture _dataFixture = new();

    public class GetLocationMappingsTests : OptimisedDataSetVersionMappingControllerTests, IAsyncLifetime
    {
        private readonly OptimisedPostgreSqlContainerUtil _psql;
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _client;

        public async Task InitializeAsync() {
            await _psql.GetDbContext().ClearTestData();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public GetLocationMappingsTests(
            OptimisedHttpClientWithPsqlFixture fixture,
            ITestOutputHelper output)
        {
            _psql = fixture.GetContainer();
            _output = output;
            var sw = new Stopwatch();
            sw.Start();

            _client = fixture.GetClient();
            
            output.WriteLine($"Testapp startup {sw.ElapsedMilliseconds}");

        }

        [Fact]
        public async Task Success()
        {
            var sw = new Stopwatch();
            sw.Start();
            
            var publicDataDbContext = _psql.GetDbContext();

            var (initialDataSetVersion, nextDataSetVersion) = await CreateInitialAndNextDataSetVersion(publicDataDbContext);

            DataSetVersionMapping mapping = _dataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(initialDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithLocationMappingPlan(_dataFixture
                    .DefaultLocationMappingPlan()
                    .AddLevel(
                        level: GeographicLevel.LocalAuthority,
                        mappings: _dataFixture
                            .DefaultLocationLevelMappings()
                            .AddMapping(
                                sourceKey: "source-location-1-key",
                                mapping: _dataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(_dataFixture.DefaultMappableLocationOption())
                                    .WithAutoMapped("target-location-1-key"))
                            .AddMapping(
                                sourceKey: "source-location-2-key",
                                mapping: _dataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(_dataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping()
                            )
                            .AddCandidate(
                                targetKey: "target-location-1-key",
                                candidate: _dataFixture.DefaultMappableLocationOption()))
                    .AddLevel(
                        level: GeographicLevel.Country,
                        mappings: _dataFixture
                            .DefaultLocationLevelMappings()
                            .AddMapping(
                                sourceKey: "source-location-1-key",
                                mapping: _dataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(_dataFixture.DefaultMappableLocationOption())
                                    .WithManualNone())
                            .AddMapping(
                                sourceKey: "source-location-2-key",
                                mapping: _dataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(_dataFixture.DefaultMappableLocationOption())
                                    .WithManualMapped("target-location-1-key")
                            )
                            .AddCandidate(
                                targetKey: "target-location-1-key",
                                candidate: _dataFixture.DefaultMappableLocationOption())));

            await publicDataDbContext.AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            _output.WriteLine($"PSQL set up test data {sw.ElapsedMilliseconds}");
            sw.Restart();

            var client = _client.WithUser("Bau");
            
            _output.WriteLine($"Create client {sw.ElapsedMilliseconds}");
            sw.Restart();

            var response = await GetLocationMappings(
                nextDataSetVersionId: nextDataSetVersion.Id,
                client);

            var retrievedMappings = response.AssertOk<LocationMappingPlan>();

            // Test that the mappings from the Controller are identical to the mappings saved in the database
            retrievedMappings.AssertDeepEqualTo(
                mapping.LocationMappingPlan,
                ignoreCollectionOrders: true);
            
            _output.WriteLine($"Do test {sw.ElapsedMilliseconds}");
            sw.Restart();
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var sw = new Stopwatch();
            sw.Start();

            var client = _client
                .WithUser("Authenticated");

            _output.WriteLine($"Create client {sw.ElapsedMilliseconds}");
            sw.Restart();

            var response = await GetLocationMappings(
                Guid.NewGuid(),
                client);

            response.AssertForbidden();

            _output.WriteLine($"Do test {sw.ElapsedMilliseconds}");
            sw.Restart();
        }

        [Fact]
        public async Task DataSetVersionMappingDoesNotExist_Returns404()
        {
            var sw = new Stopwatch();
            sw.Start();

            var client = _client
                .WithUser("Bau");

            _output.WriteLine($"Create client {sw.ElapsedMilliseconds}");
            sw.Restart();
            
            var response = await GetLocationMappings(
                Guid.NewGuid(),
                client);

            response.AssertNotFound();

            _output.WriteLine($"Do test {sw.ElapsedMilliseconds}");
            sw.Restart();
        }

        private async Task<HttpResponseMessage> GetLocationMappings(
            Guid nextDataSetVersionId,
            HttpClient client)
        {
            var uri = new Uri($"{BaseUrl}/{nextDataSetVersionId}/mapping/locations", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    // public class ApplyBatchLocationMappingUpdatesTests(
    //     TestApplicationFactory2 testApp)
    //     : DataSetVersionMappingControllerTests2
    // {
    //     [Fact]
    //     public async Task Success()
    //     {
    //         var (initialDataSetVersion, nextDataSetVersion) = await CreateInitialAndNextDataSetVersion();
    //
    //         DataSetVersionMapping mapping = DataFixture
    //             .DefaultDataSetVersionMapping()
    //             .WithSourceDataSetVersionId(initialDataSetVersion.Id)
    //             .WithTargetDataSetVersionId(nextDataSetVersion.Id)
    //             .WithLocationMappingPlan(DataFixture
    //                 .DefaultLocationMappingPlan()
    //                 .AddLevel(
    //                     level: GeographicLevel.LocalAuthority,
    //                     mappings: DataFixture
    //                         .DefaultLocationLevelMappings()
    //                         .AddMapping(
    //                             sourceKey: "source-location-1-key",
    //                             mapping: DataFixture
    //                                 .DefaultLocationOptionMapping()
    //                                 .WithSource(DataFixture.DefaultMappableLocationOption())
    //                                 .WithAutoNone())
    //                         .AddMapping(
    //                             sourceKey: "source-location-2-key",
    //                             mapping: DataFixture
    //                                 .DefaultLocationOptionMapping()
    //                                 .WithSource(DataFixture.DefaultMappableLocationOption())
    //                                 .WithAutoNone()
    //                         )
    //                         .AddCandidate(
    //                             targetKey: "target-location-1-key",
    //                             candidate: DataFixture.DefaultMappableLocationOption()))
    //                 .AddLevel(
    //                     level: GeographicLevel.Country,
    //                     mappings: DataFixture
    //                         .DefaultLocationLevelMappings()
    //                         .AddMapping(
    //                             sourceKey: "source-location-1-key",
    //                             mapping: DataFixture
    //                                 .DefaultLocationOptionMapping()
    //                                 .WithSource(DataFixture.DefaultMappableLocationOption())
    //                                 .WithAutoNone())
    //                         .AddMapping(
    //                             sourceKey: "source-location-3-key",
    //                             mapping: DataFixture
    //                                 .DefaultLocationOptionMapping()
    //                                 .WithSource(DataFixture.DefaultMappableLocationOption())
    //                                 .WithAutoMapped("target-location-3-key"))
    //                         .AddCandidate(
    //                             targetKey: "target-location-1-key",
    //                             candidate: DataFixture.DefaultMappableLocationOption())));
    //
    //         await testApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));
    //
    //         List<LocationMappingUpdateRequest> updates =
    //         [
    //             new()
    //             {
    //                 Level = GeographicLevel.LocalAuthority,
    //                 SourceKey = "source-location-1-key",
    //                 Type = MappingType.ManualMapped,
    //                 CandidateKey = "target-location-1-key"
    //             },
    //             new()
    //             {
    //                 Level = GeographicLevel.Country,
    //                 SourceKey = "source-location-3-key",
    //                 Type = MappingType.ManualNone
    //             }
    //         ];
    //
    //         var response = await ApplyBatchLocationMappingUpdates(
    //             nextDataSetVersionId: nextDataSetVersion.Id,
    //             updates: updates);
    //
    //         var viewModel = response.AssertOk<BatchLocationMappingUpdatesResponseViewModel>();
    //
    //         var originalLocalAuthorityMappingToUpdate = mapping
    //             .GetLocationOptionMapping(GeographicLevel.LocalAuthority, "source-location-1-key");
    //
    //         var originalLocalAuthorityMappingNotUpdated = mapping
    //             .GetLocationOptionMapping(GeographicLevel.LocalAuthority, "source-location-2-key");
    //
    //         var originalCountryMappingNotUpdated = mapping
    //             .GetLocationOptionMapping(GeographicLevel.Country, "source-location-1-key");
    //
    //         var originalCountryMappingToUpdate = mapping
    //             .GetLocationOptionMapping(GeographicLevel.Country, "source-location-3-key");
    //
    //         var expectedUpdateResponse = new BatchLocationMappingUpdatesResponseViewModel
    //         {
    //             Updates =
    //             [
    //                 new LocationMappingUpdateResponseViewModel
    //                 {
    //                     Level = GeographicLevel.LocalAuthority,
    //                     SourceKey = "source-location-1-key",
    //                     Mapping = originalLocalAuthorityMappingToUpdate with
    //                     {
    //                         Type = MappingType.ManualMapped,
    //                         CandidateKey = "target-location-1-key"
    //                     }
    //                 },
    //                 new LocationMappingUpdateResponseViewModel
    //                 {
    //                     Level = GeographicLevel.Country,
    //                     SourceKey = "source-location-3-key",
    //                     Mapping = originalCountryMappingToUpdate with
    //                     {
    //                         Type = MappingType.ManualNone,
    //                         CandidateKey = null
    //                     }
    //                 }
    //             ]
    //         };
    //
    //         // Test that the response from the Controller contains details of all the mappings
    //         // that were updated.
    //         viewModel.AssertDeepEqualTo(expectedUpdateResponse, ignoreCollectionOrders: true);
    //
    //         var updatedMapping = await testApp.GetDbContext<PublicDataDbContext>()
    //             .DataSetVersionMappings
    //             .Include(m => m.TargetDataSetVersion)
    //             .SingleAsync(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);
    //
    //         var expectedFullMappings = new Dictionary<GeographicLevel, LocationLevelMappings>
    //         {
    //             {
    //                 GeographicLevel.LocalAuthority, new LocationLevelMappings
    //                 {
    //                     Mappings = new Dictionary<string, LocationOptionMapping>
    //                     {
    //                         // We expect this mapping's type ot be set to ManualMapped and
    //                         // its CandidateKey set.
    //                         {
    //                             "source-location-1-key",
    //                             originalLocalAuthorityMappingToUpdate with
    //                             {
    //                                 Type = MappingType.ManualMapped,
    //                                 CandidateKey = "target-location-1-key"
    //                             }
    //                         },
    //                         { "source-location-2-key", originalLocalAuthorityMappingNotUpdated }
    //                     },
    //                     Candidates = mapping
    //                         .LocationMappingPlan
    //                         .Levels[GeographicLevel.LocalAuthority]
    //                         .Candidates
    //                 }
    //             },
    //             {
    //                 GeographicLevel.Country, new LocationLevelMappings
    //                 {
    //                     Mappings = new Dictionary<string, LocationOptionMapping>
    //                     {
    //                         { "source-location-1-key", originalCountryMappingNotUpdated },
    //                         {
    //                             // We expect this mapping's type to be set to ManualNone and
    //                             // its CandidateKey unset.
    //                             "source-location-3-key",
    //                             originalCountryMappingToUpdate with
    //                             {
    //                                 Type = MappingType.ManualNone,
    //                                 CandidateKey = null
    //                             }
    //                         }
    //                     },
    //                     Candidates = mapping
    //                         .LocationMappingPlan
    //                         .Levels[GeographicLevel.Country]
    //                         .Candidates
    //                 }
    //             }
    //         };
    //
    //         // Test that the updated mappings retrieved from the database reflect the updates
    //         // that were requested.
    //         updatedMapping.LocationMappingPlan.Levels.AssertDeepEqualTo(
    //             expectedFullMappings,
    //             ignoreCollectionOrders: true);
    //
    //         // Assert that the batch saves still show the location mappings as incomplete, as there
    //         // are still mappings with type "AutoNone" in the plan.
    //         Assert.False(updatedMapping.LocationMappingsComplete);
    //
    //         // Assert that this update constitutes a major version update, as some locations options
    //         // are 'ManualNone', indicating that some of the source location options may have been
    //         // removed thus creating a breaking change.
    //         Assert.Equal("2.0.0", updatedMapping.TargetDataSetVersion.SemVersion().ToString());
    //     }
    //
    //     [Theory]
    //     [InlineData(MappingType.ManualMapped, MappingType.AutoNone, false, "2.0.0")]
    //     [InlineData(MappingType.ManualMapped, MappingType.AutoMapped, true, "1.1.0")]
    //     public async Task Success_MappingsCompleteAndVersionUpdated(
    //         MappingType updatedMappingType,
    //         MappingType unchangedMappingType,
    //         bool expectedMappingsComplete,
    //         string expectedVersion)
    //     {
    //         var (initialDataSetVersion, nextDataSetVersion) = await CreateInitialAndNextDataSetVersion();
    //
    //         DataSetVersionMapping mapping = DataFixture
    //             .DefaultDataSetVersionMapping()
    //             .WithSourceDataSetVersionId(initialDataSetVersion.Id)
    //             .WithTargetDataSetVersionId(nextDataSetVersion.Id)
    //             .WithLocationMappingPlan(DataFixture
    //                 .DefaultLocationMappingPlan()
    //                 .AddLevel(
    //                     level: GeographicLevel.LocalAuthority,
    //                     mappings: DataFixture
    //                         .DefaultLocationLevelMappings()
    //                         .AddMapping(
    //                             sourceKey: "source-location-1-key",
    //                             mapping: DataFixture
    //                                 .DefaultLocationOptionMapping()
    //                                 .WithSource(DataFixture.DefaultMappableLocationOption())
    //                                 .WithNoMapping())
    //                         .AddCandidate(
    //                             targetKey: "target-location-1-key",
    //                             candidate: DataFixture.DefaultMappableLocationOption()))
    //                 .AddLevel(
    //                     level: GeographicLevel.Country,
    //                     mappings: DataFixture
    //                         .DefaultLocationLevelMappings()
    //                         .AddMapping(
    //                             sourceKey: "source-location-1-key",
    //                             mapping: DataFixture
    //                                 .DefaultLocationOptionMapping()
    //                                 .WithSource(DataFixture.DefaultMappableLocationOption())
    //                                 .WithType(unchangedMappingType)
    //                                 .WithCandidateKey(unchangedMappingType switch
    //                                 {
    //                                     MappingType.ManualMapped or MappingType.AutoMapped => "target-location-1-key",
    //                                     _ => null
    //                                 }))
    //                         .AddCandidate(
    //                             targetKey: "target-location-1-key",
    //                             candidate: DataFixture.DefaultMappableLocationOption())));
    //
    //         await testApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));
    //
    //         List<LocationMappingUpdateRequest> updates =
    //         [
    //             new()
    //             {
    //                 Level = GeographicLevel.LocalAuthority,
    //                 SourceKey = "source-location-1-key",
    //                 Type = updatedMappingType,
    //                 CandidateKey = updatedMappingType == MappingType.ManualMapped
    //                     ? "target-location-1-key"
    //                     : null
    //             }
    //         ];
    //
    //         var response = await ApplyBatchLocationMappingUpdates(
    //             nextDataSetVersionId: nextDataSetVersion.Id,
    //             updates: updates);
    //
    //         response.AssertOk<BatchLocationMappingUpdatesResponseViewModel>();
    //
    //         var updatedMapping = await testApp.GetDbContext<PublicDataDbContext>()
    //             .DataSetVersionMappings
    //             .Include(m => m.TargetDataSetVersion)
    //             .SingleAsync(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);
    //
    //         Assert.Equal(expectedMappingsComplete, updatedMapping.LocationMappingsComplete);
    //
    //         await AssertCorrectDataSetVersionNumbers(updatedMapping, expectedVersion);
    //     }
    //     
    //     private async Task<HttpResponseMessage> ApplyBatchLocationMappingUpdates(
    //         Guid nextDataSetVersionId,
    //         List<LocationMappingUpdateRequest> updates,
    //         HttpClient? client = null)
    //     {
    //         client ??= testApp.CreateClient();
    //
    //         var uri = new Uri($"{BaseUrl}/{nextDataSetVersionId}/mapping/locations", UriKind.Relative);
    //
    //         return await client.PatchAsync(uri,
    //             new JsonNetContent(new BatchLocationMappingUpdatesRequest { Updates = updates }));
    //     }
    // }

    private async Task AssertCorrectDataSetVersionNumbers(
        DataSetVersionMapping mapping,
        string expectedVersion,
        WebApplicationFactory<TestStartup> testApp)
    {
        Assert.Equal(expectedVersion, mapping.TargetDataSetVersion.SemVersion().ToString());

        var updatedReleaseFile = await testApp.GetDbContext<ContentDbContext, TestStartup>()
            .ReleaseFiles
            .SingleAsync(rf => rf.PublicApiDataSetId == mapping.TargetDataSetVersion.DataSetId);

        Assert.Equal(expectedVersion, updatedReleaseFile.PublicApiDataSetVersion?.ToString());
    }

    private async Task<(DataSetVersion initialVersion, DataSetVersion nextVersion)> 
        CreateInitialAndNextDataSetVersion(PublicDataDbContext publicDataDbContext)
    {
        DataSet dataSet = _dataFixture
            .DefaultDataSet()
            .WithStatusPublished();

        await publicDataDbContext.AddTestData(context => context.DataSets.Add(dataSet));

        DataSetVersion initialDataSetVersion = _dataFixture
            .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
            .WithVersionNumber(major: 1, minor: 0)
            .WithStatusPublished()
            .WithDataSet(dataSet)
            .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

        DataSetVersion nextDataSetVersion = _dataFixture
            .DefaultDataSetVersion()
            .WithVersionNumber(major: 1, minor: 1)
            .WithStatusDraft()
            .WithDataSet(dataSet)
            .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

        await publicDataDbContext.AddTestData(context =>
        {
            context.DataSetVersions.AddRange(initialDataSetVersion, nextDataSetVersion);
            context.DataSets.Update(dataSet);
        });

        ReleaseFile releaseFile = _dataFixture.DefaultReleaseFile()
            .WithId(nextDataSetVersion.Release.ReleaseFileId)
            .WithReleaseVersion(_dataFixture.DefaultReleaseVersion())
            .WithFile(_dataFixture.DefaultFile(FileType.Data))
            .WithPublicApiDataSetId(nextDataSetVersion.DataSetId)
            .WithPublicApiDataSetVersion(nextDataSetVersion.SemVersion());

        // await psql.AddTestData<ContentDbContext, Startup>(context => context.ReleaseFiles.Add(releaseFile));

        return (initialDataSetVersion, nextDataSetVersion);
    }

    // private WebApplicationFactory<TestStartup> BuildApp(ClaimsPrincipal? user = null)
    // {
    //     return testApp.SetUser(user ?? DataFixture.BauUser());
    // }
}
