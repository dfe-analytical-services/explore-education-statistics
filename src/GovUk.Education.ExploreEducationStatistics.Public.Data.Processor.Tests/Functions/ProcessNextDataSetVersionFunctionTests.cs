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

public abstract class ProcessNextDataSetVersionFunctionTests(
    ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public class ProcessNextDataSetVersionTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessNextDataSetVersionFunctionTests(fixture)
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
            var function = GetRequiredService<ProcessNextDataSetVersionFunction>();
            await function.ProcessNextDataSetVersion(
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
        : ProcessNextDataSetVersionFunctionTests(fixture)
    {
        protected const DataSetVersionImportStage Stage = DataSetVersionImportStage.CreatingMappings;

        protected async Task CreateMappings(Guid instanceId)
        {
            var function = GetRequiredService<ProcessNextDataSetVersionFunction>();
            await function.CreateMappings(instanceId, CancellationToken.None);
        }
    }

    public abstract class CreateMappingMiscTests(
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
                    .SetLevel(GeographicLevel.RscRegion)
                    .SetOptions(DataFixture
                        .DefaultLocationRscRegionOptionMeta()
                        .GenerateList(2)
                        .Select(meta => meta as LocationOptionMeta)
                        .ToList()))
                .GenerateList();

            await AddTestData<PublicDataDbContext>(context =>
                context.LocationMetas.AddRange(initialLocationMeta));

            await CreateMappings(instanceId);

            var mappings = GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Single();

            Assert.Equal(initialVersion.Id, mappings.SourceDataSetVersionId);
            Assert.Equal(nextVersion.Id, mappings.TargetDataSetVersionId);
            Assert.False(mappings.LocationMappingsComplete);

            var expectedLocationMappingsFromSource = initialLocationMeta
                .OrderBy(levelMeta => levelMeta.Level)
                .Select(levelMeta => new LocationLevelMappings
                {
                    Level = levelMeta.Level,
                    Mappings = levelMeta
                        .Options
                        .OrderBy(option => option.Label)
                        .Select(option => new LocationOptionMapping
                        {
                            CandidateKey = null,
                            Type = MappingType.AutoNone,
                            Source = new LocationOption
                            {
                                Key = $"{option.Label} :: {option.ToRow().GetRowKey()}",
                                Label = option.Label
                            }
                        })
                        .ToList()
                })
                .ToList();

            // There should be 5 levels of mappings when combining all the source and target levels. 
            Assert.Equal(ProcessorTestData
                    .AbsenceSchool
                    .ExpectedGeographicLevels
                    .Concat([GeographicLevel.RscRegion])
                    .Order(),
                mappings.LocationMappingPlan
                    .Levels
                    .Select(level => level.Level)
                    .Order());

            mappings.LocationMappingPlan.Levels.ForEach(level =>
            {
                var matchingLevelFromSource =
                    expectedLocationMappingsFromSource.SingleOrDefault(sourceLevel => sourceLevel.Level == level.Level);

                if (matchingLevelFromSource != null)
                {
                    level.Mappings.AssertDeepEqualTo(matchingLevelFromSource.Mappings);
                }
                else
                {
                    Assert.Empty(level.Mappings);
                }
            });
        }

        [Fact]
        public async Task Success_Candidates()
        {
            var (instanceId, initialVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            await CreateMappings(instanceId);

            var mappings = GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Single();

            Assert.Equal(initialVersion.Id, mappings.SourceDataSetVersionId);
            Assert.Equal(nextVersion.Id, mappings.TargetDataSetVersionId);

            var expectedLocationLevels = ProcessorTestData
                .AbsenceSchool
                .ExpectedLocations
                .Select(levelMeta => (level: levelMeta.Level, options: levelMeta.Options))
                .OrderBy(levelMeta => levelMeta.level)
                .Select(levelMeta => new LocationLevelMappings
                {
                    Level = levelMeta.level,
                    Candidates = levelMeta
                        .options
                        .OrderBy(optionMeta => optionMeta.Label)
                        .Select(option => new LocationOption
                        {
                            Key = $"{option.Label} :: {option.ToRow().GetRowKey()}",
                            Label = option.Label
                        })
                        .ToList()
                })
                .ToList();

            mappings.LocationMappingPlan.Levels.AssertDeepEqualTo(expectedLocationLevels);
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

            var mappings = GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Single();

            Assert.Equal(initialVersion.Id, mappings.SourceDataSetVersionId);
            Assert.Equal(nextVersion.Id, mappings.TargetDataSetVersionId);
            Assert.False(mappings.FilterMappingsComplete);

            var expectedFilterMappings = initialFilterMeta
                .OrderBy(filter => filter.Label)
                .Select(filter => new FilterMapping
                {
                    CandidateKey = null,
                    Type = MappingType.None,
                    Source = new Filter
                    {
                        Key = filter.Label,
                        Label = filter.Label
                    },
                    OptionMappings = filter
                        .Options
                        .OrderBy(option => option.Label)
                        .Select(option => new FilterOptionMapping
                        {
                            CandidateKey = null,
                            Type = MappingType.None,
                            Source = new FilterOption
                            {
                                Key = option.Label,
                                Label = option.Label
                            }
                        })
                        .ToList()
                })
                .ToList();

            mappings.FilterMappingPlan.Mappings.AssertDeepEqualTo(expectedFilterMappings);
        }

        [Fact]
        public async Task Success_Candidates()
        {
            var (instanceId, initialVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            await CreateMappings(instanceId);

            var mappings = GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Single();

            Assert.Equal(initialVersion.Id, mappings.SourceDataSetVersionId);
            Assert.Equal(nextVersion.Id, mappings.TargetDataSetVersionId);

            var expectedFilterTargets = ProcessorTestData
                .AbsenceSchool
                .ExpectedFilters
                .OrderBy(filterAndOptions => filterAndOptions.Label)
                .Select(filterAndOptions => new FilterMappingCandidate
                {
                    Key = filterAndOptions.Label,
                    Label = filterAndOptions.Label,
                    Options = filterAndOptions
                        .Options
                        .OrderBy(optionMeta => optionMeta.Label)
                        .Select(optionMeta => new FilterOption
                        {
                            Key = optionMeta.Label,
                            Label = optionMeta.Label
                        })
                        .ToList()
                })
                .ToList();

            mappings.FilterMappingPlan.Candidates.AssertDeepEqualTo(expectedFilterTargets);
        }
    }

    public abstract class ApplyAutoMappingsTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessNextDataSetVersionFunctionTests(fixture)
    {
        protected const DataSetVersionImportStage Stage = DataSetVersionImportStage.AutoMapping;

        protected async Task ApplyAutoMappings(Guid instanceId)
        {
            var function = GetRequiredService<ProcessNextDataSetVersionFunction>();
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
            var mapping = new DataSetVersionMapping
            {
                SourceDataSetVersionId = originalVersion.Id,
                TargetDataSetVersionId = nextVersion.Id,
                FilterMappingPlan = new FilterMappingPlan(),
                LocationMappingPlan = new LocationMappingPlan
                {
                    Levels =
                    [
                        new LocationLevelMappings
                        {
                            Level = GeographicLevel.LocalAuthority,
                            Mappings =
                            [
                                new LocationOptionMapping
                                {
                                    Source = new LocationOption
                                    {
                                        Key = "LA location 1 key",
                                        Label = "LA location 1 label"
                                    }
                                },
                                new LocationOptionMapping
                                {
                                    Source = new LocationOption
                                    {
                                        Key = "LA location 2 key",
                                        Label = "LA location 2 label"
                                    }
                                }
                            ],
                            Candidates =
                            [
                                new LocationOption
                                {
                                    Key = "LA location 1 key",
                                    Label = "LA location 1 label"
                                },
                                new LocationOption
                                {
                                    Key = "LA location 3 key",
                                    Label = "LA location 3 label"
                                }
                            ]
                        },
                        new LocationLevelMappings
                        {
                            Level = GeographicLevel.RscRegion,
                            Mappings =
                            [
                                new LocationOptionMapping
                                {
                                    Source = new LocationOption
                                    {
                                        Key = "RSC location 1 key",
                                        Label = "RSC location 1 label"
                                    }
                                }
                            ],
                            Candidates = []
                        },
                        new LocationLevelMappings
                        {
                            Level = GeographicLevel.Country,
                            Mappings = [],
                            Candidates =
                            [
                                new LocationOption
                                {
                                    Key = "Country location 1 key",
                                    Label = "Country location 1 label"
                                }
                            ]
                        }
                    ]
                }
            };

            await AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var mappings = GetDataSetVersionMapping(nextVersion);

            Assert.False(mappings.LocationMappingsComplete);

            List<LocationLevelMappings> expectedLevelMappings =
            [
                new LocationLevelMappings
                {
                    Level = GeographicLevel.LocalAuthority,
                    Mappings =
                    [
                        new LocationOptionMapping
                        {
                            Source = new LocationOption
                            {
                                Key = "LA location 1 key",
                                Label = "LA location 1 label"
                            },
                            Type = MappingType.AutoMapped,
                            CandidateKey = "LA location 1 key"
                        },
                        new LocationOptionMapping
                        {
                            Source = new LocationOption
                            {
                                Key = "LA location 2 key",
                                Label = "LA location 2 label"
                            },
                            Type = MappingType.AutoNone,
                            CandidateKey = null
                        }
                    ],
                    Candidates =
                    [
                        new LocationOption
                        {
                            Key = "LA location 1 key",
                            Label = "LA location 1 label"
                        },
                        new LocationOption
                        {
                            Key = "LA location 3 key",
                            Label = "LA location 3 label"
                        }
                    ]
                },
                new LocationLevelMappings
                {
                    Level = GeographicLevel.RscRegion,
                    Mappings =
                    [
                        new LocationOptionMapping
                        {
                            Source = new LocationOption
                            {
                                Key = "RSC location 1 key",
                                Label = "RSC location 1 label"
                            },
                            Type = MappingType.AutoNone,
                            CandidateKey = null
                        }
                    ],
                    Candidates = []
                },
                new LocationLevelMappings
                {
                    Level = GeographicLevel.Country,
                    Mappings = [],
                    Candidates =
                    [
                        new LocationOption
                        {
                            Key = "Country location 1 key",
                            Label = "Country location 1 label"
                        }
                    ]
                }
            ];

            mappings.LocationMappingPlan.Levels.AssertDeepEqualTo(expectedLevelMappings);
        }

        [Fact]
        public async Task Complete_ExactMatch()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            // Create a mapping plan based on 2 data set versions with perfectly overlapping
            // locations and levels.
            var mapping = new DataSetVersionMapping
            {
                SourceDataSetVersionId = originalVersion.Id,
                TargetDataSetVersionId = nextVersion.Id,
                FilterMappingPlan = new FilterMappingPlan(),
                LocationMappingPlan = new LocationMappingPlan
                {
                    Levels =
                    [
                        new LocationLevelMappings
                        {
                            Level = GeographicLevel.LocalAuthority,
                            Mappings =
                            [
                                new LocationOptionMapping
                                {
                                    Source = new LocationOption
                                    {
                                        Key = "LA location 1 key",
                                        Label = "LA location 1 label"
                                    }
                                }
                            ],
                            Candidates =
                            [
                                new LocationOption
                                {
                                    Key = "LA location 1 key",
                                    Label = "LA location 1 label"
                                }
                            ]
                        }
                    ]
                }
            };

            await AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var mappings = GetDataSetVersionMapping(nextVersion);

            Assert.True(mappings.LocationMappingsComplete);

            List<LocationLevelMappings> expectedLevelMappings =
            [
                new LocationLevelMappings
                {
                    Level = GeographicLevel.LocalAuthority,
                    Mappings =
                    [
                        new LocationOptionMapping
                        {
                            Source = new LocationOption
                            {
                                Key = "LA location 1 key",
                                Label = "LA location 1 label"
                            },
                            Type = MappingType.AutoMapped,
                            CandidateKey = "LA location 1 key"
                        }
                    ],
                    Candidates =
                    [
                        new LocationOption
                        {
                            Key = "LA location 1 key",
                            Label = "LA location 1 label"
                        }
                    ]
                }
            ];

            mappings.LocationMappingPlan.Levels.AssertDeepEqualTo(expectedLevelMappings);
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
            var mapping = new DataSetVersionMapping
            {
                SourceDataSetVersionId = originalVersion.Id,
                TargetDataSetVersionId = nextVersion.Id,
                FilterMappingPlan = new FilterMappingPlan(),
                LocationMappingPlan = new LocationMappingPlan
                {
                    Levels =
                    [
                        new LocationLevelMappings
                        {
                            Level = GeographicLevel.LocalAuthority,
                            Mappings =
                            [
                                new LocationOptionMapping
                                {
                                    Source = new LocationOption
                                    {
                                        Key = "LA location 1 key",
                                        Label = "LA location 1 label"
                                    }
                                }
                            ],
                            Candidates =
                            [
                                new LocationOption
                                {
                                    Key = "LA location 1 key",
                                    Label = "LA location 1 label"
                                },
                                new LocationOption
                                {
                                    Key = "LA location 2 key",
                                    Label = "LA location 2 label"
                                }
                            ]
                        },
                        new LocationLevelMappings
                        {
                            Level = GeographicLevel.RscRegion,
                            Mappings = [],
                            Candidates =
                            [
                                new LocationOption
                                {
                                    Key = "RSC location 1 key",
                                    Label = "LA location 1 label"
                                }
                            ]
                        }
                    ]
                }
            };

            await AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var mappings = GetDataSetVersionMapping(nextVersion);

            Assert.True(mappings.LocationMappingsComplete);

            List<LocationLevelMappings> expectedLevelMappings =
            [
                new LocationLevelMappings
                {
                    Level = GeographicLevel.LocalAuthority,
                    Mappings =
                    [
                        new LocationOptionMapping
                        {
                            Source = new LocationOption
                            {
                                Key = "LA location 1 key",
                                Label = "LA location 1 label"
                            },
                            Type = MappingType.AutoMapped,
                            CandidateKey = "LA location 1 key"
                        }
                    ],
                    Candidates =
                    [
                        new LocationOption
                        {
                            Key = "LA location 1 key",
                            Label = "LA location 1 label"
                        },
                        new LocationOption
                        {
                            Key = "LA location 2 key",
                            Label = "LA location 2 label"
                        }
                    ]
                },
                new LocationLevelMappings
                {
                    Level = GeographicLevel.RscRegion,
                    Mappings = [],
                    Candidates =
                    [
                        new LocationOption
                        {
                            Key = "RSC location 1 key",
                            Label = "LA location 1 label"
                        }
                    ]
                }
            ];

            mappings.LocationMappingPlan.Levels.AssertDeepEqualTo(expectedLevelMappings);
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
            var mapping = new DataSetVersionMapping
            {
                SourceDataSetVersionId = originalVersion.Id,
                TargetDataSetVersionId = nextVersion.Id,
                FilterMappingPlan = new FilterMappingPlan
                {
                    Mappings =
                    [
                        new()
                        {
                            Source = new Filter
                            {
                                Key = "Filter 1 key",
                                Label = "Filter 1 label"
                            },
                            OptionMappings =
                            [
                                new()
                                {
                                    Source = new()
                                    {
                                        Key = "Filter 1 option 1 key",
                                        Label = "Filter 1 option 1 label"
                                    }
                                },
                                new()
                                {
                                    Source = new()
                                    {
                                        Key = "Filter 1 option 2 key",
                                        Label = "Filter 1 option 2 label"
                                    }
                                }
                            ]
                        },
                        new()
                        {
                            Source = new Filter
                            {
                                Key = "Filter 2 key",
                                Label = "Filter 2 label"
                            },
                            OptionMappings =
                            [
                                new()
                                {
                                    Source = new()
                                    {
                                        Key = "Filter 2 option 1 key",
                                        Label = "Filter 2 option 1 label"
                                    }
                                }
                            ]
                        }
                    ],
                    Candidates =
                    [
                        new()
                        {
                            Key = "Filter 1 key",
                            Label = "Filter 1 label",
                            Options =
                            [
                                new()
                                {
                                    Key = "Filter 1 option 1 key",
                                    Label = "Filter 1 option 1 label"
                                },
                                new()
                                {
                                    Key = "Filter 1 option 3 key",
                                    Label = "Filter 1 option 3 label"
                                }
                            ]
                        },
                        new()
                        {
                            Key = "Filter 3 key",
                            Label = "Filter 3 label",
                            Options =
                            [
                                new()
                                {
                                    Key = "Filter 3 option 1 key",
                                    Label = "Filter 3 option 1 label"
                                }
                            ]
                        }
                    ]
                },
                LocationMappingPlan = new LocationMappingPlan()
            };

            await AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var mappings = GetDataSetVersionMapping(nextVersion);

            Assert.False(mappings.FilterMappingsComplete);

            List<FilterMapping> expectedFilterMappings =
            [
                new()
                {
                    Source = new Filter
                    {
                        Key = "Filter 1 key",
                        Label = "Filter 1 label"
                    },
                    Type = MappingType.AutoMapped,
                    CandidateKey = "Filter 1 key",
                    OptionMappings =
                    [
                        new()
                        {
                            Source = new()
                            {
                                Key = "Filter 1 option 1 key",
                                Label = "Filter 1 option 1 label"
                            },
                            Type = MappingType.AutoMapped,
                            CandidateKey = "Filter 1 option 1 key"
                        },
                        new()
                        {
                            Source = new()
                            {
                                Key = "Filter 1 option 2 key",
                                Label = "Filter 1 option 2 label"
                            },
                            Type = MappingType.AutoNone,
                            CandidateKey = null
                        }
                    ]
                },
                new()
                {
                    Source = new Filter
                    {
                        Key = "Filter 2 key",
                        Label = "Filter 2 label"
                    },
                    Type = MappingType.AutoNone,
                    CandidateKey = null,
                    OptionMappings =
                    [
                        new()
                        {
                            Source = new()
                            {
                                Key = "Filter 2 option 1 key",
                                Label = "Filter 2 option 1 label"
                            },
                            Type = MappingType.AutoNone,
                            CandidateKey = null,
                        }
                    ]
                }
            ];

            mappings.FilterMappingPlan.Mappings.AssertDeepEqualTo(expectedFilterMappings);
        }

        [Fact]
        public async Task Complete_ExactMatch()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            // Create a mapping plan based on 2 data set versions with exactly the same filters
            // and filter options.
            var mapping = new DataSetVersionMapping
            {
                SourceDataSetVersionId = originalVersion.Id,
                TargetDataSetVersionId = nextVersion.Id,
                FilterMappingPlan = new FilterMappingPlan
                {
                    Mappings =
                    [
                        new()
                        {
                            Source = new Filter
                            {
                                Key = "Filter 1 key",
                                Label = "Filter 1 label"
                            },
                            OptionMappings =
                            [
                                new()
                                {
                                    Source = new()
                                    {
                                        Key = "Filter 1 option 1 key",
                                        Label = "Filter 1 option 1 label"
                                    }
                                },
                                new()
                                {
                                    Source = new()
                                    {
                                        Key = "Filter 1 option 2 key",
                                        Label = "Filter 1 option 2 label"
                                    }
                                }
                            ]
                        }
                    ],
                    Candidates =
                    [
                        new()
                        {
                            Key = "Filter 1 key",
                            Label = "Filter 1 label",
                            Options =
                            [
                                new()
                                {
                                    Key = "Filter 1 option 1 key",
                                    Label = "Filter 1 option 1 label"
                                },
                                new()
                                {
                                    Key = "Filter 1 option 2 key",
                                    Label = "Filter 1 option 2 label"
                                }
                            ]
                        }
                    ]
                },
                LocationMappingPlan = new LocationMappingPlan()
            };

            await AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var mappings = GetDataSetVersionMapping(nextVersion);

            Assert.True(mappings.FilterMappingsComplete);

            List<FilterMapping> expectedFilterMappings =
            [
                new()
                {
                    Source = new Filter
                    {
                        Key = "Filter 1 key",
                        Label = "Filter 1 label"
                    },
                    Type = MappingType.AutoMapped,
                    CandidateKey = "Filter 1 key",
                    OptionMappings =
                    [
                        new()
                        {
                            Source = new()
                            {
                                Key = "Filter 1 option 1 key",
                                Label = "Filter 1 option 1 label"
                            },
                            Type = MappingType.AutoMapped,
                            CandidateKey = "Filter 1 option 1 key"
                        },
                        new()
                        {
                            Source = new()
                            {
                                Key = "Filter 1 option 2 key",
                                Label = "Filter 1 option 2 label"
                            },
                            Type = MappingType.AutoMapped,
                            CandidateKey = "Filter 1 option 2 key"
                        }
                    ]
                }
            ];

            mappings.FilterMappingPlan.Mappings.AssertDeepEqualTo(expectedFilterMappings);
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
            var mapping = new DataSetVersionMapping
            {
                SourceDataSetVersionId = originalVersion.Id,
                TargetDataSetVersionId = nextVersion.Id,
                FilterMappingPlan = new FilterMappingPlan
                {
                    Mappings =
                    [
                        new()
                        {
                            Source = new Filter
                            {
                                Key = "Filter 1 key",
                                Label = "Filter 1 label"
                            },
                            OptionMappings =
                            [
                                new()
                                {
                                    Source = new()
                                    {
                                        Key = "Filter 1 option 1 key",
                                        Label = "Filter 1 option 1 label"
                                    }
                                }
                            ]
                        }
                    ],
                    Candidates =
                    [
                        new()
                        {
                            Key = "Filter 1 key",
                            Label = "Filter 1 label",
                            Options =
                            [
                                new()
                                {
                                    Key = "Filter 1 option 1 key",
                                    Label = "Filter 1 option 1 label"
                                },
                                new()
                                {
                                    Key = "Filter 1 option 2 key",
                                    Label = "Filter 1 option 2 label"
                                }
                            ]
                        },
                        new()
                        {
                            Key = "Filter 2 key",
                            Label = "Filter 2 label",
                            Options =
                            [
                                new()
                                {
                                    Key = "Filter 2 option 1 key",
                                    Label = "Filter 2 option 1 label"
                                }
                            ]
                        }
                    ]
                },
                LocationMappingPlan = new LocationMappingPlan()
            };

            await AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var mappings = GetDataSetVersionMapping(nextVersion);

            Assert.True(mappings.FilterMappingsComplete);

            List<FilterMapping> expectedFilterMappings =
            [
                new()
                {
                    Source = new Filter
                    {
                        Key = "Filter 1 key",
                        Label = "Filter 1 label"
                    },
                    Type = MappingType.AutoMapped,
                    CandidateKey = "Filter 1 key",
                    OptionMappings =
                    [
                        new()
                        {
                            Source = new()
                            {
                                Key = "Filter 1 option 1 key",
                                Label = "Filter 1 option 1 label"
                            },
                            Type = MappingType.AutoMapped,
                            CandidateKey = "Filter 1 option 1 key"
                        }
                    ]
                }
            ];

            mappings.FilterMappingPlan.Mappings.AssertDeepEqualTo(expectedFilterMappings);
        }
    }

    public class CompleteNextDataSetVersionMappingProcessingTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessNextDataSetVersionFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.Completing;

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
            savedImport.Completed.AssertUtcNow();

            Assert.Equal(DataSetVersionStatus.Mapping, savedImport.DataSetVersion.Status);
        }

        private async Task CompleteProcessing(Guid instanceId)
        {
            var function = GetRequiredService<ProcessNextDataSetVersionFunction>();
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
