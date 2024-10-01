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
            Assert.Single(filterMetaChanges,
                c => c.PreviousStateId is null
                     && c.CurrentStateId == newFilterMetas["O7CLF"].FilterMeta.Id);

            // Filter added
            Assert.Single(filterMetaChanges,
                c => c.PreviousStateId is null
                     && c.CurrentStateId == newFilterMetas["7zXob"].FilterMeta.Id);

            // Filter Option added
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newFilterMetas["O7CLF"].NewFilterOptionMetas["O7CLF"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["O7CLF"].NewFilterOptionMetas["O7CLF"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["O7CLF"].NewFilterOptionMetas["O7CLF"].OptionId);

            // Filter Option added
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newFilterMetas["O7CLF"].NewFilterOptionMetas["7zXob"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["O7CLF"].NewFilterOptionMetas["7zXob"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["O7CLF"].NewFilterOptionMetas["7zXob"].OptionId);

            // Filter Option added
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newFilterMetas["7zXob"].NewFilterOptionMetas["pTSoj"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["7zXob"].NewFilterOptionMetas["pTSoj"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["7zXob"].NewFilterOptionMetas["pTSoj"].OptionId);

            // Filter Option added
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newFilterMetas["7zXob"].NewFilterOptionMetas["IzBzg"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["7zXob"].NewFilterOptionMetas["IzBzg"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["7zXob"].NewFilterOptionMetas["IzBzg"].OptionId);
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
            Assert.Single(filterMetaChanges,
                c => c.PreviousStateId == oldFilterMetas["O7CLF"].Id
                     && c.CurrentStateId is null);

            // Filter deleted
            Assert.Single(filterMetaChanges,
                c => c.PreviousStateId == oldFilterMetas["7zXob"].Id
                     && c.CurrentStateId is null);
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
            Assert.Single(filterMetaChanges,
                c => c.PreviousStateId == oldFilterMetas["O7CLF"].FilterMeta.Id
                     && c.CurrentStateId == newFilterMetas["O7CLF"].FilterMeta.Id);

            // Filter changed
            Assert.Single(filterMetaChanges,
                c => c.PreviousStateId == oldFilterMetas["7zXob"].FilterMeta.Id
                     && c.CurrentStateId == newFilterMetas["7zXob"].FilterMeta.Id);

            // Filter Option added
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newFilterMetas["O7CLF"].NewFilterOptionMetas["IzBzg"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["O7CLF"].NewFilterOptionMetas["IzBzg"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["O7CLF"].NewFilterOptionMetas["IzBzg"].OptionId);

            // Filter Option added
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newFilterMetas["O7CLF"].NewFilterOptionMetas["it6Xr"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["O7CLF"].NewFilterOptionMetas["it6Xr"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["O7CLF"].NewFilterOptionMetas["it6Xr"].OptionId);

            // Filter Option added
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newFilterMetas["7zXob"].NewFilterOptionMetas["LxWjE"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["7zXob"].NewFilterOptionMetas["LxWjE"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["7zXob"].NewFilterOptionMetas["LxWjE"].OptionId);

            // Filter Option added
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newFilterMetas["7zXob"].NewFilterOptionMetas["6jrfe"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["7zXob"].NewFilterOptionMetas["6jrfe"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["7zXob"].NewFilterOptionMetas["6jrfe"].OptionId);
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
            Assert.Single(filterMetaChanges,
                c => c.PreviousStateId == oldFilterMetas["O7CLF"].FilterMeta.Id
                     && c.CurrentStateId == newFilterMetas["O7CLF"].FilterMeta.Id);

            // Filter changed
            Assert.Single(filterMetaChanges,
                c => c.PreviousStateId == oldFilterMetas["7zXob"].FilterMeta.Id
                     && c.CurrentStateId == newFilterMetas["7zXob"].FilterMeta.Id);

            // Filter Option deleted
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldFilterMetas["O7CLF"].OldFilterOptionMetas["pTSoj"].PublicId
                     && c.PreviousState.MetaId == oldFilterMetas["O7CLF"].OldFilterOptionMetas["pTSoj"].MetaId
                     && c.PreviousState.OptionId == oldFilterMetas["O7CLF"].OldFilterOptionMetas["pTSoj"].OptionId
                     && c.CurrentState is null);

            // Filter Option deleted
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldFilterMetas["O7CLF"].OldFilterOptionMetas["IzBzg"].PublicId
                     && c.PreviousState.MetaId == oldFilterMetas["O7CLF"].OldFilterOptionMetas["IzBzg"].MetaId
                     && c.PreviousState.OptionId == oldFilterMetas["O7CLF"].OldFilterOptionMetas["IzBzg"].OptionId
                     && c.CurrentState is null);

            // Filter Option deleted
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldFilterMetas["7zXob"].OldFilterOptionMetas["LxWjE"].PublicId
                     && c.PreviousState.MetaId == oldFilterMetas["7zXob"].OldFilterOptionMetas["LxWjE"].MetaId
                     && c.PreviousState.OptionId == oldFilterMetas["7zXob"].OldFilterOptionMetas["LxWjE"].OptionId
                     && c.CurrentState is null);

            // Filter Option deleted
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldFilterMetas["7zXob"].OldFilterOptionMetas["6jrfe"].PublicId
                     && c.PreviousState.MetaId == oldFilterMetas["7zXob"].OldFilterOptionMetas["6jrfe"].MetaId
                     && c.PreviousState.OptionId == oldFilterMetas["7zXob"].OldFilterOptionMetas["6jrfe"].OptionId
                     && c.CurrentState is null);
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
            Assert.Single(filterMetaChanges,
                c => c.PreviousStateId == oldFilterMetas["O7CLF"].FilterMeta.Id
                     && c.CurrentStateId == newFilterMetas["O7CLF"].FilterMeta.Id);

            // Filter changed
            Assert.Single(filterMetaChanges,
                c => c.PreviousStateId == oldFilterMetas["7zXob"].FilterMeta.Id
                     && c.CurrentStateId == newFilterMetas["7zXob"].FilterMeta.Id);

            // Filter Option changed
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldFilterMetas["O7CLF"].OldFilterOptionMetas["pTSoj"].PublicId
                     && c.PreviousState.MetaId == oldFilterMetas["O7CLF"].OldFilterOptionMetas["pTSoj"].MetaId
                     && c.PreviousState.OptionId == oldFilterMetas["O7CLF"].OldFilterOptionMetas["pTSoj"].OptionId
                     && c.CurrentState!.PublicId == newFilterMetas["O7CLF"].NewFilterOptionMetas["pTSoj"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["O7CLF"].NewFilterOptionMetas["pTSoj"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["O7CLF"].NewFilterOptionMetas["pTSoj"].OptionId);

            // Filter Option changed
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldFilterMetas["O7CLF"].OldFilterOptionMetas["IzBzg"].PublicId
                     && c.PreviousState.MetaId == oldFilterMetas["O7CLF"].OldFilterOptionMetas["IzBzg"].MetaId
                     && c.PreviousState.OptionId == oldFilterMetas["O7CLF"].OldFilterOptionMetas["IzBzg"].OptionId
                     && c.CurrentState!.PublicId == newFilterMetas["O7CLF"].NewFilterOptionMetas["IzBzg"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["O7CLF"].NewFilterOptionMetas["IzBzg"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["O7CLF"].NewFilterOptionMetas["IzBzg"].OptionId);

            // Filter Option changed
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldFilterMetas["7zXob"].OldFilterOptionMetas["LxWjE"].PublicId
                     && c.PreviousState.MetaId == oldFilterMetas["7zXob"].OldFilterOptionMetas["LxWjE"].MetaId
                     && c.PreviousState.OptionId == oldFilterMetas["7zXob"].OldFilterOptionMetas["LxWjE"].OptionId
                     && c.CurrentState!.PublicId == newFilterMetas["7zXob"].NewFilterOptionMetas["LxWjE"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["7zXob"].NewFilterOptionMetas["LxWjE"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["7zXob"].NewFilterOptionMetas["LxWjE"].OptionId);

            // Filter Option changed
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldFilterMetas["7zXob"].OldFilterOptionMetas["6jrfe"].PublicId
                     && c.PreviousState.MetaId == oldFilterMetas["7zXob"].OldFilterOptionMetas["6jrfe"].MetaId
                     && c.PreviousState.OptionId == oldFilterMetas["7zXob"].OldFilterOptionMetas["6jrfe"].OptionId
                     && c.CurrentState!.PublicId == newFilterMetas["7zXob"].NewFilterOptionMetas["6jrfe"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["7zXob"].NewFilterOptionMetas["6jrfe"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["7zXob"].NewFilterOptionMetas["6jrfe"].OptionId);
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
            Assert.Single(filterMetaChanges,
                c => c.PreviousStateId == oldFilterMetas["O7CLF"].Id
                     && c.CurrentStateId == newFilterMetas["O7CLF"].Id);

            // Filter changed
            Assert.Single(filterMetaChanges,
                c => c.PreviousStateId == oldFilterMetas["7zXob"].Id
                     && c.CurrentStateId == newFilterMetas["7zXob"].Id);
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
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newFilterMetas["dP0Zw"]["7zXob"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["dP0Zw"]["7zXob"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["dP0Zw"]["7zXob"].OptionId);

            // Filter Option added
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newFilterMetas["dP0Zw"]["pTSoj"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["dP0Zw"]["pTSoj"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["dP0Zw"]["pTSoj"].OptionId);

            // Filter Option added
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newFilterMetas["O7CLF"]["IzBzg"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["O7CLF"]["IzBzg"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["O7CLF"]["IzBzg"].OptionId);

            // Filter Option added
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newFilterMetas["O7CLF"]["it6Xr"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["O7CLF"]["it6Xr"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["O7CLF"]["it6Xr"].OptionId);
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
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldFilterMetas["dP0Zw"]["O7CLF"].PublicId
                     && c.PreviousState.MetaId == oldFilterMetas["dP0Zw"]["O7CLF"].MetaId
                     && c.PreviousState.OptionId == oldFilterMetas["dP0Zw"]["O7CLF"].OptionId
                     && c.CurrentState is null);

            // Filter Option deleted
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldFilterMetas["dP0Zw"]["7zXob"].PublicId
                     && c.PreviousState.MetaId == oldFilterMetas["dP0Zw"]["7zXob"].MetaId
                     && c.PreviousState.OptionId == oldFilterMetas["dP0Zw"]["7zXob"].OptionId
                     && c.CurrentState is null);

            // Filter Option deleted
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldFilterMetas["O7CLF"]["IzBzg"].PublicId
                     && c.PreviousState.MetaId == oldFilterMetas["O7CLF"]["IzBzg"].MetaId
                     && c.PreviousState.OptionId == oldFilterMetas["O7CLF"]["IzBzg"].OptionId
                     && c.CurrentState is null);

            // Filter Option deleted
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldFilterMetas["O7CLF"]["it6Xr"].PublicId
                     && c.PreviousState.MetaId == oldFilterMetas["O7CLF"]["it6Xr"].MetaId
                     && c.PreviousState.OptionId == oldFilterMetas["O7CLF"]["it6Xr"].OptionId
                     && c.CurrentState is null);
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
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldFilterMetas["dP0Zw"]["O7CLF"].PublicId
                     && c.PreviousState.MetaId == oldFilterMetas["dP0Zw"]["O7CLF"].MetaId
                     && c.PreviousState.OptionId == oldFilterMetas["dP0Zw"]["O7CLF"].OptionId
                     && c.CurrentState!.PublicId == newFilterMetas["dP0Zw"]["O7CLF"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["dP0Zw"]["O7CLF"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["dP0Zw"]["O7CLF"].OptionId);

            // Filter Option changed
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldFilterMetas["dP0Zw"]["7zXob"].PublicId
                     && c.PreviousState.MetaId == oldFilterMetas["dP0Zw"]["7zXob"].MetaId
                     && c.PreviousState.OptionId == oldFilterMetas["dP0Zw"]["7zXob"].OptionId
                     && c.CurrentState!.PublicId == newFilterMetas["dP0Zw"]["7zXob"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["dP0Zw"]["7zXob"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["dP0Zw"]["7zXob"].OptionId);

            // Filter Option changed
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldFilterMetas["O7CLF"]["IzBzg"].PublicId
                     && c.PreviousState.MetaId == oldFilterMetas["O7CLF"]["IzBzg"].MetaId
                     && c.PreviousState.OptionId == oldFilterMetas["O7CLF"]["IzBzg"].OptionId
                     && c.CurrentState!.PublicId == newFilterMetas["O7CLF"]["IzBzg"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["O7CLF"]["IzBzg"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["O7CLF"]["IzBzg"].OptionId);

            // Filter Option changed
            Assert.Single(filterOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldFilterMetas["O7CLF"]["it6Xr"].PublicId
                     && c.PreviousState.MetaId == oldFilterMetas["O7CLF"]["it6Xr"].MetaId
                     && c.PreviousState.OptionId == oldFilterMetas["O7CLF"]["it6Xr"].OptionId
                     && c.CurrentState!.PublicId == newFilterMetas["O7CLF"]["it6Xr"].PublicId
                     && c.CurrentState.MetaId == newFilterMetas["O7CLF"]["it6Xr"].MetaId
                     && c.CurrentState.OptionId == newFilterMetas["O7CLF"]["it6Xr"].OptionId);
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
                .ToListAsync();
        }

        private async Task<IReadOnlyList<FilterOptionMetaChange>> GetFilterOptionMetaChanges(DataSetVersion version)
        {
            return await GetDbContext<PublicDataDbContext>()
                .FilterOptionMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == version.Id)
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
            Assert.Single(locationMetaChanges,
                c => c.PreviousStateId is null
                     && c.CurrentStateId == newLocationMetas[GeographicLevel.School].LocationMeta.Id);

            // Location added
            Assert.Single(locationMetaChanges,
                c => c.PreviousStateId is null
                     && c.CurrentStateId == newLocationMetas[GeographicLevel.RscRegion].LocationMeta.Id);

            // Location Option added
            Assert.Single(locationOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newLocationMetas[GeographicLevel.School].NewLocationOptionMetas["O7CLF"].PublicId
                     && c.CurrentState.MetaId == newLocationMetas[GeographicLevel.School].NewLocationOptionMetas["O7CLF"].MetaId
                     && c.CurrentState.OptionId == newLocationMetas[GeographicLevel.School].NewLocationOptionMetas["O7CLF"].OptionId);

            // Location Option added
            Assert.Single(locationOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newLocationMetas[GeographicLevel.School].NewLocationOptionMetas["7zXob"].PublicId
                     && c.CurrentState.MetaId == newLocationMetas[GeographicLevel.School].NewLocationOptionMetas["7zXob"].MetaId
                     && c.CurrentState.OptionId == newLocationMetas[GeographicLevel.School].NewLocationOptionMetas["7zXob"].OptionId);

            // Location Option added
            Assert.Single(locationOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newLocationMetas[GeographicLevel.RscRegion].NewLocationOptionMetas["pTSoj"].PublicId
                     && c.CurrentState.MetaId == newLocationMetas[GeographicLevel.RscRegion].NewLocationOptionMetas["pTSoj"].MetaId
                     && c.CurrentState.OptionId == newLocationMetas[GeographicLevel.RscRegion].NewLocationOptionMetas["pTSoj"].OptionId);

            // Location Option added
            Assert.Single(locationOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newLocationMetas[GeographicLevel.RscRegion].NewLocationOptionMetas["IzBzg"].PublicId
                     && c.CurrentState.MetaId == newLocationMetas[GeographicLevel.RscRegion].NewLocationOptionMetas["IzBzg"].MetaId
                     && c.CurrentState.OptionId == newLocationMetas[GeographicLevel.RscRegion].NewLocationOptionMetas["IzBzg"].OptionId);
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
            Assert.Single(locationMetaChanges,
                c => c.PreviousStateId == oldLocationMetas[GeographicLevel.School].Id
                     && c.CurrentStateId is null);

            // Location deleted
            Assert.Single(locationMetaChanges,
                c => c.PreviousStateId == oldLocationMetas[GeographicLevel.RscRegion].Id
                     && c.CurrentStateId is null);
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
            Assert.Single(locationOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newLocationMetas[GeographicLevel.LocalAuthority]["7zXob"].PublicId
                     && c.CurrentState.MetaId == newLocationMetas[GeographicLevel.LocalAuthority]["7zXob"].MetaId
                     && c.CurrentState.OptionId == newLocationMetas[GeographicLevel.LocalAuthority]["7zXob"].OptionId);

            // Location Option added
            Assert.Single(locationOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newLocationMetas[GeographicLevel.LocalAuthority]["pTSoj"].PublicId
                     && c.CurrentState.MetaId == newLocationMetas[GeographicLevel.LocalAuthority]["pTSoj"].MetaId
                     && c.CurrentState.OptionId == newLocationMetas[GeographicLevel.LocalAuthority]["pTSoj"].OptionId);

            // Location Option added
            Assert.Single(locationOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newLocationMetas[GeographicLevel.School]["IzBzg"].PublicId
                     && c.CurrentState.MetaId == newLocationMetas[GeographicLevel.School]["IzBzg"].MetaId
                     && c.CurrentState.OptionId == newLocationMetas[GeographicLevel.School]["IzBzg"].OptionId);

            // Location Option added
            Assert.Single(locationOptionMetaChanges,
                c => c.PreviousState is null
                     && c.CurrentState!.PublicId == newLocationMetas[GeographicLevel.School]["it6Xr"].PublicId
                     && c.CurrentState.MetaId == newLocationMetas[GeographicLevel.School]["it6Xr"].MetaId
                     && c.CurrentState.OptionId == newLocationMetas[GeographicLevel.School]["it6Xr"].OptionId);
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
            Assert.Single(locationOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldLocationMetas[GeographicLevel.LocalAuthority]["O7CLF"].PublicId
                     && c.PreviousState.MetaId == oldLocationMetas[GeographicLevel.LocalAuthority]["O7CLF"].MetaId
                     && c.PreviousState.OptionId == oldLocationMetas[GeographicLevel.LocalAuthority]["O7CLF"].OptionId
                     && c.CurrentState is null);

            // Location Option deleted
            Assert.Single(locationOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldLocationMetas[GeographicLevel.LocalAuthority]["7zXob"].PublicId
                     && c.PreviousState.MetaId == oldLocationMetas[GeographicLevel.LocalAuthority]["7zXob"].MetaId
                     && c.PreviousState.OptionId == oldLocationMetas[GeographicLevel.LocalAuthority]["7zXob"].OptionId
                     && c.CurrentState is null);

            // Location Option deleted
            Assert.Single(locationOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldLocationMetas[GeographicLevel.School]["IzBzg"].PublicId
                     && c.PreviousState.MetaId == oldLocationMetas[GeographicLevel.School]["IzBzg"].MetaId
                     && c.PreviousState.OptionId == oldLocationMetas[GeographicLevel.School]["IzBzg"].OptionId
                     && c.CurrentState is null);

            // Location Option deleted
            Assert.Single(locationOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldLocationMetas[GeographicLevel.School]["it6Xr"].PublicId
                     && c.PreviousState.MetaId == oldLocationMetas[GeographicLevel.School]["it6Xr"].MetaId
                     && c.PreviousState.OptionId == oldLocationMetas[GeographicLevel.School]["it6Xr"].OptionId
                     && c.CurrentState is null);
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
            Assert.Single(locationOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldLocationMetas[GeographicLevel.LocalAuthority]["O7CLF"].PublicId
                     && c.PreviousState.MetaId == oldLocationMetas[GeographicLevel.LocalAuthority]["O7CLF"].MetaId
                     && c.PreviousState.OptionId == oldLocationMetas[GeographicLevel.LocalAuthority]["O7CLF"].OptionId
                     && c.CurrentState!.PublicId == newLocationMetas[GeographicLevel.LocalAuthority]["O7CLF"].PublicId
                     && c.CurrentState.MetaId == newLocationMetas[GeographicLevel.LocalAuthority]["O7CLF"].MetaId
                     && c.CurrentState.OptionId == newLocationMetas[GeographicLevel.LocalAuthority]["O7CLF"].OptionId);

            // Location Option changed
            Assert.Single(locationOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldLocationMetas[GeographicLevel.LocalAuthority]["7zXob"].PublicId
                     && c.PreviousState.MetaId == oldLocationMetas[GeographicLevel.LocalAuthority]["7zXob"].MetaId
                     && c.PreviousState.OptionId == oldLocationMetas[GeographicLevel.LocalAuthority]["7zXob"].OptionId
                     && c.CurrentState!.PublicId == newLocationMetas[GeographicLevel.LocalAuthority]["7zXob"].PublicId
                     && c.CurrentState.MetaId == newLocationMetas[GeographicLevel.LocalAuthority]["7zXob"].MetaId
                     && c.CurrentState.OptionId == newLocationMetas[GeographicLevel.LocalAuthority]["7zXob"].OptionId);

            // Location Option changed
            Assert.Single(locationOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldLocationMetas[GeographicLevel.School]["IzBzg"].PublicId
                     && c.PreviousState.MetaId == oldLocationMetas[GeographicLevel.School]["IzBzg"].MetaId
                     && c.PreviousState.OptionId == oldLocationMetas[GeographicLevel.School]["IzBzg"].OptionId
                     && c.CurrentState!.PublicId == newLocationMetas[GeographicLevel.School]["IzBzg"].PublicId
                     && c.CurrentState.MetaId == newLocationMetas[GeographicLevel.School]["IzBzg"].MetaId
                     && c.CurrentState.OptionId == newLocationMetas[GeographicLevel.School]["IzBzg"].OptionId);

            // Location Option changed
            Assert.Single(locationOptionMetaChanges,
                c => c.PreviousState!.PublicId == oldLocationMetas[GeographicLevel.School]["it6Xr"].PublicId
                     && c.PreviousState.MetaId == oldLocationMetas[GeographicLevel.School]["it6Xr"].MetaId
                     && c.PreviousState.OptionId == oldLocationMetas[GeographicLevel.School]["it6Xr"].OptionId
                     && c.CurrentState!.PublicId == newLocationMetas[GeographicLevel.School]["it6Xr"].PublicId
                     && c.CurrentState.MetaId == newLocationMetas[GeographicLevel.School]["it6Xr"].MetaId
                     && c.CurrentState.OptionId == newLocationMetas[GeographicLevel.School]["it6Xr"].OptionId);
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
                .ToListAsync();
        }

        private async Task<IReadOnlyList<LocationOptionMetaChange>> GetLocationOptionMetaChanges(DataSetVersion version)
        {
            return await GetDbContext<PublicDataDbContext>()
                .LocationOptionMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == version.Id)
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
            Assert.Single(changes,
                c => c.PreviousStateId == oldIndicatorMetas["O7CLF"].Id
                     && c.CurrentStateId is null);

            // Indicator deleted
            Assert.Single(changes,
                c => c.PreviousStateId == oldIndicatorMetas["7zXob"].Id
                     && c.CurrentStateId is null);

            // Indicator changed
            Assert.Single(changes,
                c => c.PreviousStateId == oldIndicatorMetas["pTSoj"].Id
                     && c.CurrentStateId == newIndicatorMetas["pTSoj"].Id);

            // Indicator changed
            Assert.Single(changes,
                c => c.PreviousStateId == oldIndicatorMetas["IzBzg"].Id
                     && c.CurrentStateId == newIndicatorMetas["IzBzg"].Id);

            // Indicator added
            Assert.Single(changes,
                c => c.PreviousStateId is null
                     && c.CurrentStateId == newIndicatorMetas["it6Xr"].Id);

            // Indicator added
            Assert.Single(changes,
                c => c.PreviousStateId is null
                     && c.CurrentStateId == newIndicatorMetas["LxWjE"].Id);
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
            Assert.Single(changes,
                c => c.PreviousStateId is null
                     && c.CurrentStateId == newIndicatorMetas["O7CLF"].Id);

            // Indicator added
            Assert.Single(changes,
                c => c.PreviousStateId is null
                     && c.CurrentStateId == newIndicatorMetas["7zXob"].Id);
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
            Assert.Single(changes,
                c => c.PreviousStateId == oldIndicatorMetas["O7CLF"].Id
                     && c.CurrentStateId is null);

            // Indicator deleted
            Assert.Single(changes,
                c => c.PreviousStateId == oldIndicatorMetas["7zXob"].Id
                     && c.CurrentStateId is null);
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
