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
using Microsoft.Extensions.Options;
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
                new ProcessDataSetVersionContext {DataSetVersionId = Guid.NewGuid()});
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

    public class CreateMappingsProcessingTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessNextDataSetVersionFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.ImportingMetadata;

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
                    .LocationsByGeographicLevel
                    .Select(level => level.Level)
                    .Order(),
                metaSummary
                    .GeographicLevels
                    .Order());

            Assert.Equal(
                ProcessorTestData
                    .AbsenceSchool
                    .FiltersAndOptions
                    .Select(filterAndOptions => filterAndOptions.Label)
                    .Order(),
                metaSummary
                    .Filters
                    .Order());

            Assert.Equal(
                ProcessorTestData.AbsenceSchool.Indicators.Order(),
                metaSummary.Indicators.Order());

            Assert.Equal(
                ProcessorTestData.AbsenceSchool.TimePeriodRange,
                metaSummary.TimePeriodRange);
        }

        [Fact]
        public async Task Success_LocationSourceMappings()
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

            var expectedLocationMappings = initialLocationMeta
                .OrderBy(levelMeta => levelMeta.Level)
                .Select(levelMeta => new LocationLevelMappingPlan
                {
                    Level = levelMeta.Level,
                    Mappings = levelMeta
                        .Options
                        .OrderBy(option => option.Label)
                        .Select(option => new LocationOptionMapping
                        {
                            Source = new LocationOption
                            {
                                Key = $"Source level {levelMeta.Level} location {option.Label}",
                                Label = option.Label
                            }
                        })
                        .ToList()
                })
                .ToList();

            mappings.LocationMappingPlan.Mappings.AssertDeepEqualTo(expectedLocationMappings);
        }

        [Fact]
        public async Task Success_LocationTargetMappings()
        {
            var (instanceId, initialVersion, nextVersion) =
                await CreateNextDataSetVersionAndDataFiles(Stage.PreviousStage());

            await CreateMappings(instanceId);

            var mappings = GetDbContext<PublicDataDbContext>()
                .DataSetVersionMappings
                .Single();

            Assert.Equal(initialVersion.Id, mappings.SourceDataSetVersionId);
            Assert.Equal(nextVersion.Id, mappings.TargetDataSetVersionId);

            var expectedLocationTargets = ProcessorTestData
                .AbsenceSchool
                .LocationsByGeographicLevel
                .Select(levelMeta => (level: levelMeta.Level, options: levelMeta.Options))
                .OrderBy(levelMeta => levelMeta.level)
                .Select(levelMeta => new LocationLevelMappingCandidates
                {
                    Level = levelMeta.level,
                    Candidates = levelMeta
                        .options
                        .Order()
                        .Select(option => new LocationOption
                        {
                            Key = $"Target level {levelMeta.level} location {option}",
                            Label = option
                        })
                        .ToList()
                })
                .ToList();

            mappings.LocationMappingPlan.Candidates.AssertDeepEqualTo(expectedLocationTargets);
        }

        [Fact]
        public async Task Success_FilterSourceMappings()
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

            var expectedFilterMappings = initialFilterMeta
                .OrderBy(filter => filter.Label)
                .Select(filter => new FilterMapping
                {
                    Source = new Filter
                    {
                        Key = $"Source filter {filter.Label}",
                        Label = filter.Label
                    },
                    Options = filter
                        .Options
                        .OrderBy(option => option.Label)
                        .Select(option => new FilterOptionMapping
                        {
                            Source = new FilterOption
                            {
                                Key = $"Source filter {filter.Label} option {option.Label}",
                                Label = option.Label
                            }
                        })
                        .ToList()
                })
                .ToList();

            mappings.FilterMappingPlan.Mappings.AssertDeepEqualTo(expectedFilterMappings);
        }

        [Fact]
        public async Task Success_FilterTargetMappings()
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
                .FiltersAndOptions
                .OrderBy(filterAndOptions => filterAndOptions.Label)
                .Select(filterAndOptions => new FilterMappingCandidate
                {
                    Key = $"Target filter {filterAndOptions.Label}",
                    Label = filterAndOptions.Label,
                    Options = filterAndOptions
                        .Options
                        .Order()
                        .Select(option => new FilterOption
                        {
                            Key = $"Target filter {filterAndOptions.Label} option {option}",
                            Label = option
                        })
                        .ToList()
                })
                .ToList();

            mappings.FilterMappingPlan.Candidates.AssertDeepEqualTo(expectedFilterTargets);
        }

        private async Task CreateMappings(Guid instanceId)
        {
            var function = GetRequiredService<ProcessNextDataSetVersionFunction>();
            await function.CreateMappings(instanceId, CancellationToken.None);
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
            versionNumberMajor: 1,
            versionNumberMinor: 1);

        SetupCsvDataFilesForDataSetVersion(ProcessorTestData.AbsenceSchool, nextDataSetVersion);
        return (instanceId, initialDataSetVersion, nextDataSetVersion);
    }

    private DataSetVersion GetDataSetVersion(DataSetVersion nextVersion)
    {
        return GetDbContext<PublicDataDbContext>()
            .DataSetVersions
            .Single(dsv => dsv.Id == nextVersion.Id);
    }
}
