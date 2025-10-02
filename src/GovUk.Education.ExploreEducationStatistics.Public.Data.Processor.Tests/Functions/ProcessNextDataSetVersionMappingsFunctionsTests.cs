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
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class ProcessNextDataSetVersionMappingsFunctionsTests(ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public abstract class CreateMappingsTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessNextDataSetVersionMappingsFunctionsTests(fixture)
    {
        protected const DataSetVersionImportStage Stage = DataSetVersionImportStage.CreatingMappings;

        protected async Task CreateMappings(Guid instanceId)
        {
            var function = GetRequiredService<ProcessNextDataSetVersionMappingsFunctions>();
            await function.CreateMappings(instanceId, CancellationToken.None);
        }
    }

    public class CreateMappingMiscTests(ProcessorFunctionsIntegrationTestFixture fixture) : CreateMappingsTests(fixture)
    {
        [Fact]
        public async Task Success_ImportStatus()
        {
            var (instanceId, _, _) = await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            await CreateMappings(instanceId);

            var savedImport = await GetDbContext<PublicDataDbContext>()
                .DataSetVersionImports.Include(dataSetVersionImport => dataSetVersionImport.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            Assert.Equal(DataSetVersionStatus.Processing, savedImport.DataSetVersion.Status);
        }

        [Fact]
        public async Task Success_MetaSummary()
        {
            var (instanceId, _, nextVersion) = await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            await CreateMappings(instanceId);

            var updatedDataSetVersion = await GetDataSetVersion(nextVersion);

            // Assert that the MetaSummary has been generated correctly from the CSV.
            var metaSummary = updatedDataSetVersion.MetaSummary;
            Assert.NotNull(metaSummary);

            Assert.Equal(
                ProcessorTestData.AbsenceSchool.ExpectedLocations.Select(level => level.Level).Order(),
                metaSummary.GeographicLevels.Order()
            );

            Assert.Equal(
                ProcessorTestData
                    .AbsenceSchool.ExpectedFilters.Select(filterAndOptions => filterAndOptions.Label)
                    .Order(),
                metaSummary.Filters.Order()
            );

            Assert.Equal(
                ProcessorTestData.AbsenceSchool.ExpectedIndicators.Select(l => l.Label).Order(),
                metaSummary.Indicators.Order()
            );

            Assert.Equal(
                new TimePeriodRange
                {
                    Start = TimePeriodRangeBound.Create(ProcessorTestData.AbsenceSchool.ExpectedTimePeriods[0]),
                    End = TimePeriodRangeBound.Create(ProcessorTestData.AbsenceSchool.ExpectedTimePeriods[^1]),
                },
                metaSummary.TimePeriodRange
            );
        }

        [Fact]
        public async Task Success_HasDeletedIndicators_True()
        {
            var initialVersionMeta = new DataSetVersionMeta
            {
                IndicatorMetas = DataFixture.DefaultIndicatorMeta().GenerateList(2),
                GeographicLevelMeta = DataFixture
                    .DefaultGeographicLevelMeta()
                    .WithLevels(ProcessorTestData.AbsenceSchool.ExpectedGeographicLevels),
            };

            var (instanceId, _, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage(),
                initialVersionMeta
            );

            await CreateMappings(instanceId);

            var mapping = await GetDataSetVersionMapping(nextVersion);

            Assert.True(mapping.HasDeletedIndicators);
        }

        [Fact]
        public async Task Success_HasDeletedIndicators_SameIndicators_False()
        {
            var initialVersionMeta = new DataSetVersionMeta
            {
                IndicatorMetas = ProcessorTestData.AbsenceSchool.ExpectedIndicators,
                GeographicLevelMeta = DataFixture
                    .DefaultGeographicLevelMeta()
                    .WithLevels(ProcessorTestData.AbsenceSchool.ExpectedGeographicLevels),
            };

            var (instanceId, _, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage(),
                initialVersionMeta
            );

            await CreateMappings(instanceId);

            var mapping = await GetDataSetVersionMapping(nextVersion);

            Assert.False(mapping.HasDeletedIndicators);
        }

        [Fact]
        public async Task Success_HasDeletedIndicators_NewIndicators_False()
        {
            var initialVersionMeta = new DataSetVersionMeta
            {
                IndicatorMetas = ProcessorTestData.AbsenceSchool.ExpectedIndicators[..2],
                GeographicLevelMeta = DataFixture
                    .DefaultGeographicLevelMeta()
                    .WithLevels(ProcessorTestData.AbsenceSchool.ExpectedGeographicLevels),
            };

            var (instanceId, _, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage(),
                initialVersionMeta
            );

            await CreateMappings(instanceId);

            var mapping = await GetDataSetVersionMapping(nextVersion);

            Assert.False(mapping.HasDeletedIndicators);
        }

        [Fact]
        public async Task Success_HasDeletedGeographicLevels_True()
        {
            var initialVersionMeta = new DataSetVersionMeta
            {
                GeographicLevelMeta = DataFixture
                    .DefaultGeographicLevelMeta()
                    .WithLevels(
                        [
                            GeographicLevel.Country,
                            GeographicLevel.Region,
                            GeographicLevel.LocalAuthority,
                            // Replaced by school in next version
                            GeographicLevel.Institution,
                        ]
                    ),
            };

            var (instanceId, _, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage(),
                initialVersionMeta
            );

            await CreateMappings(instanceId);

            var mapping = await GetDataSetVersionMapping(nextVersion);

            Assert.True(mapping.HasDeletedGeographicLevels);
        }

        [Fact]
        public async Task Success_HasDeletedGeographicLevels_SameGeographicLevels_False()
        {
            var initialVersionMeta = new DataSetVersionMeta
            {
                GeographicLevelMeta = DataFixture
                    .DefaultGeographicLevelMeta()
                    .WithLevels(ProcessorTestData.AbsenceSchool.ExpectedGeographicLevels),
            };

            var (instanceId, _, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage(),
                initialVersionMeta
            );

            await CreateMappings(instanceId);

            var mapping = await GetDataSetVersionMapping(nextVersion);

            Assert.False(mapping.HasDeletedGeographicLevels);
        }

        [Fact]
        public async Task Success_HasDeletedGeographicLevels_NewGeographicLevels_False()
        {
            var initialVersionMeta = new DataSetVersionMeta
            {
                GeographicLevelMeta = DataFixture
                    .DefaultGeographicLevelMeta()
                    .WithLevels(ProcessorTestData.AbsenceSchool.ExpectedGeographicLevels[..2]),
            };

            var (instanceId, _, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage(),
                initialVersionMeta
            );

            await CreateMappings(instanceId);

            var mapping = await GetDataSetVersionMapping(nextVersion);

            Assert.False(mapping.HasDeletedGeographicLevels);
        }

        [Fact]
        public async Task Success_HasDeletedTimePeriods_True()
        {
            var initialVersionMeta = new DataSetVersionMeta
            {
                GeographicLevelMeta = DataFixture
                    .DefaultGeographicLevelMeta()
                    .WithLevels(ProcessorTestData.AbsenceSchool.ExpectedGeographicLevels),
                TimePeriodMetas = DataFixture.DefaultTimePeriodMeta().GenerateList(2),
            };

            var (instanceId, _, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage(),
                initialVersionMeta
            );

            await CreateMappings(instanceId);

            var mapping = await GetDataSetVersionMapping(nextVersion);

            Assert.True(mapping.HasDeletedTimePeriods);
        }

        [Fact]
        public async Task Success_HasDeletedTimePeriods_SameTimePeriods_False()
        {
            var initialVersionMeta = new DataSetVersionMeta
            {
                GeographicLevelMeta = DataFixture
                    .DefaultGeographicLevelMeta()
                    .WithLevels(ProcessorTestData.AbsenceSchool.ExpectedGeographicLevels),
                TimePeriodMetas = ProcessorTestData.AbsenceSchool.ExpectedTimePeriods,
            };

            var (instanceId, _, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage(),
                initialVersionMeta
            );

            await CreateMappings(instanceId);

            var mapping = await GetDataSetVersionMapping(nextVersion);

            Assert.False(mapping.HasDeletedTimePeriods);
        }

        [Fact]
        public async Task Success_HasDeletedTimePeriods_NewTimePeriods_False()
        {
            var initialVersionMeta = new DataSetVersionMeta
            {
                GeographicLevelMeta = DataFixture
                    .DefaultGeographicLevelMeta()
                    .WithLevels(ProcessorTestData.AbsenceSchool.ExpectedGeographicLevels),
                TimePeriodMetas = ProcessorTestData.AbsenceSchool.ExpectedTimePeriods[..2],
            };

            var (instanceId, _, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage(),
                initialVersionMeta
            );

            await CreateMappings(instanceId);

            var mapping = await GetDataSetVersionMapping(nextVersion);

            Assert.False(mapping.HasDeletedTimePeriods);
        }
    }

    public class CreateMappingsLocationsTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : CreateMappingsTests(fixture)
    {
        [Fact]
        public async Task Success_Mappings()
        {
            var (instanceId, initialVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            // Select a set of LocationOptions that rely on all the available Location
            // Code fields.
            var initialLocationMeta = DataFixture
                .DefaultLocationMeta()
                .WithDataSetVersionId(initialVersion.Id)
                .ForIndex(
                    0,
                    s =>
                        s.SetLevel(GeographicLevel.LocalAuthority)
                            .SetOptions(
                                DataFixture
                                    .DefaultLocationLocalAuthorityOptionMeta()
                                    .GenerateList(2)
                                    .Select(meta => meta as LocationOptionMeta)
                                    .ToList()
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetLevel(GeographicLevel.School)
                            .SetOptions(
                                DataFixture
                                    .DefaultLocationSchoolOptionMeta()
                                    .GenerateList(2)
                                    .Select(meta => meta as LocationOptionMeta)
                                    .ToList()
                            )
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetLevel(GeographicLevel.Provider)
                            .SetOptions(
                                DataFixture
                                    .DefaultLocationProviderOptionMeta()
                                    .GenerateList(2)
                                    .Select(meta => meta as LocationOptionMeta)
                                    .ToList()
                            )
                )
                .GenerateList();

            await AddTestData<PublicDataDbContext>(context => context.LocationMetas.AddRange(initialLocationMeta));

            await CreateMappings(instanceId);

            var mapping = await GetDataSetVersionMapping(nextVersion);

            Assert.Equal(initialVersion.Id, mapping.SourceDataSetVersionId);
            Assert.Equal(nextVersion.Id, mapping.TargetDataSetVersionId);
            Assert.False(mapping.LocationMappingsComplete);

            var expectedLocationMappingsFromSource = initialLocationMeta.ToDictionary(
                levelMeta => levelMeta.Level,
                levelMeta => new LocationLevelMappings
                {
                    Mappings = levelMeta.OptionLinks.ToDictionary(
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
                                Ukprn = link.Option.ToRow().Ukprn,
                            },
                        }
                    ),
                }
            );

            // There should be 5 levels of mappings when combining all the source and target levels.
            Assert.Equal(
                ProcessorTestData.AbsenceSchool.ExpectedGeographicLevels.Concat([GeographicLevel.Provider]).Order(),
                mapping.LocationMappingPlan.Levels.Select(level => level.Key).Order()
            );

            mapping.LocationMappingPlan.Levels.ForEach(level =>
            {
                var matchingLevelFromSource = expectedLocationMappingsFromSource.GetValueOrDefault(level.Key);

                if (matchingLevelFromSource != null)
                {
                    level.Value.Mappings.AssertDeepEqualTo(
                        matchingLevelFromSource.Mappings,
                        ignoreCollectionOrders: true
                    );
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
            var (instanceId, initialVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            await CreateMappings(instanceId);

            var mapping = await GetDataSetVersionMapping(nextVersion);

            Assert.Equal(initialVersion.Id, mapping.SourceDataSetVersionId);
            Assert.Equal(nextVersion.Id, mapping.TargetDataSetVersionId);

            var expectedLocationLevels = ProcessorTestData.AbsenceSchool.ExpectedLocations.ToDictionary(
                keySelector: levelMeta => levelMeta.Level,
                elementSelector: levelMeta => new LocationLevelMappings
                {
                    Candidates = levelMeta.Options.ToDictionary(
                        keySelector: MappingKeyGenerators.LocationOptionMeta,
                        elementSelector: option => new MappableLocationOption
                        {
                            Label = option.Label,
                            Code = option.ToRow().Code,
                            OldCode = option.ToRow().OldCode,
                            Urn = option.ToRow().Urn,
                            LaEstab = option.ToRow().LaEstab,
                            Ukprn = option.ToRow().Ukprn,
                        }
                    ),
                }
            );

            mapping.LocationMappingPlan.Levels.AssertDeepEqualTo(expectedLocationLevels, ignoreCollectionOrders: true);
        }
    }

    public class CreateMappingsFiltersTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : CreateMappingsTests(fixture)
    {
        [Fact]
        public async Task Success_Mappings()
        {
            var (instanceId, initialVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            var initialFilterMeta = DataFixture
                .DefaultFilterMeta()
                .WithDataSetVersionId(initialVersion.Id)
                .WithOptions(() => DataFixture.DefaultFilterOptionMeta().GenerateList(2))
                .GenerateList(2);

            await AddTestData<PublicDataDbContext>(context => context.FilterMetas.AddRange(initialFilterMeta));

            await CreateMappings(instanceId);

            var mapping = await GetDataSetVersionMapping(nextVersion);

            Assert.Equal(initialVersion.Id, mapping.SourceDataSetVersionId);
            Assert.Equal(nextVersion.Id, mapping.TargetDataSetVersionId);
            Assert.False(mapping.FilterMappingsComplete);

            var expectedFilterMappings = initialFilterMeta.ToDictionary(
                keySelector: MappingKeyGenerators.Filter,
                elementSelector: filter => new FilterMapping
                {
                    CandidateKey = null,
                    PublicId = filter.PublicId,
                    Source = new MappableFilter { Label = filter.Label },
                    OptionMappings = filter.OptionLinks.ToDictionary(
                        keySelector: MappingKeyGenerators.FilterOptionMetaLink,
                        elementSelector: link => new FilterOptionMapping
                        {
                            CandidateKey = null,
                            PublicId = link.PublicId,
                            Source = new MappableFilterOption { Label = link.Option.Label },
                        }
                    ),
                }
            );

            mapping.FilterMappingPlan.Mappings.AssertDeepEqualTo(expectedFilterMappings, ignoreCollectionOrders: true);
        }

        [Fact]
        public async Task Success_Candidates()
        {
            var (instanceId, initialVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            await CreateMappings(instanceId);

            var mapping = await GetDataSetVersionMapping(nextVersion);

            Assert.Equal(initialVersion.Id, mapping.SourceDataSetVersionId);
            Assert.Equal(nextVersion.Id, mapping.TargetDataSetVersionId);

            var expectedFilterTargets = ProcessorTestData.AbsenceSchool.ExpectedFilters.ToDictionary(
                keySelector: MappingKeyGenerators.Filter,
                elementSelector: filter => new FilterMappingCandidate
                {
                    Label = filter.Label,
                    Options = filter.Options.ToDictionary(
                        keySelector: MappingKeyGenerators.FilterOptionMeta,
                        elementSelector: optionMeta => new MappableFilterOption { Label = optionMeta.Label }
                    ),
                }
            );

            mapping.FilterMappingPlan.Candidates.AssertDeepEqualTo(expectedFilterTargets, ignoreCollectionOrders: true);
        }
    }

    public abstract class ApplyAutoMappingsTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessNextDataSetVersionMappingsFunctionsTests(fixture)
    {
        protected const DataSetVersionImportStage Stage = DataSetVersionImportStage.AutoMapping;

        protected async Task ApplyAutoMappings(Guid instanceId)
        {
            var function = GetRequiredService<ProcessNextDataSetVersionMappingsFunctions>();
            await function.ApplyAutoMappings(instanceId, CancellationToken.None);
        }
    }

    public class ApplyAutoMappingsMiscTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ApplyAutoMappingsTests(fixture)
    {
        [Fact]
        public async Task Success_ImportStatus()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            await AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersionMappings.Add(
                    new DataSetVersionMapping
                    {
                        SourceDataSetVersionId = originalVersion.Id,
                        TargetDataSetVersionId = nextVersion.Id,
                        LocationMappingPlan = new LocationMappingPlan(),
                        FilterMappingPlan = new FilterMappingPlan(),
                    }
                )
            );

            await ApplyAutoMappings(instanceId);

            var savedImport = await GetDbContext<PublicDataDbContext>()
                .DataSetVersionImports.Include(dataSetVersionImport => dataSetVersionImport.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            Assert.Equal(DataSetVersionStatus.Processing, savedImport.DataSetVersion.Status);
        }

        [Fact]
        public async Task Success_HasDeletedIndicators_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithHasDeletedIndicators(true);

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0");
        }

        [Fact]
        public async Task Success_HasDeletedGeographicLevels_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithHasDeletedGeographicLevels(true);

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0");
        }

        [Fact]
        public async Task Success_HasDeletedTimePeriods_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithHasDeletedTimePeriods(true);

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0");
        }
    }

    public class ApplyAutoMappingsLocationsTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ApplyAutoMappingsTests(fixture)
    {
        [Fact]
        public async Task Incomplete_UnmappedOptionWithNoCandidate_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithLocationMappingPlan(
                    DataFixture
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
                                        .WithNoMapping()
                                )
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
                                    candidate: DataFixture.DefaultMappableLocationOption()
                                )
                        )
                );

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            var laMapping1 = mapping.GetLocationOptionMapping(GeographicLevel.LocalAuthority, "la-location-1-key");

            var laMapping2 = mapping.GetLocationOptionMapping(GeographicLevel.LocalAuthority, "la-location-2-key");

            Dictionary<GeographicLevel, LocationLevelMappings> expectedLevelMappings = new()
            {
                {
                    GeographicLevel.LocalAuthority,
                    mapping.GetLocationLevelMappings(GeographicLevel.LocalAuthority) with
                    {
                        Mappings = new Dictionary<string, LocationOptionMapping>
                        {
                            {
                                "la-location-1-key",
                                laMapping1 with
                                {
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "la-location-1-key",
                                }
                            },
                            {
                                "la-location-2-key",
                                laMapping2 with
                                {
                                    Type = MappingType.AutoNone,
                                    CandidateKey = null,
                                }
                            },
                        },
                    }
                },
            };

            updatedMapping.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedLevelMappings,
                ignoreCollectionOrders: true
            );

            Assert.False(updatedMapping.LocationMappingsComplete);

            // Some options have no auto-mapped candidate - major version update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0");
        }

        [Fact]
        public async Task Incomplete_UnmappedOptionWithNewCandidate_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithLocationMappingPlan(
                    DataFixture
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
                                        .WithNoMapping()
                                )
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
                                    candidate: DataFixture.DefaultMappableLocationOption()
                                )
                                // 'LA Location 3' option is new candidate that can be mapped.
                                .AddCandidate(
                                    targetKey: "la-location-3-key",
                                    candidate: DataFixture.DefaultMappableLocationOption()
                                )
                        )
                );

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            var laMapping1 = mapping.GetLocationOptionMapping(GeographicLevel.LocalAuthority, "la-location-1-key");

            var laMapping2 = mapping.GetLocationOptionMapping(GeographicLevel.LocalAuthority, "la-location-2-key");

            Dictionary<GeographicLevel, LocationLevelMappings> expectedLevelMappings = new()
            {
                {
                    GeographicLevel.LocalAuthority,
                    mapping.GetLocationLevelMappings(GeographicLevel.LocalAuthority) with
                    {
                        Mappings = new Dictionary<string, LocationOptionMapping>
                        {
                            {
                                "la-location-1-key",
                                laMapping1 with
                                {
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "la-location-1-key",
                                }
                            },
                            {
                                "la-location-2-key",
                                laMapping2 with
                                {
                                    Type = MappingType.AutoNone,
                                    CandidateKey = null,
                                }
                            },
                        },
                    }
                },
            };

            updatedMapping.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedLevelMappings,
                ignoreCollectionOrders: true
            );

            Assert.False(updatedMapping.LocationMappingsComplete);

            // Some options have no auto-mapped candidate - major version update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0");
        }

        [Fact]
        public async Task Complete_DeletedLevel_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithLocationMappingPlan(
                    DataFixture
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
                                        .WithManualNone()
                                )
                        )
                );

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            var rscMapping = mapping.GetLocationOptionMapping(GeographicLevel.RscRegion, "rsc-location-1-key");

            Dictionary<GeographicLevel, LocationLevelMappings> expectedLevelMappings = new()
            {
                {
                    GeographicLevel.RscRegion,
                    mapping.GetLocationLevelMappings(GeographicLevel.RscRegion) with
                    {
                        Mappings = new Dictionary<string, LocationOptionMapping>
                        {
                            {
                                "rsc-location-1-key",
                                rscMapping with
                                {
                                    Type = MappingType.AutoNone,
                                    CandidateKey = null,
                                }
                            },
                        },
                    }
                },
            };

            updatedMapping.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedLevelMappings,
                ignoreCollectionOrders: true
            );

            Assert.True(updatedMapping.LocationMappingsComplete);

            // Level has been deleted (cannot be mapped) - major version update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0");
        }

        [Fact]
        public async Task Complete_NewLevel_MinorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithLocationMappingPlan(
                    DataFixture
                        .DefaultLocationMappingPlan()
                        .AddLevel(
                            level: GeographicLevel.Country,
                            mappings: DataFixture
                                .DefaultLocationLevelMappings()
                                .AddCandidate(
                                    targetKey: "country-location-1-key",
                                    candidate: DataFixture.DefaultMappableLocationOption()
                                )
                        )
                );

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            Dictionary<GeographicLevel, LocationLevelMappings> expectedLevelMappings = new()
            {
                { GeographicLevel.Country, mapping.GetLocationLevelMappings(GeographicLevel.Country) },
            };

            updatedMapping.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedLevelMappings,
                ignoreCollectionOrders: true
            );

            Assert.True(updatedMapping.LocationMappingsComplete);

            // Level has been added - minor version update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "1.1.0");
        }

        [Fact]
        public async Task Complete_ExactMatch_MinorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithLocationMappingPlan(
                    DataFixture
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
                                        .WithNoMapping()
                                )
                                .AddCandidate(
                                    targetKey: "location-1-key",
                                    candidate: DataFixture.DefaultMappableLocationOption()
                                )
                        )
                );

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            var originalLocationMapping = mapping.GetLocationOptionMapping(
                GeographicLevel.LocalAuthority,
                "location-1-key"
            );

            Dictionary<GeographicLevel, LocationLevelMappings> expectedLevelMappings = new()
            {
                {
                    GeographicLevel.LocalAuthority,
                    mapping.GetLocationLevelMappings(GeographicLevel.LocalAuthority) with
                    {
                        Mappings = new Dictionary<string, LocationOptionMapping>
                        {
                            {
                                "location-1-key",
                                originalLocationMapping with
                                {
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "location-1-key",
                                }
                            },
                        },
                    }
                },
            };

            updatedMapping.LocationMappingPlan.Levels.AssertDeepEqualTo(expectedLevelMappings);

            Assert.True(updatedMapping.LocationMappingsComplete);

            // All source options have auto-mapped candidates - minor version update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "1.1.0");
        }

        [Fact]
        public async Task Complete_AutoMappedAndNewOptions_MinorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithLocationMappingPlan(
                    DataFixture
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
                                        .WithNoMapping()
                                )
                                .AddCandidate(
                                    targetKey: "la-location-1-key",
                                    candidate: DataFixture.DefaultMappableLocationOption()
                                )
                                // 'LA Location 2' option is newly added.
                                .AddCandidate(
                                    targetKey: "la-location-2-key",
                                    candidate: DataFixture.DefaultMappableLocationOption()
                                )
                        )
                        .AddLevel(
                            level: GeographicLevel.RscRegion,
                            mappings: DataFixture
                                .DefaultLocationLevelMappings()
                                // 'RSC Location 1' is newly added.
                                .AddCandidate(
                                    targetKey: "rsc-location-1-key",
                                    candidate: DataFixture.DefaultMappableLocationOption()
                                )
                        )
                );

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            var originalLaMapping = mapping.GetLocationOptionMapping(
                GeographicLevel.LocalAuthority,
                "la-location-1-key"
            );

            Dictionary<GeographicLevel, LocationLevelMappings> expectedLevelMappings = new()
            {
                {
                    GeographicLevel.LocalAuthority,
                    mapping.GetLocationLevelMappings(GeographicLevel.LocalAuthority) with
                    {
                        Mappings = new Dictionary<string, LocationOptionMapping>
                        {
                            {
                                "la-location-1-key",
                                originalLaMapping with
                                {
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "la-location-1-key",
                                }
                            },
                        },
                    }
                },
                { GeographicLevel.RscRegion, mapping.GetLocationLevelMappings(GeographicLevel.RscRegion) },
            };

            updatedMapping.LocationMappingPlan.Levels.AssertDeepEqualTo(
                expectedLevelMappings,
                ignoreCollectionOrders: true
            );

            Assert.True(updatedMapping.LocationMappingsComplete);

            // All source options have auto-mapped candidates - minor version update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "1.1.0");
        }

        [Fact]
        public async Task Complete_HasDeletedIndicators_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithLocationMappingPlan(
                    DataFixture
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
                                        .WithNoMapping()
                                )
                                .AddCandidate(
                                    targetKey: "location-1-key",
                                    candidate: DataFixture.DefaultMappableLocationOption()
                                )
                        )
                )
                // Has deleted indicators that cannot be mapped
                .WithHasDeletedIndicators(true);

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            // Is an exact match but as there are deleted indicators so needs to be a major update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0");
        }

        [Fact]
        public async Task Complete_HasDeletedGeographicLevels_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithLocationMappingPlan(
                    DataFixture
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
                                        .WithNoMapping()
                                )
                                .AddCandidate(
                                    targetKey: "location-1-key",
                                    candidate: DataFixture.DefaultMappableLocationOption()
                                )
                        )
                )
                // Has deleted geographic levels that cannot be mapped
                .WithHasDeletedGeographicLevels(true);

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            // Is an exact match but as there are deleted geographic levels so needs to be a major update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0");
        }

        [Fact]
        public async Task Complete_HasDeletedTimePeriods_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithLocationMappingPlan(
                    DataFixture
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
                                        .WithNoMapping()
                                )
                                .AddCandidate(
                                    targetKey: "location-1-key",
                                    candidate: DataFixture.DefaultMappableLocationOption()
                                )
                        )
                )
                // Has deleted time periods that cannot be mapped
                .WithHasDeletedTimePeriods(true);

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            // Is an exact match but as there are deleted time periods so needs to be a major update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0");
        }
    }

    public class ApplyAutoMappingsFiltersTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ApplyAutoMappingsTests(fixture)
    {
        [Fact]
        public async Task PartiallyComplete_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            // Create a mapping plan based on 2 data set versions with partially overlapping filters.
            // Both have "Filter 1" and both have "Filter 1 option 1", but then each also contains Filter 1
            // options that the other do not, and each also contains filters that the other does not.
            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture
                        .DefaultFilterMappingPlan()
                        .AddFilterMapping(
                            "filter-1-key",
                            DataFixture
                                .DefaultFilterMapping()
                                .WithNoMapping()
                                .AddOptionMapping(
                                    "filter-1-option-1-key",
                                    DataFixture.DefaultFilterOptionMapping().WithNoMapping()
                                )
                                .AddOptionMapping(
                                    "filter-1-option-2-key",
                                    DataFixture.DefaultFilterOptionMapping().WithNoMapping()
                                )
                        )
                        .AddFilterMapping(
                            "filter-2-key",
                            DataFixture
                                .DefaultFilterMapping()
                                .WithNoMapping()
                                .AddOptionMapping(
                                    "filter-2-option-1-key",
                                    DataFixture.DefaultFilterOptionMapping().WithNoMapping()
                                )
                        )
                        .AddFilterCandidate(
                            "filter-1-key",
                            DataFixture
                                .DefaultFilterMappingCandidate()
                                .AddOptionCandidate("filter-1-option-1-key", DataFixture.DefaultMappableFilterOption())
                                .AddOptionCandidate("filter-1-option-3-key", DataFixture.DefaultMappableFilterOption())
                        )
                );

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            Dictionary<string, FilterMapping> expectedFilterMappings = new()
            {
                {
                    "filter-1-key",
                    mapping.GetFilterMapping("filter-1-key") with
                    {
                        // The code managed to establish an automapping for this filter.
                        Type = MappingType.AutoMapped,
                        CandidateKey = "filter-1-key",
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            {
                                "filter-1-option-1-key",
                                mapping.GetFilterOptionMapping("filter-1-key", "filter-1-option-1-key") with
                                {
                                    // The code managed to establish an automapping for this filter option.
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "filter-1-option-1-key",
                                }
                            },
                            {
                                "filter-1-option-2-key",
                                mapping.GetFilterOptionMapping("filter-1-key", "filter-1-option-2-key") with
                                {
                                    // The code managed to establish that no obvious automapping candidate exists for
                                    // this filter option.
                                    Type = MappingType.AutoNone,
                                    CandidateKey = null,
                                }
                            },
                        },
                    }
                },
                {
                    "filter-2-key",
                    mapping.GetFilterMapping("filter-2-key") with
                    {
                        // The code managed to establish that no obvious automapping candidate exists for
                        // this filter.
                        Type = MappingType.AutoNone,
                        CandidateKey = null,
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            {
                                "filter-2-option-1-key",
                                mapping.GetFilterOptionMapping("filter-2-key", "filter-2-option-1-key") with
                                {
                                    // The code managed to establish that no obvious automapping candidate exists for
                                    // this filter option.
                                    Type = MappingType.AutoNone,
                                    CandidateKey = null,
                                }
                            },
                        },
                    }
                },
            };

            updatedMapping.FilterMappingPlan.Mappings.AssertDeepEqualTo(
                expectedFilterMappings,
                ignoreCollectionOrders: true
            );

            Assert.False(updatedMapping.FilterMappingsComplete);

            // Some source filter options have no equivalent candidate to be mapped to, thus
            // resulting in a major version update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0");
        }

        [Fact]
        public async Task Complete_ExactMatch_MinorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            // Create a mapping plan based on 2 data set versions with exactly the same filters
            // and filter options.
            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture
                        .DefaultFilterMappingPlan()
                        .AddFilterMapping(
                            "filter-1-key",
                            DataFixture
                                .DefaultFilterMapping()
                                .WithNoMapping()
                                .AddOptionMapping(
                                    "filter-1-option-1-key",
                                    DataFixture.DefaultFilterOptionMapping().WithNoMapping()
                                )
                                .AddOptionMapping(
                                    "filter-1-option-2-key",
                                    DataFixture.DefaultFilterOptionMapping().WithNoMapping()
                                )
                        )
                        .AddFilterMapping(
                            "filter-2-key",
                            DataFixture
                                .DefaultFilterMapping()
                                .WithNoMapping()
                                .AddOptionMapping(
                                    "filter-2-option-1-key",
                                    DataFixture.DefaultFilterOptionMapping().WithNoMapping()
                                )
                        )
                        .AddFilterCandidate(
                            "filter-1-key",
                            DataFixture
                                .DefaultFilterMappingCandidate()
                                .AddOptionCandidate("filter-1-option-1-key", DataFixture.DefaultMappableFilterOption())
                                .AddOptionCandidate("filter-1-option-2-key", DataFixture.DefaultMappableFilterOption())
                        )
                        .AddFilterCandidate(
                            "filter-2-key",
                            DataFixture
                                .DefaultFilterMappingCandidate()
                                .AddOptionCandidate("filter-2-option-1-key", DataFixture.DefaultMappableFilterOption())
                        )
                );

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            Dictionary<string, FilterMapping> expectedFilterMappings = new()
            {
                {
                    "filter-1-key",
                    mapping.GetFilterMapping("filter-1-key") with
                    {
                        // The code managed to establish an automapping for this filter.
                        Type = MappingType.AutoMapped,
                        CandidateKey = "filter-1-key",
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            {
                                "filter-1-option-1-key",
                                mapping.GetFilterOptionMapping("filter-1-key", "filter-1-option-1-key") with
                                {
                                    // The code managed to establish an automapping for this filter option.
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "filter-1-option-1-key",
                                }
                            },
                            {
                                "filter-1-option-2-key",
                                mapping.GetFilterOptionMapping("filter-1-key", "filter-1-option-2-key") with
                                {
                                    // The code managed to establish an automapping for this filter option.
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "filter-1-option-2-key",
                                }
                            },
                        },
                    }
                },
                {
                    "filter-2-key",
                    mapping.GetFilterMapping("filter-2-key") with
                    {
                        // The code managed to establish an automapping for this filter.
                        Type = MappingType.AutoMapped,
                        CandidateKey = "filter-2-key",
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            {
                                "filter-2-option-1-key",
                                mapping.GetFilterOptionMapping("filter-2-key", "filter-2-option-1-key") with
                                {
                                    // The code managed to establish an automapping for this filter option.
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "filter-2-option-1-key",
                                }
                            },
                        },
                    }
                },
            };

            updatedMapping.FilterMappingPlan.Mappings.AssertDeepEqualTo(expectedFilterMappings);

            Assert.True(updatedMapping.FilterMappingsComplete);

            // All source filter options have equivalent candidates to be mapped to, thus
            // resulting in a minor version update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "1.1.0");
        }

        [Fact]
        public async Task Complete_AllSourcesMapped_NewCandidatesExist_MinorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            // Create a mapping plan based on 2 data set versions with the same filters
            // and filter options, but additional options exist in the new version.
            // Each source filter and filter option can be auto-mapped exactly to one in
            // the target version, leaving some candidates unused but essentially the mapping
            // is complete unless the user manually intervenes at this point.
            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture
                        .DefaultFilterMappingPlan()
                        .AddFilterMapping(
                            "filter-1-key",
                            DataFixture
                                .DefaultFilterMapping()
                                .WithNoMapping()
                                .AddOptionMapping(
                                    "filter-1-option-1-key",
                                    DataFixture.DefaultFilterOptionMapping().WithNoMapping()
                                )
                        )
                        .AddFilterCandidate(
                            "filter-1-key",
                            DataFixture
                                .DefaultFilterMappingCandidate()
                                .AddOptionCandidate("filter-1-option-1-key", DataFixture.DefaultMappableFilterOption())
                                .AddOptionCandidate("filter-1-option-2-key", DataFixture.DefaultMappableFilterOption())
                        )
                        .AddFilterCandidate(
                            "filter-2-key",
                            DataFixture
                                .DefaultFilterMappingCandidate()
                                .AddOptionCandidate("filter-2-option-1-key", DataFixture.DefaultMappableFilterOption())
                        )
                );

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            Dictionary<string, FilterMapping> expectedFilterMappings = new()
            {
                {
                    "filter-1-key",
                    mapping.GetFilterMapping("filter-1-key") with
                    {
                        Type = MappingType.AutoMapped,
                        CandidateKey = "filter-1-key",
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            {
                                "filter-1-option-1-key",
                                mapping.GetFilterOptionMapping("filter-1-key", "filter-1-option-1-key") with
                                {
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "filter-1-option-1-key",
                                }
                            },
                        },
                    }
                },
            };

            updatedMapping.FilterMappingPlan.Mappings.AssertDeepEqualTo(
                expectedFilterMappings,
                ignoreCollectionOrders: true
            );

            Assert.True(updatedMapping.FilterMappingsComplete);

            // All source filter options have equivalent candidates to be mapped to, thus
            // resulting in a minor version update. The inclusion of new filter options
            // not present in the original version does not matter.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "1.1.0");
        }

        // As there is currently no way in the UI for a user to resolve unmapped filters, filters
        // and their child filter options with mapping type of AutoNone should not count towards
        // the calculation of the FilterMappingsComplete flag.
        [Fact]
        public async Task Complete_SomeFiltersAutoNone_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            // Create a mapping plan based on 2 data set versions with the same filters
            // and filter options, but additional options exist in the new version.
            // Each source filter and filter option can be auto-mapped exactly to one in
            // the target version, leaving some candidates unused but essentially the mapping
            // is complete unless the user manually intervenes at this point.
            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture
                        .DefaultFilterMappingPlan()
                        .AddFilterMapping(
                            "filter-1-key",
                            DataFixture
                                .DefaultFilterMapping()
                                .WithNoMapping()
                                .AddOptionMapping(
                                    "filter-1-option-1-key",
                                    DataFixture.DefaultFilterOptionMapping().WithNoMapping()
                                )
                        )
                        .AddFilterMapping(
                            "filter-2-key",
                            DataFixture
                                .DefaultFilterMapping()
                                .WithNoMapping()
                                .AddOptionMapping(
                                    "filter-2-option-1-key",
                                    DataFixture.DefaultFilterOptionMapping().WithNoMapping()
                                )
                        )
                        .AddFilterCandidate(
                            "filter-1-key",
                            DataFixture
                                .DefaultFilterMappingCandidate()
                                .AddOptionCandidate("filter-1-option-1-key", DataFixture.DefaultMappableFilterOption())
                        )
                );

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            Dictionary<string, FilterMapping> expectedFilterMappings = new()
            {
                {
                    "filter-1-key",
                    mapping.GetFilterMapping("filter-1-key") with
                    {
                        Type = MappingType.AutoMapped,
                        CandidateKey = "filter-1-key",
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            {
                                "filter-1-option-1-key",
                                mapping.GetFilterOptionMapping("filter-1-key", "filter-1-option-1-key") with
                                {
                                    Type = MappingType.AutoMapped,
                                    CandidateKey = "filter-1-option-1-key",
                                }
                            },
                        },
                    }
                },
                {
                    "filter-2-key",
                    mapping.GetFilterMapping("filter-2-key") with
                    {
                        Type = MappingType.AutoNone,
                        OptionMappings = new Dictionary<string, FilterOptionMapping>
                        {
                            {
                                "filter-2-option-1-key",
                                mapping.GetFilterOptionMapping("filter-2-key", "filter-2-option-1-key") with
                                {
                                    Type = MappingType.AutoNone,
                                }
                            },
                        },
                    }
                },
            };

            updatedMapping.FilterMappingPlan.Mappings.AssertDeepEqualTo(
                expectedFilterMappings,
                ignoreCollectionOrders: true
            );

            Assert.True(updatedMapping.FilterMappingsComplete);

            // Some source filter options have no equivalent candidate to be mapped to, thus
            // resulting in a major version update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0");
        }

        [Fact]
        public async Task Complete_HasDeletedIndicators_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture
                        .DefaultFilterMappingPlan()
                        .AddFilterMapping(
                            "filter-1-key",
                            DataFixture
                                .DefaultFilterMapping()
                                .WithNoMapping()
                                .AddOptionMapping(
                                    "filter-1-option-1-key",
                                    DataFixture.DefaultFilterOptionMapping().WithNoMapping()
                                )
                        )
                        .AddFilterCandidate(
                            "filter-1-key",
                            DataFixture
                                .DefaultFilterMappingCandidate()
                                .AddOptionCandidate("filter-1-option-1-key", DataFixture.DefaultMappableFilterOption())
                        )
                )
                // Has deleted indicators that cannot be mapped
                .WithHasDeletedIndicators(true);

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            // Is an exact match but as there are deleted indicators so needs to be a major update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0");
        }

        [Fact]
        public async Task Complete_HasDeletedGeographicLevels_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture
                        .DefaultFilterMappingPlan()
                        .AddFilterMapping(
                            "filter-1-key",
                            DataFixture
                                .DefaultFilterMapping()
                                .WithNoMapping()
                                .AddOptionMapping(
                                    "filter-1-option-1-key",
                                    DataFixture.DefaultFilterOptionMapping().WithNoMapping()
                                )
                        )
                        .AddFilterCandidate(
                            "filter-1-key",
                            DataFixture
                                .DefaultFilterMappingCandidate()
                                .AddOptionCandidate("filter-1-option-1-key", DataFixture.DefaultMappableFilterOption())
                        )
                )
                // Has deleted geographic levels that cannot be mapped
                .WithHasDeletedGeographicLevels(true);

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            // Is an exact match but as there are deleted geographic levels so needs to be a major update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0");
        }

        [Fact]
        public async Task Complete_HasDeletedTimePeriods_MajorUpdate()
        {
            var (instanceId, originalVersion, nextVersion) = await CreateNextDataSetVersionAndDataFiles(
                Stage.PreviousStage()
            );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(nextVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture
                        .DefaultFilterMappingPlan()
                        .AddFilterMapping(
                            "filter-1-key",
                            DataFixture
                                .DefaultFilterMapping()
                                .WithNoMapping()
                                .AddOptionMapping(
                                    "filter-1-option-1-key",
                                    DataFixture.DefaultFilterOptionMapping().WithNoMapping()
                                )
                        )
                        .AddFilterCandidate(
                            "filter-1-key",
                            DataFixture
                                .DefaultFilterMappingCandidate()
                                .AddOptionCandidate("filter-1-option-1-key", DataFixture.DefaultMappableFilterOption())
                        )
                )
                // Has deleted time periods that cannot be mapped
                .WithHasDeletedTimePeriods(true);

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mapping));

            await ApplyAutoMappings(instanceId);

            var updatedMapping = await GetDataSetVersionMapping(nextVersion);

            // Is an exact match but as there are deleted time periods so needs to be a major update.
            await AssertCorrectDataSetVersionNumbers(updatedMapping, "2.0.0");
        }
    }

    public class CompleteNextDataSetVersionMappingsMappingProcessingTests(
        ProcessorFunctionsIntegrationTestFixture fixture
    ) : ProcessNextDataSetVersionMappingsFunctionsTests(fixture)
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

            var savedImport = await publicDataDbContext
                .DataSetVersionImports.Include(i => i.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            Assert.Null(savedImport.Completed);

            Assert.Equal(DataSetVersionStatus.Mapping, savedImport.DataSetVersion.Status);
        }

        private async Task CompleteProcessing(Guid instanceId)
        {
            var function = GetRequiredService<ProcessNextDataSetVersionMappingsFunctions>();
            await function.CompleteNextDataSetVersionMappingProcessing(instanceId, CancellationToken.None);
        }
    }

    private async Task<(
        Guid instanceId,
        DataSetVersion initialVersion,
        DataSetVersion nextVersion
    )> CreateNextDataSetVersionAndDataFiles(
        DataSetVersionImportStage importStage,
        DataSetVersionMeta? initialVersionMeta = null
    )
    {
        var (initialDataSetVersion, nextDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
            initialVersionMeta: initialVersionMeta ?? GetDefaultInitialDataSetVersionMeta(),
            nextVersionImportStage: importStage,
            nextVersionStatus: DataSetVersionStatus.Processing
        );

        SetupCsvDataFilesForDataSetVersion(ProcessorTestData.AbsenceSchool, nextDataSetVersion);

        ReleaseFile releaseFile = DataFixture
            .DefaultReleaseFile()
            .WithId(nextDataSetVersion.Release.ReleaseFileId)
            .WithReleaseVersion(DataFixture.DefaultReleaseVersion())
            .WithFile(DataFixture.DefaultFile())
            .WithPublicApiDataSetId(nextDataSetVersion.DataSetId)
            .WithPublicApiDataSetVersion(nextDataSetVersion.SemVersion());

        await AddTestData<ContentDbContext>(context => context.ReleaseFiles.Add(releaseFile));

        return (instanceId, initialDataSetVersion, nextDataSetVersion);
    }

    private async Task<DataSetVersion> GetDataSetVersion(DataSetVersion nextVersion)
    {
        return await GetDbContext<PublicDataDbContext>().DataSetVersions.SingleAsync(dsv => dsv.Id == nextVersion.Id);
    }

    private async Task<DataSetVersionMapping> GetDataSetVersionMapping(DataSetVersion nextVersion)
    {
        return await GetDbContext<PublicDataDbContext>()
            .DataSetVersionMappings.Include(mapping => mapping.TargetDataSetVersion)
            .SingleAsync(mapping => mapping.TargetDataSetVersionId == nextVersion.Id);
    }

    private DataSetVersionMeta GetDefaultInitialDataSetVersionMeta()
    {
        return new DataSetVersionMeta
        {
            GeographicLevelMeta = DataFixture
                .DefaultGeographicLevelMeta()
                .WithLevels(ProcessorTestData.AbsenceSchool.ExpectedGeographicLevels),
        };
    }

    private async Task AssertCorrectDataSetVersionNumbers(DataSetVersionMapping mapping, string expectedVersion)
    {
        Assert.Equal(expectedVersion, mapping.TargetDataSetVersion.SemVersion().ToString());

        var updatedReleaseFile = await GetDbContext<ContentDbContext>()
            .ReleaseFiles.SingleAsync(rf => rf.PublicApiDataSetId == mapping.TargetDataSetVersion.DataSetId);

        Assert.Equal(expectedVersion, updatedReleaseFile.PublicApiDataSetVersion?.ToString());
    }
}
