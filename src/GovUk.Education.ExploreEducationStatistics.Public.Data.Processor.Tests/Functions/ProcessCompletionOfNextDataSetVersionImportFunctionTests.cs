using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using FilterMeta = GovUk.Education.ExploreEducationStatistics.Public.Data.Model.FilterMeta;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class ProcessCompletionOfNextDataSetVersionImportFunctionTests( 
    ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    private static readonly string[] AllDataSetVersionFiles =
    [
        DataSetFilenames.CsvDataFile,
        DataSetFilenames.CsvMetadataFile,
        DataSetFilenames.DuckDbDatabaseFile,
        DataSetFilenames.DuckDbLoadSqlFile,
        DataSetFilenames.DuckDbSchemaSqlFile,
        DataTable.ParquetFile,
        FilterOptionsTable.ParquetFile,
        IndicatorsTable.ParquetFile,
        LocationOptionsTable.ParquetFile,
        TimePeriodsTable.ParquetFile
    ];

    public class ProcessCompletionOfNextDataSetVersionImportTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessCompletionOfNextDataSetVersionImportFunctionTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            var mockOrchestrationContext = DefaultMockOrchestrationContext();
            var activitySequence = new MockSequence();

            string[] expectedActivitySequence =
            [
                ActivityNames.UpdateFileStoragePath,
                ActivityNames.ImportMetadata,
                ActivityNames.CreateChanges,
                ActivityNames.ImportData,
                ActivityNames.WriteDataFiles,
                ActivityNames.CompleteNextDataSetVersionImportProcessing
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

            await ProcessCompletionOfNextDataSetVersionImport(mockOrchestrationContext.Object);

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
                    context.CallActivityAsync(ActivityNames.UpdateFileStoragePath,
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

            await ProcessCompletionOfNextDataSetVersionImport(mockOrchestrationContext.Object);

            VerifyAllMocks(mockOrchestrationContext);
        }

        private async Task ProcessCompletionOfNextDataSetVersionImport(TaskOrchestrationContext orchestrationContext)
        {
            var function = GetRequiredService<ProcessCompletionOfNextDataSetVersionFunction>();
            await function.ProcessCompletionOfNextDataSetVersion(
                orchestrationContext,
                new ProcessDataSetVersionContext { DataSetVersionId = Guid.NewGuid() });
        }

        private static Mock<TaskOrchestrationContext> DefaultMockOrchestrationContext(Guid? instanceId = null)
        {
            var mock = new Mock<TaskOrchestrationContext>(MockBehavior.Strict);

            mock.Setup(context =>
                    context.CreateReplaySafeLogger(
                        nameof(ProcessCompletionOfNextDataSetVersionFunction.ProcessCompletionOfNextDataSetVersion)))
                .Returns(NullLogger.Instance);

            mock.SetupGet(context => context.InstanceId)
                .Returns(instanceId?.ToString() ?? Guid.NewGuid().ToString());

            return mock;
        }
    }

    public abstract class CreateChangesTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessCompletionOfNextDataSetVersionImportFunctionTests(fixture)
    {
        protected const DataSetVersionImportStage Stage = DataSetVersionImportStage.CreatingChanges;

        protected async Task CreateChanges(Guid instanceId)
        {
            var function = GetRequiredService<ProcessCompletionOfNextDataSetVersionFunction>();
            await function.CreateChanges(instanceId, CancellationToken.None);
        }
    }

    public class CreateChangesFilterTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : CreateChangesTests(fixture)
    {
        [Fact]
        public async Task FiltersAdded_ChangesContainOnlyAddedFiltersAndAddedOptions()
        {
            var oldFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s =>
                    s.SetPublicId("dP0Zw")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("dP0Zw"))
                        .Generate(1)))
                .GenerateList(1);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Filter and ALL options unchanged
                .ForIndex(1, s =>
                    s.SetPublicId("O7CLF") // Filter added
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("O7CLF")) // Filter Option added
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("7zXob")) // Filter Option added
                        .Generate(2)))
                .ForIndex(2, s =>
                    s.SetPublicId("7zXob") // Filter added
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("pTSoj")) // Filter Option added
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("IzBzg")) // Filter Option added
                        .Generate(2)))
                .GenerateList(3);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // 2 Filter additions
            Assert.Equal(2, filterMetaChanges.Count);
            Assert.All(filterMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            // 4 Filter Option additions
            Assert.Equal(4, filterOptionMetaChanges.Count);
            Assert.All(filterOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var newFilterMetas = newVersion.FilterMetas
                .ToDictionary(
                    m => m.PublicId,
                    m => new { FilterMeta = m, NewFilterOptionMetas = m.OptionLinks.ToDictionary(l => l.PublicId) });

            // Filter added
            AssertSingleFilterAdded(filterMetaChanges, newFilterMetas["O7CLF"].FilterMeta);

            // Filter added
            AssertSingleFilterAdded(filterMetaChanges, newFilterMetas["7zXob"].FilterMeta);

            // Filter Option added
            AssertSingleFilterOptionAdded(filterOptionMetaChanges, newFilterMetas["O7CLF"].NewFilterOptionMetas["O7CLF"]);

            // Filter Option added
            AssertSingleFilterOptionAdded(filterOptionMetaChanges, newFilterMetas["O7CLF"].NewFilterOptionMetas["7zXob"]);

            // Filter Option added
            AssertSingleFilterOptionAdded(filterOptionMetaChanges, newFilterMetas["7zXob"].NewFilterOptionMetas["pTSoj"]);

            // Filter Option added
            AssertSingleFilterOptionAdded(filterOptionMetaChanges, newFilterMetas["7zXob"].NewFilterOptionMetas["IzBzg"]);
        }

        [Fact]
        public async Task FiltersDeleted_ChangesContainOnlyDeletedFilters()
        {
            var oldFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s =>
                    s.SetPublicId("dP0Zw")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("dP0Zw"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("O7CLF"))
                        .Generate(2)))
                .ForIndex(1, s =>
                    s.SetPublicId("O7CLF") // Filter and ALL options deleted
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("pTSoj"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("IzBzg"))
                        .Generate(2)))
                .ForIndex(2, s =>
                    s.SetPublicId("7zXob") // Filter and ALL options deleted
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("LxWjE"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("6jrfe"))
                        .Generate(2)))
                .GenerateList(3);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Filter and ALL options unchanged
                .GenerateList(1);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // 2 Filter deletions
            Assert.Equal(2, filterMetaChanges.Count);
            Assert.All(filterMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            // No Filter Option deletions
            Assert.Empty(filterOptionMetaChanges);

            var oldFilterMetas = originalVersion.FilterMetas
                .ToDictionary(m => m.PublicId);

            // Filter deleted
            AssertSingleFilterDeleted(filterMetaChanges, oldFilterMetas["O7CLF"]);

            // Filter deleted
            AssertSingleFilterDeleted(filterMetaChanges, oldFilterMetas["7zXob"]);
        }

        [Fact]
        public async Task FiltersChangedOptionsAdded_ChangesContainOnlyChangedFiltersAndAddedOptions()
        {
            var oldFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s =>
                    s.SetPublicId("dP0Zw")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("dP0Zw"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("O7CLF"))
                        .Generate(2)))
                .ForIndex(1, s =>
                    s.SetPublicId("O7CLF")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("7zXob"))
                        .Generate(1)))
                .ForIndex(2, s =>
                    s.SetPublicId("7zXob")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("pTSoj"))
                        .Generate(1)))
                .GenerateList(3);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Filter and ALL options unchanged
                .ForIndex(1, s =>
                    s.SetPublicId("O7CLF") // Filter changed
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[1].OptionLinks[0])) // Filter Option unchanged
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("IzBzg")) // Filter Option added
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("it6Xr")) // Filter Option added
                        .Generate(3)))
                .ForIndex(2, s =>
                    s.SetPublicId("7zXob") // Filter changed
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[2].OptionLinks[0])) // Filter Option unchanged
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("LxWjE")) // Filter Option added
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("6jrfe")) // Filter Option added
                        .Generate(3)))
                .GenerateList(3);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // 2 Filter changes
            Assert.Equal(2, filterMetaChanges.Count);
            Assert.All(filterMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            // 4 Filter Option changes
            Assert.Equal(4, filterOptionMetaChanges.Count);
            Assert.All(filterOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas
                .ToDictionary(
                    m => m.PublicId,
                    m => new { FilterMeta = m, OldFilterOptionMetas = m.OptionLinks.ToDictionary(l => l.PublicId) });

            var newFilterMetas = newVersion.FilterMetas
                .ToDictionary(
                    m => m.PublicId,
                    m => new { FilterMeta = m, NewFilterOptionMetas = m.OptionLinks.ToDictionary(l => l.PublicId) });

            // Filter changed
            AssertSingleFilterChanged(
                changes: filterMetaChanges, 
                expectedOldFilterMeta: oldFilterMetas["O7CLF"].FilterMeta,
                expectedNewFilterMeta: newFilterMetas["O7CLF"].FilterMeta);

            // Filter changed
            AssertSingleFilterChanged(
                changes: filterMetaChanges,
                expectedOldFilterMeta: oldFilterMetas["7zXob"].FilterMeta,
                expectedNewFilterMeta: newFilterMetas["7zXob"].FilterMeta);

            // Filter Option added
            AssertSingleFilterOptionAdded(filterOptionMetaChanges, newFilterMetas["O7CLF"].NewFilterOptionMetas["IzBzg"]);

            // Filter Option added
            AssertSingleFilterOptionAdded(filterOptionMetaChanges, newFilterMetas["O7CLF"].NewFilterOptionMetas["it6Xr"]);

            // Filter Option added
            AssertSingleFilterOptionAdded(filterOptionMetaChanges, newFilterMetas["7zXob"].NewFilterOptionMetas["LxWjE"]);

            // Filter Option added
            AssertSingleFilterOptionAdded(filterOptionMetaChanges, newFilterMetas["7zXob"].NewFilterOptionMetas["6jrfe"]);
        }

        [Fact]
        public async Task FiltersChangedOptionsDeleted_ChangesContainOnlyChangedFiltersAndDeletedOptions()
        {
            var oldFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s =>
                    s.SetPublicId("dP0Zw")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("dP0Zw"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("O7CLF"))
                        .Generate(2)))
                .ForIndex(1, s =>
                    s.SetPublicId("O7CLF")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("7zXob"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("pTSoj")) // Filter Option deleted
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("IzBzg")) // Filter Option deleted
                        .Generate(3)))
                .ForIndex(2, s =>
                    s.SetPublicId("7zXob")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("it6Xr"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("LxWjE")) // Filter Option deleted
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("6jrfe")) // Filter Option deleted
                        .Generate(3)))
                .GenerateList(3);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Filter and ALL options unchanged
                .ForIndex(1, s =>
                    s.SetPublicId("O7CLF") // Filter changed
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[1].OptionLinks[0])) // Filter Option unchanged
                        .Generate(1)))
                .ForIndex(2, s =>
                    s.SetPublicId("7zXob") // Filter changed
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[2].OptionLinks[0])) // Filter Option unchanged
                        .Generate(1)))
                .GenerateList(3);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // 2 Filter changes
            Assert.Equal(2, filterMetaChanges.Count);
            Assert.All(filterMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            // 4 Filter Option changes
            Assert.Equal(4, filterOptionMetaChanges.Count);
            Assert.All(filterOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas
                .ToDictionary(
                    m => m.PublicId,
                    m => new { FilterMeta = m, OldFilterOptionMetas = m.OptionLinks.ToDictionary(l => l.PublicId) });

            var newFilterMetas = newVersion.FilterMetas
                .ToDictionary(
                    m => m.PublicId,
                    m => new { FilterMeta = m, NewFilterOptionMetas = m.OptionLinks.ToDictionary(l => l.PublicId) });

            // Filter changed
            AssertSingleFilterChanged(
                changes: filterMetaChanges,
                expectedOldFilterMeta: oldFilterMetas["O7CLF"].FilterMeta,
                expectedNewFilterMeta: newFilterMetas["O7CLF"].FilterMeta);

            // Filter changed
            AssertSingleFilterChanged(
                changes: filterMetaChanges,
                expectedOldFilterMeta: oldFilterMetas["7zXob"].FilterMeta,
                expectedNewFilterMeta: newFilterMetas["7zXob"].FilterMeta);

            // Filter Option deleted
            AssertSingleFilterOptionDeleted(filterOptionMetaChanges, oldFilterMetas["O7CLF"].OldFilterOptionMetas["pTSoj"]);

            // Filter Option deleted
            AssertSingleFilterOptionDeleted(filterOptionMetaChanges, oldFilterMetas["O7CLF"].OldFilterOptionMetas["IzBzg"]);

            // Filter Option deleted
            AssertSingleFilterOptionDeleted(filterOptionMetaChanges, oldFilterMetas["7zXob"].OldFilterOptionMetas["LxWjE"]);

            // Filter Option deleted
            AssertSingleFilterOptionDeleted(filterOptionMetaChanges, oldFilterMetas["7zXob"].OldFilterOptionMetas["6jrfe"]);
        }

        [Fact]
        public async Task FiltersChangedOptionsChanged_ChangesContainOnlyChangedFiltersAndChangedOptions()
        {
            var oldFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s =>
                    s.SetPublicId("dP0Zw")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("dP0Zw"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("O7CLF"))
                        .Generate(2)))
                .ForIndex(1, s =>
                    s.SetPublicId("O7CLF")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("7zXob"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("pTSoj"))
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("IzBzg"))
                        .Generate(3)))
                .ForIndex(2, s =>
                    s.SetPublicId("7zXob")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("it6Xr"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("LxWjE"))
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("6jrfe"))
                        .Generate(3)))
                .GenerateList(3);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Filter and ALL options unchanged
                .ForIndex(1, s =>
                    s.SetPublicId("O7CLF") // Filter changed
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[1].OptionLinks[0])) // Filter Option unchanged
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("pTSoj")) // Filter Option changed
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("IzBzg")) // Filter Option changed
                        .Generate(3)))
                .ForIndex(2, s =>
                    s.SetPublicId("7zXob") // Filter changed
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[2].OptionLinks[0])) // Filter Option unchanged
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("LxWjE")) // Filter Option changed
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("6jrfe")) // Filter Option changed
                        .Generate(3)))
                .GenerateList(3);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // 2 Filter changes
            Assert.Equal(2, filterMetaChanges.Count);
            Assert.All(filterMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            // 4 Filter Option changes
            Assert.Equal(4, filterOptionMetaChanges.Count);
            Assert.All(filterOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas
                .ToDictionary(
                    m => m.PublicId,
                    m => new { FilterMeta = m, OldFilterOptionMetas = m.OptionLinks.ToDictionary(l => l.PublicId) });

            var newFilterMetas = newVersion.FilterMetas
                .ToDictionary(
                    m => m.PublicId,
                    m => new { FilterMeta = m, NewFilterOptionMetas = m.OptionLinks.ToDictionary(l => l.PublicId) });

            // Filter changed
            AssertSingleFilterChanged(
                changes: filterMetaChanges,
                expectedOldFilterMeta: oldFilterMetas["O7CLF"].FilterMeta,
                expectedNewFilterMeta: newFilterMetas["O7CLF"].FilterMeta);

            // Filter changed
            AssertSingleFilterChanged(
                changes: filterMetaChanges,
                expectedOldFilterMeta: oldFilterMetas["7zXob"].FilterMeta,
                expectedNewFilterMeta: newFilterMetas["7zXob"].FilterMeta);

            // Filter Option changed
            AssertSingleFilterOptionChanged(
                changes: filterOptionMetaChanges,
                expectedOldOptionLink: oldFilterMetas["O7CLF"].OldFilterOptionMetas["pTSoj"],
                expectedNewOptionLink: newFilterMetas["O7CLF"].NewFilterOptionMetas["pTSoj"]);

            // Filter Option changed
            AssertSingleFilterOptionChanged(
                changes: filterOptionMetaChanges,
                expectedOldOptionLink: oldFilterMetas["O7CLF"].OldFilterOptionMetas["IzBzg"],
                expectedNewOptionLink: newFilterMetas["O7CLF"].NewFilterOptionMetas["IzBzg"]);

            // Filter Option changed
            AssertSingleFilterOptionChanged(
                changes: filterOptionMetaChanges,
                expectedOldOptionLink: oldFilterMetas["7zXob"].OldFilterOptionMetas["LxWjE"],
                expectedNewOptionLink: newFilterMetas["7zXob"].NewFilterOptionMetas["LxWjE"]);

            // Filter Option changed
            AssertSingleFilterOptionChanged(
                changes: filterOptionMetaChanges,
                expectedOldOptionLink: oldFilterMetas["7zXob"].OldFilterOptionMetas["6jrfe"],
                expectedNewOptionLink: newFilterMetas["7zXob"].NewFilterOptionMetas["6jrfe"]);
        }

        [Fact]
        public async Task FiltersChangedOptionsUnchanged_ChangesContainOnlyChangedFilters()
        {
            var oldFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s =>
                    s.SetPublicId("dP0Zw")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("dP0Zw"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("O7CLF"))
                        .Generate(2)))
                .ForIndex(1, s =>
                    s.SetPublicId("O7CLF")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("7zXob"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("pTSoj"))
                        .Generate(2)))
                .ForIndex(2, s =>
                    s.SetPublicId("7zXob")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("IzBzg"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("it6Xr"))
                        .Generate(2)))
                .GenerateList(3);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Filter and ALL options unchanged
                .ForIndex(1, s =>
                    s.SetPublicId("O7CLF") // Filter changed
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[1].OptionLinks[0])) // Filter Option unchanged
                        .ForIndex(1, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[1].OptionLinks[1])) // Filter Option unchanged
                        .Generate(2)))
                .ForIndex(2, s =>
                    s.SetPublicId("7zXob") // Filter changed
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[2].OptionLinks[0])) // Filter Option unchanged
                        .ForIndex(1, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[2].OptionLinks[1])) // Filter Option unchanged
                        .Generate(2)))
                .GenerateList(3);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // 2 Filter changes
            Assert.Equal(2, filterMetaChanges.Count);
            Assert.All(filterMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            // No Filter Option changes
            Assert.Empty(filterOptionMetaChanges);

            var oldFilterMetas = originalVersion.FilterMetas
                .ToDictionary(m => m.PublicId);

            var newFilterMetas = newVersion.FilterMetas
                .ToDictionary(m => m.PublicId);

            // Filter changed
            AssertSingleFilterChanged(
                changes: filterMetaChanges,
                expectedOldFilterMeta: oldFilterMetas["O7CLF"],
                expectedNewFilterMeta: newFilterMetas["O7CLF"]);

            // Filter changed
            AssertSingleFilterChanged(
                changes: filterMetaChanges,
                expectedOldFilterMeta: oldFilterMetas["7zXob"],
                expectedNewFilterMeta: newFilterMetas["7zXob"]);
        }

        [Fact]
        public async Task FiltersUnchangedOptionsAdded_ChangesContainOnlyAddedOptions()
        {
            var oldFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s =>
                    s.SetPublicId("dP0Zw")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("dP0Zw"))
                        .Generate(1)))
                .ForIndex(1, s =>
                    s.SetPublicId("O7CLF")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("O7CLF"))
                        .Generate(1)))
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(
                    filterMeta: oldFilterMeta[0], // Filter unchanged
                    newOptionLinks: () => DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[0].OptionLinks[0])) // Filter Option unchanged
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("7zXob")) // Filter Option added
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("pTSoj")) // Filter Option added
                        .Generate(3)))
                .ForIndex(1, UnchangedFilterMetaSetter(
                    filterMeta: oldFilterMeta[1], // Filter unchanged
                    newOptionLinks: () => DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[1].OptionLinks[0])) // Filter Option unchanged
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("IzBzg")) // Filter Option added
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("it6Xr")) // Filter Option added
                        .Generate(3)))
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // No Filter changes
            Assert.Empty(filterMetaChanges);

            // 4 Filter Option changes
            Assert.Equal(4, filterOptionMetaChanges.Count);
            Assert.All(filterOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var newFilterMetas = newVersion.FilterMetas
                .ToDictionary(
                    m => m.PublicId,
                    m => m.OptionLinks.ToDictionary(l => l.PublicId));

            // Filter Option added
            AssertSingleFilterOptionAdded(filterOptionMetaChanges, newFilterMetas["dP0Zw"]["7zXob"]);

            // Filter Option added
            AssertSingleFilterOptionAdded(filterOptionMetaChanges, newFilterMetas["dP0Zw"]["pTSoj"]);

            // Filter Option added
            AssertSingleFilterOptionAdded(filterOptionMetaChanges, newFilterMetas["O7CLF"]["IzBzg"]);

            // Filter Option added
            AssertSingleFilterOptionAdded(filterOptionMetaChanges, newFilterMetas["O7CLF"]["it6Xr"]);
        }

        [Fact]
        public async Task FiltersUnchangedOptionsDeleted_ChangesContainOnlyDeletedOptions()
        {
            var oldFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s =>
                    s.SetPublicId("dP0Zw")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("dP0Zw"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("O7CLF")) // Filter Option deleted
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("7zXob")) // Filter Option deleted
                        .Generate(3)))
                .ForIndex(1, s =>
                    s.SetPublicId("O7CLF")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("pTSoj"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("IzBzg")) // Filter Option deleted
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("it6Xr")) // Filter Option deleted
                        .Generate(3)))
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(
                    filterMeta: oldFilterMeta[0], // Filter unchanged
                    newOptionLinks: () => DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[0].OptionLinks[0])) // Filter Option unchanged
                        .Generate(1)))
                .ForIndex(1, UnchangedFilterMetaSetter(
                    filterMeta: oldFilterMeta[1], // Filter unchanged
                    newOptionLinks: () => DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[1].OptionLinks[0])) // Filter Option unchanged
                        .Generate(1)))
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // No Filter changes
            Assert.Empty(filterMetaChanges);

            // 4 Filter Option changes
            Assert.Equal(4, filterOptionMetaChanges.Count);
            Assert.All(filterOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas
                .ToDictionary(
                    m => m.PublicId,
                    m => m.OptionLinks.ToDictionary(l => l.PublicId));

            // Filter Option deleted
            AssertSingleFilterOptionDeleted(filterOptionMetaChanges, oldFilterMetas["dP0Zw"]["O7CLF"]);

            // Filter Option deleted
            AssertSingleFilterOptionDeleted(filterOptionMetaChanges, oldFilterMetas["dP0Zw"]["7zXob"]);

            // Filter Option deleted
            AssertSingleFilterOptionDeleted(filterOptionMetaChanges, oldFilterMetas["O7CLF"]["IzBzg"]);

            // Filter Option deleted
            AssertSingleFilterOptionDeleted(filterOptionMetaChanges, oldFilterMetas["O7CLF"]["it6Xr"]);
        }

        [Fact]
        public async Task FiltersUnchangedOptionsChanged_ChangesContainOnlyChangedOptions()
        {
            var oldFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s =>
                    s.SetPublicId("dP0Zw")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("dP0Zw"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("O7CLF"))
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("7zXob"))
                        .Generate(3)))
                .ForIndex(1, s =>
                    s.SetPublicId("O7CLF")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("pTSoj"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("IzBzg"))
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("it6Xr"))
                        .Generate(3)))
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(
                    filterMeta: oldFilterMeta[0], // Filter unchanged
                    newOptionLinks: () => DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[0].OptionLinks[0])) // Filter Option unchanged
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("O7CLF")) // Filter Option changed
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("7zXob")) // Filter Option changed
                        .Generate(3)))
                .ForIndex(1, UnchangedFilterMetaSetter(
                    filterMeta: oldFilterMeta[1], // Filter unchanged
                    newOptionLinks: () => DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[1].OptionLinks[0])) // Filter Option unchanged
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("IzBzg")) // Filter Option changed
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("it6Xr")) // Filter Option changed
                        .Generate(3)))
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // No Filter changes
            Assert.Empty(filterMetaChanges);

            // 4 Filter Option changes
            Assert.Equal(4, filterOptionMetaChanges.Count);
            Assert.All(filterOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas
                .ToDictionary(
                    m => m.PublicId,
                    m => m.OptionLinks.ToDictionary(l => l.PublicId));

            var newFilterMetas = newVersion.FilterMetas
                .ToDictionary(
                    m => m.PublicId,
                    m => m.OptionLinks.ToDictionary(l => l.PublicId));

            // Filter Option changed
            AssertSingleFilterOptionChanged(
                changes: filterOptionMetaChanges,
                expectedOldOptionLink: oldFilterMetas["dP0Zw"]["O7CLF"],
                expectedNewOptionLink: newFilterMetas["dP0Zw"]["O7CLF"]);

            // Filter Option changed
            AssertSingleFilterOptionChanged(
                changes: filterOptionMetaChanges,
                expectedOldOptionLink: oldFilterMetas["dP0Zw"]["7zXob"],
                expectedNewOptionLink: newFilterMetas["dP0Zw"]["7zXob"]);

            // Filter Option changed
            AssertSingleFilterOptionChanged(
                changes: filterOptionMetaChanges,
                expectedOldOptionLink: oldFilterMetas["O7CLF"]["IzBzg"],
                expectedNewOptionLink: newFilterMetas["O7CLF"]["IzBzg"]);

            // Filter Option changed
            AssertSingleFilterOptionChanged(
                changes: filterOptionMetaChanges,
                expectedOldOptionLink: oldFilterMetas["O7CLF"]["it6Xr"],
                expectedNewOptionLink: newFilterMetas["O7CLF"]["it6Xr"]);
        }

        [Fact]
        public async Task FiltersUnchangedOptionsUnchanged_ChangesAreEmpty()
        {
            var oldFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s => 
                    s.SetPublicId("dP0Zw")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s => 
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("dP0Zw"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("O7CLF"))
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("7zXob"))
                        .Generate(3)))
                .ForIndex(1, s =>
                    s.SetPublicId("O7CLF")
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("pTSoj"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("IzBzg"))
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("it6Xr"))
                        .Generate(3)))
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Filter and ALL options unchanged
                .ForIndex(1, UnchangedFilterMetaSetter(oldFilterMeta[1])) // Filter and ALL options unchanged
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // No Filter changes
            Assert.Empty(filterMetaChanges);

            // No Filter Option changes
            Assert.Empty(filterOptionMetaChanges);
        }

        [Fact]
        public async Task FiltersAddedAndDeletedAndChanged_ChangesInsertedIntoDatabaseInCorrectOrder()
        {
            var oldFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s =>
                    s.SetPublicId(SqidEncoder.Encode(1)) // Filter deleted
                    .SetLabel("f"))
                .ForIndex(1, s =>
                    s.SetPublicId(SqidEncoder.Encode(2)) // Filter deleted
                    .SetLabel("a"))
                .ForIndex(2, s =>
                    s.SetPublicId(SqidEncoder.Encode(3))
                    .SetLabel("e"))
                .ForIndex(3, s =>
                    s.SetPublicId(SqidEncoder.Encode(4))
                    .SetLabel("b"))
                .GenerateList(4);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s => s.SetPublicId(SqidEncoder.Encode(3))) // Filter changed
                .ForIndex(1, s => s.SetPublicId(SqidEncoder.Encode(4))) // Filter changed
                .ForIndex(2, s =>
                    s.SetPublicId(SqidEncoder.Encode(5)) // Filter added
                    .SetLabel("d"))
                .ForIndex(3, s =>
                    s.SetPublicId(SqidEncoder.Encode(6)) // Filter added
                    .SetLabel("c"))
                .GenerateList(4);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);

            // 6 Filter changes
            Assert.Equal(6, filterMetaChanges.Count);
            Assert.All(filterMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas
                .ToDictionary(m => m.PublicId);

            var newFilterMetas = newVersion.FilterMetas
                .ToDictionary(m => m.PublicId);

            // The changes should be inserted into each database table ordered alphabetically by 'Label'.
            // They should also be ordered such that all additions come last.

            // Therefore, the expected order of Filter changes are (as per their Public IDs):
            // Sqid index 2 deleted
            // Sqid index 4 changed
            // Sqid index 3 changed
            // Sqid index 1 deleted
            // Sqid index 6 added
            // Sqid index 5 added

            AssertFilterDeleted(
                expectedFilterMeta: oldFilterMetas[SqidEncoder.Encode(2)], 
                change: filterMetaChanges[0]);
            AssertFilterChanged(
                expectedOldFilterMeta: oldFilterMetas[SqidEncoder.Encode(4)],
                expectedNewFilterMeta: newFilterMetas[SqidEncoder.Encode(4)],
                change: filterMetaChanges[1]);
            AssertFilterChanged(
                expectedOldFilterMeta: oldFilterMetas[SqidEncoder.Encode(3)],
                expectedNewFilterMeta: newFilterMetas[SqidEncoder.Encode(3)],
                change: filterMetaChanges[2]);
            AssertFilterDeleted(
                expectedFilterMeta: oldFilterMetas[SqidEncoder.Encode(1)], 
                change: filterMetaChanges[3]);
            AssertFilterAdded(
                expectedFilterMeta: newFilterMetas[SqidEncoder.Encode(6)],
                change: filterMetaChanges[4]);
            AssertFilterAdded(
                expectedFilterMeta: newFilterMetas[SqidEncoder.Encode(5)],
                change: filterMetaChanges[5]);
        }

        [Fact]
        public async Task FiltersOptionsAddedAndDeletedAndChanged_ChangesInsertedIntoDatabaseInCorrectOrder()
        {
            var oldFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s =>
                    s.SetPublicId(SqidEncoder.Encode(1))
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(
                                DataFixture.DefaultFilterOptionMeta()
                                .WithLabel("d"))
                            .SetPublicId(SqidEncoder.Encode(1))) // Filter Option deleted
                        .ForIndex(1, s =>
                            s.SetOption(
                                DataFixture.DefaultFilterOptionMeta()
                                .WithLabel("a"))
                            .SetPublicId(SqidEncoder.Encode(2))) // Filter Option deleted
                        .ForIndex(2, s =>
                            s.SetOption(
                                DataFixture.DefaultFilterOptionMeta()
                                .WithLabel("b"))
                            .SetPublicId(SqidEncoder.Encode(3)))
                        .ForIndex(3, s =>
                            s.SetOption(
                                DataFixture.DefaultFilterOptionMeta()
                                .WithLabel("k"))
                            .SetPublicId(SqidEncoder.Encode(4)))
                        .Generate(4)))
                .ForIndex(1, s =>
                    s.SetPublicId(SqidEncoder.Encode(2))
                    .SetOptionLinks(() =>
                        DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(
                                DataFixture.DefaultFilterOptionMeta()
                                .WithLabel("c"))
                            .SetPublicId(SqidEncoder.Encode(5))) // Filter Option deleted
                        .ForIndex(1, s =>
                            s.SetOption(
                                DataFixture.DefaultFilterOptionMeta()
                                .WithLabel("h"))
                            .SetPublicId(SqidEncoder.Encode(6))) // Filter Option deleted
                        .ForIndex(2, s =>
                            s.SetOption(
                                DataFixture.DefaultFilterOptionMeta()
                                .WithLabel("f"))
                            .SetPublicId(SqidEncoder.Encode(7)))
                        .ForIndex(3, s =>
                            s.SetOption(
                                DataFixture.DefaultFilterOptionMeta()
                                .WithLabel("i"))
                            .SetPublicId(SqidEncoder.Encode(8)))
                        .Generate(4)))
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(
                    filterMeta: oldFilterMeta[0],
                    newOptionLinks: () => DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId(SqidEncoder.Encode(3))) // Filter Option changed
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId(SqidEncoder.Encode(4))) // Filter Option changed
                        .ForIndex(2, s =>
                            s.SetOption(
                                DataFixture.DefaultFilterOptionMeta()
                                .WithLabel("j"))
                            .SetPublicId(SqidEncoder.Encode(9))) // Filter Option added
                        .ForIndex(3, s =>
                            s.SetOption(
                                DataFixture.DefaultFilterOptionMeta()
                                .WithLabel("e"))
                            .SetPublicId(SqidEncoder.Encode(10))) // Filter Option added
                        .Generate(4)))
                .ForIndex(1, UnchangedFilterMetaSetter(
                    filterMeta: oldFilterMeta[1],
                    newOptionLinks: () => DataFixture.DefaultFilterOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId(SqidEncoder.Encode(7))) // Filter Option changed
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId(SqidEncoder.Encode(8))) // Filter Option changed
                        .ForIndex(2, s =>
                            s.SetOption(
                                DataFixture.DefaultFilterOptionMeta()
                                .WithLabel("g"))
                            .SetPublicId(SqidEncoder.Encode(11))) // Filter Option added
                        .ForIndex(3, s =>
                            s.SetOption(
                                DataFixture.DefaultFilterOptionMeta()
                                .WithLabel("l"))
                            .SetPublicId(SqidEncoder.Encode(12))) // Filter Option added
                        .Generate(4)))
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // 12 Filter Option changes
            Assert.Equal(12, filterOptionMetaChanges.Count);
            Assert.All(filterOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas
                .ToDictionary(
                    m => m.PublicId,
                    m => m.OptionLinks.ToDictionary(l => l.PublicId));

            var newFilterMetas = newVersion.FilterMetas
                .ToDictionary(
                    m => m.PublicId,
                    m => m.OptionLinks.ToDictionary(l => l.PublicId));

            // The changes should be inserted into each database table ordered alphabetically by 'Label'.
            // They should also be ordered such that all additions come last.

            // Therefore, the expected order of Filter Option changes are (as per their Public IDs):
            // Sqid index 2 in filter with Sqid index 1 deleted
            // Sqid index 3 in filter with Sqid index 1 changed
            // Sqid index 5 in filter with Sqid index 2 deleted
            // Sqid index 1 in filter with Sqid index 1 deleted
            // Sqid index 7 in filter with Sqid index 2 changed
            // Sqid index 6 in filter with Sqid index 2 deleted
            // Sqid index 8 in filter with Sqid index 2 changed
            // Sqid index 4 in filter with Sqid index 1 changed
            // Sqid index 10 in filter with Sqid index 1 added
            // Sqid index 11 in filter with Sqid index 2 added
            // Sqid index 9 in filter with Sqid index 1 added
            // Sqid index 12 in filter with Sqid index 2 added

            AssertFilterOptionDeleted(
                expectedOptionLink: oldFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(2)], 
                change: filterOptionMetaChanges[0]);
            AssertFilterOptionChanged(
                expectedOldOptionLink: oldFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(3)], 
                expectedNewOptionLink: newFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(3)], 
                change: filterOptionMetaChanges[1]);
            AssertFilterOptionDeleted(
                expectedOptionLink: oldFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(5)],
                change: filterOptionMetaChanges[2]);
            AssertFilterOptionDeleted(
                expectedOptionLink: oldFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(1)],
                change: filterOptionMetaChanges[3]);
            AssertFilterOptionChanged(
                expectedOldOptionLink: oldFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(7)],
                expectedNewOptionLink: newFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(7)],
                change: filterOptionMetaChanges[4]);
            AssertFilterOptionDeleted(
                expectedOptionLink: oldFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(6)],
                change: filterOptionMetaChanges[5]);
            AssertFilterOptionChanged(
                expectedOldOptionLink: oldFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(8)],
                expectedNewOptionLink: newFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(8)],
                change: filterOptionMetaChanges[6]);
            AssertFilterOptionChanged(
                expectedOldOptionLink: oldFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(4)],
                expectedNewOptionLink: newFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(4)],
                change: filterOptionMetaChanges[7]);
            AssertFilterOptionAdded(
                expectedOptionLink: newFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(10)],
                change: filterOptionMetaChanges[8]);
            AssertFilterOptionAdded(
                expectedOptionLink: newFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(11)],
                change: filterOptionMetaChanges[9]);
            AssertFilterOptionAdded(
                expectedOptionLink: newFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(9)],
                change: filterOptionMetaChanges[10]);
            AssertFilterOptionAdded(
                expectedOptionLink: newFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(12)],
                change: filterOptionMetaChanges[11]);
        }

        private static void AssertSingleFilterDeleted(IReadOnlyList<FilterMetaChange> changes, FilterMeta expectedFilterMeta)
        {
            Assert.Single(changes,
                c => c.PreviousStateId == expectedFilterMeta.Id
                     && c.CurrentStateId is null);
        }

        private static void AssertSingleFilterAdded(IReadOnlyList<FilterMetaChange> changes, FilterMeta expectedFilterMeta)
        {
            Assert.Single(changes,
                c => c.PreviousStateId is null
                     && c.CurrentStateId == expectedFilterMeta.Id);
        }

        private static void AssertSingleFilterChanged(
            IReadOnlyList<FilterMetaChange> changes,
            FilterMeta expectedOldFilterMeta,
            FilterMeta expectedNewFilterMeta)
        {
            Assert.Single(changes,
                c => c.PreviousStateId == expectedOldFilterMeta.Id
                     && c.CurrentStateId == expectedNewFilterMeta.Id);
        }

        private static void AssertFilterDeleted(FilterMeta expectedFilterMeta, FilterMetaChange change)
        {
            Assert.Equal(expectedFilterMeta.Id, change.PreviousStateId);
            Assert.Null(change.CurrentStateId);
        }

        private static void AssertFilterAdded(FilterMeta expectedFilterMeta, FilterMetaChange change)
        {
            Assert.Null(change.PreviousStateId);
            Assert.Equal(expectedFilterMeta.Id, change.CurrentStateId);
        }

        private static void AssertFilterChanged(
            FilterMeta expectedOldFilterMeta,
            FilterMeta expectedNewFilterMeta,
            FilterMetaChange change)
        {
            Assert.Equal(expectedOldFilterMeta.Id, change.PreviousStateId);
            Assert.Equal(expectedNewFilterMeta.Id, change.CurrentStateId);
        }

        private static void AssertSingleFilterOptionDeleted(IReadOnlyList<FilterOptionMetaChange> changes, FilterOptionMetaLink expectedOptionLink)
        {
            Assert.Single(changes,
                c => c.PreviousState!.PublicId == expectedOptionLink.PublicId
                     && c.PreviousState.MetaId == expectedOptionLink.MetaId
                     && c.PreviousState.OptionId == expectedOptionLink.OptionId
                     && c.CurrentState is null);
        }

        private static void AssertSingleFilterOptionAdded(IReadOnlyList<FilterOptionMetaChange> changes, FilterOptionMetaLink expectedOptionLink)
        {
            Assert.Single(changes,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == expectedOptionLink.PublicId
                     && c.CurrentState.MetaId == expectedOptionLink.MetaId
                     && c.CurrentState.OptionId == expectedOptionLink.OptionId);
        }

        private static void AssertSingleFilterOptionChanged(
            IReadOnlyList<FilterOptionMetaChange> changes,
            FilterOptionMetaLink expectedOldOptionLink,
            FilterOptionMetaLink expectedNewOptionLink)
        {
            Assert.Single(changes,
                c => c.PreviousState!.PublicId == expectedOldOptionLink.PublicId
                     && c.PreviousState.MetaId == expectedOldOptionLink.MetaId
                     && c.PreviousState.OptionId == expectedOldOptionLink.OptionId
                     && c.CurrentState!.PublicId == expectedNewOptionLink.PublicId
                     && c.CurrentState.MetaId == expectedNewOptionLink.MetaId
                     && c.CurrentState.OptionId == expectedNewOptionLink.OptionId);
        }

        private static void AssertFilterOptionDeleted(FilterOptionMetaLink expectedOptionLink, FilterOptionMetaChange change)
        {
            Assert.Equal(expectedOptionLink.PublicId, change.PreviousState!.PublicId);
            Assert.Equal(expectedOptionLink.MetaId, change.PreviousState.MetaId);
            Assert.Equal(expectedOptionLink.OptionId, change.PreviousState.OptionId);
            Assert.Null(change.CurrentState);
        }

        private static void AssertFilterOptionAdded(FilterOptionMetaLink expectedOptionLink, FilterOptionMetaChange change)
        {
            Assert.Null(change.PreviousState);
            Assert.Equal(expectedOptionLink.PublicId, change.CurrentState!.PublicId);
            Assert.Equal(expectedOptionLink.MetaId, change.CurrentState.MetaId);
            Assert.Equal(expectedOptionLink.OptionId, change.CurrentState.OptionId);
        }

        private static void AssertFilterOptionChanged(
            FilterOptionMetaLink expectedOldOptionLink, 
            FilterOptionMetaLink expectedNewOptionLink,
            FilterOptionMetaChange change)
        {
            Assert.Equal(expectedOldOptionLink.PublicId, change.PreviousState!.PublicId);
            Assert.Equal(expectedOldOptionLink.MetaId, change.PreviousState.MetaId);
            Assert.Equal(expectedOldOptionLink.OptionId, change.PreviousState.OptionId); 
            Assert.Equal(expectedNewOptionLink.PublicId, change.CurrentState!.PublicId);
            Assert.Equal(expectedNewOptionLink.MetaId, change.CurrentState.MetaId);
            Assert.Equal(expectedNewOptionLink.OptionId, change.CurrentState.OptionId);
        }

        private Action<InstanceSetters<FilterMeta>> UnchangedFilterMetaSetter(
            FilterMeta filterMeta,
            Func<IEnumerable<FilterOptionMetaLink>>? newOptionLinks = null)
        {
            return s =>
            {
                s.SetPublicId(filterMeta.PublicId);
                s.SetColumn(filterMeta.Column);
                s.SetLabel(filterMeta.Label);
                s.SetHint(filterMeta.Hint);

                newOptionLinks ??= () =>
                {
                    var newOptionLinks = new List<FilterOptionMetaLink>();

                    foreach (var oldOptionLink in filterMeta.OptionLinks)
                    {
                        FilterOptionMetaLink newOptionLink = DataFixture.DefaultFilterOptionMetaLink()
                            .ForInstance(UnchangedFilterOptionMetaLinkSetter(oldOptionLink));

                        newOptionLinks.Add(newOptionLink);
                    }

                    return newOptionLinks;
                };

                s.SetOptionLinks(newOptionLinks);
            };
        }

        private static Action<InstanceSetters<FilterOptionMetaLink>> UnchangedFilterOptionMetaLinkSetter(FilterOptionMetaLink filterOptionMetaLink)
        {
            return s =>
            {
                s.SetPublicId(filterOptionMetaLink.PublicId);
                s.SetOptionId(filterOptionMetaLink.OptionId);
            };
        }

        private async Task<IReadOnlyList<FilterMetaChange>> GetFilterMetaChanges(DataSetVersion version)
        {
            return await GetDbContext<PublicDataDbContext>()
                .FilterMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == version.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        private async Task<IReadOnlyList<FilterOptionMetaChange>> GetFilterOptionMetaChanges(DataSetVersion version)
        {
            return await GetDbContext<PublicDataDbContext>()
                .FilterOptionMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == version.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        private async Task<(DataSetVersion originalVersion, Guid instanceId)> CreateDataSetInitialVersion(
            List<FilterMeta> filterMeta)
        {
            return await CreateDataSetInitialVersion(
                dataSetStatus: DataSetStatus.Published,
                dataSetVersionStatus: DataSetVersionStatus.Published,
                importStage: DataSetVersionImportStage.Completing,
                meta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    FilterMetas = filterMeta
                });
        }

        private async Task<(DataSetVersion nextVersion, Guid instanceId)> CreateDataSetNextVersion(
            DataSetVersion originalVersion,
            List<FilterMeta> filterMeta)
        {
            return await CreateDataSetNextVersion(
                initialVersion: originalVersion,
                status: DataSetVersionStatus.Mapping,
                importStage: Stage.PreviousStage(),
                meta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    FilterMetas = filterMeta
                });
        }
    }

    public class CreateChangesLocationTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : CreateChangesTests(fixture)
    {
        [Fact]
        public async Task LocationsAdded_ChangesContainOnlyAddedLocationsAndAddedOptions()
        {
            var oldLocationMeta = DataFixture.DefaultLocationMeta()
                .ForIndex(0, s =>
                    s.SetLevel(GeographicLevel.LocalAuthority)
                    .SetOptionLinks(() =>
                        DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("dP0Zw"))
                        .Generate(1)))
                .GenerateList(1);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldLocationMeta);

            var newLocationMeta = DataFixture.DefaultLocationMeta()
                .ForIndex(0, UnchangedLocationMetaSetter(oldLocationMeta[0])) // Location and ALL options unchanged
                .ForIndex(1, s =>
                    s.SetLevel(GeographicLevel.School) // Location added
                    .SetOptionLinks(() =>
                        DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("O7CLF")) // Location Option added
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("7zXob")) // Location Option added
                        .Generate(2)))
                .ForIndex(2, s =>
                    s.SetLevel(GeographicLevel.RscRegion) // Location added
                    .SetOptionLinks(() =>
                        DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultLocationRscRegionOptionMeta())
                            .SetPublicId("pTSoj")) // Location Option added
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationRscRegionOptionMeta())
                            .SetPublicId("IzBzg")) // Location Option added
                        .Generate(2)))
                .GenerateList(3);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                locationMeta: newLocationMeta);

            await CreateChanges(instanceId);

            var locationMetaChanges = await GetLocationMetaChanges(newVersion);
            var locationOptionMetaChanges = await GetLocationOptionMetaChanges(newVersion);

            // 2 Location additions
            Assert.Equal(2, locationMetaChanges.Count);
            Assert.All(locationMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            // 4 Location Option additions
            Assert.Equal(4, locationOptionMetaChanges.Count);
            Assert.All(locationOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var newLocationMetas = newVersion.LocationMetas
                .ToDictionary(
                    m => m.Level,
                    m => new { LocationMeta = m, NewLocationOptionMetas = m.OptionLinks.ToDictionary(l => l.PublicId) });

            // Location added
            AssertSingleLocationAdded(locationMetaChanges, newLocationMetas[GeographicLevel.School].LocationMeta);

            // Location added
            AssertSingleLocationAdded(locationMetaChanges, newLocationMetas[GeographicLevel.RscRegion].LocationMeta);

            // Location Option added
            AssertSingleLocationOptionAdded(locationOptionMetaChanges, newLocationMetas[GeographicLevel.School].NewLocationOptionMetas["O7CLF"]);

            // Location Option added
            AssertSingleLocationOptionAdded(locationOptionMetaChanges, newLocationMetas[GeographicLevel.School].NewLocationOptionMetas["7zXob"]);

            // Location Option added
            AssertSingleLocationOptionAdded(locationOptionMetaChanges, newLocationMetas[GeographicLevel.RscRegion].NewLocationOptionMetas["pTSoj"]);

            // Location Option added
            AssertSingleLocationOptionAdded(locationOptionMetaChanges, newLocationMetas[GeographicLevel.RscRegion].NewLocationOptionMetas["IzBzg"]);
        }

        [Fact]
        public async Task LocationsDeleted_ChangesContainOnlyDeletedLocations()
        {
            var oldLocationMeta = DataFixture.DefaultLocationMeta()
                .ForIndex(0, s =>
                    s.SetLevel(GeographicLevel.LocalAuthority)
                    .SetOptionLinks(() =>
                        DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("dP0Zw"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("O7CLF"))
                        .Generate(2)))
                .ForIndex(1, s =>
                    s.SetLevel(GeographicLevel.School) // Location and ALL options deleted
                    .SetOptionLinks(() =>
                        DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("pTSoj"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("IzBzg"))
                        .Generate(2)))
                .ForIndex(2, s =>
                    s.SetLevel(GeographicLevel.RscRegion) // Location and ALL options deleted
                    .SetOptionLinks(() =>
                        DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultLocationRscRegionOptionMeta())
                            .SetPublicId("LxWjE"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationRscRegionOptionMeta())
                            .SetPublicId("6jrfe"))
                        .Generate(2)))
                .GenerateList(3);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldLocationMeta);

            var newLocationMeta = DataFixture.DefaultLocationMeta()
                .ForIndex(0, UnchangedLocationMetaSetter(oldLocationMeta[0])) // Location and ALL options unchanged
                .GenerateList(1);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                locationMeta: newLocationMeta);

            await CreateChanges(instanceId);

            var locationMetaChanges = await GetLocationMetaChanges(newVersion);
            var locationOptionMetaChanges = await GetLocationOptionMetaChanges(newVersion);

            // 2 Location deletions
            Assert.Equal(2, locationMetaChanges.Count);
            Assert.All(locationMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            // No Location Option deletions
            Assert.Empty(locationOptionMetaChanges);

            var oldLocationMetas = originalVersion.LocationMetas
                .ToDictionary(m => m.Level);

            // Location deleted
            AssertSingleLocationDeleted(locationMetaChanges, oldLocationMetas[GeographicLevel.School]);

            // Location deleted
            AssertSingleLocationDeleted(locationMetaChanges, oldLocationMetas[GeographicLevel.RscRegion]);
        }

        [Fact]
        public async Task LocationsUnchangedOptionsAdded_ChangesContainOnlyAddedOptions()
        {
            var oldLocationMeta = DataFixture.DefaultLocationMeta()
                .ForIndex(0, s =>
                    s.SetLevel(GeographicLevel.LocalAuthority)
                    .SetOptionLinks(() =>
                        DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("dP0Zw"))
                        .Generate(1)))
                .ForIndex(1, s =>
                    s.SetLevel(GeographicLevel.School)
                    .SetOptionLinks(() =>
                        DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("O7CLF"))
                        .Generate(1)))
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldLocationMeta);

            var newLocationMeta = DataFixture.DefaultLocationMeta()
                .ForIndex(0, UnchangedLocationMetaSetter(
                    locationMeta: oldLocationMeta[0], // Location unchanged
                    newOptionLinks: () => DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, UnchangedLocationOptionMetaLinkSetter(oldLocationMeta[0].OptionLinks[0])) // Location Option unchanged
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("7zXob")) // Location Option added
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("pTSoj")) // Location Option added
                        .Generate(3)))
                .ForIndex(1, UnchangedLocationMetaSetter(
                    locationMeta: oldLocationMeta[1], // Location unchanged
                    newOptionLinks: () => DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, UnchangedLocationOptionMetaLinkSetter(oldLocationMeta[1].OptionLinks[0])) // Location Option unchanged
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("IzBzg")) // Location Option added
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("it6Xr")) // Location Option added
                        .Generate(3)))
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                locationMeta: newLocationMeta);

            await CreateChanges(instanceId);

            var locationMetaChanges = await GetLocationMetaChanges(newVersion);
            var locationOptionMetaChanges = await GetLocationOptionMetaChanges(newVersion);

            // No Location changes
            Assert.Empty(locationMetaChanges);

            // 4 Location Option changes
            Assert.Equal(4, locationOptionMetaChanges.Count);
            Assert.All(locationOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var newLocationMetas = newVersion.LocationMetas
                .ToDictionary(
                    m => m.Level,
                    m => m.OptionLinks.ToDictionary(l => l.PublicId));

            // Location Option added
            AssertSingleLocationOptionAdded(locationOptionMetaChanges, newLocationMetas[GeographicLevel.LocalAuthority]["7zXob"]);

            // Location Option added
            AssertSingleLocationOptionAdded(locationOptionMetaChanges, newLocationMetas[GeographicLevel.LocalAuthority]["pTSoj"]);

            // Location Option added
            AssertSingleLocationOptionAdded(locationOptionMetaChanges, newLocationMetas[GeographicLevel.School]["IzBzg"]);

            // Location Option added
            AssertSingleLocationOptionAdded(locationOptionMetaChanges, newLocationMetas[GeographicLevel.School]["it6Xr"]);
        }

        [Fact]
        public async Task LocationsUnchangedOptionsDeleted_ChangesContainOnlyDeletedOptions()
        {
            var oldLocationMeta = DataFixture.DefaultLocationMeta()
                .ForIndex(0, s =>
                    s.SetLevel(GeographicLevel.LocalAuthority)
                    .SetOptionLinks(() =>
                        DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("dP0Zw"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("O7CLF")) // Location Option deleted
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("7zXob")) // Location Option deleted
                        .Generate(3)))
                .ForIndex(1, s =>
                    s.SetLevel(GeographicLevel.School)
                    .SetOptionLinks(() =>
                        DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("pTSoj"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("IzBzg")) // Location Option deleted
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("it6Xr")) // Location Option deleted
                        .Generate(3)))
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldLocationMeta);

            var newLocationMeta = DataFixture.DefaultLocationMeta()
                .ForIndex(0, UnchangedLocationMetaSetter(
                    locationMeta: oldLocationMeta[0], // Location unchanged
                    newOptionLinks: () => DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, UnchangedLocationOptionMetaLinkSetter(oldLocationMeta[0].OptionLinks[0])) // Location Option unchanged
                        .Generate(1)))
                .ForIndex(1, UnchangedLocationMetaSetter(
                    locationMeta: oldLocationMeta[1], // Location unchanged
                    newOptionLinks: () => DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, UnchangedLocationOptionMetaLinkSetter(oldLocationMeta[1].OptionLinks[0])) // Location Option unchanged
                        .Generate(1)))
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                locationMeta: newLocationMeta);

            await CreateChanges(instanceId);

            var locationMetaChanges = await GetLocationMetaChanges(newVersion);
            var locationOptionMetaChanges = await GetLocationOptionMetaChanges(newVersion);

            // No Location changes
            Assert.Empty(locationMetaChanges);

            // 4 Location Option changes
            Assert.Equal(4, locationOptionMetaChanges.Count);
            Assert.All(locationOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldLocationMetas = originalVersion.LocationMetas
                .ToDictionary(
                    m => m.Level,
                    m => m.OptionLinks.ToDictionary(l => l.PublicId));

            // Location Option deleted
            AssertSingleLocationOptionDeleted(locationOptionMetaChanges, oldLocationMetas[GeographicLevel.LocalAuthority]["O7CLF"]);

            // Location Option deleted
            AssertSingleLocationOptionDeleted(locationOptionMetaChanges, oldLocationMetas[GeographicLevel.LocalAuthority]["7zXob"]);

            // Location Option deleted
            AssertSingleLocationOptionDeleted(locationOptionMetaChanges, oldLocationMetas[GeographicLevel.School]["IzBzg"]);

            // Location Option deleted
            AssertSingleLocationOptionDeleted(locationOptionMetaChanges, oldLocationMetas[GeographicLevel.School]["it6Xr"]);
        }

        [Fact]
        public async Task LocationsUnchangedOptionsChanged_ChangesContainOnlyChangedOptions()
        {
            var oldLocationMeta = DataFixture.DefaultLocationMeta()
                .ForIndex(0, s =>
                    s.SetLevel(GeographicLevel.LocalAuthority)
                    .SetOptionLinks(() =>
                        DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("dP0Zw"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("O7CLF"))
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("7zXob"))
                        .Generate(3)))
                .ForIndex(1, s =>
                    s.SetLevel(GeographicLevel.School)
                    .SetOptionLinks(() =>
                        DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("pTSoj"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("IzBzg"))
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("it6Xr"))
                        .Generate(3)))
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldLocationMeta);

            var newLocationMeta = DataFixture.DefaultLocationMeta()
                .ForIndex(0, UnchangedLocationMetaSetter(
                    locationMeta: oldLocationMeta[0], // Location unchanged
                    newOptionLinks: () => DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, UnchangedLocationOptionMetaLinkSetter(oldLocationMeta[0].OptionLinks[0])) // Location Option unchanged
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("O7CLF")) // Location Option changed
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("7zXob")) // Location Option changed
                        .Generate(3)))
                .ForIndex(1, UnchangedLocationMetaSetter(
                    locationMeta: oldLocationMeta[1], // Location unchanged
                    newOptionLinks: () => DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, UnchangedLocationOptionMetaLinkSetter(oldLocationMeta[1].OptionLinks[0])) // Location Option unchanged
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("IzBzg")) // Location Option changed
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("it6Xr")) // Location Option changed
                        .Generate(3)))
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                locationMeta: newLocationMeta);

            await CreateChanges(instanceId);

            var locationMetaChanges = await GetLocationMetaChanges(newVersion);
            var locationOptionMetaChanges = await GetLocationOptionMetaChanges(newVersion);

            // No Location changes
            Assert.Empty(locationMetaChanges);

            // 4 Location Option changes
            Assert.Equal(4, locationOptionMetaChanges.Count);
            Assert.All(locationOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldLocationMetas = originalVersion.LocationMetas
                .ToDictionary(
                    m => m.Level,
                    m => m.OptionLinks.ToDictionary(l => l.PublicId));

            var newLocationMetas = newVersion.LocationMetas
                .ToDictionary(
                    m => m.Level,
                    m => m.OptionLinks.ToDictionary(l => l.PublicId));

            // Location Option changed
            AssertSingleLocationOptionChanged(
                changes: locationOptionMetaChanges,
                expectedOldOptionLink: oldLocationMetas[GeographicLevel.LocalAuthority]["O7CLF"],
                expectedNewOptionLink: newLocationMetas[GeographicLevel.LocalAuthority]["O7CLF"]);

            // Location Option changed
            AssertSingleLocationOptionChanged(
                changes: locationOptionMetaChanges,
                expectedOldOptionLink: oldLocationMetas[GeographicLevel.LocalAuthority]["7zXob"],
                expectedNewOptionLink: newLocationMetas[GeographicLevel.LocalAuthority]["7zXob"]);

            // Location Option changed
            AssertSingleLocationOptionChanged(
                changes: locationOptionMetaChanges,
                expectedOldOptionLink: oldLocationMetas[GeographicLevel.School]["IzBzg"],
                expectedNewOptionLink: newLocationMetas[GeographicLevel.School]["IzBzg"]);

            // Location Option changed
            AssertSingleLocationOptionChanged(
                changes: locationOptionMetaChanges,
                expectedOldOptionLink: oldLocationMetas[GeographicLevel.School]["it6Xr"],
                expectedNewOptionLink: newLocationMetas[GeographicLevel.School]["it6Xr"]);
        }

        [Fact]
        public async Task LocationsUnchangedOptionsUnchanged_ChangesAreEmpty()
        {
            var oldLocationMeta = DataFixture.DefaultLocationMeta()
                .ForIndex(0, s =>
                    s.SetLevel(GeographicLevel.LocalAuthority)
                    .SetOptionLinks(() =>
                        DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("dP0Zw"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("O7CLF"))
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                            .SetPublicId("7zXob"))
                        .Generate(3)))
                .ForIndex(1, s =>
                    s.SetLevel(GeographicLevel.School)
                    .SetOptionLinks(() =>
                        DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("pTSoj"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("IzBzg"))
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                            .SetPublicId("it6Xr"))
                        .Generate(3)))
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldLocationMeta);

            var newLocationMeta = DataFixture.DefaultLocationMeta()
                .ForIndex(0, UnchangedLocationMetaSetter(oldLocationMeta[0])) // Location and ALL options unchanged
                .ForIndex(1, UnchangedLocationMetaSetter(oldLocationMeta[1])) // Location and ALL options unchanged
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                locationMeta: newLocationMeta);

            await CreateChanges(instanceId);

            var locationMetaChanges = await GetLocationMetaChanges(newVersion);
            var locationOptionMetaChanges = await GetLocationOptionMetaChanges(newVersion);

            // No Location changes
            Assert.Empty(locationMetaChanges);

            // No Location Option changes
            Assert.Empty(locationOptionMetaChanges);
        }

        [Fact]
        public async Task LocationsAddedAndDeleted_ChangesInsertedIntoDatabaseInCorrectOrder()
        {
            var oldLocationMeta = DataFixture.DefaultLocationMeta()
                .ForIndex(0, s => s.SetLevel(GeographicLevel.School)) // Location deleted
                .ForIndex(1, s => s.SetLevel(GeographicLevel.LocalAuthority)) // Location deleted
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldLocationMeta);

            var newLocationMeta = DataFixture.DefaultLocationMeta()
                .ForIndex(0, s => s.SetLevel(GeographicLevel.RscRegion)) // Location added
                .ForIndex(1, s => s.SetLevel(GeographicLevel.Provider)) // Location added
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                locationMeta: newLocationMeta);

            await CreateChanges(instanceId);

            var locationMetaChanges = await GetLocationMetaChanges(newVersion);

            // 4 Location changes
            Assert.Equal(4, locationMetaChanges.Count);
            Assert.All(locationMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldLocationMetas = originalVersion.LocationMetas
                .ToDictionary(m => m.Level);

            var newLocationMetas = newVersion.LocationMetas
                .ToDictionary(m => m.Level);

            // The changes should be inserted into each database table ordered alphabetically by 'Level'.
            // They should also be ordered such that all additions come last.

            // Therefore, the expected order of Location changes are (as per their Geographic Level):
            // LocalAuthority deleted
            // School deleted
            // Provider added
            // RscRegion added

            AssertLocationDeleted(
                expectedLocationMeta: oldLocationMetas[GeographicLevel.LocalAuthority],
                change: locationMetaChanges[0]);
            AssertLocationDeleted(
                expectedLocationMeta: oldLocationMetas[GeographicLevel.School],
                change: locationMetaChanges[1]);
            AssertLocationAdded(
                expectedLocationMeta: newLocationMetas[GeographicLevel.Provider],
                change: locationMetaChanges[2]);
            AssertLocationAdded(
                expectedLocationMeta: newLocationMetas[GeographicLevel.RscRegion],
                change: locationMetaChanges[3]);
        }

        [Fact]
        public async Task LocationsOptionsAddedAndDeletedAndChanged_ChangesInsertedIntoDatabaseInCorrectOrder()
        {
            var oldLocationMeta = DataFixture.DefaultLocationMeta()
                .ForIndex(0, s =>
                    s.SetLevel(GeographicLevel.EnglishDevolvedArea)
                    .SetOptionLinks(() =>
                        DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(
                                DataFixture.DefaultLocationCodedOptionMeta()
                                .WithLabel("d"))
                            .SetPublicId(SqidEncoder.Encode(1))) // Location Option deleted
                        .ForIndex(1, s =>
                            s.SetOption(
                                DataFixture.DefaultLocationCodedOptionMeta()
                                .WithLabel("a"))
                            .SetPublicId(SqidEncoder.Encode(2))) // Location Option deleted
                        .ForIndex(2, s =>
                            s.SetOption(
                                DataFixture.DefaultLocationCodedOptionMeta()
                                .WithLabel("b"))
                            .SetPublicId(SqidEncoder.Encode(3)))
                        .ForIndex(3, s =>
                            s.SetOption(
                                DataFixture.DefaultLocationCodedOptionMeta()
                                .WithLabel("k"))
                            .SetPublicId(SqidEncoder.Encode(4)))
                        .Generate(4)))
                .ForIndex(1, s =>
                    s.SetLevel(GeographicLevel.Country)
                    .SetOptionLinks(() =>
                        DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(
                                DataFixture.DefaultLocationCodedOptionMeta()
                                .WithLabel("c"))
                            .SetPublicId(SqidEncoder.Encode(5))) // Location Option deleted
                        .ForIndex(1, s =>
                            s.SetOption(
                                DataFixture.DefaultLocationCodedOptionMeta()
                                .WithLabel("h"))
                            .SetPublicId(SqidEncoder.Encode(6))) // Location Option deleted
                        .ForIndex(2, s =>
                            s.SetOption(
                                DataFixture.DefaultLocationCodedOptionMeta()
                                .WithLabel("f"))
                            .SetPublicId(SqidEncoder.Encode(7)))
                        .ForIndex(3, s =>
                            s.SetOption(
                                DataFixture.DefaultLocationCodedOptionMeta()
                                .WithLabel("i"))
                            .SetPublicId(SqidEncoder.Encode(8)))
                        .Generate(4)))
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldLocationMeta);

            var newLocationMeta = DataFixture.DefaultLocationMeta()
                .ForIndex(0, UnchangedLocationMetaSetter(
                    locationMeta: oldLocationMeta[0],
                    newOptionLinks: () => DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultLocationCodedOptionMeta())
                            .SetPublicId(SqidEncoder.Encode(3))) // Location Option changed
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationCodedOptionMeta())
                            .SetPublicId(SqidEncoder.Encode(4))) // Location Option changed
                        .ForIndex(2, s =>
                            s.SetOption(
                                DataFixture.DefaultLocationCodedOptionMeta()
                                .WithLabel("j"))
                            .SetPublicId(SqidEncoder.Encode(9))) // Location Option added
                        .ForIndex(3, s =>
                            s.SetOption(
                                DataFixture.DefaultLocationCodedOptionMeta()
                                .WithLabel("e"))
                            .SetPublicId(SqidEncoder.Encode(10))) // Location Option added
                        .Generate(4)))
                .ForIndex(1, UnchangedLocationMetaSetter(
                    locationMeta: oldLocationMeta[1],
                    newOptionLinks: () => DataFixture.DefaultLocationOptionMetaLink()
                        .ForIndex(0, s =>
                            s.SetOption(DataFixture.DefaultLocationCodedOptionMeta())
                            .SetPublicId(SqidEncoder.Encode(7))) // Location Option changed
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultLocationCodedOptionMeta())
                            .SetPublicId(SqidEncoder.Encode(8))) // Location Option changed
                        .ForIndex(2, s =>
                            s.SetOption(
                                DataFixture.DefaultLocationCodedOptionMeta()
                                .WithLabel("g"))
                            .SetPublicId(SqidEncoder.Encode(11))) // Location Option added
                        .ForIndex(3, s =>
                            s.SetOption(
                                DataFixture.DefaultLocationCodedOptionMeta()
                                .WithLabel("l"))
                            .SetPublicId(SqidEncoder.Encode(12))) // Location Option added
                        .Generate(4)))
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                locationMeta: newLocationMeta);

            await CreateChanges(instanceId);

            var locationOptionMetaChanges = await GetLocationOptionMetaChanges(newVersion);

            // 12 Location Option changes
            Assert.Equal(12, locationOptionMetaChanges.Count);
            Assert.All(locationOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldLocationMetas = originalVersion.LocationMetas
                .ToDictionary(
                    m => m.Level,
                    m => m.OptionLinks.ToDictionary(l => l.PublicId));

            var newLocationMetas = newVersion.LocationMetas
                .ToDictionary(
                    m => m.Level,
                    m => m.OptionLinks.ToDictionary(l => l.PublicId));

            // The changes should be inserted into each database table ordered alphabetically by 'Label'.
            // They should also be ordered such that all additions come last.

            // Therefore, the expected order of Location Option changes are (as per their Public IDs):
            // Sqid index 2 in Location with Level EnglishDevolvedArea deleted
            // Sqid index 3 in Location with Level EnglishDevolvedArea changed
            // Sqid index 5 in Location with Level Country deleted
            // Sqid index 1 in Location with Level EnglishDevolvedArea deleted
            // Sqid index 7 in Location with Level Country changed
            // Sqid index 6 in Location with Level Country deleted
            // Sqid index 8 in Location with Level Country changed
            // Sqid index 4 in Location with Level EnglishDevolvedArea changed
            // Sqid index 10 in Location with Level EnglishDevolvedArea added
            // Sqid index 11 in Location with Level Country added
            // Sqid index 9 in Location with Level EnglishDevolvedArea added
            // Sqid index 12 in Location with Level Country added

            AssertLocationOptionDeleted(
                expectedOptionLink: oldLocationMetas[GeographicLevel.EnglishDevolvedArea][SqidEncoder.Encode(2)],
                change: locationOptionMetaChanges[0]);
            AssertLocationOptionChanged(
                expectedOldOptionLink: oldLocationMetas[GeographicLevel.EnglishDevolvedArea][SqidEncoder.Encode(3)],
                expectedNewOptionLink: newLocationMetas[GeographicLevel.EnglishDevolvedArea][SqidEncoder.Encode(3)],
                change: locationOptionMetaChanges[1]);
            AssertLocationOptionDeleted(
                expectedOptionLink: oldLocationMetas[GeographicLevel.Country][SqidEncoder.Encode(5)],
                change: locationOptionMetaChanges[2]);
            AssertLocationOptionDeleted(
                expectedOptionLink: oldLocationMetas[GeographicLevel.EnglishDevolvedArea][SqidEncoder.Encode(1)],
                change: locationOptionMetaChanges[3]);
            AssertLocationOptionChanged(
                expectedOldOptionLink: oldLocationMetas[GeographicLevel.Country][SqidEncoder.Encode(7)],
                expectedNewOptionLink: newLocationMetas[GeographicLevel.Country][SqidEncoder.Encode(7)],
                change: locationOptionMetaChanges[4]);
            AssertLocationOptionDeleted(
                expectedOptionLink: oldLocationMetas[GeographicLevel.Country][SqidEncoder.Encode(6)],
                change: locationOptionMetaChanges[5]);
            AssertLocationOptionChanged(
                expectedOldOptionLink: oldLocationMetas[GeographicLevel.Country][SqidEncoder.Encode(8)],
                expectedNewOptionLink: newLocationMetas[GeographicLevel.Country][SqidEncoder.Encode(8)],
                change: locationOptionMetaChanges[6]);
            AssertLocationOptionChanged(
                expectedOldOptionLink: oldLocationMetas[GeographicLevel.EnglishDevolvedArea][SqidEncoder.Encode(4)],
                expectedNewOptionLink: newLocationMetas[GeographicLevel.EnglishDevolvedArea][SqidEncoder.Encode(4)],
                change: locationOptionMetaChanges[7]);
            AssertLocationOptionAdded(
                expectedOptionLink: newLocationMetas[GeographicLevel.EnglishDevolvedArea][SqidEncoder.Encode(10)],
                change: locationOptionMetaChanges[8]);
            AssertLocationOptionAdded(
                expectedOptionLink: newLocationMetas[GeographicLevel.Country][SqidEncoder.Encode(11)],
                change: locationOptionMetaChanges[9]);
            AssertLocationOptionAdded(
                expectedOptionLink: newLocationMetas[GeographicLevel.EnglishDevolvedArea][SqidEncoder.Encode(9)],
                change: locationOptionMetaChanges[10]);
            AssertLocationOptionAdded(
                expectedOptionLink: newLocationMetas[GeographicLevel.Country][SqidEncoder.Encode(12)],
                change: locationOptionMetaChanges[11]);
        }

        private static void AssertSingleLocationDeleted(IReadOnlyList<LocationMetaChange> changes, LocationMeta expectedLocationMeta)
        {
            Assert.Single(changes,
                c => c.PreviousStateId == expectedLocationMeta.Id
                     && c.CurrentStateId is null);
        }

        private static void AssertSingleLocationAdded(IReadOnlyList<LocationMetaChange> changes, LocationMeta expectedLocationMeta)
        {
            Assert.Single(changes,
                c => c.PreviousStateId is null
                     && c.CurrentStateId == expectedLocationMeta.Id);
        }

        private static void AssertSingleLocationChanged(
            IReadOnlyList<LocationMetaChange> changes,
            LocationMeta expectedOldLocationMeta,
            LocationMeta expectedNewLocationMeta)
        {
            Assert.Single(changes,
                c => c.PreviousStateId == expectedOldLocationMeta.Id
                     && c.CurrentStateId == expectedNewLocationMeta.Id);
        }

        private static void AssertLocationDeleted(LocationMeta expectedLocationMeta, LocationMetaChange change)
        {
            Assert.Equal(expectedLocationMeta.Id, change.PreviousStateId);
            Assert.Null(change.CurrentStateId);
        }

        private static void AssertLocationAdded(LocationMeta expectedLocationMeta, LocationMetaChange change)
        {
            Assert.Null(change.PreviousStateId);
            Assert.Equal(expectedLocationMeta.Id, change.CurrentStateId);
        }

        private static void AssertSingleLocationOptionDeleted(IReadOnlyList<LocationOptionMetaChange> changes, LocationOptionMetaLink expectedOptionLink)
        {
            Assert.Single(changes,
                c => c.PreviousState!.PublicId == expectedOptionLink.PublicId
                     && c.PreviousState.MetaId == expectedOptionLink.MetaId
                     && c.PreviousState.OptionId == expectedOptionLink.OptionId
                     && c.CurrentState is null);
        }

        private static void AssertSingleLocationOptionAdded(IReadOnlyList<LocationOptionMetaChange> changes, LocationOptionMetaLink expectedOptionLink)
        {
            Assert.Single(changes,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == expectedOptionLink.PublicId
                     && c.CurrentState.MetaId == expectedOptionLink.MetaId
                     && c.CurrentState.OptionId == expectedOptionLink.OptionId);
        }

        private static void AssertSingleLocationOptionChanged(
            IReadOnlyList<LocationOptionMetaChange> changes,
            LocationOptionMetaLink expectedOldOptionLink,
            LocationOptionMetaLink expectedNewOptionLink)
        {
            Assert.Single(changes,
                c => c.PreviousState!.PublicId == expectedOldOptionLink.PublicId
                     && c.PreviousState.MetaId == expectedOldOptionLink.MetaId
                     && c.PreviousState.OptionId == expectedOldOptionLink.OptionId
                     && c.CurrentState!.PublicId == expectedNewOptionLink.PublicId
                     && c.CurrentState.MetaId == expectedNewOptionLink.MetaId
                     && c.CurrentState.OptionId == expectedNewOptionLink.OptionId);
        }

        private static void AssertLocationOptionDeleted(LocationOptionMetaLink expectedOptionLink, LocationOptionMetaChange change)
        {
            Assert.Equal(expectedOptionLink.PublicId, change.PreviousState!.PublicId);
            Assert.Equal(expectedOptionLink.MetaId, change.PreviousState.MetaId);
            Assert.Equal(expectedOptionLink.OptionId, change.PreviousState.OptionId);
            Assert.Null(change.CurrentState);
        }

        private static void AssertLocationOptionAdded(LocationOptionMetaLink expectedOptionLink, LocationOptionMetaChange change)
        {
            Assert.Null(change.PreviousState);
            Assert.Equal(expectedOptionLink.PublicId, change.CurrentState!.PublicId);
            Assert.Equal(expectedOptionLink.MetaId, change.CurrentState.MetaId);
            Assert.Equal(expectedOptionLink.OptionId, change.CurrentState.OptionId);
        }

        private static void AssertLocationOptionChanged(
            LocationOptionMetaLink expectedOldOptionLink,
            LocationOptionMetaLink expectedNewOptionLink,
            LocationOptionMetaChange change)
        {
            Assert.Equal(expectedOldOptionLink.PublicId, change.PreviousState!.PublicId);
            Assert.Equal(expectedOldOptionLink.MetaId, change.PreviousState.MetaId);
            Assert.Equal(expectedOldOptionLink.OptionId, change.PreviousState.OptionId);
            Assert.Equal(expectedNewOptionLink.PublicId, change.CurrentState!.PublicId);
            Assert.Equal(expectedNewOptionLink.MetaId, change.CurrentState.MetaId);
            Assert.Equal(expectedNewOptionLink.OptionId, change.CurrentState.OptionId);
        }

        private Action<InstanceSetters<LocationMeta>> UnchangedLocationMetaSetter(
            LocationMeta locationMeta,
            Func<IEnumerable<LocationOptionMetaLink>>? newOptionLinks = null)
        {
            return s =>
            {
                s.SetLevel(locationMeta.Level);

                newOptionLinks ??= () =>
                {
                    var newOptionLinks = new List<LocationOptionMetaLink>();

                    foreach (var oldOptionLink in locationMeta.OptionLinks)
                    {
                        LocationOptionMetaLink newOptionLink = DataFixture.DefaultLocationOptionMetaLink()
                            .ForInstance(UnchangedLocationOptionMetaLinkSetter(oldOptionLink));

                        newOptionLinks.Add(newOptionLink);
                    }

                    return newOptionLinks;
                };

                s.SetOptionLinks(newOptionLinks);
            };
        }

        private static Action<InstanceSetters<LocationOptionMetaLink>> UnchangedLocationOptionMetaLinkSetter(LocationOptionMetaLink locationOptionMetaLink)
        {
            return s =>
            {
                s.SetPublicId(locationOptionMetaLink.PublicId);
                s.SetOptionId(locationOptionMetaLink.OptionId);
            };
        }

        private async Task<IReadOnlyList<LocationMetaChange>> GetLocationMetaChanges(DataSetVersion version)
        {
            return await GetDbContext<PublicDataDbContext>()
                .LocationMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == version.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        private async Task<IReadOnlyList<LocationOptionMetaChange>> GetLocationOptionMetaChanges(DataSetVersion version)
        {
            return await GetDbContext<PublicDataDbContext>()
                .LocationOptionMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == version.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        private async Task<(DataSetVersion originalVersion, Guid instanceId)> CreateDataSetInitialVersion(
            List<LocationMeta> locationMeta)
        {
            return await CreateDataSetInitialVersion(
                dataSetStatus: DataSetStatus.Published,
                dataSetVersionStatus: DataSetVersionStatus.Published,
                importStage: DataSetVersionImportStage.Completing,
                meta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    LocationMetas = locationMeta
                });
        }

        private async Task<(DataSetVersion nextVersion, Guid instanceId)> CreateDataSetNextVersion(
            DataSetVersion originalVersion,
            List<LocationMeta> locationMeta)
        {
            return await CreateDataSetNextVersion(
                initialVersion: originalVersion,
                status: DataSetVersionStatus.Mapping,
                importStage: Stage.PreviousStage(),
                meta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    LocationMetas = locationMeta
                });
        }
    }

    public class CreateChangesGeographicLevelTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : CreateChangesTests(fixture)
    {
        [Fact]
        public async Task GeographicLevelsAddedAndDeleted_ChangeExists()
        {
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta()
                        .WithLevels([
                            GeographicLevel.Country,
                            GeographicLevel.Region,
                            GeographicLevel.LocalAuthority
                        ])
                },
                nextVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta()
                        .WithLevels([
                            GeographicLevel.LocalAuthority,
                            GeographicLevel.LocalAuthorityDistrict,
                            GeographicLevel.School
                        ])
                });

            await CreateChanges(instanceId);

            var actualChange = await GetDbContext<PublicDataDbContext>()
                .GeographicLevelMetaChanges
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.DataSetVersionId == newVersion.Id);

            Assert.NotNull(actualChange);
            Assert.Equal(newVersion.Id, actualChange.DataSetVersionId);
            Assert.Equal(originalVersion.GeographicLevelMeta!.Id, actualChange.PreviousStateId);
            Assert.Equal(newVersion.GeographicLevelMeta!.Id, actualChange.CurrentStateId);
        }

        [Fact]
        public async Task GeographicLevelsUnchanged_ChangeIsNull()
        {
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta()
                        .WithLevels([
                            GeographicLevel.Country,
                            GeographicLevel.Region,
                            GeographicLevel.LocalAuthority
                        ])
                },
                nextVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta()
                        .WithLevels([
                            GeographicLevel.Country,
                            GeographicLevel.Region,
                            GeographicLevel.LocalAuthority
                        ])
                });

            await CreateChanges(instanceId);

            var actualChange = await GetDbContext<PublicDataDbContext>()
                .GeographicLevelMetaChanges
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.DataSetVersionId == newVersion.Id);

            Assert.Null(actualChange);
        }

        [Fact]
        public async Task GeographicLevelsAdded_ChangeExists()
        {
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta()
                        .WithLevels([
                            GeographicLevel.Country
                        ])
                },
                nextVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta()
                        .WithLevels([
                            GeographicLevel.Country,
                            GeographicLevel.Region,
                            GeographicLevel.LocalAuthority
                        ])
                });

            await CreateChanges(instanceId);

            var actualChange = await GetDbContext<PublicDataDbContext>()
                .GeographicLevelMetaChanges
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.DataSetVersionId == newVersion.Id);

            Assert.NotNull(actualChange);
            Assert.Equal(newVersion.Id, actualChange.DataSetVersionId);
            Assert.Equal(originalVersion.GeographicLevelMeta!.Id, actualChange.PreviousStateId);
            Assert.Equal(newVersion.GeographicLevelMeta!.Id, actualChange.CurrentStateId);
        }

        [Fact]
        public async Task GeographicLevelsDeleted_ChangeExists()
        {
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta()
                        .WithLevels([
                            GeographicLevel.Country,
                            GeographicLevel.Region,
                            GeographicLevel.LocalAuthority
                        ])
                },
                nextVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta()
                        .WithLevels([
                            GeographicLevel.Country
                        ])
                });

            await CreateChanges(instanceId);

            var actualChange = await GetDbContext<PublicDataDbContext>()
                .GeographicLevelMetaChanges
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.DataSetVersionId == newVersion.Id);

            Assert.NotNull(actualChange);
            Assert.Equal(newVersion.Id, actualChange.DataSetVersionId);
            Assert.Equal(originalVersion.GeographicLevelMeta!.Id, actualChange.PreviousStateId);
            Assert.Equal(newVersion.GeographicLevelMeta!.Id, actualChange.CurrentStateId);
        }
    }

    public class CreateChangesIndicatorTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : CreateChangesTests(fixture)
    {
        [Fact]
        public async Task IndicatorsAddedAndDeletedAndChanged_ChangesContainAddedAndDeletedAndChangedIndicators()
        {
            var oldIndicatorMeta = DataFixture.DefaultIndicatorMeta()
                .ForIndex(0, s => s.SetPublicId("dP0Zw"))
                .ForIndex(1, s => s.SetPublicId("O7CLF")) // Indicator deleted
                .ForIndex(2, s => s.SetPublicId("7zXob")) // Indicator deleted
                .ForIndex(3, s => s.SetPublicId("pTSoj"))
                .ForIndex(4, s => s.SetPublicId("IzBzg"))
                .GenerateList(5);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldIndicatorMeta);

            var newIndicatorMeta = DataFixture.DefaultIndicatorMeta()
                .ForIndex(0, UnchangedIndicatorMetaSetter(oldIndicatorMeta[0])) // Indicator unchanged
                .ForIndex(1, s => s.SetPublicId("pTSoj")) // Indicator changed
                .ForIndex(2, s => s.SetPublicId("IzBzg")) // Indicator changed
                .ForIndex(3, s => s.SetPublicId("it6Xr")) // Indicator added
                .ForIndex(4, s => s.SetPublicId("LxWjE")) // Indicator added
                .GenerateList(5);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                indicatorMeta: newIndicatorMeta);

            await CreateChanges(instanceId);

            var changes = await GetIndicatorMetaChanges(newVersion);

            // 2 Indicator changes
            Assert.Equal(6, changes.Count);
            Assert.All(changes, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldIndicatorMetas = originalVersion.IndicatorMetas
                .ToDictionary(m => m.PublicId);

            var newIndicatorMetas = newVersion.IndicatorMetas
                .ToDictionary(m => m.PublicId);

            // Indicator deleted
            AssertSingleIndicatorDeleted(changes, oldIndicatorMetas["O7CLF"]);

            // Indicator deleted
            AssertSingleIndicatorDeleted(changes, oldIndicatorMetas["7zXob"]);

            // Indicator changed
            AssertSingleIndicatorChanged(
                changes: changes,
                expectedOldIndicatorMeta: oldIndicatorMetas["pTSoj"],
                expectedNewIndicatorMeta: newIndicatorMetas["pTSoj"]);

            // Indicator changed
            AssertSingleIndicatorChanged(
                changes: changes,
                expectedOldIndicatorMeta: oldIndicatorMetas["IzBzg"],
                expectedNewIndicatorMeta: newIndicatorMetas["IzBzg"]);

            // Indicator added
            AssertSingleIndicatorAdded(changes, newIndicatorMetas["it6Xr"]);

            // Indicator added
            AssertSingleIndicatorAdded(changes, newIndicatorMetas["LxWjE"]);
        }

        [Fact]
        public async Task IndicatorsAdded_ChangesContainOnlyAddedIndicators()
        {
            var oldIndicatorMeta = DataFixture.DefaultIndicatorMeta()
                .ForIndex(0, s => s.SetPublicId("dP0Zw"))
                .GenerateList(1);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldIndicatorMeta);

            var newIndicatorMeta = DataFixture.DefaultIndicatorMeta()
                .ForIndex(0, UnchangedIndicatorMetaSetter(oldIndicatorMeta[0])) // Indicator unchanged
                .ForIndex(1, s => s.SetPublicId("O7CLF")) // Indicator added
                .ForIndex(2, s => s.SetPublicId("7zXob")) // Indicator added
                .GenerateList(3);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                indicatorMeta: newIndicatorMeta);

            await CreateChanges(instanceId);

            var changes = await GetIndicatorMetaChanges(newVersion);

            // 2 Indicator changes
            Assert.Equal(2, changes.Count);
            Assert.All(changes, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var newIndicatorMetas = newVersion.IndicatorMetas
                .ToDictionary(m => m.PublicId);

            // Indicator added
            AssertSingleIndicatorAdded(changes, newIndicatorMetas["O7CLF"]);

            // Indicator added
            AssertSingleIndicatorAdded(changes, newIndicatorMetas["7zXob"]);
        }

        [Fact]
        public async Task IndicatorsDeleted_ChangesContainOnlyDeletedIndicators()
        {
            var oldIndicatorMeta = DataFixture.DefaultIndicatorMeta()
                .ForIndex(0, s => s.SetPublicId("dP0Zw"))
                .ForIndex(1, s => s.SetPublicId("O7CLF")) // Indicator deleted
                .ForIndex(2, s => s.SetPublicId("7zXob")) // Indicator deleted
                .GenerateList(3);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldIndicatorMeta);

            var newIndicatorMeta = DataFixture.DefaultIndicatorMeta()
                .ForIndex(0, UnchangedIndicatorMetaSetter(oldIndicatorMeta[0])) // Indicator unchanged
                .GenerateList(1);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                indicatorMeta: newIndicatorMeta);

            await CreateChanges(instanceId);

            var changes = await GetIndicatorMetaChanges(newVersion);

            // 2 Indicator changes
            Assert.Equal(2, changes.Count);
            Assert.All(changes, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldIndicatorMetas = originalVersion.IndicatorMetas
                .ToDictionary(m => m.PublicId);

            // Indicator deleted
            AssertSingleIndicatorDeleted(changes, oldIndicatorMetas["O7CLF"]);

            // Indicator deleted
            AssertSingleIndicatorDeleted(changes, oldIndicatorMetas["7zXob"]);
        }

        [Fact]
        public async Task IndicatorsUnchanged_ChangesAreEmpty()
        {
            var oldIndicatorMeta = DataFixture.DefaultIndicatorMeta()
                .ForIndex(0, s => s.SetPublicId("dP0Zw"))
                .ForIndex(1, s => s.SetPublicId("O7CLF"))
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldIndicatorMeta);

            var newIndicatorMeta = DataFixture.DefaultIndicatorMeta()
                .ForIndex(0, UnchangedIndicatorMetaSetter(oldIndicatorMeta[0])) // Indicator unchanged
                .ForIndex(1, UnchangedIndicatorMetaSetter(oldIndicatorMeta[1])) // Indicator unchanged
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                indicatorMeta: newIndicatorMeta);

            await CreateChanges(instanceId);

            var changes = await GetIndicatorMetaChanges(newVersion);

            // No Indicator changes
            Assert.Empty(changes);
        }

        [Fact]
        public async Task FiltersAddedAndDeletedAndChanged_ChangesInsertedIntoDatabaseInCorrectOrder()
        {
            var oldIndicatorMeta = DataFixture.DefaultIndicatorMeta()
                .ForIndex(0, s =>
                    s.SetPublicId(SqidEncoder.Encode(1)) // Indicator deleted
                    .SetLabel("f"))
                .ForIndex(1, s =>
                    s.SetPublicId(SqidEncoder.Encode(2)) // Indicator deleted
                    .SetLabel("a"))
                .ForIndex(2, s =>
                    s.SetPublicId(SqidEncoder.Encode(3))
                    .SetLabel("e"))
                .ForIndex(3, s =>
                    s.SetPublicId(SqidEncoder.Encode(4))
                    .SetLabel("b"))
                .GenerateList(4);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldIndicatorMeta);

            var newIndicatorMeta = DataFixture.DefaultIndicatorMeta()
                .ForIndex(0, s => s.SetPublicId(SqidEncoder.Encode(3))) // Indicator changed
                .ForIndex(1, s => s.SetPublicId(SqidEncoder.Encode(4))) // Indicator changed
                .ForIndex(2, s =>
                    s.SetPublicId(SqidEncoder.Encode(5)) // Indicator added
                    .SetLabel("d"))
                .ForIndex(3, s =>
                    s.SetPublicId(SqidEncoder.Encode(6)) // Indicator added
                    .SetLabel("c"))
                .GenerateList(4);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                indicatorMeta: newIndicatorMeta);

            await CreateChanges(instanceId);

            var indicatorMetaChanges = await GetIndicatorMetaChanges(newVersion);

            // 6 Indicator changes
            Assert.Equal(6, indicatorMetaChanges.Count);
            Assert.All(indicatorMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldIndicatorMetas = originalVersion.IndicatorMetas
                .ToDictionary(m => m.PublicId);

            var newIndicatorMetas = newVersion.IndicatorMetas
                .ToDictionary(m => m.PublicId);

            // The changes should be inserted into each database table ordered alphabetically by 'Label'.
            // They should also be ordered such that all additions come last.

            // Therefore, the expected order of Indicator changes are (as per their Public IDs):
            // Sqid index 2 deleted
            // Sqid index 4 changed
            // Sqid index 3 changed
            // Sqid index 1 deleted
            // Sqid index 6 added
            // Sqid index 5 added

            AssertIndicatorDeleted(
                expectedIndicatorMeta: oldIndicatorMetas[SqidEncoder.Encode(2)],
                change: indicatorMetaChanges[0]);
            AssertIndicatorChanged(
                expectedOldIndicatorMeta: oldIndicatorMetas[SqidEncoder.Encode(4)],
                expectedNewIndicatorMeta: newIndicatorMetas[SqidEncoder.Encode(4)],
                change: indicatorMetaChanges[1]);
            AssertIndicatorChanged(
                expectedOldIndicatorMeta: oldIndicatorMetas[SqidEncoder.Encode(3)],
                expectedNewIndicatorMeta: newIndicatorMetas[SqidEncoder.Encode(3)],
                change: indicatorMetaChanges[2]);
            AssertIndicatorDeleted(
                expectedIndicatorMeta: oldIndicatorMetas[SqidEncoder.Encode(1)],
                change: indicatorMetaChanges[3]);
            AssertIndicatorAdded(
                expectedIndicatorMeta: newIndicatorMetas[SqidEncoder.Encode(6)],
                change: indicatorMetaChanges[4]);
            AssertIndicatorAdded(
                expectedIndicatorMeta: newIndicatorMetas[SqidEncoder.Encode(5)],
                change: indicatorMetaChanges[5]);
        }

        private static void AssertSingleIndicatorDeleted(IReadOnlyList<IndicatorMetaChange> changes, IndicatorMeta expectedIndicatorMeta)
        {
            Assert.Single(changes,
                c => c.PreviousStateId == expectedIndicatorMeta.Id
                     && c.CurrentStateId is null);
        }

        private static void AssertSingleIndicatorAdded(IReadOnlyList<IndicatorMetaChange> changes, IndicatorMeta expectedIndicatorMeta)
        {
            Assert.Single(changes,
                c => c.PreviousStateId is null
                     && c.CurrentStateId == expectedIndicatorMeta.Id);
        }

        private static void AssertSingleIndicatorChanged(
            IReadOnlyList<IndicatorMetaChange> changes,
            IndicatorMeta expectedOldIndicatorMeta,
            IndicatorMeta expectedNewIndicatorMeta)
        {
            Assert.Single(changes,
                c => c.PreviousStateId == expectedOldIndicatorMeta.Id
                     && c.CurrentStateId == expectedNewIndicatorMeta.Id);
        }

        private static void AssertIndicatorDeleted(IndicatorMeta expectedIndicatorMeta, IndicatorMetaChange change)
        {
            Assert.Equal(expectedIndicatorMeta.Id, change.PreviousStateId);
            Assert.Null(change.CurrentStateId);
        }

        private static void AssertIndicatorAdded(IndicatorMeta expectedIndicatorMeta, IndicatorMetaChange change)
        {
            Assert.Null(change.PreviousStateId);
            Assert.Equal(expectedIndicatorMeta.Id, change.CurrentStateId);
        }

        private static void AssertIndicatorChanged(
            IndicatorMeta expectedOldIndicatorMeta,
            IndicatorMeta expectedNewIndicatorMeta,
            IndicatorMetaChange change)
        {
            Assert.Equal(expectedOldIndicatorMeta.Id, change.PreviousStateId);
            Assert.Equal(expectedNewIndicatorMeta.Id, change.CurrentStateId);
        }

        private static Action<InstanceSetters<IndicatorMeta>> UnchangedIndicatorMetaSetter(IndicatorMeta indicatorMeta)
        {
            return s =>
            {
                s.SetPublicId(indicatorMeta.PublicId);
                s.SetColumn(indicatorMeta.Column);
                s.SetLabel(indicatorMeta.Label);
                s.SetUnit(indicatorMeta.Unit);
                s.SetDecimalPlaces(indicatorMeta.DecimalPlaces);
            };
        }

        private async Task<IReadOnlyList<IndicatorMetaChange>> GetIndicatorMetaChanges(DataSetVersion version)
        {
            return await GetDbContext<PublicDataDbContext>()
                .IndicatorMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == version.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        private async Task<(DataSetVersion originalVersion, Guid instanceId)> CreateDataSetInitialVersion(
            List<IndicatorMeta> indicatorMeta)
        {
            return await CreateDataSetInitialVersion(
                dataSetStatus: DataSetStatus.Published,
                dataSetVersionStatus: DataSetVersionStatus.Published,
                importStage: DataSetVersionImportStage.Completing,
                meta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    IndicatorMetas = indicatorMeta
                });
        }

        private async Task<(DataSetVersion nextVersion, Guid instanceId)> CreateDataSetNextVersion(
            DataSetVersion originalVersion,
            List<IndicatorMeta> indicatorMeta)
        {
            return await CreateDataSetNextVersion(
                initialVersion: originalVersion,
                status: DataSetVersionStatus.Mapping,
                importStage: Stage.PreviousStage(),
                meta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    IndicatorMetas = indicatorMeta
                });
        }
    }

    public class CreateChangesTimePeriodTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : CreateChangesTests(fixture)
    {
        [Fact]
        public async Task TimePeriodsAddedAndDeleted_ChangesContainAdditionsAndDeletions()
        {
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    TimePeriodMetas = DataFixture.DefaultTimePeriodMeta()
                        .WithCode(TimeIdentifier.AcademicYear)
                        .ForIndex(0, s => s.SetPeriod("2020"))
                        .ForIndex(1, s => s.SetPeriod("2021"))
                        .ForIndex(2, s => s.SetPeriod("2022"))
                        .Generate(3)
                },
                nextVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    TimePeriodMetas = DataFixture.DefaultTimePeriodMeta()
                        .WithCode(TimeIdentifier.AcademicYear)
                        .ForIndex(0, s => s.SetPeriod("2022"))
                        .ForIndex(1, s => s.SetPeriod("2023"))
                        .ForIndex(2, s => s.SetPeriod("2024"))
                        .Generate(3)
                });

            await CreateChanges(instanceId);

            var actualChanges = await GetDbContext<PublicDataDbContext>()
                .TimePeriodMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == newVersion.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();

            Assert.Equal(4, actualChanges.Count);
            Assert.All(actualChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var originalTimePeriodMetas = originalVersion.TimePeriodMetas
                .ToDictionary(m => (m.Code, m.Period));

            Assert.Equal(originalTimePeriodMetas[(TimeIdentifier.AcademicYear, "2020")].Id,
                actualChanges[0].PreviousStateId);
            Assert.Null(actualChanges[0].CurrentStateId);

            Assert.Equal(originalTimePeriodMetas[(TimeIdentifier.AcademicYear, "2021")].Id,
                actualChanges[1].PreviousStateId);
            Assert.Null(actualChanges[1].CurrentStateId);

            var newTimePeriodMetas = newVersion.TimePeriodMetas
                .ToDictionary(m => (m.Code, m.Period));

            Assert.Null(actualChanges[2].PreviousStateId);
            Assert.Equal(newTimePeriodMetas[(TimeIdentifier.AcademicYear, "2023")].Id, actualChanges[2].CurrentStateId);

            Assert.Null(actualChanges[3].PreviousStateId);
            Assert.Equal(newTimePeriodMetas[(TimeIdentifier.AcademicYear, "2024")].Id, actualChanges[3].CurrentStateId);
        }

        [Fact]
        public async Task TimePeriodsUnchanged_ChangesAreEmpty()
        {
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    TimePeriodMetas = DataFixture.DefaultTimePeriodMeta()
                        .WithCode(TimeIdentifier.AcademicYear)
                        .ForIndex(0, s => s.SetPeriod("2019").SetCode(TimeIdentifier.AcademicYear))
                        .ForIndex(1, s => s.SetPeriod("2020").SetCode(TimeIdentifier.CalendarYear))
                        .ForIndex(2, s => s.SetPeriod("2021").SetCode(TimeIdentifier.January))
                        .Generate(3)
                },
                nextVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    TimePeriodMetas = DataFixture.DefaultTimePeriodMeta()
                        .ForIndex(0, s => s.SetPeriod("2019").SetCode(TimeIdentifier.AcademicYear))
                        .ForIndex(1, s => s.SetPeriod("2020").SetCode(TimeIdentifier.CalendarYear))
                        .ForIndex(2, s => s.SetPeriod("2021").SetCode(TimeIdentifier.January))
                        .Generate(3)
                });

            await CreateChanges(instanceId);

            Assert.False(await GetDbContext<PublicDataDbContext>()
                .TimePeriodMetaChanges
                .AnyAsync(c => c.DataSetVersionId == newVersion.Id));
        }

        [Fact]
        public async Task TimePeriodsAdded_ChangesContainOnlyAdditions()
        {
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    TimePeriodMetas = DataFixture.DefaultTimePeriodMeta()
                        .WithCode(TimeIdentifier.CalendarYear)
                        .WithPeriod("2020")
                        .Generate(1)
                },
                nextVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    TimePeriodMetas =
                    [
                        ..DataFixture.DefaultTimePeriodMeta()
                            .WithCode(TimeIdentifier.CalendarYear)
                            .WithPeriod("2020")
                            .Generate(1),
                        ..DataFixture.DefaultTimePeriodMeta()
                            .WithCode(TimeIdentifier.AcademicYear)
                            .ForIndex(0, s => s.SetPeriod("2019"))
                            .ForIndex(1, s => s.SetPeriod("2020"))
                            .ForIndex(2, s => s.SetPeriod("2021"))
                            .Generate(3)
                    ]
                });

            await CreateChanges(instanceId);

            var actualChanges = await GetDbContext<PublicDataDbContext>()
                .TimePeriodMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == newVersion.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();

            Assert.Equal(3, actualChanges.Count);
            Assert.All(actualChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));
            Assert.All(actualChanges, c => Assert.Null(c.PreviousStateId));

            var newTimePeriodMetas = newVersion.TimePeriodMetas
                .ToDictionary(m => (m.Code, m.Period));

            Assert.Equal(newTimePeriodMetas[(TimeIdentifier.AcademicYear, "2019")].Id, actualChanges[0].CurrentStateId);
            Assert.Equal(newTimePeriodMetas[(TimeIdentifier.AcademicYear, "2020")].Id, actualChanges[1].CurrentStateId);
            Assert.Equal(newTimePeriodMetas[(TimeIdentifier.AcademicYear, "2021")].Id, actualChanges[2].CurrentStateId);
        }

        [Fact]
        public async Task TimePeriodsDeleted_ChangesContainOnlyDeletions()
        {
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    TimePeriodMetas =
                    [
                        .. DataFixture.DefaultTimePeriodMeta()
                            .WithCode(TimeIdentifier.CalendarYear)
                            .WithPeriod("2020")
                            .Generate(1),
                        ..DataFixture.DefaultTimePeriodMeta()
                            .WithCode(TimeIdentifier.AcademicYear)
                            .ForIndex(0, s => s.SetPeriod("2019"))
                            .ForIndex(1, s => s.SetPeriod("2020"))
                            .ForIndex(2, s => s.SetPeriod("2021"))
                            .Generate(3)
                    ]
                },
                nextVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    TimePeriodMetas = DataFixture.DefaultTimePeriodMeta()
                        .WithCode(TimeIdentifier.CalendarYear)
                        .WithPeriod("2020")
                        .Generate(1)
                });

            await CreateChanges(instanceId);

            var actualChanges = await GetDbContext<PublicDataDbContext>()
                .TimePeriodMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == newVersion.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();

            Assert.Equal(3, actualChanges.Count);
            Assert.All(actualChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));
            Assert.All(actualChanges, c => Assert.Null(c.CurrentStateId));

            var oldTimePeriodMetas = originalVersion.TimePeriodMetas
                .ToDictionary(m => (m.Code, m.Period));

            Assert.Equal(oldTimePeriodMetas[(TimeIdentifier.AcademicYear, "2019")].Id,
                actualChanges[0].PreviousStateId);
            Assert.Equal(oldTimePeriodMetas[(TimeIdentifier.AcademicYear, "2020")].Id,
                actualChanges[1].PreviousStateId);
            Assert.Equal(oldTimePeriodMetas[(TimeIdentifier.AcademicYear, "2021")].Id,
                actualChanges[2].PreviousStateId);
        }
    }

    public class UpdateFileStoragePathTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessCompletionOfNextDataSetVersionImportFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.ManualMapping;

        [Fact]
        public async Task Success_PathUpdated()
        {
            var (_, nextDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage);

            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            var originalStoragePath = dataSetVersionPathResolver.DirectoryPath(nextDataSetVersion);
            Directory.CreateDirectory(originalStoragePath);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            nextDataSetVersion.VersionMajor++;

            publicDataDbContext.DataSetVersions.Update(nextDataSetVersion);
            await publicDataDbContext.SaveChangesAsync();

            var newStoragePath = dataSetVersionPathResolver.DirectoryPath(nextDataSetVersion);

            await UpdateFileStoragePath(instanceId);

            Assert.False(Directory.Exists(originalStoragePath));
            Assert.True(Directory.Exists(newStoragePath));
        }

        [Fact]
        public async Task Success_PathNotUpdated()
        {
            var (_, nextDataSetVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage);

            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            var originalStoragePath = dataSetVersionPathResolver.DirectoryPath(nextDataSetVersion);
            Directory.CreateDirectory(originalStoragePath);

            await UpdateFileStoragePath(instanceId);

            Assert.True(Directory.Exists(originalStoragePath));
        }

        private async Task UpdateFileStoragePath(Guid instanceId)
        {
            var function = GetRequiredService<ProcessCompletionOfNextDataSetVersionFunction>();
            await function.UpdateFileStoragePath(instanceId, CancellationToken.None);
        }
    }

    public class CompleteNextDataSetVersionImportProcessingTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessCompletionOfNextDataSetVersionImportFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.Completing;

        [Fact]
        public async Task Success()
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            Directory.CreateDirectory(dataSetVersionPathResolver.DirectoryPath(dataSetVersion));

            await CompleteProcessing(instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext.DataSetVersionImports
                .Include(i => i.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            savedImport.Completed.AssertUtcNow();

            Assert.Equal(DataSetVersionStatus.Draft, savedImport.DataSetVersion.Status);
        }

        [Fact]
        public async Task DuckDbFileIsDeleted()
        {
            var (dataSetVersion, instanceId) = await CreateDataSetInitialVersion(Stage.PreviousStage());

            // Create empty data set version files for all file paths
            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            var directoryPath = dataSetVersionPathResolver.DirectoryPath(dataSetVersion);
            Directory.CreateDirectory(directoryPath);
            foreach (var filename in AllDataSetVersionFiles)
            {
                await File.Create(Path.Combine(directoryPath, filename)).DisposeAsync();
            }

            await CompleteProcessing(instanceId);

            // Ensure the duck db database file is the only file that was deleted
            AssertDataSetVersionDirectoryContainsOnlyFiles(dataSetVersion,
                AllDataSetVersionFiles
                    .Where(file => file != DataSetFilenames.DuckDbDatabaseFile)
                    .ToArray());
        }

        private async Task CompleteProcessing(Guid instanceId)
        {
            var function = GetRequiredService<ProcessCompletionOfNextDataSetVersionFunction>();
            await function.CompleteNextDataSetVersionImportProcessing(instanceId, CancellationToken.None);
        }
    }
}
