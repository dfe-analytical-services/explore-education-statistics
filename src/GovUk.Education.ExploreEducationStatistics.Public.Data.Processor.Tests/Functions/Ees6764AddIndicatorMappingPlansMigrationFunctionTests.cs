using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TestData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TheoryData;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class Ees6764AddIndicatorMappingPlansMigrationFunctionTestsFixture()
    : OptimisedPublicDataProcessorCollectionFixture(
        capabilities: [PublicDataProcessorIntegrationTestCapability.Postgres]
    )
{
    public Ees6764AddIndicatorMappingPlansMigrationFunction Function = null!;

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

        Function = lookups.GetService<Ees6764AddIndicatorMappingPlansMigrationFunction>();
    }
}

[CollectionDefinition(nameof(Ees6764AddIndicatorMappingPlansMigrationFunctionTestsFixture))]
public class Ees6764AddIndicatorMappingPlansMigrationFunctionTestsCollection
    : ICollectionFixture<Ees6764AddIndicatorMappingPlansMigrationFunctionTestsFixture>;

[Collection(nameof(Ees6764AddIndicatorMappingPlansMigrationFunctionTestsFixture))]
public class Ees6764AddIndicatorMappingPlansMigrationFunctionTests(
    Ees6764AddIndicatorMappingPlansMigrationFunctionTestsFixture fixture
) : OptimisedFunctionAppIntegrationTestBase(fixture)
{
    private static readonly DataFixture DataFixture = new();

    private static readonly ProcessorTestData TestData = ProcessorTestData.AbsenceSchool;

    public static readonly TheoryData<(DataSetVersionStatus, DataSetVersionImportStage)> AllStatusesAndImportStages =
        new(EnumUtil.GetEnums<DataSetVersionStatus>().Cartesian(EnumUtil.GetEnums<DataSetVersionImportStage>()));

    [Fact]
    public async Task DryRunFlagSetToTrue_NoActualDatabaseUpdatesOccur()
    {
        // Arrange
        var (_, targetDataSetVersion, _) = await SetupInitialAndNextDataSetVersions(
            sourceIndicators: TestData.ExpectedIndicators,
            targetIndicators: TestData.ExpectedIndicators,
            nextVersionStatus: DataSetVersionStatus.Draft,
            nextVersionImportStage: DataSetVersionImportStage.ManualMapping
        );

        // Act
        var response = await fixture.Function.AddIndicatorMappingsToExistingDataSetVersionMappings(
            request: null!,
            dryRun: true
        );

        // Assert
        var updates = response.AssertOkObjectResult<List<IndicatorMappingPlanUpdate>>();

        var update = Assert.Single(updates);

        // Check that the JSON response contains the mapping updates that we expect to see being planned out.
        Assert.True(update.IndicatorMappingPlan.Mappings.Values.All(m => m.Type == MappingType.AutoMapped));
        Assert.True(update.IndicatorMappingPlan.Mappings.All(m => m.Value.CandidateKey == m.Key));
        Assert.True(update.IndicatorMappingsComplete);

        var actualMapping = fixture
            .GetPublicDataDbContext()
            .DataSetVersionMappings.Single(m => m.TargetDataSetVersionId == targetDataSetVersion.Id);

        // Check that the database has not yet been updated thanks to the dryRun flag being true.
        Assert.Null(actualMapping.IndicatorMappingPlan);
        Assert.False(actualMapping.IndicatorMappingsComplete);
    }

    [Fact]
    public async Task ExistingIndicatorMappingPlan_NoUpdatesNecessary()
    {
        // Arrange
        var (_, targetDataSetVersion, _) = await SetupInitialAndNextDataSetVersions(
            sourceIndicators: TestData.ExpectedIndicators,
            targetIndicators: TestData.ExpectedIndicators,
            nextVersionStatus: DataSetVersionStatus.Draft,
            nextVersionImportStage: DataSetVersionImportStage.ManualMapping
        );

        var mappings = fixture
            .GetPublicDataDbContext()
            .DataSetVersionMappings.Single(m => m.TargetDataSetVersionId == targetDataSetVersion.Id);

        mappings.IndicatorMappingPlan = new IndicatorMappingPlan();

        await fixture.GetPublicDataDbContext().AddTestData(context => context.Update(mappings));

        // Act
        var response = await fixture.Function.AddIndicatorMappingsToExistingDataSetVersionMappings(
            request: null!,
            dryRun: false
        );

        // Assert
        var updates = response.AssertOkObjectResult<List<IndicatorMappingPlanUpdate>>();

        Assert.Empty(updates);
    }

    [Theory]
    [MemberData(memberName: nameof(AllStatusesAndImportStages))]
    public async Task ExactSameIndicators_AllAutoMapped_IndicatorMappingsComplete(
        (DataSetVersionStatus dataSetVersionStatus, DataSetVersionImportStage dataSetVersionImportStage) statusAndStage
    )
    {
        // Arrange
        var (_, targetDataSetVersion, _) = await SetupInitialAndNextDataSetVersions(
            sourceIndicators: TestData.ExpectedIndicators,
            targetIndicators: TestData.ExpectedIndicators,
            nextVersionStatus: statusAndStage.dataSetVersionStatus,
            nextVersionImportStage: statusAndStage.dataSetVersionImportStage
        );

        // Act
        var response = await fixture.Function.AddIndicatorMappingsToExistingDataSetVersionMappings(
            request: null!,
            dryRun: false
        );

        // Assert
        var updates = response.AssertOkObjectResult<List<IndicatorMappingPlanUpdate>>();

        var update = Assert.Single(updates);

        // Check that the JSON response contains the mapping updates that we expect to see being planned out.
        Assert.True(update.IndicatorMappingPlan.Mappings.Values.All(m => m.Type == MappingType.AutoMapped));
        Assert.True(update.IndicatorMappingPlan.Mappings.All(m => m.Value.CandidateKey == m.Key));
        Assert.True(update.IndicatorMappingsComplete);

        var actualMapping = fixture
            .GetPublicDataDbContext()
            .DataSetVersionMappings.Single(m => m.TargetDataSetVersionId == targetDataSetVersion.Id);

        // Check that the database has been updated in the same way as the planned updates returned in the JSON response.
        Assert.True(actualMapping.IndicatorMappingPlan!.Mappings.Values.All(m => m.Type == MappingType.AutoMapped));
        Assert.True(actualMapping.IndicatorMappingPlan.Mappings.All(m => m.Value.CandidateKey == m.Key));
        Assert.True(actualMapping.IndicatorMappingsComplete);
    }

    [Theory]
    [MemberData(
        memberName: nameof(DataSetVersionStatusTheoryData.PreMappingsFinalisedStatusesTheoryData),
        MemberType = typeof(DataSetVersionStatusTheoryData)
    )]
    public async Task DifferentIndicators_NonFinalisedDataSetVersion_AllAutoNone_IndicatorMappingsNotComplete(
        DataSetVersionStatus dataSetVersionStatus
    )
    {
        // Arrange
        var differentSourceIndicators = TestData
            .ExpectedIndicators.Select(indicator =>
            {
                var sourceIndicator = indicator.ShallowClone();
                sourceIndicator.Column += " different";
                return sourceIndicator;
            })
            .ToList();

        var (_, targetDataSetVersion, _) = await SetupInitialAndNextDataSetVersions(
            sourceIndicators: differentSourceIndicators,
            targetIndicators: TestData.ExpectedIndicators,
            nextVersionStatus: dataSetVersionStatus,
            nextVersionImportStage: DataSetVersionImportStage.ManualMapping
        );

        // Act
        var response = await fixture.Function.AddIndicatorMappingsToExistingDataSetVersionMappings(
            request: null!,
            dryRun: false
        );

        // Assert
        var updates = response.AssertOkObjectResult<List<IndicatorMappingPlanUpdate>>();

        var update = Assert.Single(updates);

        // Check that the JSON response contains the mapping updates that we expect to see being planned out.
        Assert.True(update.IndicatorMappingPlan.Mappings.Values.All(m => m.Type == MappingType.AutoNone));
        Assert.True(update.IndicatorMappingPlan.Mappings.All(m => m.Value.CandidateKey == null));
        Assert.False(update.IndicatorMappingsComplete);

        var actualMapping = fixture
            .GetPublicDataDbContext()
            .DataSetVersionMappings.Single(m => m.TargetDataSetVersionId == targetDataSetVersion.Id);

        // Check that the database has been updated in the same way as the planned updates returned in the JSON response.
        Assert.True(actualMapping.IndicatorMappingPlan!.Mappings.Values.All(m => m.Type == MappingType.AutoNone));
        Assert.True(actualMapping.IndicatorMappingPlan.Mappings.All(m => m.Value.CandidateKey == null));
        Assert.False(actualMapping.IndicatorMappingsComplete);
    }

    [Theory]
    [MemberData(
        memberName: nameof(DataSetVersionStatusTheoryData.PostMappingStatusesTheoryData),
        MemberType = typeof(DataSetVersionStatusTheoryData)
    )]
    public async Task DifferentIndicators_FinalisedDataSetVersion_AllManualNone_IndicatorMappingsComplete(
        DataSetVersionStatus dataSetVersionStatus
    )
    {
        // Arrange
        var differentSourceIndicators = TestData
            .ExpectedIndicators.Select(indicator =>
            {
                var sourceIndicator = indicator.ShallowClone();
                sourceIndicator.Column += " different";
                return sourceIndicator;
            })
            .ToList();

        var (_, targetDataSetVersion, _) = await SetupInitialAndNextDataSetVersions(
            sourceIndicators: differentSourceIndicators,
            targetIndicators: TestData.ExpectedIndicators,
            nextVersionStatus: dataSetVersionStatus,
            nextVersionImportStage: DataSetVersionImportStage.ManualMapping
        );

        // Act
        var response = await fixture.Function.AddIndicatorMappingsToExistingDataSetVersionMappings(
            request: null!,
            dryRun: false
        );

        // Assert
        var updates = response.AssertOkObjectResult<List<IndicatorMappingPlanUpdate>>();

        var update = Assert.Single(updates);

        // Check that the JSON response contains the mapping updates that we expect to see being planned out.
        Assert.True(update.IndicatorMappingPlan.Mappings.Values.All(m => m.Type == MappingType.ManualNone));
        Assert.True(update.IndicatorMappingPlan.Mappings.All(m => m.Value.CandidateKey == null));
        Assert.True(update.IndicatorMappingsComplete);

        var actualMapping = fixture
            .GetPublicDataDbContext()
            .DataSetVersionMappings.Single(m => m.TargetDataSetVersionId == targetDataSetVersion.Id);

        // Check that the database has been updated in the same way as the planned updates returned in the JSON response.
        Assert.True(actualMapping.IndicatorMappingPlan!.Mappings.Values.All(m => m.Type == MappingType.ManualNone));
        Assert.True(actualMapping.IndicatorMappingPlan.Mappings.All(m => m.Value.CandidateKey == null));
        Assert.True(actualMapping.IndicatorMappingsComplete);
    }

    private async Task<(
        DataSetVersion sourceDataSetVersion,
        DataSetVersion targetDataSetVersion,
        Guid instanceId
    )> SetupInitialAndNextDataSetVersions(
        List<IndicatorMeta> sourceIndicators,
        List<IndicatorMeta> targetIndicators,
        DataSetVersionImportStage nextVersionImportStage,
        DataSetVersionStatus nextVersionStatus
    )
    {
        var (sourceDataSetVersion, targetDataSetVersion, instanceId) =
            await CommonTestDataUtils.CreateDataSetInitialAndNextVersion(
                fixture.GetPublicDataDbContext(),
                nextVersionImportStage: nextVersionImportStage,
                nextVersionStatus: nextVersionStatus,
                initialVersionMeta: new DataSetVersionMeta { IndicatorMetas = ResetIndicatorMetaIds(sourceIndicators) },
                nextVersionMeta: new DataSetVersionMeta { IndicatorMetas = ResetIndicatorMetaIds(targetIndicators) }
            );

        CommonTestDataUtils.SetupCsvDataFilesForDataSetVersion(
            fixture.GetDataSetVersionPathResolver(),
            TestData,
            sourceDataSetVersion
        );

        CommonTestDataUtils.SetupCsvDataFilesForDataSetVersion(
            fixture.GetDataSetVersionPathResolver(),
            TestData,
            targetDataSetVersion
        );

        DataSetVersionMapping mapping = DataFixture
            .DefaultDataSetVersionMapping()
            .WithSourceDataSetVersionId(sourceDataSetVersion.Id)
            .WithTargetDataSetVersionId(targetDataSetVersion.Id)
            .WithIndicatorMappingPlan(null!);

        await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSetVersionMappings.Add(mapping));

        return (sourceDataSetVersion, targetDataSetVersion, instanceId);
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
