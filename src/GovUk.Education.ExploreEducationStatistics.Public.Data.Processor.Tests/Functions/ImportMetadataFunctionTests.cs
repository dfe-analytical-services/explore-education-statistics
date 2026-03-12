using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TestData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using InterpolatedSql.Dapper;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class ImportMetadataFunctionTestsFixture()
    : OptimisedPublicDataProcessorCollectionFixture(
        capabilities: [PublicDataProcessorIntegrationTestCapability.Postgres]
    )
{
    public ImportMetadataFunction Function = null!;

    protected override void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        base.ConfigureServicesAndConfiguration(serviceModifications);

        var dataFilesBasePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        serviceModifications.AddInMemoryCollection([
            new KeyValuePair<string, string?>("DataFiles:BasePath", dataFilesBasePath),
        ]);
    }

    protected override async Task AfterFactoryConstructed(OptimisedServiceCollectionLookups lookups)
    {
        await base.AfterFactoryConstructed(lookups);

        Function = lookups.GetService<ImportMetadataFunction>();
    }

    public override async Task BeforeEachTest()
    {
        await base.BeforeEachTest();

        // The expected test data in a great deal of these tests is dependent on id sequences
        // beginning at 1, so for simplicity we reset data between each test.
        await GetPublicDataDbContext().ClearTestData();
    }
}

[CollectionDefinition(nameof(ImportMetadataFunctionTestsFixture))]
public class ImportMetadataFunctionTestsCollection : ICollectionFixture<ImportMetadataFunctionTestsFixture>;

[Collection(nameof(ImportMetadataFunctionTestsFixture))]
public abstract class ImportMetadataFunctionTests(ImportMetadataFunctionTestsFixture fixture)
    : OptimisedFunctionAppIntegrationTestBase(fixture)
{
    private static readonly DataFixture DataFixture = new();

    private const DataSetVersionImportStage Stage = DataSetVersionImportStage.ImportingMetadata;

    public static readonly TheoryData<ProcessorTestData> TestDataFiles = new()
    {
        ProcessorTestData.AbsenceSchool,
        ProcessorTestData.FilterDefaultOptions,
    };

    public class ImportMetadataDbTests(ImportMetadataFunctionTestsFixture fixture)
        : ImportMetadataFunctionTests(fixture)
    {
        [Theory]
        [MemberData(nameof(TestDataFiles))]
        public async Task InitialVersion_Success(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                fixture.GetPublicDataDbContext(),
                Stage.PreviousStage()
            );

            await ImportMetadata(testData, dataSetVersion, instanceId);

            var savedImport = await fixture
                .GetPublicDataDbContext()
                .DataSetVersionImports.Include(i => i.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);
            var savedDataSetVersion = savedImport.DataSetVersion;

            Assert.Equal(Stage, savedImport.Stage);
            Assert.Equal(DataSetVersionStatus.Processing, savedDataSetVersion.Status);

            CommonTestDataUtils.AssertDataSetVersionDirectoryContainsOnlyFiles(
                fixture.GetDataSetVersionPathResolver(),
                dataSetVersion,
                DataSetFilenames.CsvDataFile,
                DataSetFilenames.CsvMetadataFile,
                DataSetFilenames.DuckDbDatabaseFile
            );

            Assert.Equal(testData.ExpectedTotalResults, savedDataSetVersion.TotalResults);

            var firstExpectedTimePeriod = testData.ExpectedTimePeriods.First();
            var lastExpectedTimePeriod = testData.ExpectedTimePeriods.Last();

            var expectedMetaSummary = new DataSetVersionMetaSummary
            {
                Filters = testData.ExpectedFilters.Select(fm => fm.Label).ToList(),
                Indicators = testData.ExpectedIndicators.Select(fm => fm.Label).ToList(),
                GeographicLevels = testData.ExpectedGeographicLevels,
                TimePeriodRange = new TimePeriodRange
                {
                    Start = new TimePeriodRangeBound
                    {
                        Code = firstExpectedTimePeriod.Code,
                        Period = firstExpectedTimePeriod.Period,
                    },
                    End = new TimePeriodRangeBound
                    {
                        Code = firstExpectedTimePeriod.Code,
                        Period = lastExpectedTimePeriod.Period,
                    },
                },
            };

            savedDataSetVersion.MetaSummary.AssertDeepEqualTo(expectedMetaSummary);
        }

        [Theory]
        [MemberData(nameof(TestDataFiles))]
        public async Task InitialVersion_CorrectGeographicLevels(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                fixture.GetPublicDataDbContext(),
                Stage.PreviousStage()
            );

            await ImportMetadata(testData, dataSetVersion, instanceId);

            var actualGeographicLevelMeta = await fixture
                .GetPublicDataDbContext()
                .GeographicLevelMetas.SingleAsync(glm => glm.DataSetVersionId == dataSetVersion.Id);

            var actualGeographicLevels = actualGeographicLevelMeta
                .Levels.OrderBy(EnumToEnumLabelConverter<GeographicLevel>.ToProvider)
                .ToList();

            Assert.Equal(testData.ExpectedGeographicLevels.Order(), actualGeographicLevels.Order());
        }

        [Theory]
        [MemberData(nameof(TestDataFiles))]
        public async Task InitialVersion_CorrectTimePeriods_NoExistingTimePeriods(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                fixture.GetPublicDataDbContext(),
                Stage.PreviousStage()
            );

            await ImportMetadata(testData, dataSetVersion, instanceId);

            var actualTimePeriods = await fixture
                .GetPublicDataDbContext()
                .TimePeriodMetas.Where(tpm => tpm.DataSetVersionId == dataSetVersion.Id)
                .OrderBy(tpm => tpm.Period)
                .ToListAsync();

            Assert.Equal(testData.ExpectedTimePeriods.Count, actualTimePeriods.Count);
            Assert.All(
                testData.ExpectedTimePeriods,
                (expectedTimePeriod, index) =>
                {
                    var actualTimePeriod = actualTimePeriods[index];
                    actualTimePeriod.AssertDeepEqualTo(
                        expectedTimePeriod,
                        ignoreProperties: [tpm => tpm.Id, tpm => tpm.DataSetVersionId, tpm => tpm.Created]
                    );

                    Assert.Equal(dataSetVersion.Id, actualTimePeriod.DataSetVersionId);
                    Assert.NotEqual(expectedTimePeriod.Created, actualTimePeriod.Created);
                }
            );
        }

        [Theory]
        [InlineData(MappingType.AutoMapped)]
        [InlineData(MappingType.ManualMapped)]
        public async Task NextVersion_CorrectLocations_Mapped_ReuseOriginalPublicIds(MappingType mappingType)
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) =
                await CommonTestDataUtils.CreateDataSetInitialAndNextVersion(
                    fixture.GetPublicDataDbContext(),
                    nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                    nextVersionStatus: DataSetVersionStatus.Mapping
                );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithLocationMappingPlan(
                    DataFixture.LocationMappingPlanFromLocationMeta(
                        sourceLocations: testData.ExpectedLocations,
                        targetLocations: testData.ExpectedLocations
                    )
                );

            foreach (var locationLevelMapping in mapping.LocationMappingPlan.Levels.Values)
            {
                foreach (var optionMapping in locationLevelMapping.Mappings.Values)
                {
                    optionMapping.Type = mappingType;
                    optionMapping.PublicId = $"{optionMapping.PublicId}-orig";
                    optionMapping.Source.Label = $"{optionMapping.Source.Label} original";
                }
            }

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            var actualLocationLevels = await GetDbLocationMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedLocations.Count, actualLocationLevels.Count);
            Assert.All(
                testData.ExpectedLocations,
                (expectedLocationLevel, levelIndex) =>
                {
                    var actualLocationLevel = actualLocationLevels[levelIndex];

                    AssertLocationLevelsEqual(expectedLocationLevel, actualLocationLevel);

                    // Location options re-use original public IDs as they were mapped.
                    foreach (var expectedOptionLink in expectedLocationLevel.OptionLinks)
                    {
                        expectedOptionLink.PublicId = $"{expectedOptionLink.PublicId}-orig";
                    }

                    AssertAllLocationOptionLabelsAndPublicIdsEqual(expectedLocationLevel, actualLocationLevel);
                }
            );
        }

        [Theory]
        [InlineData(MappingType.AutoNone)]
        [InlineData(MappingType.ManualNone)]
        public async Task NextVersion_CorrectLocations_NotMapped_NewPublicIds(MappingType mappingType)
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) =
                await CommonTestDataUtils.CreateDataSetInitialAndNextVersion(
                    fixture.GetPublicDataDbContext(),
                    nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                    nextVersionStatus: DataSetVersionStatus.Mapping
                );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithLocationMappingPlan(
                    DataFixture.LocationMappingPlanFromLocationMeta(
                        sourceLocations: testData.ExpectedLocations,
                        targetLocations: testData.ExpectedLocations
                    )
                );

            foreach (var optionMapping in mapping.LocationMappingPlan.Levels.Values.SelectMany(l => l.Mappings.Values))
            {
                optionMapping.Type = mappingType;
                optionMapping.PublicId = $"{optionMapping.PublicId}-orig";
                optionMapping.Source.Label = $"{optionMapping.Source.Label} original";
                optionMapping.CandidateKey = null;
            }

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            var actualLocationLevels = await GetDbLocationMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedLocations.Count, actualLocationLevels.Count);
            Assert.All(
                testData.ExpectedLocations,
                (expectedLocationLevel, levelIndex) =>
                {
                    var actualLocationLevel = actualLocationLevels[levelIndex];

                    // Location level gets a new public ID as it had no mapping.
                    AssertLocationLevelsEqual(expectedLocationLevel, actualLocationLevel);

                    // Location options get new public IDs as they had no mappings.
                    AssertAllLocationOptionLabelsAndPublicIdsEqual(expectedLocationLevel, actualLocationLevel);
                }
            );
        }

        [Theory]
        [InlineData(MappingType.AutoMapped)]
        [InlineData(MappingType.ManualMapped)]
        public async Task NextVersion_CorrectFiltersAndOptions_Mapped_ReuseOriginalPublicIds(MappingType mappingType)
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) =
                await CommonTestDataUtils.CreateDataSetInitialAndNextVersion(
                    fixture.GetPublicDataDbContext(),
                    nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                    nextVersionStatus: DataSetVersionStatus.Mapping
                );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture.FilterMappingPlanFromFilterMeta(
                        sourceFilters: testData.ExpectedFilters,
                        targetFilters: testData.ExpectedFilters
                    )
                );

            foreach (var (_, filterMapping) in mapping.FilterMappingPlan.Mappings)
            {
                filterMapping.Type = mappingType;
                filterMapping.PublicId = $"{filterMapping.PublicId}-orig";
                filterMapping.Source.Label = $"{filterMapping.Source.Label} original";

                foreach (var (_, optionMapping) in filterMapping.OptionMappings)
                {
                    optionMapping.Type = mappingType;
                    optionMapping.PublicId = $"{optionMapping.PublicId}-orig";
                    optionMapping.Source.Label = $"{optionMapping.Source.Label} original";
                }
            }

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            var actualFilters = await GetDbFilterMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedFilters.Count, actualFilters.Count);
            Assert.All(
                testData.ExpectedFilters,
                (expectedFilter, filterIndex) =>
                {
                    var actualFilter = actualFilters[filterIndex];

                    // Filter gets original public ID as it was mapped
                    expectedFilter.PublicId = $"{expectedFilter.PublicId}-orig";

                    AssertFiltersEqual(expectedFilter, actualFilter);

                    // Filter options re-use original public IDs as they were mapped
                    foreach (var expectedOptionLink in expectedFilter.OptionLinks)
                    {
                        expectedOptionLink.PublicId = $"{expectedOptionLink.PublicId}-orig";
                    }

                    AssertAllFilterOptionLabelsAndPublicIdsEqual(expectedFilter, actualFilter);
                }
            );
        }

        [Theory]
        [InlineData(MappingType.AutoNone)]
        [InlineData(MappingType.ManualNone)]
        public async Task NextVersion_CorrectFiltersAndOptions_NotMapped_NewPublicIds(MappingType mappingType)
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) =
                await CommonTestDataUtils.CreateDataSetInitialAndNextVersion(
                    fixture.GetPublicDataDbContext(),
                    nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                    nextVersionStatus: DataSetVersionStatus.Mapping
                );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture.FilterMappingPlanFromFilterMeta(
                        sourceFilters: testData.ExpectedFilters,
                        targetFilters: testData.ExpectedFilters
                    )
                );

            foreach (var (_, filterMapping) in mapping.FilterMappingPlan.Mappings)
            {
                filterMapping.Type = mappingType;
                filterMapping.PublicId = $"{filterMapping.PublicId}-orig";
                filterMapping.Source.Label = $"{filterMapping.Source.Label} original";
                filterMapping.CandidateKey = null;

                foreach (var (_, optionMapping) in filterMapping.OptionMappings)
                {
                    optionMapping.Type = mappingType;
                    optionMapping.PublicId = $"{optionMapping.PublicId}-orig";
                    optionMapping.Source.Label = $"{optionMapping.Source.Label} original";
                    optionMapping.CandidateKey = null;
                }
            }

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            var actualFilters = await GetDbFilterMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedFilters.Count, actualFilters.Count);
            Assert.All(
                testData.ExpectedFilters,
                (expectedFilter, filterIndex) =>
                {
                    var actualFilter = actualFilters[filterIndex];

                    // Filter gets new public ID as it had no mapping
                    AssertFiltersEqual(expectedFilter, actualFilter);

                    // Filter options get new public IDs as they had no mapping
                    AssertAllFilterOptionLabelsAndPublicIdsEqual(expectedFilter, actualFilter);
                }
            );
        }

        [Fact]
        public async Task InitialVersion_CorrectIndicators()
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var (dataSetVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                fixture.GetPublicDataDbContext(),
                Stage.PreviousStage()
            );

            await ImportMetadata(testData, dataSetVersion, instanceId);

            var actualIndicators = await GetDbIndicatorMetas(dataSetVersion.Id);

            Assert.Equal(testData.ExpectedIndicators.Count, actualIndicators.Count);
            Assert.All(
                testData.ExpectedIndicators,
                (expectedIndicator, index) =>
                {
                    var actualIndicator = actualIndicators[index];

                    actualIndicator.AssertDeepEqualTo(
                        expectedIndicator,
                        ignoreProperties: [m => m.DataSetVersionId, m => m.Created]
                    );

                    Assert.Equal(dataSetVersion.Id, actualIndicator.DataSetVersionId);
                    Assert.NotEqual(expectedIndicator.Created, actualIndicator.Created);
                }
            );
        }

        [Theory]
        [InlineData(MappingType.AutoMapped)]
        [InlineData(MappingType.ManualMapped)]
        public async Task NextVersion_CorrectIndicators_Mapped_ReuseOriginalPublicIds(MappingType mappingType)
        {
            var testData = ProcessorTestData.AbsenceSchool;

            var originalIndicators = ResetIndicatorMetaIds(testData.ExpectedIndicators)
                .Select(i =>
                {
                    var indicator = i.ShallowClone();
                    indicator.PublicId = $"{indicator.PublicId}-orig";
                    return indicator;
                })
                .ToList();

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) =
                await CommonTestDataUtils.CreateDataSetInitialAndNextVersion(
                    fixture.GetPublicDataDbContext(),
                    initialVersionMeta: new DataSetVersionMeta { IndicatorMetas = originalIndicators },
                    nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                    nextVersionStatus: DataSetVersionStatus.Mapping
                );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithIndicatorMappingPlan(
                    DataFixture.IndicatorMappingPlanFromIndicatorMeta(
                        sourceIndicators: originalIndicators,
                        targetIndicators: testData.ExpectedIndicators
                    )
                );

            foreach (var indicatorMapping in mapping.IndicatorMappingPlan!.Mappings.Values)
            {
                indicatorMapping.Type = mappingType;
            }

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            var actualIndicators = await GetDbIndicatorMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedIndicators.Count, actualIndicators.Count);
            Assert.All(
                testData.ExpectedIndicators,
                (expectedIndicator, index) =>
                {
                    var actualIndicator = actualIndicators[index];

                    actualIndicator.AssertDeepEqualTo(
                        expectedIndicator,
                        ignoreProperties: [m => m.Id, m => m.PublicId, m => m.DataSetVersionId, m => m.Created]
                    );

                    // Indicators re-use original public IDs of original indicators.
                    Assert.Equal($"{expectedIndicator.PublicId}-orig", actualIndicator.PublicId);
                    Assert.Equal(targetDataSetVersion.Id, actualIndicator.DataSetVersionId);
                    Assert.NotEqual(expectedIndicator.Created, actualIndicator.Created);
                }
            );
        }

        [Theory]
        [InlineData(MappingType.AutoNone)]
        [InlineData(MappingType.ManualNone)]
        public async Task NextVersion_CorrectIndicators_NotMapped_NewPublicIds(MappingType mappingType)
        {
            var testData = ProcessorTestData.AbsenceSchool;

            IndicatorMeta originalIndicatorNotPresentInTargetVersion = DataFixture.DefaultIndicatorMeta();

            // Set up the source data set version's Indicators, including one that will not be present in the
            // new version.
            List<IndicatorMeta> originalIndicators =
            [
                .. ResetIndicatorMetaIds(testData.ExpectedIndicators),
                originalIndicatorNotPresentInTargetVersion,
            ];

            var (sourceDataSetVersion, targetDataSetVersion, instanceId) =
                await CommonTestDataUtils.CreateDataSetInitialAndNextVersion(
                    fixture.GetPublicDataDbContext(),
                    initialVersionMeta: new DataSetVersionMeta { IndicatorMetas = originalIndicators },
                    nextVersionImportStage: DataSetVersionImportStage.ManualMapping,
                    nextVersionStatus: DataSetVersionStatus.Mapping
                );

            DataSetVersionMapping mapping = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
                .WithTargetDataSetVersionId(targetDataSetVersion.Id)
                .WithIndicatorMappingPlan(
                    DataFixture.IndicatorMappingPlanFromIndicatorMeta(
                        sourceIndicators: originalIndicators,
                        targetIndicators: testData.ExpectedIndicators
                    )
                );

            // TODO EES-6764 - remove null-forgiving operator.
            // Set a couple of the new data set version's Indicators to not be mapped to original Indicators.
            foreach (var indicatorMapping in mapping.IndicatorMappingPlan!.Mappings.Values)
            {
                indicatorMapping.Type = mappingType;
            }

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

            await ImportMetadata(testData, targetDataSetVersion, instanceId);

            var actualIndicators = await GetDbIndicatorMetas(targetDataSetVersion.Id);

            Assert.Equal(testData.ExpectedIndicators.Count, actualIndicators.Count);

            for (var i = 0; i < actualIndicators.Count; i++)
            {
                Assert.Equal(
                    SqidEncoder.Encode(testData.ExpectedIndicators[i].Id + originalIndicators.Count),
                    actualIndicators[i].PublicId
                );
            }
        }
    }

    public class ImportMetadataDuckDbTests(ImportMetadataFunctionTestsFixture fixture)
        : ImportMetadataFunctionTests(fixture)
    {
        [Theory]
        [MemberData(nameof(TestDataFiles))]
        public async Task InitialVersion_CorrectLocationOptions(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                fixture.GetPublicDataDbContext(),
                Stage.PreviousStage()
            );

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var actualLocationOptions = (
                await duckDbConnection
                    .SqlBuilder(
                        $"""
                        SELECT *
                        FROM {LocationOptionsTable.TableName:raw}
                        ORDER BY {LocationOptionsTable.Cols.Label:raw}
                        """
                    )
                    .QueryAsync<ParquetLocationOption>()
            ).AsList();

            var actualOptionsByLevel = actualLocationOptions
                .GroupBy(o => o.Level)
                .ToDictionary(g => EnumUtil.GetFromEnumValue<GeographicLevel>(g.Key), g => g.ToList());

            Assert.Equal(testData.ExpectedLocations.Count, actualOptionsByLevel.Count);
            Assert.All(
                testData.ExpectedLocations,
                expectedLocation =>
                {
                    Assert.True(actualOptionsByLevel.TryGetValue(expectedLocation.Level, out var actualOptions));
                    Assert.Equal(expectedLocation.Options.Count, actualOptions.Count);
                    Assert.All(
                        expectedLocation.Options.OrderBy(o => o.Label),
                        (expectedOption, index) => expectedOption.AssertEqual(actualOptions[index])
                    );
                }
            );
        }

        [Theory]
        [MemberData(nameof(TestDataFiles))]
        public async Task InitialVersion_CorrectTimePeriods(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                fixture.GetPublicDataDbContext(),
                Stage.PreviousStage()
            );

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var actualTimePeriods = (
                await duckDbConnection
                    .SqlBuilder(
                        $"""
                        SELECT *
                        FROM {TimePeriodsTable.TableName:raw}
                        ORDER BY {TimePeriodsTable.Cols.Period:raw}
                        """
                    )
                    .QueryAsync<ParquetTimePeriod>()
            ).AsList();

            Assert.Equal(testData.ExpectedTimePeriods.Count, actualTimePeriods.Count);
            Assert.All(
                testData.ExpectedTimePeriods,
                (expectedTimePeriod, index) =>
                {
                    var actualTimePeriod = actualTimePeriods[index];
                    Assert.Equal(
                        expectedTimePeriod.Code,
                        EnumUtil.GetFromEnumLabel<TimeIdentifier>(actualTimePeriod.Identifier)
                    );
                    Assert.Equal(expectedTimePeriod.Period, TimePeriodFormatter.FormatFromCsv(actualTimePeriod.Period));
                }
            );
        }

        [Theory]
        [MemberData(nameof(TestDataFiles))]
        public async Task InitialVersion_CorrectFilterOptions(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                fixture.GetPublicDataDbContext(),
                Stage.PreviousStage()
            );

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var actualFilterOptions = (
                await duckDbConnection
                    .SqlBuilder(
                        $"""
                        SELECT *
                        FROM {FilterOptionsTable.TableName:raw}
                        ORDER BY {FilterOptionsTable.Cols.Label:raw}
                        """
                    )
                    .QueryAsync<ParquetFilterOption>()
            ).AsList();

            var actualOptionsByFilterId = actualFilterOptions
                .GroupBy(o => o.FilterId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var globalOptionIndex = 0;

            Assert.Equal(testData.ExpectedFilters.Count, actualOptionsByFilterId.Count);
            Assert.All(
                testData.ExpectedFilters,
                expectedFilter =>
                {
                    Assert.True(actualOptionsByFilterId.TryGetValue(expectedFilter.PublicId, out var actualOptions));
                    Assert.Equal(expectedFilter.Options.Count, actualOptions.Count);
                    Assert.All(
                        expectedFilter.Options,
                        (expectedOption, index) =>
                        {
                            var actualOption = actualOptions[index];
                            Assert.Equal(expectedOption.Label, actualOption.Label);
                            Assert.Equal(expectedFilter.PublicId, actualOption.FilterId);

                            // Expect the PublicId to be that of the option link which in turn is expected to be encoded
                            // based on the sequence of option links inserted across all filters
                            Assert.Equal(SqidEncoder.Encode(++globalOptionIndex), actualOption.PublicId);
                        }
                    );
                }
            );
        }

        [Theory]
        [MemberData(nameof(TestDataFiles))]
        public async Task InitialVersion_CorrectIndicators(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                fixture.GetPublicDataDbContext(),
                Stage.PreviousStage()
            );

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var actualIndicators = (
                await duckDbConnection
                    .SqlBuilder(
                        $"""
                        SELECT *
                        FROM {IndicatorsTable.TableName:raw}
                        ORDER BY {IndicatorsTable.Cols.Label:raw}
                        """
                    )
                    .QueryAsync<ParquetIndicator>()
            ).AsList();

            Assert.Equal(testData.ExpectedIndicators.Count, actualIndicators.Count);
            Assert.All(
                testData.ExpectedIndicators,
                (expectedIndicator, index) =>
                {
                    var actualIndicator = actualIndicators[index];
                    Assert.Equal(expectedIndicator.PublicId, actualIndicator.Id);
                    Assert.Equal(expectedIndicator.Label, actualIndicator.Label);
                    Assert.Equal(expectedIndicator.DecimalPlaces, actualIndicator.DecimalPlaces);

                    if (expectedIndicator.Unit != null)
                    {
                        Assert.Equal(
                            expectedIndicator.Unit,
                            EnumUtil.GetFromEnumValue<IndicatorUnit>(actualIndicator.Unit)
                        );
                    }
                    else
                    {
                        Assert.Empty(actualIndicator.Unit);
                    }
                }
            );
        }
    }

    private async Task ImportMetadata(ProcessorTestData testData, DataSetVersion dataSetVersion, Guid instanceId)
    {
        CommonTestDataUtils.SetupCsvDataFilesForDataSetVersion(
            fixture.GetDataSetVersionPathResolver(),
            testData,
            dataSetVersion
        );

        await fixture.Function.ImportMetadata(instanceId, CancellationToken.None);
    }

    private async Task<List<LocationMeta>> GetDbLocationMetas(Guid dataSetVersionId) =>
        await fixture
            .GetPublicDataDbContext()
            .LocationMetas.Include(level => level.Options.OrderBy(o => o.Id))
            .Include(level => level.OptionLinks.OrderBy(l => l.OptionId))
                .ThenInclude(location => location.Option)
            .Where(level => level.DataSetVersionId == dataSetVersionId)
            .OrderBy(level => level.Id)
            .ToListAsync();

    private async Task<List<FilterMeta>> GetDbFilterMetas(Guid dataSetVersionId) =>
        await fixture
            .GetPublicDataDbContext()
            .FilterMetas.Include(fm => fm.Options.OrderBy(o => o.Label))
            .Include(fm => fm.OptionLinks.OrderBy(l => l.Option.Label))
                .ThenInclude(fm => fm.Option)
            .Where(fm => fm.DataSetVersionId == dataSetVersionId)
            .OrderBy(fm => fm.Label)
            .ToListAsync();

    private Task<List<IndicatorMeta>> GetDbIndicatorMetas(Guid dataSetVersionId) =>
        fixture
            .GetPublicDataDbContext()
            .IndicatorMetas.Where(im => im.DataSetVersionId == dataSetVersionId)
            .OrderBy(im => im.Label)
            .ToListAsync();

    private static void AssertLocationLevelsEqual(LocationMeta expectedLocationLevel, LocationMeta actualLocationLevel)
    {
        actualLocationLevel.AssertDeepEqualTo(
            expectedLocationLevel,
            ignoreProperties: [m => m.DataSetVersionId, m => m.Options, m => m.OptionLinks, m => m.Created]
        );
    }

    private static void AssertAllLocationOptionLabelsAndPublicIdsEqual(
        LocationMeta expectedLocationLevel,
        LocationMeta actualLocationLevel
    )
    {
        Assert.Equal(expectedLocationLevel.Options.Count, actualLocationLevel.Options.Count);
        Assert.Equal(expectedLocationLevel.OptionLinks.Count, actualLocationLevel.OptionLinks.Count);

        Assert.All(
            expectedLocationLevel.OptionLinks,
            (expectedOptionLink, linkIndex) =>
            {
                var actualOptionLink = actualLocationLevel.OptionLinks[linkIndex];

                Assert.Equal(expectedOptionLink.PublicId, actualOptionLink.PublicId);

                var expectedOption = expectedLocationLevel.Options[linkIndex];

                Assert.Equal(expectedOption.Label, actualOptionLink.Option.Label);
            }
        );
    }

    private static void AssertAllFilterOptionLabelsAndPublicIdsEqual(FilterMeta expectedFilter, FilterMeta actualFilter)
    {
        Assert.Equal(expectedFilter.Options.Count, actualFilter.Options.Count);
        Assert.Equal(expectedFilter.OptionLinks.Count, actualFilter.OptionLinks.Count);

        Assert.All(
            expectedFilter.OptionLinks,
            (expectedOptionLink, linkIndex) =>
            {
                var actualOptionLink = actualFilter.OptionLinks[linkIndex];

                Assert.Equal(expectedOptionLink.PublicId, actualOptionLink.PublicId);

                var expectedOption = expectedFilter.Options[linkIndex];

                Assert.Equal(expectedOption.Label, actualOptionLink.Option.Label);
            }
        );
    }

    private static void AssertFiltersEqual(FilterMeta expectedFilter, FilterMeta actualFilter)
    {
        actualFilter.AssertDeepEqualTo(
            expectedFilter,
            ignoreProperties:
            [
                m => m.DefaultOption,
                m => m.DataSetVersionId,
                m => m.Options,
                m => m.OptionLinks,
                m => m.Created,
            ]
        );
    }

    private DuckDbConnection GetDuckDbConnection(DataSetVersion dataSetVersion)
    {
        return DuckDbConnection.CreateFileConnectionReadOnly(
            fixture.GetDataSetVersionPathResolver().DuckDbPath(dataSetVersion)
        );
    }

    /// <summary>
    /// Sets IndicatorMeta ids to 0 to allow PostgreSql / sequences to assign them values.
    /// </summary>
    private List<IndicatorMeta> ResetIndicatorMetaIds(IEnumerable<IndicatorMeta> indicatorMetas)
    {
        return indicatorMetas
            .Select(originalIndicator =>
            {
                var indicatorWithResetId = originalIndicator.ShallowClone();

                indicatorWithResetId.Id = 0;

                return indicatorWithResetId;
            })
            .ToList();
    }
}
