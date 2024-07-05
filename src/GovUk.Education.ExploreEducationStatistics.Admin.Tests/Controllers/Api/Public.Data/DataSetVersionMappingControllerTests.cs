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
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Public.Data;

public abstract class DataSetVersionMappingControllerTests(
    TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/public-data/data-set-versions";

    public class ApplyBatchMappingUpdatesTests(
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

            var mappings = new DataSetVersionMapping
            {
                SourceDataSetVersionId = currentDataSetVersion.Id,
                TargetDataSetVersionId = nextDataSetVersion.Id,
                LocationMappingPlan = new LocationMappingPlan
                {
                    Levels = new Dictionary<GeographicLevel, LocationLevelMappings>
                    {
                        {
                            GeographicLevel.LocalAuthority,
                            new LocationLevelMappings
                            {
                                Mappings = new Dictionary<string, LocationOptionMapping>
                                {
                                    {
                                        "source-location-1-key",
                                        new LocationOptionMapping
                                        {
                                            Source = new MappableLocationOption("Source location 1")
                                            {
                                                Code = "Source location 1 code"
                                            },
                                            Type = MappingType.None,
                                            CandidateKey = null
                                        }
                                    },
                                    {
                                        "source-location-2-key",
                                        new LocationOptionMapping
                                        {
                                            Source = new MappableLocationOption("Source location 2")
                                            {
                                                Code = "Source location 2 code"
                                            },
                                            Type = MappingType.None,
                                            CandidateKey = null
                                        }
                                    }
                                }
                            }
                        },
                        {
                            GeographicLevel.Country,
                            new LocationLevelMappings
                            {
                                Mappings = new Dictionary<string, LocationOptionMapping>
                                {
                                    {
                                        "source-location-1-key",
                                        new LocationOptionMapping
                                        {
                                            Source = new MappableLocationOption("Source location 1")
                                            {
                                                Code = "Source location 1 code"
                                            },
                                            Type = MappingType.None,
                                            CandidateKey = null
                                        }
                                    },
                                    {
                                        "source-location-3-key",
                                        new LocationOptionMapping
                                        {
                                            Source = new MappableLocationOption("Source location 3")
                                            {
                                                Code = "Source location 3 code"
                                            },
                                            Type = MappingType.AutoMapped,
                                            CandidateKey = "target-location-3-key"
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                FilterMappingPlan = new FilterMappingPlan()
            };

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

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

            var response = await ApplyBatchMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client);

            var viewModel = response.AssertOk<BatchLocationMappingUpdatesResponseViewModel>();

            var expectedUpdateResponse = new BatchLocationMappingUpdatesResponseViewModel
            {
                Updates =
                [
                    new LocationMappingUpdateResponseViewModel
                    {
                        Level = GeographicLevel.LocalAuthority,
                        SourceKey = "source-location-1-key",
                        Mapping = new LocationOptionMapping
                        {
                            Source =
                                new MappableLocationOption("Source location 1") { Code = "Source location 1 code" },
                            Type = MappingType.ManualMapped,
                            CandidateKey = "target-location-1-key"
                        }
                    },
                    new LocationMappingUpdateResponseViewModel
                    {
                        Level = GeographicLevel.Country,
                        SourceKey = "source-location-3-key",
                        Mapping = new LocationOptionMapping
                        {
                            Source =
                                new MappableLocationOption("Source location 3") { Code = "Source location 3 code" },
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
                                new LocationOptionMapping
                                {
                                    Source = new MappableLocationOption("Source location 1")
                                    {
                                        Code = "Source location 1 code"
                                    },
                                    Type = MappingType.ManualMapped,
                                    CandidateKey = "target-location-1-key"
                                }
                            },
                            {
                                "source-location-2-key",
                                new LocationOptionMapping
                                {
                                    Source = new MappableLocationOption("Source location 2")
                                    {
                                        Code = "Source location 2 code"
                                    },
                                    Type = MappingType.None,
                                    CandidateKey = null
                                }
                            }
                        }
                    }
                },
                {
                    GeographicLevel.Country, new LocationLevelMappings
                    {
                        Mappings = new Dictionary<string, LocationOptionMapping>
                        {
                            {
                                "source-location-1-key",
                                new LocationOptionMapping
                                {
                                    Source = new MappableLocationOption("Source location 1")
                                    {
                                        Code = "Source location 1 code"
                                    },
                                    Type = MappingType.None,
                                    CandidateKey = null
                                }
                            },
                            {
                                // We expect this mapping's type to be set to ManualNone and
                                // its CandidateKey unset.
                                "source-location-3-key",
                                new LocationOptionMapping
                                {
                                    Source = new MappableLocationOption("Source location 3")
                                    {
                                        Code = "Source location 3 code"
                                    },
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
            updatedMappings.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedFullMappings,
                ignoreCollectionOrders: true);
        }

        [Fact]
        public async Task UpdateMappingDoesNotExist_Returns400_AndRollsBackTransaction()
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

            var mappings = new DataSetVersionMapping
            {
                SourceDataSetVersionId = currentDataSetVersion.Id,
                TargetDataSetVersionId = nextDataSetVersion.Id,
                LocationMappingPlan = new LocationMappingPlan
                {
                    Levels = new Dictionary<GeographicLevel, LocationLevelMappings>
                    {
                        {
                            GeographicLevel.LocalAuthority,
                            new LocationLevelMappings
                            {
                                Mappings = new Dictionary<string, LocationOptionMapping>
                                {
                                    {
                                        "source-location-1-key",
                                        new LocationOptionMapping
                                        {
                                            Source = new MappableLocationOption("Source location 1")
                                            {
                                                Code = "Source location 1 code"
                                            },
                                            Type = MappingType.None,
                                            CandidateKey = null
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                FilterMappingPlan = new FilterMappingPlan()
            };

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            var client = BuildApp().CreateClient();

            List<LocationMappingUpdateRequest> updates =
            [
                // This mapping exists.
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-location-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-location-1-key"
                },
                // This mapping does not exist.
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-location-2-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-location-2-key"
                },
                // This mapping does not exist.
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-location-3-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-location-3-key"
                }
            ];

            var response = await ApplyBatchMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client);

            var validationProblem = response.AssertValidationProblem();

            // The 2 non-existent mappings in the batch update request should have failure messages
            // indicating that the mappings they were attempting to update do not exist.
            Assert.Equal(2, validationProblem.Errors.Count);

            validationProblem.AssertHasError(
                expectedPath: "updates[1].sourceKey",
                expectedCode: nameof(ValidationMessages.DataSetVersionMappingPathDoesNotExist));

            validationProblem.AssertHasError(
                expectedPath: "updates[2].sourceKey",
                expectedCode: nameof(ValidationMessages.DataSetVersionMappingPathDoesNotExist));

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

            var response = await ApplyBatchMappingUpdates(
                Guid.NewGuid(),
                new List<LocationMappingUpdateRequest>(),
                client);

            response.AssertForbidden();
        }

        [Fact]
        public async Task DataSetVersionMappingDoesNotExist_Returns404()
        {
            var client = BuildApp().CreateClient();

            var response = await ApplyBatchMappingUpdates(
                Guid.NewGuid(),
                new List<LocationMappingUpdateRequest>(),
                client);

            response.AssertNotFound();
        }

        [Fact]
        public async Task EmptyRequiredFields_Return400()
        {
            var client = BuildApp().CreateClient();

            var response = await ApplyBatchMappingUpdates(
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

            var response = await ApplyBatchMappingUpdates(
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

            var response = await ApplyBatchMappingUpdates(
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

            var response = await ApplyBatchMappingUpdates(
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

        private async Task<HttpResponseMessage> ApplyBatchMappingUpdates(
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

    private WebApplicationFactory<TestStartup> BuildApp(ClaimsPrincipal? user = null)
    {
        return TestApp.SetUser(user ?? BauUser());
    }
}
