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
                                            Type = MappingType.AutoMapped,
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
                                },
                                Candidates =
                                    new Dictionary<string, MappableLocationOption>
                                    {
                                        {
                                            "target-location-1-key",
                                            new MappableLocationOption("Target location 1")
                                            {
                                                Code = "Target location 1 code"
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
                                            Type = MappingType.ManualNone,
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
                                            Type = MappingType.ManualMapped,
                                            CandidateKey = "target-location-1-key"
                                        }
                                    }
                                },
                                Candidates = new Dictionary<string, MappableLocationOption>
                                {
                                    {
                                        "target-location-1-key",
                                        new MappableLocationOption("Target location 1")
                                        {
                                            Code = "Target location 1 code"
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
                                        "source-la-location-1-key",
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
                                        "source-la-location-2-key",
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
                                },
                                Candidates =
                                    new Dictionary<string, MappableLocationOption>
                                    {
                                        {
                                            "target-la-location-1-key",
                                            new MappableLocationOption("Target location 1")
                                            {
                                                Code = "Target location 1 code"
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
                                        "source-country-location-1-key",
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
                                        "source-country-location-3-key",
                                        new LocationOptionMapping
                                        {
                                            Source = new MappableLocationOption("Source location 3")
                                            {
                                                Code = "Source location 3 code"
                                            },
                                            Type = MappingType.AutoMapped,
                                            CandidateKey = "target-country-location-3-key"
                                        }
                                    }
                                },
                                Candidates = new Dictionary<string, MappableLocationOption>
                                {
                                    {
                                        "target-country-location-1-key",
                                        new MappableLocationOption("Target location 1")
                                        {
                                            Code = "Target location 1 code"
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
                    SourceKey = "source-la-location-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-la-location-1-key"
                },
                new()
                {
                    Level = GeographicLevel.Country,
                    SourceKey = "source-country-location-3-key",
                    Type = MappingType.ManualNone
                }
            ];

            var response = await ApplyBatchLocationMappingUpdates(
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
                        SourceKey = "source-la-location-1-key",
                        Mapping = new LocationOptionMapping
                        {
                            Source =
                                new MappableLocationOption("Source location 1") { Code = "Source location 1 code" },
                            Type = MappingType.ManualMapped,
                            CandidateKey = "target-la-location-1-key"
                        }
                    },
                    new LocationMappingUpdateResponseViewModel
                    {
                        Level = GeographicLevel.Country,
                        SourceKey = "source-country-location-3-key",
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
                                "source-la-location-1-key",
                                new LocationOptionMapping
                                {
                                    Source = new MappableLocationOption("Source location 1")
                                    {
                                        Code = "Source location 1 code"
                                    },
                                    Type = MappingType.ManualMapped,
                                    CandidateKey = "target-la-location-1-key"
                                }
                            },
                            {
                                "source-la-location-2-key",
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
                        },
                        Candidates =
                            new Dictionary<string, MappableLocationOption>
                            {
                                {
                                    "target-la-location-1-key",
                                    new MappableLocationOption("Target location 1") { Code = "Target location 1 code" }
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
                                "source-country-location-1-key",
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
                                "source-country-location-3-key",
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
                        },
                        Candidates = new Dictionary<string, MappableLocationOption>
                        {
                            {
                                "target-country-location-1-key",
                                new MappableLocationOption("Target location 1") { Code = "Target location 1 code" }
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
                                        "source-la-location-1-key",
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
                                        "source-la-location-2-key",
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
                                },
                                Candidates = new Dictionary<string, MappableLocationOption>
                                {
                                    {
                                        "target-la-location-1-key",
                                        new MappableLocationOption("Target location 1")
                                        {
                                            Code = "Target location 1 code"
                                        }
                                    },
                                    {
                                        "target-la-location-2-key",
                                        new MappableLocationOption("Target location 2")
                                        {
                                            Code = "Target location 2 code"
                                        }
                                    }
                                }
                            }
                        },
                        {
                            GeographicLevel.Country,
                            new LocationLevelMappings
                            {
                                Mappings = new Dictionary<string, LocationOptionMapping>(),
                                Candidates = new Dictionary<string, MappableLocationOption>
                                {
                                    {
                                        "target-country-location-1-key",
                                        new MappableLocationOption("Target location 1")
                                        {
                                            Code = "Target location 1 code"
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
            validationProblem.AssertHasError(
                expectedPath: "updates[1].sourceKey",
                expectedCode: nameof(ValidationMessages.DataSetVersionMappingSourcePathDoesNotExist));

            validationProblem.AssertHasError(
                expectedPath: "updates[2].sourceKey",
                expectedCode: nameof(ValidationMessages.DataSetVersionMappingSourcePathDoesNotExist));

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
        public async Task CandidateKeyDoesNotExist_Returns400_AndRollsBackTransaction()
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
                                    },
                                    {
                                        "source-location-3-key",
                                        new LocationOptionMapping
                                        {
                                            Source = new MappableLocationOption("Source location 3")
                                            {
                                                Code = "Source location 3 code"
                                            },
                                            Type = MappingType.None,
                                            CandidateKey = null
                                        }
                                    }
                                },
                                Candidates = new Dictionary<string, MappableLocationOption>()
                                {
                                    {
                                        "target-la-location-1-key",
                                        new MappableLocationOption("Target location 1")
                                        {
                                            Code = "Target location 1 code"
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
                // This candidate exists.
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-la-location-1-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-la-location-1-key"
                },
                // This candidate does not exist.
                new()
                {
                    Level = GeographicLevel.LocalAuthority,
                    SourceKey = "source-la-location-2-key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-la-location-2-key"
                },
                // This candidate does not exist.
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

            validationProblem.AssertHasError(
                expectedPath: "updates[0].type",
                expectedCode: ValidationMessages.ManualMappingTypeInvalid.Code,
                expectedMessage: "Type must be one of the following values: ManualMapped, ManualNone");

            Assert.Single(validationProblem.Errors);
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

            var mappings = new DataSetVersionMapping
            {
                SourceDataSetVersionId = currentDataSetVersion.Id,
                TargetDataSetVersionId = nextDataSetVersion.Id,
                LocationMappingPlan = new LocationMappingPlan(),
                FilterMappingPlan = new FilterMappingPlan
                {
                    Mappings =
                    {
                        {
                            "Filter 1 key", new FilterMapping
                            {
                                Source = new MappableFilter("Filter 1 label"),
                                Type = MappingType.AutoMapped,
                                CandidateKey = "Filter 1 key",
                                OptionMappings =
                                {
                                    {
                                        "Filter 1 option 1 key",
                                        new FilterOptionMapping
                                        {
                                            Source = new MappableFilterOption("Filter 1 option 1 label"),
                                            Type = MappingType.AutoMapped,
                                            CandidateKey = "Filter 1 option 1 key"
                                        }
                                    },
                                    {
                                        "Filter 1 option 2 key",
                                        new FilterOptionMapping
                                        {
                                            Source = new MappableFilterOption("Filter 1 option 2 label"),
                                            Type = MappingType.ManualNone
                                        }
                                    }
                                }
                            }
                        },
                        {
                            "Filter 2 key", new FilterMapping
                            {
                                Source = new MappableFilter("Filter 2 label"),
                                Type = MappingType.AutoNone,
                                OptionMappings =
                                {
                                    {
                                        "Filter 2 option 1 key",
                                        new FilterOptionMapping
                                        {
                                            Source = new MappableFilterOption("Filter 2 option 1 label"),
                                            Type = MappingType.AutoNone
                                        }
                                    }
                                }
                            }
                        }
                    },
                    Candidates =
                    {
                        {
                            "Filter 1 key",
                            new FilterMappingCandidate("Filter 1 label")
                            {
                                Options =
                                {
                                    {
                                        "Filter 1 option 1 key", new MappableFilterOption("Filter 1 option 1 label")
                                    },
                                    { "Filter 1 option 3 key", new MappableFilterOption("Filter 1 option 3 label") }
                                }
                            }
                        }
                    }
                }
            };

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

            var mappings = new DataSetVersionMapping
            {
                SourceDataSetVersionId = currentDataSetVersion.Id,
                TargetDataSetVersionId = nextDataSetVersion.Id,
                LocationMappingPlan = new LocationMappingPlan(),
                FilterMappingPlan = new FilterMappingPlan
                {
                    Mappings =
                    {
                        {
                            "Filter 1 key", new FilterMapping
                            {
                                Source = new MappableFilter("Filter 1 label"),
                                Type = MappingType.AutoMapped,
                                CandidateKey = "Filter 1 target key",
                                OptionMappings =
                                {
                                    {
                                        "Filter 1 option 1 key",
                                        new FilterOptionMapping
                                        {
                                            Source = new MappableFilterOption("Filter 1 option 1 label"),
                                            Type = MappingType.AutoMapped,
                                            CandidateKey = "Filter 1 option 1 key"
                                        }
                                    },
                                    {
                                        "Filter 1 option 2 key",
                                        new FilterOptionMapping
                                        {
                                            Source = new MappableFilterOption("Filter 1 option 2 label"),
                                            Type = MappingType.ManualNone
                                        }
                                    }
                                }
                            }
                        },
                        {
                            "Filter 2 key", new FilterMapping
                            {
                                Source = new MappableFilter("Filter 2 label"),
                                Type = MappingType.AutoMapped,
                                CandidateKey = "Filter 2 key",
                                OptionMappings =
                                {
                                    {
                                        "Filter 2 option 1 key",
                                        new FilterOptionMapping
                                        {
                                            Source = new MappableFilterOption("Filter 2 option 1 label"),
                                            Type = MappingType.AutoMapped,
                                            CandidateKey = "Filter 1 option 1 key"
                                        }
                                    }
                                }
                            }
                        }
                    },
                    Candidates = new Dictionary<string, FilterMappingCandidate>
                    {
                        {
                            "Filter 1 target key",
                            new FilterMappingCandidate("Filter 1")
                            {
                                Options = new Dictionary<string, MappableFilterOption>
                                {
                                    {
                                        "target-filter-option-1-key",
                                        new MappableFilterOption("Filter 1 option 1 label")
                                    }
                                }
                            }
                        }
                    }
                }
            };

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            var client = BuildApp().CreateClient();

            List<FilterOptionMappingUpdateRequest> updates =
            [
                new()
                {
                    FilterKey = "Filter 1 key",
                    SourceKey = "Filter 1 option 1 key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "target-filter-option-1-key"
                },
                new()
                {
                    FilterKey = "Filter 2 key",
                    SourceKey = "Filter 2 option 1 key",
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
                        FilterKey = "Filter 1 key",
                        SourceKey = "Filter 1 option 1 key",
                        Mapping = new FilterOptionMapping
                        {
                            Source = new MappableFilterOption("Filter 1 option 1 label"),
                            Type = MappingType.ManualMapped,
                            CandidateKey = "target-filter-option-1-key"
                        }
                    },
                    new FilterOptionMappingUpdateResponseViewModel
                    {
                        FilterKey = "Filter 2 key",
                        SourceKey = "Filter 2 option 1 key",
                        Mapping = new FilterOptionMapping
                        {
                            Source = new MappableFilterOption("Filter 2 option 1 label"),
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
                .Single(m => m.TargetDataSetVersionId == nextDataSetVersion.Id);

            var expectedFullMappings = new Dictionary<string, FilterMapping>
            {
                {
                    "Filter 1 key", new FilterMapping
                    {
                        Source = new MappableFilter("Filter 1 label"),
                        Type = MappingType.AutoMapped,
                        CandidateKey = "Filter 1 target key",
                        OptionMappings =
                        {
                            {
                                "Filter 1 option 1 key",
                                new FilterOptionMapping
                                {
                                    Source = new MappableFilterOption("Filter 1 option 1 label"),
                                    Type = MappingType.ManualMapped,
                                    CandidateKey = "target-filter-option-1-key"
                                }
                            },
                            {
                                "Filter 1 option 2 key",
                                new FilterOptionMapping
                                {
                                    Source = new MappableFilterOption("Filter 1 option 2 label"),
                                    Type = MappingType.ManualNone
                                }
                            }
                        }
                    }
                },
                {
                    "Filter 2 key", new FilterMapping
                    {
                        Source = new MappableFilter("Filter 2 label"),
                        Type = MappingType.AutoMapped,
                        CandidateKey = "Filter 2 key",
                        OptionMappings =
                        {
                            {
                                "Filter 2 option 1 key",
                                new FilterOptionMapping
                                {
                                    Source = new MappableFilterOption("Filter 2 option 1 label"),
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

            var mappings = new DataSetVersionMapping
            {
                SourceDataSetVersionId = currentDataSetVersion.Id,
                TargetDataSetVersionId = nextDataSetVersion.Id,
                LocationMappingPlan = new LocationMappingPlan(),
                FilterMappingPlan = new FilterMappingPlan
                {
                    Mappings =
                    {
                        {
                            "Filter 1 key", new FilterMapping
                            {
                                Source = new MappableFilter("Filter 1 label"),
                                Type = MappingType.AutoMapped,
                                CandidateKey = "Target filter 1 key",
                                OptionMappings =
                                {
                                    {
                                        "Filter 1 option 1 key",
                                        new FilterOptionMapping
                                        {
                                            Source = new MappableFilterOption("Filter 1 option 1 label"),
                                            Type = MappingType.AutoMapped,
                                            CandidateKey = "Filter 1 option 1 key"
                                        }
                                    },
                                    {
                                        "Filter 1 option 2 key",
                                        new FilterOptionMapping
                                        {
                                            Source = new MappableFilterOption("Filter 1 option 2 label"),
                                            Type = MappingType.ManualNone
                                        }
                                    }
                                }
                            }
                        }
                    },
                    Candidates =
                    {
                        {
                            "Target filter 1 key",
                            new FilterMappingCandidate("Filter 1")
                            {
                                Options =
                                {
                                    {
                                        "Target filter 1 option 1 key", new MappableFilterOption("Filter 1 option 1 label")
                                    },
                                    {
                                        "Target filter 1 option 2 key", new MappableFilterOption("Filter 1 option 2 label")
                                    },
                                    { "Target filter 1 option 3 key", new MappableFilterOption("Filter 1 option 3 label") }
                                }
                            }
                        }
                    }
                }
            };

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
                    FilterKey = "Filter 1 key",
                    SourceKey = "Filter 1 option 1 key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "Target filter 1 option 1 key"
                },
                // This mapping does not exist.
                new()
                {
                    FilterKey = "Filter 1 key",
                    SourceKey = "Filter 1 option 3 key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "Target filter 1 option 2 key"
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
        public async Task CandidateKeyDoesNotExist_Returns400_AndRollsBackTransaction()
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
                LocationMappingPlan = new LocationMappingPlan(),
                FilterMappingPlan = new FilterMappingPlan
                {
                    Mappings =
                    {
                        {
                            "Filter 1 key", new FilterMapping
                            {
                                Source = new MappableFilter("Filter 1 label"),
                                Type = MappingType.AutoMapped,
                                CandidateKey = "Filter 1 key",
                                OptionMappings =
                                {
                                    {
                                        "Filter 1 option 1 key",
                                        new FilterOptionMapping
                                        {
                                            Source = new MappableFilterOption("Filter 1 option 1 label"),
                                            Type = MappingType.AutoMapped,
                                            CandidateKey = "Filter 1 option 1 key"
                                        }
                                    },
                                    {
                                        "Filter 1 option 2 key",
                                        new FilterOptionMapping
                                        {
                                            Source = new MappableFilterOption("Filter 1 option 2 label"),
                                            Type = MappingType.ManualNone
                                        }
                                    }
                                }
                            }
                        },
                        {
                            "Filter 2 key", new FilterMapping
                            {
                                Source = new MappableFilter("Filter 2 label"),
                                Type = MappingType.AutoMapped,
                                CandidateKey = "Filter 2 key",
                                OptionMappings =
                                {
                                    {
                                        "Filter 2 option 1 key",
                                        new FilterOptionMapping
                                        {
                                            Source = new MappableFilterOption("Filter 1 option 1 label"),
                                            Type = MappingType.AutoMapped,
                                            CandidateKey = "Filter 1 option 1 key"
                                        }
                                    }
                                }
                            }
                        }
                    },
                    Candidates =
                    {
                        {
                            "Filter 1 key",
                            new FilterMappingCandidate("Filter 1")
                            {
                                Options =
                                {
                                    {
                                        "Target filter 1 option 1 key",
                                        new MappableFilterOption("Filter 1 option 1 label")
                                    },
                                    {
                                        "Target filter 1 option 2 key",
                                        new MappableFilterOption("Filter 1 option 2 label")
                                    }
                                }
                            }
                        },
                        {
                            "Filter 2 key",
                            new FilterMappingCandidate("Filter 2")
                            {
                                Options =
                                {
                                    {
                                        "Target filter 2 option 1 key",
                                        new MappableFilterOption("Filter 2 option 1 label")
                                    }
                                }
                            }
                        }
                    }
                }
            };

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
                    FilterKey = "Filter 1 key",
                    SourceKey = "Filter 1 option 1 key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "Target filter 1 option 1 key"
                },
                // This candidate does not exist.
                new()
                {
                    FilterKey = "Filter 1 key",
                    SourceKey = "Filter 1 option 2 key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "Non existent candidate key"
                },
                // This candidate does not exist.
                new()
                {
                    FilterKey = "Filter 2 key",
                    SourceKey = "Filter 2 option 1 key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "Target filter 1 option 1 key"
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
        public async Task OwningFilterNotMapped_Returns400_AndRollsBackTransaction()
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
                LocationMappingPlan = new LocationMappingPlan(),
                FilterMappingPlan = new FilterMappingPlan
                {
                    Mappings =
                    {
                        {
                            "Filter 1 key", new FilterMapping
                            {
                                Source = new MappableFilter("Filter 1 label"),
                                Type = MappingType.ManualNone,
                                OptionMappings =
                                {
                                    {
                                        "Filter 1 option 1 key",
                                        new FilterOptionMapping
                                        {
                                            Source = new MappableFilterOption("Filter 1 option 1 label"),
                                            Type = MappingType.AutoMapped,
                                            CandidateKey = "Filter 1 option 1 key"
                                        }
                                    },
                                    {
                                        "Filter 1 option 2 key",
                                        new FilterOptionMapping
                                        {
                                            Source = new MappableFilterOption("Filter 1 option 2 label"),
                                            Type = MappingType.ManualNone
                                        }
                                    }
                                }
                            }
                        }
                    },
                    Candidates =
                    {
                        {
                            "Filter 1 key",
                            new FilterMappingCandidate("Filter 1")
                            {
                                Options =
                                {
                                    {
                                        "Target filter 1 option 1 key",
                                        new MappableFilterOption("Filter 1 option 1 label")
                                    }
                                }
                            }
                        }
                    }
                }
            };

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersionMappings.Add(mappings);
            });

            var client = BuildApp().CreateClient();

            List<FilterOptionMappingUpdateRequest> updates =
            [
                // This candidate exists, but the filter that owns "Filter 1 option 1 key" has not itself
                // been mapped, so it cannot supply any valid candidate filter options.
                new()
                {
                    FilterKey = "Filter 1 key",
                    SourceKey = "Filter 1 option 1 key",
                    Type = MappingType.ManualMapped,
                    CandidateKey = "Target filter 1 option 1 key"
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
