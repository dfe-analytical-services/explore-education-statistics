using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class ProcessNextDataSetVersionMappingsFunctionTests(
    ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public class ProcessNextDataSetVersionMappingsTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessNextDataSetVersionMappingsFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var mockOrchestrationContext = DefaultMockOrchestrationContext();
            var activitySequence = new MockSequence();

            string[] expectedActivitySequence =
            [
                ActivityNames.CopyCsvFiles,
                ActivityNames.CreateMappings,
                ActivityNames.ApplyAutoMappings,
                ActivityNames.CompleteNextDataSetVersionMappingProcessing,
            ];

            foreach (var activityName in expectedActivitySequence)
            {
                mockOrchestrationContext
                    .InSequence(activitySequence)
                    .Setup(context => context.CallActivityAsync(activityName,
                        mockOrchestrationContext.Object.InstanceId,
                        null))
                    .Returns(Task.CompletedTask);
            }

            await ProcessNextDataSetVersion(mockOrchestrationContext.Object);

            VerifyAllMocks(mockOrchestrationContext);
        }

        [Fact]
        public async Task ActivityFunctionThrowsException_CallsHandleFailureActivity()
        {
            var mockOrchestrationContext = DefaultMockOrchestrationContext();

            var activitySequence = new MockSequence();

            mockOrchestrationContext
                .InSequence(activitySequence)
                .Setup(context =>
                    context.CallActivityAsync(ActivityNames.CopyCsvFiles,
                        mockOrchestrationContext.Object.InstanceId,
                        null))
                .Throws<Exception>();

            mockOrchestrationContext
                .InSequence(activitySequence)
                .Setup(context =>
                    context.CallActivityAsync(ActivityNames.HandleProcessingFailure,
                        mockOrchestrationContext.Object.InstanceId,
                        null))
                .Returns(Task.CompletedTask);

            await ProcessNextDataSetVersion(mockOrchestrationContext.Object);

            VerifyAllMocks(mockOrchestrationContext);
        }

        private async Task ProcessNextDataSetVersion(TaskOrchestrationContext orchestrationContext)
        {
            var function = GetRequiredService<ProcessNextDataSetVersionMappingsFunction>();
            await function.ProcessNextDataSetVersionMappings(
                orchestrationContext,
                new ProcessDataSetVersionContext { DataSetVersionId = Guid.NewGuid() });
        }

        private static Mock<TaskOrchestrationContext> DefaultMockOrchestrationContext(
            Guid? instanceId = null,
            bool isReplaying = false)
        {
            var mock = new Mock<TaskOrchestrationContext>(MockBehavior.Strict);

            mock
                .Protected()
                .Setup<ILoggerFactory>("LoggerFactory")
                .Returns(NullLoggerFactory.Instance);

            mock
                .SetupGet(context => context.InstanceId)
                .Returns(instanceId?.ToString() ?? Guid.NewGuid().ToString());

            mock
                .SetupGet(context => context.IsReplaying)
                .Returns(isReplaying);

            return mock;
        }
    }

    public abstract class CreateMappingsTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessNextDataSetVersionMappingsFunctionTests(fixture)
    {
        protected const DataSetVersionImportStage Stage = DataSetVersionImportStage.CreatingMappings;

        protected async Task CreateMappings(Guid instanceId)
        {
            var function = GetRequiredService<ProcessNextDataSetVersionMappingsFunction>();
            await function.CreateMappings(instanceId, CancellationToken.None);
        }
    }

    public class CreateMappingMiscTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : CreateMappingsTests(fixture)
    {
        [Fact]
        public async Task Success_ImportStatus()
        {
            var (instanceId, _, _) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            await CreateMappings(instanceId);

            var savedImport = await GetDbContext<PublicDataDbContext>()
                .DataSetVersionImports
                .Include(dataSetVersionImport => dataSetVersionImport.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            Assert.Equal(DataSetVersionStatus.Processing, savedImport.DataSetVersion.Status);
        }

        [Fact]
        public async Task Success_MetaSummary()
        {
            var (instanceId, _, nextVersion) = await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            await CreateMappings(instanceId);

            var updatedDataSetVersion = GetDataSetVersion(nextVersion);

            // Assert that the MetaSummary has been generated correctly from the CSV.
            var metaSummary = updatedDataSetVersion.MetaSummary;
            Assert.NotNull(metaSummary);

            Assert.Equal(
                ProcessorTestData
                    .AbsenceSchool
                    .ExpectedLocations
                    .Select(level => level.Level)
                    .Order(),
                metaSummary
                    .GeographicLevels
                    .Order());

            Assert.Equal(
                ProcessorTestData
                    .AbsenceSchool
                    .ExpectedFilters
                    .Select(filterAndOptions => filterAndOptions.Label)
                    .Order(),
                metaSummary
                    .Filters
                    .Order());

            Assert.Equal(
                ProcessorTestData
                    .AbsenceSchool
                    .ExpectedIndicators
                    .Select(l => l.Label)
                    .Order(),
                metaSummary
                    .Indicators.Order());

            Assert.Equal(
                new TimePeriodRange
                {
                    Start = TimePeriodRangeBound.Create(
                        ProcessorTestData
                            .AbsenceSchool
                            .ExpectedTimePeriods[0]),
                    End = TimePeriodRangeBound.Create(
                        ProcessorTestData
                            .AbsenceSchool
                            .ExpectedTimePeriods[^1])
                },
                metaSummary.TimePeriodRange);
        }
    }

    public class CreateMappingsLocationsTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : CreateMappingsTests(fixture)
    {
        [Fact]
        public async Task Success_Mappings()
        {
            var (instanceId, initialVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            // Select a set of LocationOptions that rely on all the available Location
            // Code fields.
            var initialLocationMeta = DataFixture
                .DefaultLocationMeta()
                .WithDataSetVersionId(initialVersion.Id)
                .ForIndex(0, s => s
                    .SetLevel(GeographicLevel.LocalAuthority)
                    .SetOptions(DataFixture
                        .DefaultLocationLocalAuthorityOptionMeta()
                        .GenerateList(2)
                        .Select(meta => meta as LocationOptionMeta)
                        .ToList()))
                .ForIndex(1, s => s
                    .SetLevel(GeographicLevel.School)
                    .SetOptions(DataFixture
                        .DefaultLocationSchoolOptionMeta()
                        .GenerateList(2)
                        .Select(meta => meta as LocationOptionMeta)
                        .ToList()))
                .ForIndex(2, s => s
                    .SetLevel(GeographicLevel.Provider)
                    .SetOptions(DataFixture
                        .DefaultLocationProviderOptionMeta()
                        .GenerateList(2)
                        .Select(meta => meta as LocationOptionMeta)
                        .ToList()))
                .GenerateList();

            await AddTestData<PublicDataDbContext>(context =>
                context.LocationMetas.AddRange(initialLocationMeta));

            await CreateMappings(instanceId);

            var mappings = GetDataSetVersionMapping(nextVersion);

            Assert.Equal(initialVersion.Id, mappings.SourceDataSetVersionId);
            Assert.Equal(nextVersion.Id, mappings.TargetDataSetVersionId);
            Assert.False(mappings.LocationMappingsComplete);

            var expectedLocationMappingsFromSource = initialLocationMeta
                .ToDictionary(
                    levelMeta => levelMeta.Level,
                    levelMeta => new LocationLevelMappings
                    {
                        Mappings = levelMeta
                            .Options
                            .ToDictionary(
                                keySelector: option => $"{option.Label} :: {option.ToRow().GetRowKey()}",
                                elementSelector: option => new LocationOptionMapping
                                {
                                    CandidateKey = null,
                                    Type = MappingType.None,
                                    Source = new MappableLocationOption(option.Label)
                                    {
                                        Code = option.ToRow().Code,
                                        OldCode = option.ToRow().OldCode,
                                        Urn = option.ToRow().Urn,
                                        LaEstab = option.ToRow().LaEstab,
                                        Ukprn = option.ToRow().Ukprn
                                    }
                                })
                    });

            // There should be 5 levels of mappings when combining all the source and target levels. 
            Assert.Equal(ProcessorTestData
                    .AbsenceSchool
                    .ExpectedGeographicLevels
                    .Concat([GeographicLevel.Provider])
                    .Order(),
                mappings.LocationMappingPlan
                    .Levels
                    .Select(level => level.Key)
                    .Order());

            mappings.LocationMappingPlan.Levels.ForEach(level =>
            {
                var matchingLevelFromSource = expectedLocationMappingsFromSource.GetValueOrDefault(level.Key);

                if (matchingLevelFromSource != null)
                {
                    level.Value.Mappings.AssertDeepEqualTo(
                        matchingLevelFromSource.Mappings,
                        ignoreCollectionOrders: true);
                }
                else
                {
                    Assert.Empty(level.Value.Mappings);
                }
            });
        }

        [Fact]
        public async Task Success_Candidates()
        {
            var (instanceId, initialVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            await CreateMappings(instanceId);

            var mappings = GetDataSetVersionMapping(nextVersion);

            Assert.Equal(initialVersion.Id, mappings.SourceDataSetVersionId);
            Assert.Equal(nextVersion.Id, mappings.TargetDataSetVersionId);

            var expectedLocationLevels = ProcessorTestData
                .AbsenceSchool
                .ExpectedLocations
                .Select(levelMeta => (level: levelMeta.Level, options: levelMeta.Options))
                .ToDictionary(
                    keySelector: levelMeta => levelMeta.level,
                    elementSelector: levelMeta =>
                        new LocationLevelMappings
                        {
                            Candidates = levelMeta
                                .options
                                .ToDictionary(
                                    keySelector: option => $"{option.Label} :: {option.ToRow().GetRowKey()}",
                                    elementSelector: option => new MappableLocationOption(option.Label)
                                    {
                                        Code = option.ToRow().Code,
                                        OldCode = option.ToRow().OldCode,
                                        Urn = option.ToRow().Urn,
                                        LaEstab = option.ToRow().LaEstab,
                                        Ukprn = option.ToRow().Ukprn
                                    })
                        });

            mappings.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedLocationLevels,
                ignoreCollectionOrders: true);
        }
    }

    public class CreateMappingsFiltersTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : CreateMappingsTests(fixture)
    {
        [Fact]
        public async Task Success_Mappings()
        {
            var (instanceId, initialVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            var initialFilterMeta = DataFixture
                .DefaultFilterMeta()
                .WithDataSetVersionId(initialVersion.Id)
                .WithOptions(() => DataFixture
                    .DefaultFilterOptionMeta()
                    .GenerateList(2))
                .GenerateList(2);

            await AddTestData<PublicDataDbContext>(context =>
                context.FilterMetas.AddRange(initialFilterMeta));

            await CreateMappings(instanceId);

            var mappings = GetDataSetVersionMapping(nextVersion);

            Assert.Equal(initialVersion.Id, mappings.SourceDataSetVersionId);
            Assert.Equal(nextVersion.Id, mappings.TargetDataSetVersionId);
            Assert.False(mappings.FilterMappingsComplete);

            var expectedFilterMappings = initialFilterMeta
                .ToDictionary(
                    keySelector: filter => filter.PublicId,
                    elementSelector: filter =>
                        new FilterMapping
                        {
                            CandidateKey = null,
                            Type = MappingType.None,
                            Source = new MappableFilter(filter.Label),
                            OptionMappings = filter
                                .Options
                                .ToDictionary(
                                    keySelector: option => option.Label,
                                    elementSelector: option =>
                                        new FilterOptionMapping
                                        {
                                            CandidateKey = null,
                                            Type = MappingType.None,
                                            Source = new MappableFilterOption(option.Label)
                                        })
                        });

            mappings.FilterMappingPlan.Mappings.AssertDeepEqualTo(
                expectedFilterMappings,
                ignoreCollectionOrders: true);
        }

        [Fact]
        public async Task Success_Candidates()
        {
            var (instanceId, initialVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            await CreateMappings(instanceId);

            var mappings = GetDataSetVersionMapping(nextVersion);

            Assert.Equal(initialVersion.Id, mappings.SourceDataSetVersionId);
            Assert.Equal(nextVersion.Id, mappings.TargetDataSetVersionId);

            var expectedFilterTargets = ProcessorTestData
                .AbsenceSchool
                .ExpectedFilters
                .ToDictionary(
                    keySelector: filterAndOptions => filterAndOptions.PublicId,
                    elementSelector: filterAndOptions =>
                        new FilterMappingCandidate(filterAndOptions.Label)
                        {
                            Options = filterAndOptions
                                .Options
                                .ToDictionary(
                                    keySelector: optionMeta => optionMeta.Label,
                                    elementSelector: optionMeta =>
                                        new MappableFilterOption(optionMeta.Label))
                        });

            mappings.FilterMappingPlan.Candidates.AssertDeepEqualTo(
                expectedFilterTargets,
                ignoreCollectionOrders: true);
        }
    }

    public abstract class ApplyAutoMappingsTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessNextDataSetVersionMappingsFunctionTests(fixture)
    {
        protected const DataSetVersionImportStage Stage = DataSetVersionImportStage.AutoMapping;

        protected async Task ApplyAutoMappings(Guid instanceId)
        {
            var function = GetRequiredService<ProcessNextDataSetVersionMappingsFunction>();
            await function.ApplyAutoMappings(instanceId, CancellationToken.None);
        }
    }

    public class ApplyAutoMappingsMiscTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ApplyAutoMappingsTests(fixture)
    {
        [Fact]
        public async Task Success_ImportStatus()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            await AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersionMappings.Add(new DataSetVersionMapping
                {
                    SourceDataSetVersionId = originalVersion.Id,
                    TargetDataSetVersionId = nextVersion.Id,
                    LocationMappingPlan = new LocationMappingPlan(),
                    FilterMappingPlan = new FilterMappingPlan()
                }));

            await ApplyAutoMappings(instanceId);

            var savedImport = await GetDbContext<PublicDataDbContext>()
                .DataSetVersionImports
                .Include(dataSetVersionImport => dataSetVersionImport.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            Assert.Equal(DataSetVersionStatus.Processing, savedImport.DataSetVersion.Status);
        }
    }

    public class ApplyAutoMappingsLocationsTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ApplyAutoMappingsTests(fixture)
    {
        [Fact]
        public async Task PartiallyComplete()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            // Create a mapping plan based on 2 data set versions with partially overlapping locations and levels.
            // Both have the "Local Authority" level and the "LA location 1" option, but the source has an additional
            // "LA location 2" option that the target does not, and the target has an additional "LA location 3" option
            // that the source does not.
            //
            // In addition to this, the source has a "RSC Region" level that the target does not have, and the target
            // has a "Country" level that the source does not have.
            var mappings = new DataSetVersionMapping
            {
                SourceDataSetVersionId = originalVersion.Id,
                TargetDataSetVersionId = nextVersion.Id,
                FilterMappingPlan = new FilterMappingPlan(),
                LocationMappingPlan = new LocationMappingPlan
                {
                    Levels =
                    {
                        {
                            GeographicLevel.LocalAuthority, new LocationLevelMappings
                            {
                                Mappings =
                                {
                                    {
                                        "LA location 1 key",
                                        new LocationOptionMapping
                                        {
                                            Source = new MappableLocationOption("LA location 1 label")
                                        }
                                    },
                                    {
                                        "LA location 2 key",
                                        new LocationOptionMapping
                                        {
                                            Source = new MappableLocationOption("LA location 2 label")
                                        }
                                    }
                                },
                                Candidates =
                                {
                                    { "LA location 1 key", new MappableLocationOption("LA location 1 label") },
                                    { "LA location 3 key", new MappableLocationOption("LA location 3 label") },
                                }
                            }
                        },
                        {
                            GeographicLevel.RscRegion,
                            new LocationLevelMappings
                            {
                                Mappings =
                                {
                                    {
                                        "RSC location 1 key",
                                        new LocationOptionMapping
                                        {
                                            Source = new MappableLocationOption("RSC location 1 label")
                                        }
                                    }
                                },
                                Candidates = []
                            }
                        },
                        {
                            GeographicLevel.Country, new LocationLevelMappings
                            {
                                Mappings = [],
                                Candidates =
                                {
                                    { "Country location 1 key", new MappableLocationOption("Country location 1 label") }
                                }
                            }
                        }
                    }
                }
            };

            await AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersionMappings.Add(mappings));

            await ApplyAutoMappings(instanceId);

            var updatedMappings = GetDataSetVersionMapping(nextVersion);

            Assert.False(updatedMappings.LocationMappingsComplete);

            Dictionary<GeographicLevel, LocationLevelMappings> expectedLevelMappings = new()
            {
                {
                    GeographicLevel.LocalAuthority, new LocationLevelMappings
                    {
                        Mappings =
                        {
                            {
                                "LA location 1 key",
                                new LocationOptionMapping
                                {
                                    Source = new MappableLocationOption("LA location 1 label"),
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "LA location 1 key"
                                }
                            },
                            {
                                "LA location 2 key",
                                new LocationOptionMapping
                                {
                                    Source = new MappableLocationOption("LA location 2 label"),
                                    Type = MappingType.AutoNone,
                                    CandidateKey = null
                                }
                            }
                        },
                        Candidates =
                        {
                            { "LA location 1 key", new MappableLocationOption("LA location 1 label") },
                            { "LA location 3 key", new MappableLocationOption("LA location 3 label") },
                        }
                    }
                },
                {
                    GeographicLevel.RscRegion,
                    new LocationLevelMappings
                    {
                        Mappings =
                        {
                            {
                                "RSC location 1 key",
                                new LocationOptionMapping
                                {
                                    Source = new MappableLocationOption("RSC location 1 label"),
                                    Type = MappingType.AutoNone,
                                    CandidateKey = null
                                }
                            }
                        },
                        Candidates = []
                    }
                },
                {
                    GeographicLevel.Country, new LocationLevelMappings
                    {
                        Mappings = [],
                        Candidates =
                            {
                                { "Country location 1 key", new MappableLocationOption("Country location 1 label") }
                            }
                    }
                }
            };

            updatedMappings.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedLevelMappings,
                ignoreCollectionOrders: true);
        }

        [Fact]
        public async Task Complete_ExactMatch()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            // Create a mapping plan based on 2 data set versions with perfectly overlapping
            // locations and levels.
            var mappings = new DataSetVersionMapping
            {
                SourceDataSetVersionId = originalVersion.Id,
                TargetDataSetVersionId = nextVersion.Id,
                FilterMappingPlan = new FilterMappingPlan(),
                LocationMappingPlan = new LocationMappingPlan
                {
                    Levels =
                    {
                        {
                            GeographicLevel.LocalAuthority,
                            new LocationLevelMappings
                            {
                                Mappings =
                                {
                                    {
                                        "LA location 1 key",
                                        new LocationOptionMapping
                                        {
                                            Source = new MappableLocationOption("LA location 1 label")
                                        }
                                    }
                                },
                                Candidates =
                                {
                                    { "LA location 1 key", new MappableLocationOption("LA location 1 label") }
                                }
                            }
                        }
                    }
                }
            };

            await AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersionMappings.Add(mappings));

            await ApplyAutoMappings(instanceId);

            var updatedMappings = GetDataSetVersionMapping(nextVersion);

            Assert.True(updatedMappings.LocationMappingsComplete);

            Dictionary<GeographicLevel, LocationLevelMappings> expectedLevelMappings = new()
            {
                {
                    GeographicLevel.LocalAuthority,
                    new LocationLevelMappings
                    {
                        Mappings =
                        {
                            {
                                "LA location 1 key",
                                new LocationOptionMapping
                                {
                                    Source = new MappableLocationOption("LA location 1 label"),
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "LA location 1 key"
                                }
                            }
                        },
                        Candidates = { { "LA location 1 key", new MappableLocationOption("LA location 1 label") } }
                    }
                }
            };

            updatedMappings.LocationMappingPlan.Levels.AssertDeepEqualTo(expectedLevelMappings);
        }

        [Fact]
        public async Task Complete_AllSourcesMapped_OtherUnmappedCandidatesExist()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            // Create a mapping plan based on 2 data set versions with the same levels
            // and location options, but additional options exist in the new version.
            // Each source location option can be auto-mapped exactly to one in
            // the target version, leaving some candidates and new levels unused but
            // essentially the mapping is complete unless the user manually intervenes
            // at this point.
            var mappings = new DataSetVersionMapping
            {
                SourceDataSetVersionId = originalVersion.Id,
                TargetDataSetVersionId = nextVersion.Id,
                FilterMappingPlan = new FilterMappingPlan(),
                LocationMappingPlan = new LocationMappingPlan
                {
                    Levels =
                    {
                        {
                            GeographicLevel.LocalAuthority,
                            new LocationLevelMappings
                            {
                                Mappings =
                                {
                                    {
                                        "LA location 1 key",
                                        new LocationOptionMapping
                                        {
                                            Source = new MappableLocationOption("LA location 1 label")
                                        }
                                    }
                                },
                                Candidates =
                                {
                                    { "LA location 1 key", new MappableLocationOption("LA location 1 label") },
                                    { "LA location 2 key", new MappableLocationOption("LA location 2 label") }
                                }
                            }
                        },
                        {
                            GeographicLevel.RscRegion, new LocationLevelMappings
                            {
                                Mappings = [],
                                Candidates =
                                    {
                                        { "RSC location 1 key", new MappableLocationOption("RSC location 1 label") }
                                    }
                            }
                        }
                    }
                }
            };

            await AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersionMappings.Add(mappings));

            await ApplyAutoMappings(instanceId);

            var updatedMappings = GetDataSetVersionMapping(nextVersion);

            Assert.True(updatedMappings.LocationMappingsComplete);

            Dictionary<GeographicLevel, LocationLevelMappings> expectedLevelMappings = new()
            {
                {
                    GeographicLevel.LocalAuthority,
                    new LocationLevelMappings
                    {
                        Mappings =
                        {
                            {
                                "LA location 1 key",
                                new LocationOptionMapping
                                {
                                    Source = new MappableLocationOption("LA location 1 label"),
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "LA location 1 key"
                                }
                            }
                        },
                        Candidates =
                        {
                            { "LA location 1 key", new MappableLocationOption("LA location 1 label") },
                            { "LA location 2 key", new MappableLocationOption("LA location 2 label") }
                        }
                    }
                },
                {
                    GeographicLevel.RscRegion, new LocationLevelMappings
                    {
                        Mappings = [],
                        Candidates = { { "RSC location 1 key", new MappableLocationOption("RSC location 1 label") } }
                    }
                }
            };

            updatedMappings.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedLevelMappings,
                ignoreCollectionOrders: true);
        }
    }

    public class ApplyAutoMappingsFiltersTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ApplyAutoMappingsTests(fixture)
    {
        [Fact]
        public async Task PartiallyComplete()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            // Create a mapping plan based on 2 data set versions with partially overlapping filters.
            // Both have "Filter 1" and both have "Filter 1 option 1", but then each also contains Filter 1
            // options that the other do not, and each also contains filters that the other does not. 
            var mappings = new DataSetVersionMapping
            {
                SourceDataSetVersionId = originalVersion.Id,
                TargetDataSetVersionId = nextVersion.Id,
                FilterMappingPlan = new FilterMappingPlan
                {
                    Mappings =
                    {
                        {
                            "Filter 1 key", new FilterMapping
                            {
                                Source = new MappableFilter("Filter 1 label"),
                                OptionMappings =
                                {
                                    {
                                        "Filter 1 option 1 key",
                                        new FilterOptionMapping
                                            {
                                                Source = new MappableFilterOption("Filter 1 option 1 label")
                                            }
                                    },
                                    {
                                        "Filter 1 option 2 key",
                                        new FilterOptionMapping
                                            {
                                                Source = new MappableFilterOption("Filter 1 option 2 label")
                                            }
                                    }
                                }
                            }
                        },
                        {
                            "Filter 2 key", new FilterMapping
                            {
                                Source = new MappableFilter("Filter 2 label"),
                                OptionMappings =
                                {
                                    {
                                        "Filter 2 option 1 key",
                                        new FilterOptionMapping
                                            {
                                                Source = new MappableFilterOption("Filter 2 option 1 label")
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
                },
                LocationMappingPlan = new LocationMappingPlan()
            };

            await AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersionMappings.Add(mappings));

            await ApplyAutoMappings(instanceId);

            var updatedMappings = GetDataSetVersionMapping(nextVersion);

            Assert.False(updatedMappings.FilterMappingsComplete);

            Dictionary<string, FilterMapping> expectedFilterMappings = new()
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
                                    Type = MappingType.AutoNone,
                                    CandidateKey = null
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
                        CandidateKey = null,
                        OptionMappings =
                        {
                            {
                                "Filter 2 option 1 key",
                                new FilterOptionMapping
                                {
                                    Source = new MappableFilterOption("Filter 2 option 1 label"),
                                    Type = MappingType.AutoNone,
                                    CandidateKey = null
                                }
                            }
                        }
                    }
                }
            };

            updatedMappings.FilterMappingPlan.Mappings.AssertDeepEqualTo(
                expectedFilterMappings,
                ignoreCollectionOrders: true);
        }

        [Fact]
        public async Task Complete_ExactMatch()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            // Create a mapping plan based on 2 data set versions with exactly the same filters
            // and filter options.

            var mappings = new DataSetVersionMapping
            {
                SourceDataSetVersionId = originalVersion.Id,
                TargetDataSetVersionId = nextVersion.Id,
                FilterMappingPlan = new FilterMappingPlan
                {
                    Mappings =
                    {
                        {
                            "Filter 1 key", new FilterMapping
                            {
                                Source = new MappableFilter("Filter 1 label"),
                                OptionMappings =
                                {
                                    {
                                        "Filter 1 option 1 key",
                                        new FilterOptionMapping
                                            {
                                                Source = new MappableFilterOption("Filter 1 option 1 label")
                                            }
                                    },
                                    {
                                        "Filter 1 option 2 key",
                                        new FilterOptionMapping
                                            {
                                                Source = new MappableFilterOption("Filter 1 option 2 label")
                                            }
                                    }
                                }
                            }
                        },
                        {
                            "Filter 2 key", new FilterMapping
                            {
                                Source = new MappableFilter("Filter 2 label"),
                                OptionMappings =
                                {
                                    {
                                        "Filter 2 option 1 key",
                                        new FilterOptionMapping
                                            {
                                                Source = new MappableFilterOption("Filter 2 option 1 label")
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
                                    { "Filter 1 option 2 key", new MappableFilterOption("Filter 1 option 2 label") }
                                }
                            }
                        },
                        {
                            "Filter 2 key",
                            new FilterMappingCandidate("Filter 2 label")
                            {
                                Options =
                                {
                                    {
                                        "Filter 2 option 1 key", new MappableFilterOption("Filter 2 option 1 label")
                                    }
                                }
                            }
                        }
                    }
                },
                LocationMappingPlan = new LocationMappingPlan()
            };
            await AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersionMappings.Add(mappings));

            await ApplyAutoMappings(instanceId);

            var updatedMappings = GetDataSetVersionMapping(nextVersion);

            Assert.True(updatedMappings.FilterMappingsComplete);

            Dictionary<string, FilterMapping> expectedFilterMappings = new()
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
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "Filter 1 option 2 key"
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
                                    CandidateKey = "Filter 2 option 1 key"
                                }
                            }
                        }
                    }
                }
            };

            updatedMappings.FilterMappingPlan.Mappings.AssertDeepEqualTo(expectedFilterMappings);
        }

        [Fact]
        public async Task Complete_AllSourcesMapped_OtherUnmappedCandidatesExist()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            // Create a mapping plan based on 2 data set versions with the same filters
            // and filter options, but additional options exist in the new version.
            // Each source filter and filter option can be auto-mapped exactly to one in
            // the target version, leaving some candidates unused but essentially the mapping
            // is complete unless the user manually intervenes at this point.
            var mappings = new DataSetVersionMapping
            {
                SourceDataSetVersionId = originalVersion.Id,
                TargetDataSetVersionId = nextVersion.Id,
                FilterMappingPlan = new FilterMappingPlan
                {
                    Mappings =
                    {
                        {
                            "Filter 1 key", new FilterMapping
                            {
                                Source = new MappableFilter("Filter 1 label"),
                                OptionMappings =
                                {
                                    {
                                        "Filter 1 option 1 key",
                                        new FilterOptionMapping
                                            {
                                                Source = new MappableFilterOption("Filter 1 option 1 label")
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
                                    { "Filter 1 option 2 key", new MappableFilterOption("Filter 1 option 2 label") }
                                }
                            }
                        },
                        {
                            "Filter 2 key",
                            new FilterMappingCandidate("Filter 2 label")
                            {
                                Options =
                                {
                                    {
                                        "Filter 2 option 1 key", new MappableFilterOption("Filter 2 option 1 label")
                                    }
                                }
                            }
                        }
                    }
                },
                LocationMappingPlan = new LocationMappingPlan()
            };

            await AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersionMappings.Add(mappings));

            await ApplyAutoMappings(instanceId);

            var updatedMappings = GetDataSetVersionMapping(nextVersion);

            Assert.True(updatedMappings.FilterMappingsComplete);

            Dictionary<string, FilterMapping> expectedFilterMappings = new()
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
                            }
                        }
                    }
                }
            };

            updatedMappings.FilterMappingPlan.Mappings.AssertDeepEqualTo(
                expectedFilterMappings,
                ignoreCollectionOrders: true);
        }
    }

    public class CompleteNextDataSetVersionMappingsMappingProcessingTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessNextDataSetVersionMappingsFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.ManualMapping;

        [Fact]
        public async Task Success()
        {
            var (instanceId, _, nextVersion) = await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            Directory.CreateDirectory(dataSetVersionPathResolver.DirectoryPath(nextVersion));

            await CompleteProcessing(instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext.DataSetVersionImports
                .Include(i => i.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            Assert.Null(savedImport.Completed);

            Assert.Equal(DataSetVersionStatus.Mapping, savedImport.DataSetVersion.Status);
        }

        private async Task CompleteProcessing(Guid instanceId)
        {
            var function = GetRequiredService<ProcessNextDataSetVersionMappingsFunction>();
            await function.CompleteNextDataSetVersionMappingProcessing(instanceId, CancellationToken.None);
        }
    }

    private async Task<(Guid instanceId, DataSetVersion initialVersion, DataSetVersion nextVersion)>
        CreateNextDataSetVersionAndDataFiles(DataSetVersionImportStage importStage)
    {
        var (initialDataSetVersion, _) = await CreateDataSet(
            importStage: DataSetVersionImportStage.Completing,
            status: DataSetVersionStatus.Published);

        var dataSet = await GetDbContext<PublicDataDbContext>().DataSets.SingleAsync(dataSet =>
            dataSet.Id == initialDataSetVersion.DataSet.Id);

        var (nextDataSetVersion, instanceId) = await CreateDataSetVersionAndImport(
            dataSet: dataSet,
            importStage: importStage,
            versionMajor: 1,
            versionMinor: 1);

        SetupCsvDataFilesForDataSetVersion(ProcessorTestData.AbsenceSchool, nextDataSetVersion);
        return (instanceId, initialDataSetVersion, nextDataSetVersion);
    }

    private DataSetVersion GetDataSetVersion(DataSetVersion nextVersion)
    {
        return GetDbContext<PublicDataDbContext>()
            .DataSetVersions
            .Single(dsv => dsv.Id == nextVersion.Id);
    }

    private DataSetVersionMapping GetDataSetVersionMapping(DataSetVersion nextVersion)
    {
        return GetDbContext<PublicDataDbContext>()
            .DataSetVersionMappings
            .Single(mapping => mapping.TargetDataSetVersionId == nextVersion.Id);
    }
}
