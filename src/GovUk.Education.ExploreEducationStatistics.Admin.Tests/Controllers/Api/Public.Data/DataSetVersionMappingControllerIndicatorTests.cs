#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data.DataSetVersionMappingControllerIndicatorTestsHelpers;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

// TODO EES-6764 - remove null-forgiving operators when IndicatorMappings are non-nullable.
// ReSharper disable once ClassNeverInstantiated.Global
public class DataSetVersionMappingControllerIndicatorTestsFixture()
    : OptimisedAdminCollectionFixture(
        capabilities: [AdminIntegrationTestCapability.UserAuth, AdminIntegrationTestCapability.Postgres]
    );

[CollectionDefinition(nameof(DataSetVersionMappingControllerIndicatorTestsFixture))]
public class DataSetVersionMappingControllerIndicatorTestsCollection
    : ICollectionFixture<DataSetVersionMappingControllerIndicatorTestsFixture>;

[Collection(nameof(DataSetVersionMappingControllerIndicatorTestsFixture))]
public abstract class DataSetVersionMappingControllerIndicatorTests(
    DataSetVersionMappingControllerIndicatorTestsFixture fixture
) : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private const string BaseUrl = "api/public-data/data-set-versions";
    private static readonly DataFixture DataFixture = new();

    public class GetIndicatorMappingsTests(DataSetVersionMappingControllerIndicatorTestsFixture fixture)
        : DataSetVersionMappingControllerIndicatorTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var (initialDataSetVersion, nextDataSetVersion) = await CreateInitialAndNextDataSetVersion(
                fixture.GetPublicDataDbContext(),
                fixture.GetContentDbContext()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(initialDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithIndicatorMappingPlan(
                    DataFixture
                        .DefaultIndicatorMappingPlan()
                        .AddIndicatorMapping(
                            "indicator-1-key",
                            DataFixture.DefaultIndicatorMapping().WithAutoMapped("indicator-1-key")
                        )
                        .AddIndicatorMapping(
                            "indicator-2-key",
                            DataFixture.DefaultIndicatorMapping().WithAutoMapped("indicator-2-key")
                        )
                );

            await fixture
                .GetPublicDataDbContext()
                .AddTestData(context =>
                {
                    context.DataSetVersionMappings.Add(mapping);
                });

            var response = await GetIndicatorMappings(nextDataSetVersionId: nextDataSetVersion.Id);

            var retrievedMappings = response.AssertOk<IndicatorMappingPlan>();

            // Test that the mappings from the Controller are identical to the mappings saved in the database
            retrievedMappings.AssertDeepEqualTo(mapping.IndicatorMappingPlan, ignoreCollectionOrders: true);
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var response = await GetIndicatorMappings(
                nextDataSetVersionId: Guid.NewGuid(),
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
        }

        [Fact]
        public async Task DataSetVersionMappingDoesNotExist_Returns404()
        {
            var response = await GetIndicatorMappings(Guid.NewGuid());

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> GetIndicatorMappings(
            Guid nextDataSetVersionId,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            var uri = new Uri($"{BaseUrl}/{nextDataSetVersionId}/mapping/indicators", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    public class ApplyBatchIndicatorMappingUpdatesTests(DataSetVersionMappingControllerIndicatorTestsFixture fixture)
        : DataSetVersionMappingControllerIndicatorTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var (initialDataSetVersion, nextDataSetVersion) = await CreateInitialAndNextDataSetVersion(
                fixture.GetPublicDataDbContext(),
                fixture.GetContentDbContext()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(initialDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithIndicatorMappingPlan(
                    DataFixture
                        .DefaultIndicatorMappingPlan()
                        .AddIndicatorMapping(
                            "indicator-1-key",
                            DataFixture.DefaultIndicatorMapping().WithAutoMapped("indicator-1-key")
                        )
                        .AddIndicatorMapping(
                            "indicator-2-key",
                            DataFixture.DefaultIndicatorMapping().WithAutoMapped("indicator-2-key")
                        )
                        .AddIndicatorCandidate("indicator-3-key", DataFixture.DefaultMappableIndicator())
                );

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            List<IndicatorMappingUpdateRequest> updates =
            [
                new()
                {
                    SourceKey = "indicator-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "indicator-3-key",
                },
                new() { SourceKey = "indicator-2-key", Type = MappingType.ManualNone },
            ];

            var response = await ApplyBatchIndicatorMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates
            );

            var viewModel = response.AssertOk<BatchIndicatorMappingUpdatesResponseViewModel>();

            var expectedUpdateResponse = new BatchIndicatorMappingUpdatesResponseViewModel
            {
                Updates =
                [
                    new IndicatorMappingUpdateResponseViewModel
                    {
                        SourceKey = "indicator-1-key",
                        Mapping = mapping.GetIndicatorMapping(indicatorColumn: "indicator-1-key")! with
                        {
                            Type = MappingType.ManualMapped,
                            CandidateKey = "indicator-3-key",
                        },
                    },
                    new IndicatorMappingUpdateResponseViewModel
                    {
                        SourceKey = "indicator-2-key",
                        Mapping = mapping.GetIndicatorMapping(indicatorColumn: "indicator-2-key")! with
                        {
                            Type = MappingType.ManualNone,
                            CandidateKey = null,
                        },
                    },
                ],
            };

            // Test that the response from the Controller contains details of all the mappings
            // that were updated.
            viewModel.AssertDeepEqualTo(expectedUpdateResponse, ignoreCollectionOrders: true);

            var updatedMapping = await fixture
                .GetPublicDataDbContext()
                .DataSetVersionMappings.Include(m => m.TargetDataSetVersion)
                .SingleAsync(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            var expectedFullMappings = new Dictionary<string, IndicatorMapping>
            {
                {
                    "indicator-1-key",
                    mapping.GetIndicatorMapping(indicatorColumn: "indicator-1-key")! with
                    {
                        Type = MappingType.ManualMapped,
                        CandidateKey = "indicator-3-key",
                    }
                },
                {
                    "indicator-2-key",
                    mapping.GetIndicatorMapping(indicatorColumn: "indicator-2-key")! with
                    {
                        Type = MappingType.ManualNone,
                        CandidateKey = null,
                    }
                },
            };

            // Test that the updated mappings retrieved from the database reflect the updates
            // that were requested.
            updatedMapping.IndicatorMappingPlan!.Mappings.AssertDeepEqualTo(
                expectedFullMappings,
                ignoreCollectionOrders: true
            );

            // Assert that the batch saves show the Indicator mappings as complete, as there
            // are no remaining mappings with type "None" or "AutoNone" in the plan.
            Assert.True(updatedMapping.IndicatorMappingsComplete);

            // Assert that this update constitutes a major version update, as some Indicator options
            // belonging to mapped Indicators have a mapping type of "ManualNone", indicating that
            // some of the source Indicator options are no longer available in the target data set
            // version, thus creating a breaking change.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0", fixture.GetContentDbContext());
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
            bool expectedMappingsComplete
        )
        {
            var (initialDataSetVersion, nextDataSetVersion) = await CreateInitialAndNextDataSetVersion(
                fixture.GetPublicDataDbContext(),
                fixture.GetContentDbContext()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(initialDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithIndicatorMappingPlan(
                    DataFixture
                        .DefaultIndicatorMappingPlan()
                        .AddIndicatorMapping(
                            "indicator-1-key",
                            DataFixture.DefaultIndicatorMapping().WithAutoMapped("indicator-1-key")
                        )
                        .AddIndicatorMapping(
                            "indicator-2-key",
                            DataFixture.DefaultIndicatorMapping().WithType(unchangedMappingType)
                        )
                        .AddIndicatorCandidate("indicator-1-key", DataFixture.DefaultMappableIndicator())
                        .AddIndicatorCandidate("indicator-2-key", DataFixture.DefaultMappableIndicator())
                );

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            var mappingCandidateKey = updatedMappingType == MappingType.ManualMapped ? "indicator-1-key" : null;

            List<IndicatorMappingUpdateRequest> updates =
            [
                new()
                {
                    SourceKey = "indicator-1-key",
                    Type = updatedMappingType,
                    CandidateKey = mappingCandidateKey,
                },
            ];

            var response = await ApplyBatchIndicatorMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates
            );

            response.AssertOk<BatchIndicatorMappingUpdatesResponseViewModel>();

            var updatedMapping = await fixture
                .GetPublicDataDbContext()
                .DataSetVersionMappings.SingleAsync(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            // Assert that the batch save calculates the IndicatorMappingsComplete flag as expected given the
            // combination of the requested mapping update and the existing mapping that is untouched.
            Assert.Equal(expectedMappingsComplete, updatedMapping.IndicatorMappingsComplete);
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
            string expectedVersion
        )
        {
            var (initialDataSetVersion, nextDataSetVersion) = await CreateInitialAndNextDataSetVersion(
                fixture.GetPublicDataDbContext(),
                fixture.GetContentDbContext()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(initialDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithIndicatorMappingPlan(
                    DataFixture
                        .DefaultIndicatorMappingPlan()
                        .AddIndicatorMapping(
                            "indicator-1-key",
                            DataFixture.DefaultIndicatorMapping().WithAutoMapped("indicator-1-key")
                        )
                        .AddIndicatorMapping(
                            "indicator-2-key",
                            DataFixture.DefaultIndicatorMapping().WithType(unchangedMappingType)
                        )
                        .AddIndicatorCandidate("indicator-1-key", DataFixture.DefaultMappableIndicator())
                        .AddIndicatorCandidate("indicator-2-key", DataFixture.DefaultMappableIndicator())
                );

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            var mappingCandidateKey = updatedMappingType == MappingType.ManualMapped ? "indicator-1-key" : null;

            List<IndicatorMappingUpdateRequest> updates =
            [
                new()
                {
                    SourceKey = "indicator-1-key",
                    Type = updatedMappingType,
                    CandidateKey = mappingCandidateKey,
                },
            ];

            var response = await ApplyBatchIndicatorMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates
            );

            response.AssertOk<BatchIndicatorMappingUpdatesResponseViewModel>();

            var updatedMapping = await fixture
                .GetPublicDataDbContext()
                .DataSetVersionMappings.Include(m => m.TargetDataSetVersion)
                .SingleAsync(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            await AssertCorrectDataSetVersionNumbers(updatedMapping, expectedVersion, fixture.GetContentDbContext());
        }

        [Fact]
        public async Task SourceKeyDoesNotExist_Returns400_AndRollsBackTransaction()
        {
            var (initialDataSetVersion, nextDataSetVersion) = await CreateInitialAndNextDataSetVersion(
                fixture.GetPublicDataDbContext(),
                fixture.GetContentDbContext()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(initialDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithIndicatorMappingPlan(
                    DataFixture
                        .DefaultIndicatorMappingPlan()
                        .AddIndicatorMapping(
                            "indicator-1-key",
                            DataFixture.DefaultIndicatorMapping().WithAutoMapped("indicator-1-key")
                        )
                        .AddIndicatorCandidate("indicator-1-key", DataFixture.DefaultMappableIndicator())
                        .AddIndicatorCandidate("indicator-2-key", DataFixture.DefaultMappableIndicator())
                );

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            List<IndicatorMappingUpdateRequest> updates =
            [
                // This mapping exists.
                new()
                {
                    SourceKey = "indicator-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "indicator-1-key",
                },
                // This mapping does not exist.
                new()
                {
                    SourceKey = "indicator-2-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "indicator-2-key",
                },
            ];

            var response = await ApplyBatchIndicatorMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates
            );

            var validationProblem = response.AssertValidationProblem();

            // The non-existent mapping in the batch update request should have failure messages
            // indicating that the mappings they were attempting to update do not exist.
            validationProblem.AssertHasError(
                expectedPath: "updates[1].sourceKey",
                expectedCode: nameof(ValidationMessages.DataSetVersionMappingSourcePathDoesNotExist)
            );

            Assert.Single(validationProblem.Errors);

            var retrievedMappings = await fixture
                .GetPublicDataDbContext()
                .DataSetVersionMappings.SingleAsync(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            // Test that the mappings are not updated due to the failures of some of the update requests.
            retrievedMappings.IndicatorMappingPlan!.Mappings.AssertDeepEqualTo(
                mapping.IndicatorMappingPlan!.Mappings,
                ignoreCollectionOrders: true
            );
        }

        [Fact]
        public async Task CandidateKeyDoesNotExist_Returns400()
        {
            var (initialDataSetVersion, nextDataSetVersion) = await CreateInitialAndNextDataSetVersion(
                fixture.GetPublicDataDbContext(),
                fixture.GetContentDbContext()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(initialDataSetVersion.Id)
                .WithTargetDataSetVersionId(nextDataSetVersion.Id)
                .WithIndicatorMappingPlan(
                    DataFixture
                        .DefaultIndicatorMappingPlan()
                        .AddIndicatorMapping(
                            "indicator-1-key",
                            DataFixture.DefaultIndicatorMapping().WithAutoMapped("indicator-1-key")
                        )
                        .AddIndicatorMapping(
                            "indicator-2-key",
                            DataFixture.DefaultIndicatorMapping().WithAutoMapped("indicator-2-key")
                        )
                        .AddIndicatorMapping(
                            "indicator-3-key",
                            DataFixture.DefaultIndicatorMapping().WithAutoMapped("indicator-3-key")
                        )
                        .AddIndicatorCandidate("indicator-1-key", DataFixture.DefaultMappableIndicator())
                );

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            List<IndicatorMappingUpdateRequest> updates =
            [
                // This candidate exists.
                new()
                {
                    SourceKey = "indicator-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "indicator-1-key",
                },
                // This candidate does not exist as there is no candidate with the key
                // "Non-existent candidate key".  This tests the simple case where a candidate
                // doesn't exist at all with the given key.
                new()
                {
                    SourceKey = "indicator-2-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "Non-existent candidate key",
                },
                // This candidate does not exist as there is no candidate with the key
                // "Non-existent candidate key 2".  This tests the simple case where a candidate
                // doesn't exist at all with the given key.
                new()
                {
                    SourceKey = "indicator-3-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "Non-existent candidate key 2",
                },
            ];

            var response = await ApplyBatchIndicatorMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates
            );

            var validationProblem = response.AssertValidationProblem();

            // The 2 non-existent mappings in the batch update request should have failure messages
            // indicating that the mappings they were attempting to update do not exist.
            validationProblem.AssertHasError(
                expectedPath: "updates[1].candidateKey",
                expectedCode: nameof(ValidationMessages.DataSetVersionMappingCandidatePathDoesNotExist)
            );

            validationProblem.AssertHasError(
                expectedPath: "updates[2].candidateKey",
                expectedCode: nameof(ValidationMessages.DataSetVersionMappingCandidatePathDoesNotExist)
            );

            Assert.Equal(2, validationProblem.Errors.Count);

            var retrievedMappings = await fixture
                .GetPublicDataDbContext()
                .DataSetVersionMappings.SingleAsync(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            // Test that the mappings are not updated due to the failures of some of the update requests.
            retrievedMappings.IndicatorMappingPlan!.Mappings.AssertDeepEqualTo(
                mapping.IndicatorMappingPlan!.Mappings,
                ignoreCollectionOrders: true
            );
        }

        [Fact]
        public async Task NotBauUser_Returns403()
        {
            var response = await ApplyBatchIndicatorMappingUpdates(
                nextDataSetVersionId: Guid.NewGuid(),
                updates: [],
                user: OptimisedTestUsers.Authenticated
            );

            response.AssertForbidden();
        }

        [Fact]
        public async Task DataSetVersionMappingDoesNotExist_Returns404()
        {
            var response = await ApplyBatchIndicatorMappingUpdates(nextDataSetVersionId: Guid.NewGuid(), updates: []);

            response.AssertNotFound();
        }

        [Fact]
        public async Task EmptyRequiredFields_Return400()
        {
            var response = await ApplyBatchIndicatorMappingUpdates(
                nextDataSetVersionId: Guid.NewGuid(),
                updates: [new IndicatorMappingUpdateRequest()]
            );

            var validationProblem = response.AssertValidationProblem();
            Assert.Equal(2, validationProblem.Errors.Count);
            validationProblem.AssertHasNotNullError("updates[0].type");
            validationProblem.AssertHasNotEmptyError("updates[0].sourceKey");
        }

        [Theory]
        [InlineData(MappingType.ManualMapped, null)]
        [InlineData(MappingType.ManualMapped, "")]
        public async Task MappingTypeExpectsCandidateKey_Returns400(MappingType type, string? candidateKeyValue)
        {
            var response = await ApplyBatchIndicatorMappingUpdates(
                nextDataSetVersionId: Guid.NewGuid(),
                updates:
                [
                    new IndicatorMappingUpdateRequest
                    {
                        SourceKey = "indicator-1",
                        Type = type,
                        CandidateKey = candidateKeyValue,
                    },
                ]
            );

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "updates[0].candidateKey",
                expectedCode: ValidationMessages.CandidateKeyMustBeSpecifiedWithMappedMappingType.Code,
                expectedMessage: ValidationMessages.CandidateKeyMustBeSpecifiedWithMappedMappingType.Message
            );

            Assert.Single(validationProblem.Errors);
        }

        [Theory]
        [InlineData(MappingType.ManualNone)]
        public async Task MappingTypeDoeNotExpectCandidateKey_Returns400(MappingType type)
        {
            var response = await ApplyBatchIndicatorMappingUpdates(
                nextDataSetVersionId: Guid.NewGuid(),
                updates:
                [
                    new IndicatorMappingUpdateRequest
                    {
                        SourceKey = "indicator-1",
                        Type = type,
                        CandidateKey = "target-indicator-1",
                    },
                ]
            );

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "updates[0].candidateKey",
                expectedCode: ValidationMessages.CandidateKeyMustBeEmptyWithNoneMappingType.Code,
                expectedMessage: ValidationMessages.CandidateKeyMustBeEmptyWithNoneMappingType.Message
            );

            Assert.Single(validationProblem.Errors);
        }

        [Theory]
        [InlineData(MappingType.None)]
        [InlineData(MappingType.AutoMapped)]
        [InlineData(MappingType.AutoNone)]
        public async Task InvalidMappingTypeForManualInteraction_Returns400(MappingType type)
        {
            var response = await ApplyBatchIndicatorMappingUpdates(
                nextDataSetVersionId: Guid.NewGuid(),
                updates:
                [
                    new IndicatorMappingUpdateRequest
                    {
                        SourceKey = "indicator-1",
                        Type = type,
                        CandidateKey = null,
                    },
                ]
            );

            var validationProblem = response.AssertValidationProblem();

            validationProblem.AssertHasError(
                expectedPath: "updates[0].type",
                expectedCode: ValidationMessages.ManualMappingTypeInvalid.Code,
                expectedMessage: "Type must be one of the following values: ManualMapped, ManualNone"
            );

            Assert.Single(validationProblem.Errors);
        }

        private async Task<HttpResponseMessage> ApplyBatchIndicatorMappingUpdates(
            Guid nextDataSetVersionId,
            List<IndicatorMappingUpdateRequest> updates,
            ClaimsPrincipal? user = null
        )
        {
            var client = fixture.CreateClient(user: user ?? OptimisedTestUsers.Bau);

            var uri = new Uri($"{BaseUrl}/{nextDataSetVersionId}/mapping/indicators", UriKind.Relative);

            return await client.PatchAsync(
                uri,
                new JsonNetContent(new BatchIndicatorMappingUpdatesRequest { Updates = updates })
            );
        }
    }
}

static class DataSetVersionMappingControllerIndicatorTestsHelpers
{
    private static readonly DataFixture DataFixture = new();

    public static async Task AssertCorrectDataSetVersionNumbers(
        DataSetVersionMapping mapping,
        string expectedVersion,
        ContentDbContext contentDbContext
    )
    {
        Assert.Equal(expectedVersion, mapping.TargetDataSetVersion.SemVersion().ToString());

        var updatedReleaseFile = await contentDbContext.ReleaseFiles.SingleAsync(rf =>
            rf.PublicApiDataSetId == mapping.TargetDataSetVersion.DataSetId
        );

        Assert.Equal(expectedVersion, updatedReleaseFile.PublicApiDataSetVersion?.ToString());
    }

    public static async Task<(
        DataSetVersion initialVersion,
        DataSetVersion nextVersion
    )> CreateInitialAndNextDataSetVersion(PublicDataDbContext publicDataDbContext, ContentDbContext contentDbContext)
    {
        DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished();

        await publicDataDbContext.AddTestData(context => context.DataSets.Add(dataSet));

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

        await publicDataDbContext.AddTestData(context =>
        {
            context.DataSetVersions.AddRange(initialDataSetVersion, nextDataSetVersion);
            context.DataSets.Update(dataSet);
        });

        ReleaseFile releaseFile = DataFixture
            .DefaultReleaseFile()
            .WithId(nextDataSetVersion.Release.ReleaseFileId)
            .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
            .WithFile(DataFixture.DefaultFile(FileType.Data))
            .WithPublicApiDataSetId(nextDataSetVersion.DataSetId)
            .WithPublicApiDataSetVersion(nextDataSetVersion.SemVersion());

        await contentDbContext.AddTestData(context => context.ReleaseFiles.Add(releaseFile));

        return (initialDataSetVersion, nextDataSetVersion);
    }
}
