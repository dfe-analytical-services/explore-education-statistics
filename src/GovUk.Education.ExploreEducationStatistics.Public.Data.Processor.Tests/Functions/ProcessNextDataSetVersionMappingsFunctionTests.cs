using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
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

        private static Mock<TaskOrchestrationContext> DefaultMockOrchestrationContext(Guid? instanceId = null)
        {
            var mock = new Mock<TaskOrchestrationContext>(MockBehavior.Strict);

            mock.Setup(context => context.CreateReplaySafeLogger(
                    nameof(ProcessNextDataSetVersionMappingsFunction.ProcessNextDataSetVersionMappings)))
                .Returns(NullLogger.Instance);

            mock.SetupGet(context => context.InstanceId)
                .Returns(instanceId?.ToString() ?? Guid.NewGuid().ToString());

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
                            .OptionLinks
                            .ToDictionary(
                                keySelector: MappingKeyGenerators.LocationOptionMetaLink,
                                elementSelector: link => new LocationOptionMapping
                                {
                                    CandidateKey = null,
                                    PublicId = link.PublicId,
                                    Type = MappingType.None,
                                    Source = new MappableLocationOption
                                    {
                                        Label = link.Option.Label,
                                        Code = link.Option.ToRow().Code,
                                        OldCode = link.Option.ToRow().OldCode,
                                        Urn = link.Option.ToRow().Urn,
                                        LaEstab = link.Option.ToRow().LaEstab,
                                        Ukprn = link.Option.ToRow().Ukprn
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
                .ToDictionary(
                    keySelector: levelMeta => levelMeta.Level,
                    elementSelector: levelMeta =>
                        new LocationLevelMappings
                        {
                            Candidates = levelMeta
                                .Options
                                .ToDictionary(
                                    keySelector: MappingKeyGenerators.LocationOptionMeta,
                                    elementSelector: option => new MappableLocationOption
                                    {
                                        Label = option.Label,
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
                    keySelector: MappingKeyGenerators.Filter,
                    elementSelector: filter =>
                        new FilterMapping
                        {
                            CandidateKey = null,
                            PublicId = filter.PublicId,
                            Source = new MappableFilter { Label = filter.Label },
                            OptionMappings = filter
                                .OptionLinks
                                .ToDictionary(
                                    keySelector: MappingKeyGenerators.FilterOptionMetaLink,
                                    elementSelector: link =>
                                        new FilterOptionMapping
                                        {
                                            CandidateKey = null,
                                            PublicId = link.PublicId,
                                            Source = new MappableFilterOption { Label = link.Option.Label }
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
                    keySelector: MappingKeyGenerators.Filter,
                    elementSelector: filter =>
                        new FilterMappingCandidate
                        {
                            Label = filter.Label,
                            Options = filter
                                .Options
                                .ToDictionary(
                                    keySelector: MappingKeyGenerators.FilterOptionMeta,
                                    elementSelector: optionMeta =>
                                        new MappableFilterOption { Label = optionMeta.Label })
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

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextVersion.Release.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile())
                .WithPublicApiDataSetId(nextVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextVersion.SemVersion());

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

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
        public async Task Incomplete_UnmappedOptionWithNoCandidate_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithLocationMappingPlan(DataFixture
                    .DefaultLocationMappingPlan()
                    .AddLevel(
                        level: GeographicLevel.LocalAuthority,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            // 'LA Location 1' option is auto-mapped.
                            .AddMapping(
                                sourceKey: "la-location-1-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping())
                            // 'LA Location 2' option has no candidate - it was deleted.
                            .AddMapping(
                                sourceKey: "la-location-2-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping()
                            )
                            .AddCandidate(
                                targetKey: "la-location-1-key",
                                candidate: DataFixture.DefaultMappableLocationOption())));

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mappings));

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextVersion.Release.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile())
                .WithPublicApiDataSetId(nextVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextVersion.SemVersion());

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            await ApplyAutoMappings(instanceId);

            var updatedMappings = GetDataSetVersionMapping(nextVersion);

            var laMapping1 = mappings
                .GetLocationOptionMapping(GeographicLevel.LocalAuthority, "la-location-1-key");

            var laMapping2 = mappings
                .GetLocationOptionMapping(GeographicLevel.LocalAuthority, "la-location-2-key");

            Dictionary<GeographicLevel, LocationLevelMappings> expectedLevelMappings = new()
            {
                {
                    GeographicLevel.LocalAuthority,
                    mappings.GetLocationLevelMappings(GeographicLevel.LocalAuthority) with
                    {
                        Mappings = new Dictionary<string, LocationOptionMapping>
                        {
                            {
                                "la-location-1-key", laMapping1 with
                                {
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "la-location-1-key"
                                }
                            },
                            {
                                "la-location-2-key", laMapping2 with
                                {
                                    Type = MappingType.AutoNone,
                                    CandidateKey = null
                                }
                            }
                        }
                    }
                },
            };

            updatedMappings.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedLevelMappings,
                ignoreCollectionOrders: true);

            Assert.False(updatedMappings.LocationMappingsComplete);

            // Some options have no auto-mapped candidate - major version update.
            Assert.Equal("2.0.0", updatedMappings.TargetDataSetVersion.SemVersion());

            var updatedReleaseFile = await GetDbContext<ContentDbContext>()
                .ReleaseFiles
                .SingleAsync(rf => rf.PublicApiDataSetId == updatedMappings.TargetDataSetVersion.DataSetId);

            Assert.Equal(updatedMappings.TargetDataSetVersion.SemVersion(), updatedReleaseFile.PublicApiDataSetVersion);
        }

        [Fact]
        public async Task Incomplete_UnmappedOptionWithNewCandidate_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithLocationMappingPlan(DataFixture
                    .DefaultLocationMappingPlan()
                    .AddLevel(
                        level: GeographicLevel.LocalAuthority,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            // 'LA Location 1' is auto mapped.
                            .AddMapping(
                                sourceKey: "la-location-1-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping())
                            // 'LA Location 2' option is unmapped.
                            .AddMapping(
                                sourceKey: "la-location-2-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping()
                            )
                            .AddCandidate(
                                targetKey: "la-location-1-key",
                                candidate: DataFixture.DefaultMappableLocationOption())
                            // 'LA Location 3' option is new candidate that can be mapped.
                            .AddCandidate(
                                targetKey: "la-location-3-key",
                                candidate: DataFixture.DefaultMappableLocationOption())));

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mappings));

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextVersion.Release.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile())
                .WithPublicApiDataSetId(nextVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextVersion.SemVersion());

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            await ApplyAutoMappings(instanceId);

            var updatedMappings = GetDataSetVersionMapping(nextVersion);

            var laMapping1 = mappings
                .GetLocationOptionMapping(GeographicLevel.LocalAuthority, "la-location-1-key");

            var laMapping2 = mappings
                .GetLocationOptionMapping(GeographicLevel.LocalAuthority, "la-location-2-key");

            Dictionary<GeographicLevel, LocationLevelMappings> expectedLevelMappings = new()
            {
                {
                    GeographicLevel.LocalAuthority,
                    mappings.GetLocationLevelMappings(GeographicLevel.LocalAuthority) with
                    {
                        Mappings = new Dictionary<string, LocationOptionMapping>
                        {
                            {
                                "la-location-1-key", laMapping1 with
                                {
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "la-location-1-key"
                                }
                            },
                            {
                                "la-location-2-key", laMapping2 with
                                {
                                    Type = MappingType.AutoNone,
                                    CandidateKey = null
                                }
                            }
                        }
                    }
                },
            };

            updatedMappings.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedLevelMappings,
                ignoreCollectionOrders: true);

            Assert.False(updatedMappings.LocationMappingsComplete);

            // Some options have no auto-mapped candidate - major version update.
            Assert.Equal("2.0.0", updatedMappings.TargetDataSetVersion.SemVersion());

            var updatedReleaseFile = await GetDbContext<ContentDbContext>()
                .ReleaseFiles
                .SingleAsync(rf => rf.PublicApiDataSetId == updatedMappings.TargetDataSetVersion.DataSetId);

            Assert.Equal(updatedMappings.TargetDataSetVersion.SemVersion(), updatedReleaseFile.PublicApiDataSetVersion);
        }

        [Fact]
        public async Task Complete_DeletedLevel_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithLocationMappingPlan(DataFixture
                    .DefaultLocationMappingPlan()
                    .AddLevel(
                        level: GeographicLevel.RscRegion,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            .AddMapping(
                                sourceKey: "rsc-location-1-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithManualNone())));

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mappings));

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextVersion.Release.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile())
                .WithPublicApiDataSetId(nextVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextVersion.SemVersion());

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            await ApplyAutoMappings(instanceId);

            var updatedMappings = GetDataSetVersionMapping(nextVersion);

            var rscMapping1 = mappings
                .GetLocationOptionMapping(GeographicLevel.RscRegion, "rsc-location-1-key");

            Dictionary<GeographicLevel, LocationLevelMappings> expectedLevelMappings = new()
            {
                {
                    GeographicLevel.RscRegion,
                    mappings.GetLocationLevelMappings(GeographicLevel.RscRegion) with
                    {
                        Mappings = new Dictionary<string, LocationOptionMapping>
                        {
                            {
                                "rsc-location-1-key", rscMapping1 with
                                {
                                    Type = MappingType.AutoNone,
                                    CandidateKey = null
                                }
                            }
                        }
                    }
                },
            };

            updatedMappings.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedLevelMappings,
                ignoreCollectionOrders: true);

            Assert.True(updatedMappings.LocationMappingsComplete);

            // Level has been deleted (cannot be mapped) - major version update.
            Assert.Equal("2.0.0", updatedMappings.TargetDataSetVersion.SemVersion());

            var updatedReleaseFile = await GetDbContext<ContentDbContext>()
                .ReleaseFiles
                .SingleAsync(rf => rf.PublicApiDataSetId == updatedMappings.TargetDataSetVersion.DataSetId);

            Assert.Equal(updatedMappings.TargetDataSetVersion.SemVersion(), updatedReleaseFile.PublicApiDataSetVersion);
        }

        [Fact]
        public async Task Complete_NewLevel_MinorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithLocationMappingPlan(DataFixture
                    .DefaultLocationMappingPlan()
                    .AddLevel(
                        level: GeographicLevel.Country,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            .AddCandidate(
                                targetKey: "country-location-1-key",
                                candidate: DataFixture
                                    .DefaultMappableLocationOption())));

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mappings));

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextVersion.Release.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile())
                .WithPublicApiDataSetId(nextVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextVersion.SemVersion());

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            await ApplyAutoMappings(instanceId);

            var updatedMappings = GetDataSetVersionMapping(nextVersion);

            Dictionary<GeographicLevel, LocationLevelMappings> expectedLevelMappings = new()
            {
                { GeographicLevel.Country, mappings.GetLocationLevelMappings(GeographicLevel.Country) }
            };

            updatedMappings.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedLevelMappings,
                ignoreCollectionOrders: true);

            Assert.True(updatedMappings.LocationMappingsComplete);

            // Level has been added - minor version update.
            Assert.Equal("1.1.0", updatedMappings.TargetDataSetVersion.SemVersion());

            var updatedReleaseFile = await GetDbContext<ContentDbContext>()
                .ReleaseFiles
                .SingleAsync(rf => rf.PublicApiDataSetId == updatedMappings.TargetDataSetVersion.DataSetId);

            Assert.Equal(updatedMappings.TargetDataSetVersion.SemVersion(), updatedReleaseFile.PublicApiDataSetVersion);
        }

        [Fact]
        public async Task Complete_ExactMatch_MinorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithLocationMappingPlan(DataFixture
                    .DefaultLocationMappingPlan()
                    .AddLevel(
                        level: GeographicLevel.LocalAuthority,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            .AddMapping(
                                sourceKey: "location-1-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping())
                            .AddCandidate(
                                targetKey: "location-1-key",
                                candidate: DataFixture.DefaultMappableLocationOption())));

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mappings));

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextVersion.Release.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile())
                .WithPublicApiDataSetId(nextVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextVersion.SemVersion());

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            await ApplyAutoMappings(instanceId);

            var updatedMappings = GetDataSetVersionMapping(nextVersion);

            var originalLocationMapping = mappings
                .GetLocationOptionMapping(GeographicLevel.LocalAuthority, "location-1-key");

            Dictionary<GeographicLevel, LocationLevelMappings> expectedLevelMappings = new()
            {
                {
                    GeographicLevel.LocalAuthority,
                    mappings.GetLocationLevelMappings(GeographicLevel.LocalAuthority) with
                    {
                        Mappings = new Dictionary<string, LocationOptionMapping>
                        {
                            {
                                "location-1-key", originalLocationMapping with
                                {
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "location-1-key"
                                }
                            }
                        }
                    }
                }
            };

            updatedMappings.LocationMappingPlan.Levels.AssertDeepEqualTo(expectedLevelMappings);

            Assert.True(updatedMappings.LocationMappingsComplete);

            // All source options have auto-mapped candidates - minor version update.
            Assert.Equal("1.1.0", updatedMappings.TargetDataSetVersion.SemVersion());

            var updatedReleaseFile = await GetDbContext<ContentDbContext>()
                .ReleaseFiles
                .SingleAsync(rf => rf.PublicApiDataSetId == updatedMappings.TargetDataSetVersion.DataSetId);

            Assert.Equal(updatedMappings.TargetDataSetVersion.SemVersion(), updatedReleaseFile.PublicApiDataSetVersion);
        }

        [Fact]
        public async Task Complete_AutoMappedAndNewOptions_MinorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithLocationMappingPlan(DataFixture
                    .DefaultLocationMappingPlan()
                    .AddLevel(
                        level: GeographicLevel.LocalAuthority,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            // 'LA Location 1' option is auto-mapped.
                            .AddMapping(
                                sourceKey: "la-location-1-key",
                                mapping: DataFixture
                                    .DefaultLocationOptionMapping()
                                    .WithSource(DataFixture.DefaultMappableLocationOption())
                                    .WithNoMapping())
                            .AddCandidate(
                                targetKey: "la-location-1-key",
                                candidate: DataFixture.DefaultMappableLocationOption())
                            // 'LA Location 2' option is newly added.
                            .AddCandidate(
                                targetKey: "la-location-2-key",
                                candidate: DataFixture.DefaultMappableLocationOption()))
                    .AddLevel(
                        level: GeographicLevel.RscRegion,
                        mappings: DataFixture
                            .DefaultLocationLevelMappings()
                            // 'RSC Location 1' is newly added.
                            .AddCandidate(
                                targetKey: "rsc-location-1-key",
                                candidate: DataFixture
                                    .DefaultMappableLocationOption())));

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mappings));

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextVersion.Release.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile())
                .WithPublicApiDataSetId(nextVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextVersion.SemVersion());

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            await ApplyAutoMappings(instanceId);

            var updatedMappings = GetDataSetVersionMapping(nextVersion);

            var originalLaMapping = mappings
                .GetLocationOptionMapping(GeographicLevel.LocalAuthority, "la-location-1-key");

            Dictionary<GeographicLevel, LocationLevelMappings> expectedLevelMappings = new()
            {
                {
                    GeographicLevel.LocalAuthority,
                    mappings.GetLocationLevelMappings(GeographicLevel.LocalAuthority) with
                    {
                        Mappings = new Dictionary<string, LocationOptionMapping>
                        {
                            {
                                "la-location-1-key", originalLaMapping with
                                {
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "la-location-1-key"
                                }
                            }
                        }
                    }
                },
                { GeographicLevel.RscRegion, mappings.GetLocationLevelMappings(GeographicLevel.RscRegion) }
            };

            updatedMappings.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedLevelMappings,
                ignoreCollectionOrders: true);

            Assert.True(updatedMappings.LocationMappingsComplete);

            // All source options have auto-mapped candidates - minor version update.
            Assert.Equal("1.1.0", updatedMappings.TargetDataSetVersion.SemVersion());

            var updatedReleaseFile = await GetDbContext<ContentDbContext>()
                .ReleaseFiles
                .SingleAsync(rf => rf.PublicApiDataSetId == updatedMappings.TargetDataSetVersion.DataSetId);

            Assert.Equal(updatedMappings.TargetDataSetVersion.SemVersion(), updatedReleaseFile.PublicApiDataSetVersion);
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
            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithFilterMappingPlan(DataFixture
                    .DefaultFilterMappingPlan()
                    .AddFilterMapping("filter-1-key", DataFixture
                        .DefaultFilterMapping()
                        .WithNoMapping()
                        .AddOptionMapping("filter-1-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithNoMapping())
                        .AddOptionMapping("filter-1-option-2-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithNoMapping()))
                    .AddFilterMapping("filter-2-key", DataFixture
                        .DefaultFilterMapping()
                        .WithNoMapping()
                        .AddOptionMapping("filter-2-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithNoMapping()))
                    .AddFilterCandidate("filter-1-key", DataFixture
                        .DefaultFilterMappingCandidate()
                        .AddOptionCandidate("filter-1-option-1-key", DataFixture
                            .DefaultMappableFilterOption())
                        .AddOptionCandidate("filter-1-option-3-key", DataFixture
                            .DefaultMappableFilterOption())));

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mappings));

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextVersion.Release.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile())
                .WithPublicApiDataSetId(nextVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextVersion.SemVersion());

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            await ApplyAutoMappings(instanceId);

            var updatedMappings = GetDataSetVersionMapping(nextVersion);

            Dictionary<string, FilterMapping> expectedFilterMappings = new()
            {
                {
                    "filter-1-key", mappings.GetFilterMapping("filter-1-key") with
                    {
                        // The code managed to establish an automapping for this filter.
                        Type = MappingType.AutoMapped,
                        CandidateKey = "filter-1-key",
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            {
                                "filter-1-option-1-key",
                                mappings.GetFilterOptionMapping("filter-1-key", "filter-1-option-1-key") with
                                {
                                    // The code managed to establish an automapping for this filter option.
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "filter-1-option-1-key"
                                }
                            },
                            {
                                "filter-1-option-2-key",
                                mappings.GetFilterOptionMapping("filter-1-key", "filter-1-option-2-key") with
                                {
                                    // The code managed to establish that no obvious automapping candidate exists for
                                    // this filter option.
                                    Type = MappingType.AutoNone,
                                    CandidateKey = null
                                }
                            }
                        }
                    }
                },
                {
                    "filter-2-key", mappings.GetFilterMapping("filter-2-key") with
                    {
                        // The code managed to establish that no obvious automapping candidate exists for
                        // this filter.
                        Type = MappingType.AutoNone,
                        CandidateKey = null,
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            {
                                "filter-2-option-1-key",
                                mappings.GetFilterOptionMapping("filter-2-key", "filter-2-option-1-key") with
                                {
                                    // The code managed to establish that no obvious automapping candidate exists for
                                    // this filter option.
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

            Assert.False(updatedMappings.FilterMappingsComplete);

            // Some source filter options have no equivalent candidate to be mapped to, thus
            // resulting in a major version update.
            Assert.Equal("2.0.0", updatedMappings.TargetDataSetVersion.SemVersion());

            var updatedReleaseFile = await GetDbContext<ContentDbContext>()
                .ReleaseFiles
                .SingleAsync(rf => rf.PublicApiDataSetId == updatedMappings.TargetDataSetVersion.DataSetId);

            Assert.Equal(updatedMappings.TargetDataSetVersion.SemVersion(), updatedReleaseFile.PublicApiDataSetVersion);
        }

        [Fact]
        public async Task Complete_ExactMatch()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            // Create a mapping plan based on 2 data set versions with exactly the same filters
            // and filter options.
            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithFilterMappingPlan(DataFixture
                    .DefaultFilterMappingPlan()
                    .AddFilterMapping("filter-1-key", DataFixture
                        .DefaultFilterMapping()
                        .WithNoMapping()
                        .AddOptionMapping("filter-1-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithNoMapping())
                        .AddOptionMapping("filter-1-option-2-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithNoMapping()))
                    .AddFilterMapping("filter-2-key", DataFixture
                        .DefaultFilterMapping()
                        .WithNoMapping()
                        .AddOptionMapping("filter-2-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithNoMapping()))
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

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mappings));

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextVersion.Release.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile())
                .WithPublicApiDataSetId(nextVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextVersion.SemVersion());

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            await ApplyAutoMappings(instanceId);

            var updatedMappings = GetDataSetVersionMapping(nextVersion);

            Dictionary<string, FilterMapping> expectedFilterMappings = new()
            {
                {
                    "filter-1-key", mappings.GetFilterMapping("filter-1-key") with
                    {
                        // The code managed to establish an automapping for this filter.
                        Type = MappingType.AutoMapped,
                        CandidateKey = "filter-1-key",
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            {
                                "filter-1-option-1-key",
                                mappings.GetFilterOptionMapping("filter-1-key", "filter-1-option-1-key") with
                                {
                                    // The code managed to establish an automapping for this filter option.
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "filter-1-option-1-key"
                                }
                            },
                            {
                                "filter-1-option-2-key",
                                mappings.GetFilterOptionMapping("filter-1-key", "filter-1-option-2-key") with
                                {
                                    // The code managed to establish an automapping for this filter option.
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "filter-1-option-2-key"
                                }
                            }
                        }
                    }
                },
                {
                    "filter-2-key", mappings.GetFilterMapping("filter-2-key") with
                    {
                        // The code managed to establish an automapping for this filter.
                        Type = MappingType.AutoMapped,
                        CandidateKey = "filter-2-key",
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            {
                                "filter-2-option-1-key",
                                mappings.GetFilterOptionMapping("filter-2-key", "filter-2-option-1-key") with
                                {
                                    // The code managed to establish an automapping for this filter option.
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "filter-2-option-1-key"
                                }
                            }
                        }
                    }
                }
            };

            updatedMappings.FilterMappingPlan.Mappings.AssertDeepEqualTo(expectedFilterMappings);

            Assert.True(updatedMappings.FilterMappingsComplete);

            // All source filter options have equivalent candidates to be mapped to, thus
            // resulting in a minor version update.
            Assert.Equal("1.1.0", updatedMappings.TargetDataSetVersion.SemVersion());

            var updatedReleaseFile = await GetDbContext<ContentDbContext>()
                .ReleaseFiles
                .SingleAsync(rf => rf.PublicApiDataSetId == updatedMappings.TargetDataSetVersion.DataSetId);

            Assert.Equal(updatedMappings.TargetDataSetVersion.SemVersion(), updatedReleaseFile.PublicApiDataSetVersion);
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
            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithFilterMappingPlan(DataFixture
                    .DefaultFilterMappingPlan()
                    .AddFilterMapping("filter-1-key", DataFixture
                        .DefaultFilterMapping()
                        .WithNoMapping()
                        .AddOptionMapping("filter-1-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithNoMapping()))
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

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mappings));

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextVersion.Release.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile())
                .WithPublicApiDataSetId(nextVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextVersion.SemVersion());

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            await ApplyAutoMappings(instanceId);

            var updatedMappings = GetDataSetVersionMapping(nextVersion);

            Dictionary<string, FilterMapping> expectedFilterMappings = new()
            {
                {
                    "filter-1-key", mappings.GetFilterMapping("filter-1-key") with
                    {
                        Type = MappingType.AutoMapped,
                        CandidateKey = "filter-1-key",
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            {
                                "filter-1-option-1-key",
                                mappings.GetFilterOptionMapping("filter-1-key", "filter-1-option-1-key") with
                                {
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "filter-1-option-1-key"
                                }
                            }
                        }
                    }
                }
            };

            updatedMappings.FilterMappingPlan.Mappings.AssertDeepEqualTo(
                expectedFilterMappings,
                ignoreCollectionOrders: true);

            Assert.True(updatedMappings.FilterMappingsComplete);

            // All source filter options have equivalent candidates to be mapped to, thus
            // resulting in a minor version update. The inclusion of new filter options
            // not present in the original version does not matter.
            Assert.Equal("1.1.0", updatedMappings.TargetDataSetVersion.SemVersion());

            var updatedReleaseFile = await GetDbContext<ContentDbContext>()
                .ReleaseFiles
                .SingleAsync(rf => rf.PublicApiDataSetId == updatedMappings.TargetDataSetVersion.DataSetId);

            Assert.Equal(updatedMappings.TargetDataSetVersion.SemVersion(), updatedReleaseFile.PublicApiDataSetVersion);
        }

        // As there is currently no way in the UI for a user to resolve unmapped filters, filters
        // and their child filter options with mapping type of AutoNone should not count towards
        // the calculation of the FilterMappingsComplete flag.
        [Fact]
        public async Task Complete_SomeFiltersAutoNone()
        {
            var (instanceId, originalVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            // Create a mapping plan based on 2 data set versions with the same filters
            // and filter options, but additional options exist in the new version.
            // Each source filter and filter option can be auto-mapped exactly to one in
            // the target version, leaving some candidates unused but essentially the mapping
            // is complete unless the user manually intervenes at this point.
            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithFilterMappingPlan(DataFixture
                    .DefaultFilterMappingPlan()
                    .AddFilterMapping("filter-1-key", DataFixture
                        .DefaultFilterMapping()
                        .WithNoMapping()
                        .AddOptionMapping("filter-1-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithNoMapping()))
                    .AddFilterMapping("filter-2-key", DataFixture
                        .DefaultFilterMapping()
                        .WithNoMapping()
                        .AddOptionMapping("filter-2-option-1-key", DataFixture
                            .DefaultFilterOptionMapping()
                            .WithNoMapping()))
                    .AddFilterCandidate("filter-1-key", DataFixture
                        .DefaultFilterMappingCandidate()
                        .AddOptionCandidate("filter-1-option-1-key", DataFixture
                            .DefaultMappableFilterOption())));

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mappings));

            ReleaseFile releaseFile = DataFixture.DefaultReleaseFile()
                .WithId(nextVersion.Release.ReleaseFileId)
                .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
                .WithFile(DataFixture.DefaultFile())
                .WithPublicApiDataSetId(nextVersion.DataSetId)
                .WithPublicApiDataSetVersion(nextVersion.SemVersion());

            await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

            await ApplyAutoMappings(instanceId);

            var updatedMappings = GetDataSetVersionMapping(nextVersion);

            Dictionary<string, FilterMapping> expectedFilterMappings = new()
            {
                {
                    "filter-1-key", mappings.GetFilterMapping("filter-1-key") with
                    {
                        Type = MappingType.AutoMapped,
                        CandidateKey = "filter-1-key",
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            {
                                "filter-1-option-1-key",
                                mappings.GetFilterOptionMapping("filter-1-key", "filter-1-option-1-key") with
                                {
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "filter-1-option-1-key"
                                }
                            }
                        }
                    }
                },
                {
                    "filter-2-key", mappings.GetFilterMapping("filter-2-key") with
                    {
                        Type = MappingType.AutoNone,
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            {
                                "filter-2-option-1-key",
                                mappings.GetFilterOptionMapping("filter-2-key", "filter-2-option-1-key") with
                                {
                                    Type = MappingType.AutoNone
                                }
                            }
                        }
                    }
                }
            };

            updatedMappings.FilterMappingPlan.Mappings.AssertDeepEqualTo(
                expectedFilterMappings,
                ignoreCollectionOrders: true);

            Assert.True(updatedMappings.FilterMappingsComplete);

            // Some source filter options have no equivalent candidate to be mapped to, thus
            // resulting in a major version update.
            Assert.Equal("2.0.0", updatedMappings.TargetDataSetVersion.SemVersion());

            var updatedReleaseFile = await GetDbContext<ContentDbContext>()
                .ReleaseFiles
                .SingleAsync(rf => rf.PublicApiDataSetId == updatedMappings.TargetDataSetVersion.DataSetId);

            Assert.Equal(updatedMappings.TargetDataSetVersion.SemVersion(), updatedReleaseFile.PublicApiDataSetVersion);
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

        var (nextDataSetVersion, instanceId) = await CreateDataSetVersionAndImport(
            dataSetId: initialDataSetVersion.DataSetId,
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
            .Include(mapping => mapping.TargetDataSetVersion)
            .Single(mapping => mapping.TargetDataSetVersionId == nextVersion.Id);
    }
}
