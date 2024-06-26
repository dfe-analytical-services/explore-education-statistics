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
                                            Type = MappingType.None,
                                            CandidateKey = null
                                        }
                                    },
                                    {
                                        "source-location-2-key", new LocationOptionMapping
                                        {
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

            List<LocationMappingUpdate> updates =
            [
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-location-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-location-1-key"
                }
            ];

            var response = await ApplyBatchMappingUpdates(
                nextDataSetVersionId: nextDataSetVersion.Id,
                updates: updates,
                client);

            // do something with thi  when returning JSON fragments
            var viewModel = response.AssertOk<DataSetVersionSummaryViewModel>();

            var updatedMappings = TestApp.GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Single(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            var expectedMappings = new Dictionary<GeographicLevel, LocationLevelMappings>
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
                                    Type = MappingType.ManualMapped,
                                    CandidateKey = "target-location-1-key"
                                }
                            },
                            {
                                "source-location-2-key", new LocationOptionMapping
                                {
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
                                "source-location-1-key", new LocationOptionMapping
                                {
                                    Type = MappingType.None,
                                    CandidateKey = null
                                }
                            }
                        }
                    }
                }
            };

            updatedMappings.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedMappings,
                ignoreCollectionOrders: true);
        }

        private async Task<HttpResponseMessage> ApplyBatchMappingUpdates(
            Guid nextDataSetVersionId,
            List<LocationMappingUpdate> updates,
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
