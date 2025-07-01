#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data.OptimisedDataSetVersionMappingControllerTestsHelpers;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

[Collection(nameof(OptimisedDataSetVersionMappingControllerTests))]
public abstract class OptimisedDataSetVersionMappingControllerTests
{
    public class OptimisedGetLocationMappingsTests : OptimisedDataSetVersionMappingControllerTests, IAsyncLifetime
    {
        private const string BaseUrl = "api/public-data/data-set-versions";

        private readonly DataFixture _dataFixture = new();

        private readonly OptimisedPostgreSqlContainerUtil _psql;
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _client;
        private readonly OptimisedHttpClientWithPsqlFixture _fixture;

        public async Task InitializeAsync()
        {
            var sw = new Stopwatch();
            sw.Start();

            await _fixture.GetPublicDataDbContext().ClearTestData();

            _output.WriteLine($"Clear up test data {sw.ElapsedMilliseconds}");
            sw.Restart();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public OptimisedGetLocationMappingsTests(
            OptimisedHttpClientWithPsqlFixture fixture,
            ITestOutputHelper output)
        {
            _fixture = fixture;
            _psql = fixture.GetContainer();
            _output = output;

            var sw = new Stopwatch();
            sw.Start();

            _client = fixture.CreateClient();

            output.WriteLine($"Create client {sw.ElapsedMilliseconds}");
        }

        [Fact]
        public async Task Success()
        {
            var totalSw = new Stopwatch();
            totalSw.Start();

            var sw = new Stopwatch();
            sw.Start();

            var (initialDataSetVersion, nextDataSetVersion) = await CreateInitialAndNextDataSetVersion(
                _fixture.GetPublicDataDbContext(), _fixture.GetContentDbContext(), _output);

            _output.WriteLine($"Set up initial data set version test data {sw.ElapsedMilliseconds}");
            sw.Restart();

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

            _output.WriteLine($"Set up in-memory test data {sw.ElapsedMilliseconds}");
            sw.Restart();

            await _fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            _output.WriteLine($"PSQL set up test data {sw.ElapsedMilliseconds}");
            sw.Restart();

            var client = _client.WithUser("Bau");

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

            _output.WriteLine($"Total test run time {totalSw.ElapsedMilliseconds}");
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var sw = new Stopwatch();
            sw.Start();

            var response = await GetLocationMappings(
                Guid.NewGuid(),
                _fixture.CreateClient().WithUser("Authenticated"));

            response.AssertForbidden();

            _output.WriteLine($"Do test {sw.ElapsedMilliseconds}");
            sw.Restart();
        }

        [Fact]
        public async Task DataSetVersionMappingDoesNotExist_Returns404()
        {
            var sw = new Stopwatch();
            sw.Start();

            var response = await GetLocationMappings(
                Guid.NewGuid(),
                _fixture.CreateClient().WithUser("Bau"));

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

    public class OptimisedApplyBatchLocationMappingUpdatesTests : OptimisedDataSetVersionMappingControllerTests,
        IAsyncLifetime
    {
        private const string BaseUrl = "api/public-data/data-set-versions";

        private readonly DataFixture _dataFixture = new();

        private readonly OptimisedPostgreSqlContainerUtil _psql;
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _client;
        private readonly OptimisedHttpClientWithPsqlFixture _fixture;

        public OptimisedApplyBatchLocationMappingUpdatesTests(
            OptimisedHttpClientWithPsqlFixture fixture,
            ITestOutputHelper output)
        {
            _fixture = fixture;
            _psql = fixture.GetContainer();
            _output = output;

            var sw = new Stopwatch();
            sw.Start();

            _client = fixture.CreateClient();

            output.WriteLine($"Create client {sw.ElapsedMilliseconds}");
        }

        public async Task InitializeAsync()
        {
            var sw = new Stopwatch();
            sw.Start();

            await _fixture.GetPublicDataDbContext().ClearTestData();

            _output.WriteLine($"Clear up test data {sw.ElapsedMilliseconds}");
            sw.Restart();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Success()
        {
            var (initialDataSetVersion, nextDataSetVersion) = await CreateInitialAndNextDataSetVersion(
                _fixture.GetPublicDataDbContext(), _fixture.GetContentDbContext(), _output);

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
                                    .WithAutoNone())
                            .AddMapping(
                                sourceKey: "source-location-2-key",
                                mapping: _dataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(_dataFixture.DefaultMappableLocationOption())
                                    .WithAutoNone()
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
                                    .WithAutoNone())
                            .AddMapping(
                                sourceKey: "source-location-3-key",
                                mapping: _dataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(_dataFixture.DefaultMappableLocationOption())
                                    .WithAutoMapped("target-location-3-key"))
                            .AddCandidate(
                                targetKey: "target-location-1-key",
                                candidate: _dataFixture.DefaultMappableLocationOption())));

            await _fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

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
                _client.WithUser("Bau"));

            var viewModel = response.AssertOk<BatchLocationMappingUpdatesResponseViewModel>();

            var originalLocalAuthorityMappingToUpdate = mapping
                .GetLocationOptionMapping(GeographicLevel.LocalAuthority, "source-location-1-key");

            var originalLocalAuthorityMappingNotUpdated = mapping
                .GetLocationOptionMapping(GeographicLevel.LocalAuthority, "source-location-2-key");

            var originalCountryMappingNotUpdated = mapping
                .GetLocationOptionMapping(GeographicLevel.Country, "source-location-1-key");

            var originalCountryMappingToUpdate = mapping
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

            // _fixture.GetPublicDataDbContext().ChangeTracker.Clear();

            var updatedMapping = await _fixture.GetPublicDataDbContext()
                .DataSetVersionMappings
                .Include(m => m.TargetDataSetVersion)
                .SingleAsync(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

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
                        Candidates = mapping
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
                        Candidates = mapping
                            .LocationMappingPlan
                            .Levels[GeographicLevel.Country]
                            .Candidates
                    }
                }
            };

            // Test that the updated mappings retrieved from the database reflect the updates
            // that were requested.
            updatedMapping.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedFullMappings,
                ignoreCollectionOrders: true);

            // Assert that the batch saves still show the location mappings as incomplete, as there
            // are still mappings with type "AutoNone" in the plan.
            Assert.False(updatedMapping.LocationMappingsComplete);

            // Assert that this update constitutes a major version update, as some locations options
            // are 'ManualNone', indicating that some of the source location options may have been
            // removed thus creating a breaking change.
            Assert.Equal("2.0.0", updatedMapping.TargetDataSetVersion.SemVersion().ToString());
        }

        [Theory]
        [InlineData(MappingType.ManualMapped, MappingType.AutoNone, false, "2.0.0")]
        [InlineData(MappingType.ManualMapped, MappingType.AutoMapped, true, "1.1.0")]
        [InlineData(MappingType.ManualMapped, MappingType.ManualMapped, true, "1.1.0")]
        [InlineData(MappingType.ManualMapped, MappingType.ManualNone, true, "2.0.0")]
        [InlineData(MappingType.ManualNone, MappingType.AutoNone, false, "2.0.0")]
        [InlineData(MappingType.ManualNone, MappingType.AutoMapped, true, "2.0.0")]
        [InlineData(MappingType.ManualNone, MappingType.ManualMapped, true, "2.0.0")]
        [InlineData(MappingType.ManualNone, MappingType.ManualNone, true, "2.0.0")]
        public async Task Success_MappingsCompleteAndVersionUpdated(
            MappingType updatedMappingType,
            MappingType unchangedMappingType,
            bool expectedMappingsComplete,
            string expectedVersion)
        {
            var (initialDataSetVersion, nextDataSetVersion) = await CreateInitialAndNextDataSetVersion(
                _fixture.GetPublicDataDbContext(), _fixture.GetContentDbContext(), _output);

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
                                    .WithNoMapping())
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
                                    .WithType(unchangedMappingType)
                                    .WithCandidateKey(unchangedMappingType switch
                                    {
                                        MappingType.ManualMapped or MappingType.AutoMapped => "target-location-1-key",
                                        _ => null
                                    }))
                            .AddCandidate(
                                targetKey: "target-location-1-key",
                                candidate: _dataFixture.DefaultMappableLocationOption())));

            await _fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            List<LocationMappingUpdateRequest> updates =
            [
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-location-1-key",
                    Type = updatedMappingType,
                    CandidateKey = updatedMappingType == MappingType.ManualMapped
                        ? "target-location-1-key"
                        : null
                }
            ];

            var response = await ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                _client.WithUser("Bau"));

            response.AssertOk<BatchLocationMappingUpdatesResponseViewModel>();

            var updatedMapping = await _fixture.GetPublicDataDbContext()
                .DataSetVersionMappings
                .Include(m => m.TargetDataSetVersion)
                .SingleAsync(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            Assert.Equal(expectedMappingsComplete, updatedMapping.LocationMappingsComplete);

            await AssertCorrectDataSetVersionNumbers(updatedMapping, expectedVersion, _fixture.GetContentDbContext());
        }

        [Fact]
        public async Task Success_MappedLocation_HasDeletedLocationLevels_MajorUpdate()
        {
            var (initialDataSetVersion, nextDataSetVersion) =
                await CreateInitialAndNextDataSetVersion(_fixture.GetPublicDataDbContext(),
                    _fixture.GetContentDbContext(), _output);

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
                                    .WithNoMapping())
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
                                    .WithAutoNone())));

            await _fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            List<LocationMappingUpdateRequest> updates =
            [
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-location-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-location-1-key"
                }
            ];

            var response = await ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client: _fixture.CreateClient().WithUser("Bau"));

            response.AssertOk<BatchLocationMappingUpdatesResponseViewModel>();

            var updatedMapping = await _fixture.GetPublicDataDbContext()
                .DataSetVersionMappings
                .Include(m => m.TargetDataSetVersion)
                .SingleAsync(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            // This update completes the mapping but as there's a
            // location level deletion, it's a major version update.
            Assert.True(updatedMapping.LocationMappingsComplete);

            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0", _fixture.GetContentDbContext());
        }

        [Fact]
        public async Task Success_MappedLocation_HasDeletedIndicators_MajorUpdate()
        {
            var (initialDataSetVersion, nextDataSetVersion) =
                await CreateInitialAndNextDataSetVersion(_fixture.GetPublicDataDbContext(),
                    _fixture.GetContentDbContext(), _output);

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
                                    .WithNoMapping())
                            .AddCandidate(
                                targetKey: "target-location-1-key",
                                candidate: _dataFixture.DefaultMappableLocationOption())))
                // There are deleted indicators that cannot be mapped.
                .WithHasDeletedIndicators(true);

            await _fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            List<LocationMappingUpdateRequest> updates =
            [
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-location-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-location-1-key"
                }
            ];

            var response = await ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client: _fixture.CreateClient().WithUser("Bau"));

            response.AssertOk<BatchLocationMappingUpdatesResponseViewModel>();

            var updatedMapping = await _fixture.GetPublicDataDbContext()
                .DataSetVersionMappings
                .Include(m => m.TargetDataSetVersion)
                .SingleAsync(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            Assert.True(updatedMapping.LocationMappingsComplete);

            // This update completes the mapping and would normally be a minor version
            // update, but the deleted indicators mean this is still a major version update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0", _fixture.GetContentDbContext());
        }

        [Fact]
        public async Task Success_MappedLocation_HasDeletedGeographicLevels_MajorUpdate()
        {
            var (initialDataSetVersion, nextDataSetVersion) =
                await CreateInitialAndNextDataSetVersion(_fixture.GetPublicDataDbContext(),
                    _fixture.GetContentDbContext(), _output);

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
                                    .WithNoMapping())
                            .AddCandidate(
                                targetKey: "target-location-1-key",
                                candidate: _dataFixture.DefaultMappableLocationOption())))
                // There are deleted geographic levels that cannot be mapped.
                .WithHasDeletedGeographicLevels(true);

            await _fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            List<LocationMappingUpdateRequest> updates =
            [
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-location-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-location-1-key"
                }
            ];

            var response = await ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client: _fixture.CreateClient().WithUser("Bau"));

            response.AssertOk<BatchLocationMappingUpdatesResponseViewModel>();

            var updatedMapping = await _fixture.GetPublicDataDbContext()
                .DataSetVersionMappings
                .Include(m => m.TargetDataSetVersion)
                .SingleAsync(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            Assert.True(updatedMapping.LocationMappingsComplete);

            // This update completes the mapping and would normally be a minor version
            // update, but the deleted geographic levels mean this is still a major version update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0", _fixture.GetContentDbContext());
        }

        [Fact]
        public async Task Success_MappedLocation_HasDeletedTimePeriods_MajorUpdate()
        {
            var (initialDataSetVersion, nextDataSetVersion) =
                await CreateInitialAndNextDataSetVersion(_fixture.GetPublicDataDbContext(),
                    _fixture.GetContentDbContext(), _output);

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
                                    .WithNoMapping())
                            .AddCandidate(
                                targetKey: "target-location-1-key",
                                candidate: _dataFixture.DefaultMappableLocationOption())))
                // There are deleted time periods that cannot be mapped.
                .WithHasDeletedTimePeriods(true);

            await _fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            List<LocationMappingUpdateRequest> updates =
            [
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-location-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-location-1-key"
                }
            ];

            var response = await ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client: _fixture.CreateClient().WithUser("Bau"));

            response.AssertOk<BatchLocationMappingUpdatesResponseViewModel>();

            var updatedMapping = await _fixture.GetPublicDataDbContext()
                .DataSetVersionMappings
                .Include(m => m.TargetDataSetVersion)
                .SingleAsync(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            Assert.True(updatedMapping.LocationMappingsComplete);

            // This update completes the mapping and would normally be a minor version
            // update, but the deleted time periods mean this is still a major version update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0", _fixture.GetContentDbContext());
        }


        [Fact]
        public async Task SourceKeyDoesNotExist_Returns400_AndRollsBackTransaction()
        {
            var (initialDataSetVersion, nextDataSetVersion) =
                await CreateInitialAndNextDataSetVersion(_fixture.GetPublicDataDbContext(),
                    _fixture.GetContentDbContext(), _output);

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
                                sourceKey: "source-la-location-1-key",
                                mapping: _dataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(_dataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping())
                            .AddMapping(
                                sourceKey: "source-la-location-2-key",
                                mapping: _dataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(_dataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping()
                            )
                            .AddCandidate(
                                targetKey: "target-la-location-1-key",
                                candidate: _dataFixture.DefaultMappableLocationOption())
                            .AddCandidate(
                                targetKey: "target-la-location-2-key",
                                candidate: _dataFixture.DefaultMappableLocationOption()))
                    .AddLevel(
                        level: GeographicLevel.Country,
                        mappings: _dataFixture
                            .DefaultLocationLevelMappings()
                            .AddCandidate(
                                targetKey: "target-country-location-1-key",
                                candidate: _dataFixture.DefaultMappableLocationOption())));

            await _fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

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
                client: _fixture.CreateClient().WithUser("Bau"));

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

            var retrievedMappings = await _fixture.GetPublicDataDbContext()
                .DataSetVersionMappings
                .SingleAsync(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            // Test that the mappings are not updated due to the failures of some of the update requests.
            retrievedMappings.LocationMappingPlan.Levels.AssertDeepEqualTo(
                mapping.LocationMappingPlan.Levels,
                ignoreCollectionOrders: true);
        }

        [Fact]
        public async Task CandidateKeyDoesNotExist_Returns400()
        {
            var (initialDataSetVersion, nextDataSetVersion) =
                await CreateInitialAndNextDataSetVersion(_fixture.GetPublicDataDbContext(),
                    _fixture.GetContentDbContext(), _output);

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
                                sourceKey: "source-la-location-1-key",
                                mapping: _dataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(_dataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping())
                            .AddMapping(
                                sourceKey: "source-la-location-2-key",
                                mapping: _dataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(_dataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping()
                            )
                            .AddMapping(
                                sourceKey: "source-la-location-3-key",
                                mapping: _dataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(_dataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping()
                            )
                            .AddCandidate(
                                targetKey: "target-la-location-1-key",
                                candidate: _dataFixture.DefaultMappableLocationOption()))
                    .AddLevel(
                        level: GeographicLevel.Country,
                        mappings: _dataFixture
                            .DefaultLocationLevelMappings()
                            .AddCandidate(
                                targetKey: "target-country-location-1-key",
                                candidate: _dataFixture.DefaultMappableLocationOption())));

            await _fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

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
                client: _fixture.CreateClient().WithUser("Bau"));

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

            var retrievedMappings = await _fixture.GetPublicDataDbContext()
                .DataSetVersionMappings
                .SingleAsync(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            // Test that the mappings are not updated due to the failures of some of the update requests.
            retrievedMappings.LocationMappingPlan.Levels.AssertDeepEqualTo(
                mapping.LocationMappingPlan.Levels,
                ignoreCollectionOrders: true);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var response = await ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: Guid.NewGuid(),
                updates: [],
                client: _fixture.CreateClient().WithUser("Authenticated"));

            response.AssertForbidden();
        }

        [Fact]
        public async Task DataSetVersionMappingDoesNotExist_Returns404()
        {
            var response = await ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: Guid.NewGuid(),
                updates: [],
                client: _fixture.CreateClient().WithUser("Bau"));

            response.AssertNotFound();
        }

        [Fact]
        public async Task EmptyRequiredFields_Return400()
        {
            var response = await ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: Guid.NewGuid(),
                updates:
                [
                    new LocationMappingUpdateRequest()
                ],
                client: _fixture.CreateClient().WithUser("Bau"));

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
            var response = await ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: Guid.NewGuid(),
                updates:
                [
                    new LocationMappingUpdateRequest
                    {
                        Level = GeographicLevel.LocalAuthority,
                        SourceKey = "location-1",
                        Type = type,
                        CandidateKey = candidateKeyValue
                    }
                ],
                client: _fixture.CreateClient().WithUser("Bau"));

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
            var response = await ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: Guid.NewGuid(),
                updates:
                [
                    new LocationMappingUpdateRequest
                    {
                        Level = GeographicLevel.LocalAuthority,
                        SourceKey = "location-1",
                        Type = type,
                        CandidateKey = "target-location-1"
                    }
                ],
                client: _fixture.CreateClient().WithUser("Bau"));

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
            var response = await ApplyBatchLocationMappingUpdates(
                nextDataSetVersionId: Guid.NewGuid(),
                updates:
                [
                    new LocationMappingUpdateRequest
                    {
                        Level = GeographicLevel.LocalAuthority,
                        SourceKey = "location-1",
                        Type = type,
                        CandidateKey = null
                    }
                ],
                client: _fixture.CreateClient().WithUser("Bau"));

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
            HttpClient client)
        {
            var uri = new Uri($"{BaseUrl}/{nextDataSetVersionId}/mapping/locations", UriKind.Relative);

            return await client.PatchAsync(uri,
                new JsonNetContent(new BatchLocationMappingUpdatesRequest { Updates = updates }));
        }
    }
    
    [CollectionDefinition(nameof(OptimisedDataSetVersionMappingControllerTests))]
    public class
        OptimisedDataSetVersionMappingControllerCollection : ICollectionFixture<OptimisedHttpClientWithPsqlFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}

static class OptimisedDataSetVersionMappingControllerTestsHelpers
{
    private static readonly DataFixture DataFixture = new();

    public static async Task AssertCorrectDataSetVersionNumbers(
        DataSetVersionMapping mapping,
        string expectedVersion,
        ContentDbContext contentDbContext)
    {
        Assert.Equal(expectedVersion, mapping.TargetDataSetVersion.SemVersion().ToString());

        var updatedReleaseFile = await contentDbContext
            .ReleaseFiles
            .SingleAsync(rf => rf.PublicApiDataSetId == mapping.TargetDataSetVersion.DataSetId);

        Assert.Equal(expectedVersion, updatedReleaseFile.PublicApiDataSetVersion?.ToString());
    }

    public static async Task<(DataSetVersion initialVersion, DataSetVersion nextVersion)>
        CreateInitialAndNextDataSetVersion(
            PublicDataDbContext publicDataDbContext,
            ContentDbContext contentDbContext,
            ITestOutputHelper output)
    {
        var sw = new Stopwatch();
        sw.Start();

        DataSet dataSet = DataFixture
            .DefaultDataSet()
            .WithStatusPublished();

        output.WriteLine($"  Creating in-memory Data Set {sw.ElapsedMilliseconds}");
        sw.Restart();

        await publicDataDbContext.AddTestData(context => context.DataSets.Add(dataSet));

        output.WriteLine($"  Saving initial Data Set {sw.ElapsedMilliseconds}");
        sw.Restart();

        DataSetVersion initialDataSetVersion = DataFixture
            .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
            .WithVersionNumber(major: 1, minor: 0)
            .WithStatusPublished()
            .WithDataSet(dataSet)
            .FinishWith(dsv => dsv.DataSet.LatestLiveVersion = dsv);

        DataSetVersion nextDataSetVersion = DataFixture
            .DefaultDataSetVersion()
            .WithVersionNumber(major: 1, minor: 1)
            .WithStatusDraft()
            .WithDataSet(dataSet)
            .FinishWith(dsv => dsv.DataSet.LatestDraftVersion = dsv);

        output.WriteLine($"  Creating in-memory Data Set Versions {sw.ElapsedMilliseconds}");
        sw.Restart();

        await publicDataDbContext.AddTestData(context =>
        {
            context.DataSetVersions.AddRange(initialDataSetVersion, nextDataSetVersion);
            context.DataSets.Update(dataSet);
        });

        output.WriteLine($"  Saving Data Set Versions {sw.ElapsedMilliseconds}");
        sw.Restart();

        ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
            .WithId(nextDataSetVersion.Release.ReleaseFileId)
            .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
            .WithFile(DataFixture.DefaultFile(FileType.Data))
            .WithPublicApiDataSetId(nextDataSetVersion.DataSetId)
            .WithPublicApiDataSetVersion(nextDataSetVersion.SemVersion());

        await contentDbContext.AddTestData(context => context.ReleaseFiles.Add(releaseFile));

        output.WriteLine($"  Saving Release File {sw.ElapsedMilliseconds}");
        sw.Restart();

        return (initialDataSetVersion, nextDataSetVersion);
    }
}

