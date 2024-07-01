#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
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
                                        "source-location-2-key", new LocationOptionMapping
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
                    new LocationMappingUpdateResponse
                    {
                        Level = GeographicLevel.LocalAuthority,
                        SourceKey = "source-location-1-key",
                        Mapping = new LocationOptionMapping
                        {
                            Source = new MappableLocationOption("Source location 1") 
                            {
                                Code = "Source location 1 code"
                            },
                            Type = MappingType.ManualMapped,
                            CandidateKey = "target-location-1-key"
                        }
                    },
                    new LocationMappingUpdateResponse
                    {
                        Level = GeographicLevel.Country,
                        SourceKey = "source-location-3-key",
                        Mapping = new LocationOptionMapping
                        {
                            Source = new MappableLocationOption("Source location 3") 
                            {
                                Code = "Source location 3 code"
                            },
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
                                "source-location-1-key", new LocationOptionMapping
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
                                "source-location-2-key", new LocationOptionMapping
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
                                "source-location-1-key", new LocationOptionMapping
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
                                "source-location-3-key", new LocationOptionMapping
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

    // permission tests (and do we have these for other controller integration tests too)?

    private WebApplicationFactory<TestStartup> BuildApp(ClaimsPrincipal? user = null)
    {
        return TestApp.SetUser(user ?? BauUser());
    }
}
