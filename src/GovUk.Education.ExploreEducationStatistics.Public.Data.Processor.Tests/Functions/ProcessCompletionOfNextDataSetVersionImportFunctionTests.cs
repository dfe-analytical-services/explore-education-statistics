using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using Microsoft.DurableTask;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Semver;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using FilterMeta = GovUk.Education.ExploreEducationStatistics.Public.Data.Model.FilterMeta;
using IndicatorMeta = GovUk.Education.ExploreEducationStatistics.Public.Data.Model.IndicatorMeta;

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

        //        // filter DELETED, with all options DELETED. ===> Should just have 1 change for the DELETED filter.
        //        // filter ADDED, with ADDED options. ===> Should have 1 change for the ADDED filter, and 1 change for EACH of the ADDED options.
        //        // filter UNCHANGED, options UNCHANGED. ===> Should have NO changes.
        //        // filter UNCHANGED, with some DELETED options and some ADDED options. ===> Should have 1 change for EACH of the DELETED options and EACH of the ADDED options.
        //        // filter UNCHANGED, SOME options CHANGED. ===> Should have 1 change for EACH of the CHANGED options.
        //        // filter CHANGED, options UNCHANGED. ===> Should just have 1 change for the CHANGED filter.
        //        // filter CHANGED + SOME options CHANGED. ===> Should have 1 change for the CHANGED filter, and 1 change for EACH of the CHANGED options.

        [Fact]
        public async Task FiltersAddedAndDeletedAndChanged_ChangesContainAdditionsAndDeletionsAndChanges()
        {
            var oldFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s => s.SetPublicId("dP0Zw"))
                .ForIndex(1, s => s.SetPublicId("O7CLF"))
                .ForIndex(2, s => s.SetPublicId("7zXob")) // Deleted
                .GenerateList(3);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Unchanged
                .ForIndex(1, s => s.SetPublicId("O7CLF")) // Changed
                .ForIndex(2, s => s.SetPublicId("pTSoj")) // Added
                .GenerateList(3);

            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                oldFilterMeta: oldFilterMeta,
                newFilterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var actualChanges = await GetFilterMetaChanges(newVersion);

            Assert.Equal(3, actualChanges.Count);
            Assert.All(actualChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas
                .ToDictionary(m => m.PublicId);

            var newFilterMetas = newVersion.FilterMetas
                .ToDictionary(m => m.PublicId);

            // Deleted
            Assert.Single(actualChanges, 
                c => c.PreviousStateId == oldFilterMetas["7zXob"].Id
                     && c.CurrentStateId is null);

            // Changed
            Assert.Single(actualChanges, 
                c => c.PreviousStateId == oldFilterMetas["O7CLF"].Id
                     && c.CurrentStateId == newFilterMetas["O7CLF"].Id);

            // Added
            Assert.Single(actualChanges, 
                c => c.PreviousStateId is null
                     && c.CurrentStateId == newFilterMetas["pTSoj"].Id);
        }

        [Fact]
        public async Task FiltersChanged_ChangesContainOnlyChanged()
        {
            var oldFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s => s.SetPublicId("dP0Zw"))
                .ForIndex(1, s => s.SetPublicId("O7CLF"))
                .ForIndex(2, s => s.SetPublicId("7zXob"))
                .GenerateList(3);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Unchanged
                .ForIndex(1, s => s.SetPublicId("O7CLF")) // Changed
                .ForIndex(2, s => s.SetPublicId("7zXob")) // Changed
                .GenerateList(3);

            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                oldFilterMeta: oldFilterMeta, 
                newFilterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var actualChanges = await GetFilterMetaChanges(newVersion);

            Assert.Equal(2, actualChanges.Count);
            Assert.All(actualChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas
                .ToDictionary(m => m.PublicId);

            var newFilterMetas = newVersion.FilterMetas
                .ToDictionary(m => m.PublicId);

            // Changed
            Assert.Single(actualChanges,
                c => c.PreviousStateId == oldFilterMetas["O7CLF"].Id
                     && c.CurrentStateId == newFilterMetas["O7CLF"].Id);

            // Changed
            Assert.Single(actualChanges,
                c => c.PreviousStateId == oldFilterMetas["7zXob"].Id
                     && c.CurrentStateId == newFilterMetas["7zXob"].Id);
        }

        [Fact]
        public async Task FiltersAdded_ChangesContainOnlyAdditions()
        {
            var oldFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s => s.SetPublicId("dP0Zw"))
                .GenerateList(1);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Unchanged
                .ForIndex(1, s => s.SetPublicId("O7CLF")) // Added
                .ForIndex(2, s => s.SetPublicId("7zXob")) // Added
                .GenerateList(3);

            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                oldFilterMeta: oldFilterMeta,
                newFilterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var actualChanges = await GetFilterMetaChanges(newVersion);

            Assert.Equal(2, actualChanges.Count);
            Assert.All(actualChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var newFilterMetas = newVersion.FilterMetas
                .ToDictionary(m => m.PublicId);

            // Added
            Assert.Single(actualChanges,
                c => c.PreviousStateId is null
                     && c.CurrentStateId == newFilterMetas["O7CLF"].Id);

            // Added
            Assert.Single(actualChanges,
                c => c.PreviousStateId is null
                     && c.CurrentStateId == newFilterMetas["7zXob"].Id);
        }

        [Fact]
        public async Task FiltersDeleted_ChangesContainOnlyDeletions()
        {
            var oldFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, s => s.SetPublicId("dP0Zw"))
                .ForIndex(1, s => s.SetPublicId("O7CLF")) // Deleted
                .ForIndex(2, s => s.SetPublicId("7zXob")) // Deleted
                .GenerateList(3);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Unchanged
                .GenerateList(1);

            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                oldFilterMeta: oldFilterMeta,
                newFilterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var actualChanges = await GetFilterMetaChanges(newVersion);

            Assert.Equal(2, actualChanges.Count);
            Assert.All(actualChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas
                .ToDictionary(m => m.PublicId);

            // Deleted
            Assert.Single(actualChanges,
                c => c.PreviousStateId == oldFilterMetas["O7CLF"].Id
                     && c.CurrentStateId is null);

            // Deleted
            Assert.Single(actualChanges,
                c => c.PreviousStateId == oldFilterMetas["7zXob"].Id
                     && c.CurrentStateId is null);
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
                            .SetPublicId("dP0Zw"))
                        .ForIndex(1, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("O7CLF"))
                        .ForIndex(2, s =>
                            s.SetOption(DataFixture.DefaultFilterOptionMeta())
                            .SetPublicId("7zXob"))
                        .Generate(3)))
                .GenerateList(2);

            var newFilterMeta = DataFixture.DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0]))
                .ForIndex(1, UnchangedFilterMetaSetter(oldFilterMeta[1]))
                .GenerateList(2);

            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                oldFilterMeta: oldFilterMeta,
                newFilterMeta: newFilterMeta);

            await CreateChanges(instanceId);

            var actualChanges = await GetFilterMetaChanges(newVersion);

            Assert.Empty(actualChanges);
        }

        private static Action<InstanceSetters<FilterMeta>> UnchangedFilterMetaSetter(
            FilterMeta filterMeta,
            Action<InstanceSetters<FilterMeta>>)
        {
            return s =>
            {
                s.SetPublicId(filterMeta.PublicId);
                s.SetColumn(filterMeta.Column);
                s.SetLabel(filterMeta.Label);
                s.SetHint(filterMeta.Hint);

                s.SetOptionLinks();
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

        private async Task<(DataSetVersion originalVersion, DataSetVersion newVersion, Guid instanceId)> CreateDataSetInitialAndNextVersion(
            List<FilterMeta> oldFilterMeta,
            List<FilterMeta> newFilterMeta)
        {
            return await CreateDataSetInitialAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    FilterMetas = oldFilterMeta
                },
                nextVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    FilterMetas = newFilterMeta
                });
        }
    }

    public class CreateChangesLocationTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : CreateChangesTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            // SCENARIOS WE TEST:
            // location DELETED, with all options DELETED. ===> Should just have 1 change for the DELETED location.
            // location ADDED, with ADDED options. ===> Should have 1 change for the ADDED location, and 1 change for EACH of the ADDED options.
            // location UNCHANGED, options UNCHANGED. ===> Should have NO changes.
            // location UNCHANGED, with some DELETED options and some ADDED options. ===> Should have 1 change for EACH of the DELETED options and EACH of the ADDED options.
            // location UNCHANGED, SOME options CHANGED. ===> Should have 1 change for EACH of the CHANGED options.

            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage());

            // Here we define what changes have occurred between the original and new data set versions.
            // The metadata and mappings are then automatically calculated and stored from this, which can then be used
            // to create the changes.
            List<LocationChange> allChanges =
            [
                new()
                {
                    ParentIdentifier = GeographicLevel.LocalAuthority,
                    ChangeType = ChangeType.Deleted,
                    OptionChanges =
                    [
                        new OptionChange
                        {
                            ChangeType = ChangeType.Deleted,
                            OptionIndex = 0
                        },
                        new OptionChange
                        {
                            ChangeType = ChangeType.Deleted,
                            OptionIndex = 1
                        }
                    ]
                },
                new()
                {
                    ParentIdentifier = GeographicLevel.School,
                    ChangeType = ChangeType.Added,
                    OptionChanges =
                    [
                        new OptionChange
                        {
                            ChangeType = ChangeType.Added,
                            OptionIndex = 0
                        },
                        new OptionChange
                        {
                            ChangeType = ChangeType.Added,
                            OptionIndex = 1
                        }
                    ]
                },
                new()
                {
                    ParentIdentifier = GeographicLevel.Country,
                    ChangeType = ChangeType.Unchanged,
                    OptionChanges =
                    [
                        new OptionChange
                        {
                            ChangeType = ChangeType.Unchanged,
                            OptionIndex = 0
                        },
                        new OptionChange
                        {
                            ChangeType = ChangeType.Unchanged,
                            OptionIndex = 1
                        }
                    ]
                },
                new()
                {
                    ParentIdentifier = GeographicLevel.RscRegion,
                    ChangeType = ChangeType.Unchanged,
                    OptionChanges =
                    [
                        new OptionChange
                        {
                            ChangeType = ChangeType.Unchanged,
                            OptionIndex = 0
                        },
                        new OptionChange
                        {
                            ChangeType = ChangeType.Deleted,
                            OptionIndex = 1
                        },
                        new OptionChange
                        {
                            ChangeType = ChangeType.Added,
                            OptionIndex = 1
                        }
                    ]
                },
                new()
                {
                    ParentIdentifier = GeographicLevel.Provider,
                    ChangeType = ChangeType.Unchanged,
                    OptionChanges =
                    [
                        new OptionChange
                        {
                            ChangeType = ChangeType.Unchanged,
                            OptionIndex = 0
                        },
                        new OptionChange
                        {
                            ChangeType = ChangeType.Changed,
                            OptionIndex = 1
                        },
                        new OptionChange
                        {
                            ChangeType = ChangeType.Changed,
                            OptionIndex = 2
                        }
                    ]
                }
            ];

            var (originalVersionLocationMetasWithLinksByLevel, newVersionLocationMetasWithLinksByLevel) =
                await CreateMeta(
                    allChanges: allChanges,
                    originalVersion: originalVersion,
                    newVersion: newVersion);

            await CreateMappings(
                allChanges: allChanges,
                originalVersion: originalVersion,
                newVersion: newVersion,
                originalVersionLocationMetasWithLinksByLevel: originalVersionLocationMetasWithLinksByLevel,
                newVersionLocationMetasWithLinksByLevel: newVersionLocationMetasWithLinksByLevel);

            await CreateChanges(instanceId);

            var newVersionWithChanges = await GetDbContext<PublicDataDbContext>()
                .DataSetVersions
                .AsNoTracking()
                .Include(dsv => dsv.LocationMetaChanges)
                .Include(dsv => dsv.LocationOptionMetaChanges)
                .SingleAsync(dsv => dsv.Id == newVersion.Id);

            // Should only have 2 LocationMetaChanges:
            // 1 for the DELETED 'LocalAuthority' level
            // 1 for the ADDED 'School' level.
            Assert.Equal(2, newVersionWithChanges.LocationMetaChanges.Count);

            // Should have 6 LocationOptionMetaChanges:
            // 2 for the 2 ADDED 'School' options
            // 1 for the ADDED 'RscRegion' option
            // 1 for the DELETED 'RscRegion' option
            // 2 for the 2 CHANGED 'Provider' options
            Assert.Equal(6, newVersionWithChanges.LocationOptionMetaChanges.Count);

            // All changes should be for the new data set version
            Assert.All(newVersionWithChanges.LocationMetaChanges,
                lmc => Assert.Equal(newVersionWithChanges.Id, lmc.DataSetVersionId));
            Assert.All(newVersionWithChanges.LocationOptionMetaChanges,
                lomc => Assert.Equal(newVersionWithChanges.Id, lomc.DataSetVersionId));

            foreach (var parentChange in allChanges)
            {
                originalVersionLocationMetasWithLinksByLevel.TryGetValue(parentChange.ParentIdentifier,
                    out var originalLocationMeta);
                newVersionLocationMetasWithLinksByLevel.TryGetValue(parentChange.ParentIdentifier,
                    out var newLocationMeta);

                // If the parent level has been DELETED, there should just be 1 change for this but none for the options - hence, we exit early
                if (parentChange.ChangeType is ChangeType.Deleted)
                {
                    Assert.Single(newVersionWithChanges.LocationMetaChanges,
                        lmc =>
                            lmc.PreviousStateId == originalLocationMeta!.Id
                            && lmc.CurrentStateId == null);

                    continue;
                }

                // If the parent level has been ADDED, there should be 1 change for this. There will also be changes for the options - hence, we DON'T exit early
                if (parentChange.ChangeType is ChangeType.Added)
                {
                    Assert.Single(newVersionWithChanges.LocationMetaChanges,
                        lmc =>
                            lmc.PreviousStateId == null
                            && lmc.CurrentStateId == newLocationMeta!.Id);
                }

                foreach (var optionChange in parentChange.OptionChanges)
                {
                    // If the option is UNCHANGED, there should be no change for this
                    if (optionChange.ChangeType is ChangeType.Unchanged)
                    {
                        continue;
                    }

                    var originalOptionLink = originalLocationMeta?.OptionLinks[optionChange.OptionIndex];
                    var newOptionLink = newLocationMeta?.OptionLinks[optionChange.OptionIndex];

                    switch (optionChange.ChangeType)
                    {
                        // If the option has been DELETED, ADDED or CHANGED, there should be 1 change for this
                        case ChangeType.Deleted:
                            Assert.Single(newVersionWithChanges.LocationOptionMetaChanges,
                                lmc =>
                                    lmc.PreviousState?.MetaId == originalOptionLink!.MetaId
                                    && lmc.PreviousState?.OptionId == originalOptionLink.OptionId
                                    && lmc.PreviousState?.PublicId == originalOptionLink.PublicId
                                    && lmc.CurrentState == null);
                            break;
                        case ChangeType.Added:
                            Assert.Single(newVersionWithChanges.LocationOptionMetaChanges,
                                lmc =>
                                    lmc.PreviousState == null
                                    && lmc.CurrentState?.MetaId == newOptionLink!.MetaId
                                    && lmc.CurrentState?.OptionId == newOptionLink.OptionId
                                    && lmc.CurrentState?.PublicId == newOptionLink.PublicId);
                            break;
                        case ChangeType.Changed:
                            Assert.Single(newVersionWithChanges.LocationOptionMetaChanges,
                                lmc =>
                                    lmc.PreviousState?.MetaId == originalOptionLink!.MetaId
                                    && lmc.PreviousState?.OptionId == originalOptionLink.OptionId
                                    && lmc.PreviousState?.PublicId == originalOptionLink.PublicId
                                    && lmc.CurrentState?.MetaId == newOptionLink!.MetaId
                                    && lmc.CurrentState?.OptionId == newOptionLink.OptionId
                                    && lmc.CurrentState?.PublicId == newOptionLink.PublicId);
                            break;
                    }
                }
            }
        }

        private async Task<(
                IReadOnlyDictionary<GeographicLevel, LocationMeta> originalVersionLocationMetasWithLinksByLevel,
                IReadOnlyDictionary<GeographicLevel, LocationMeta> newVersionLocationMetasWithLinksByLevel)>
            CreateMeta(
                IReadOnlyList<LocationChange> allChanges,
                DataSetVersion originalVersion,
                DataSetVersion newVersion)
        {
            var oldGeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta()
                .WithDataSetVersionId(originalVersion.Id);
            await CreateMeta(geographicLevelMeta: oldGeographicLevelMeta);

            var oldLocationMetasByLevel = allChanges
                .Where(pc => pc.ChangeType is ChangeType.Unchanged or ChangeType.Deleted)
                .Select(pc => DataFixture
                    .DefaultLocationMeta()
                    .WithDataSetVersionId(originalVersion.Id)
                    .WithLevel(pc.ParentIdentifier)
                    .Generate())
                .ToDictionary(lm => lm.Level);

            await CreateMeta(locationMetas: oldLocationMetasByLevel.Values);

            var newGeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta()
                .WithDataSetVersionId(newVersion.Id);
            await CreateMeta(geographicLevelMeta: newGeographicLevelMeta);

            var newLocationMetasByLevel = allChanges
                .Where(pc => pc.ChangeType is ChangeType.Unchanged or ChangeType.Added)
                .Select(pc => DataFixture
                    .DefaultLocationMeta()
                    .WithDataSetVersionId(newVersion.Id)
                    .WithLevel(pc.ParentIdentifier)
                    .Generate())
                .ToDictionary(lm => lm.Level);

            await CreateMeta(locationMetas: newLocationMetasByLevel.Values);

            var oldLocationOptionMetaLinks = new List<LocationOptionMetaLink>();

            foreach (var parentChange in allChanges)
            {
                foreach (var optionChange in parentChange.OptionChanges)
                {
                    if (optionChange.ChangeType
                        is ChangeType.Unchanged
                        or ChangeType.Changed
                        or ChangeType.Deleted)
                    {
                        var metaId = oldLocationMetasByLevel[parentChange.ParentIdentifier].Id;

                        oldLocationOptionMetaLinks.Add(CreateAddedLocationOptionLink(
                            level: parentChange.ParentIdentifier,
                            metaId: metaId));
                    }
                }
            }

            await CreateMeta(locationOptionMetaLinks: oldLocationOptionMetaLinks);

            var originalVersionLocationMetasWithLinksByLevel = (await GetLocationMeta(originalVersion))
                .ToDictionary(lm => lm.Level);

            var newLocationOptionMetaLinks = new List<LocationOptionMetaLink>();

            foreach (var parentChange in allChanges)
            {
                if (parentChange.ChangeType is ChangeType.Deleted)
                {
                    continue;
                }

                originalVersionLocationMetasWithLinksByLevel.TryGetValue(parentChange.ParentIdentifier,
                    out var originalLocationMeta);
                var newLocationMeta = newLocationMetasByLevel[parentChange.ParentIdentifier];

                foreach (var optionChange in parentChange.OptionChanges)
                {
                    if (optionChange.ChangeType is ChangeType.Deleted)
                    {
                        continue;
                    }

                    var metaId = newLocationMeta.Id;

                    if (optionChange.ChangeType is ChangeType.Added)
                    {
                        newLocationOptionMetaLinks.Add(CreateAddedLocationOptionLink(
                            level: parentChange.ParentIdentifier,
                            metaId: metaId));

                        continue;
                    }

                    if (optionChange.ChangeType is ChangeType.Changed)
                    {
                        newLocationOptionMetaLinks.Add(CreateChangedLocationOptionLink(
                            level: parentChange.ParentIdentifier,
                            metaId: metaId,
                            originalLocationMeta: originalLocationMeta!,
                            optionChange: optionChange));

                        continue;
                    }

                    newLocationOptionMetaLinks.Add(CreateUnchangedLocationOptionLink(
                        metaId: metaId,
                        originalLocationMeta: originalLocationMeta!,
                        optionChange: optionChange));
                }
            }

            await CreateMeta(locationOptionMetaLinks: newLocationOptionMetaLinks);

            var newVersionLocationMetasWithLinksByLevel = (await GetLocationMeta(newVersion))
                .ToDictionary(lm => lm.Level); // Re-fetching to get the links back this time

            return (originalVersionLocationMetasWithLinksByLevel, newVersionLocationMetasWithLinksByLevel);
        }

        private LocationOptionMetaLink CreateAddedLocationOptionLink(GeographicLevel level, int metaId)
        {
            switch (level)
            {
                case GeographicLevel.LocalAuthority:
                    return DataFixture
                        .DefaultLocationOptionMetaLink()
                        .WithOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                        .WithMetaId(metaId);
                case GeographicLevel.School:
                    return DataFixture
                        .DefaultLocationOptionMetaLink()
                        .WithOption(DataFixture.DefaultLocationSchoolOptionMeta())
                        .WithMetaId(metaId);
                case GeographicLevel.RscRegion:
                    return DataFixture
                        .DefaultLocationOptionMetaLink()
                        .WithOption(DataFixture.DefaultLocationRscRegionOptionMeta())
                        .WithMetaId(metaId);
                case GeographicLevel.Provider:
                    return DataFixture
                        .DefaultLocationOptionMetaLink()
                        .WithOption(DataFixture.DefaultLocationProviderOptionMeta())
                        .WithMetaId(metaId);
                default:
                    return DataFixture
                        .DefaultLocationOptionMetaLink()
                        .WithOption(DataFixture.DefaultLocationCodedOptionMeta())
                        .WithMetaId(metaId);
            }
        }

        private LocationOptionMetaLink CreateChangedLocationOptionLink(
            GeographicLevel level,
            int metaId,
            LocationMeta originalLocationMeta,
            OptionChange optionChange)
        {
            switch (level)
            {
                case GeographicLevel.LocalAuthority:
                    return DataFixture
                        .DefaultLocationOptionMetaLink()
                        .WithOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                        .WithMetaId(metaId)
                        .WithPublicId(originalLocationMeta!.OptionLinks[optionChange.OptionIndex].PublicId);
                case GeographicLevel.School:
                    return DataFixture
                        .DefaultLocationOptionMetaLink()
                        .WithOption(DataFixture.DefaultLocationSchoolOptionMeta())
                        .WithMetaId(metaId)
                        .WithPublicId(originalLocationMeta.OptionLinks[optionChange.OptionIndex].PublicId);
                case GeographicLevel.RscRegion:
                    return DataFixture
                        .DefaultLocationOptionMetaLink()
                        .WithOption(DataFixture.DefaultLocationRscRegionOptionMeta())
                        .WithMetaId(metaId)
                        .WithPublicId(originalLocationMeta.OptionLinks[optionChange.OptionIndex].PublicId);
                case GeographicLevel.Provider:
                    return DataFixture
                        .DefaultLocationOptionMetaLink()
                        .WithOption(DataFixture.DefaultLocationProviderOptionMeta())
                        .WithMetaId(metaId)
                        .WithPublicId(originalLocationMeta.OptionLinks[optionChange.OptionIndex].PublicId);
                default:
                    return DataFixture
                        .DefaultLocationOptionMetaLink()
                        .WithOption(DataFixture.DefaultLocationCodedOptionMeta())
                        .WithMetaId(metaId)
                        .WithPublicId(originalLocationMeta.OptionLinks[optionChange.OptionIndex].PublicId);
            }
        }

        private LocationOptionMetaLink CreateUnchangedLocationOptionLink(
            int metaId,
            LocationMeta originalLocationMeta,
            OptionChange optionChange)
        {
            return DataFixture
                .DefaultLocationOptionMetaLink()
                .WithMetaId(metaId)
                .WithOptionId(originalLocationMeta.OptionLinks[optionChange.OptionIndex].OptionId)
                .WithPublicId(originalLocationMeta.OptionLinks[optionChange.OptionIndex].PublicId);
        }

        private async Task CreateMappings(
            List<LocationChange> allChanges,
            DataSetVersion originalVersion,
            DataSetVersion newVersion,
            IReadOnlyDictionary<GeographicLevel, LocationMeta> originalVersionLocationMetasWithLinksByLevel,
            IReadOnlyDictionary<GeographicLevel, LocationMeta> newVersionLocationMetasWithLinksByLevel)
        {
            var mappedOptionCandidateKeysByOptionSourceKey = allChanges
                .SelectMany(
                    pc => pc.OptionChanges,
                    (pc, oc) => new
                    {
                        GeographicLevel = pc.ParentIdentifier,
                        OptionChange = oc
                    })
                // Only 'Unchanged' and 'Changed' change types correspond to an option which can be mapped
                .Where(a => a.OptionChange.ChangeType is ChangeType.Unchanged or ChangeType.Changed)
                .ToDictionary(
                    a => MappingKeyGenerators.LocationOptionMetaLink(
                        originalVersionLocationMetasWithLinksByLevel[a.GeographicLevel]
                            .OptionLinks[a.OptionChange.OptionIndex]),
                    a => MappingKeyGenerators.LocationOptionMetaLink(
                        newVersionLocationMetasWithLinksByLevel[a.GeographicLevel]
                            .OptionLinks[a.OptionChange.OptionIndex]));

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(newVersion.Id)
                .WithLocationMappingPlan(
                    DataFixture.LocationMappingPlanFromLocationMeta(
                        sourceLocations: [.. originalVersionLocationMetasWithLinksByLevel.Values],
                        targetLocations: [.. newVersionLocationMetasWithLinksByLevel.Values],
                        mappedOptionCandidateKeysByOptionSourceKey: mappedOptionCandidateKeysByOptionSourceKey));

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mappings));
        }

        private async Task<IReadOnlyList<LocationMeta>> GetLocationMeta(DataSetVersion version)
        {
            return await GetDbContext<PublicDataDbContext>()
                .LocationMetas
                .Include(dsv => dsv.Options)
                .Include(dsv => dsv.OptionLinks)
                .Where(lm => lm.DataSetVersionId == version.Id)
                .ToListAsync();
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
        public async Task IndicatorsAddedAndDeleted_ChangesContainAdditionsAndDeletions()
        {
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    IndicatorMetas = DataFixture.DefaultIndicatorMeta()
                        .ForIndex(0, s => s.SetPublicId("dP0Zw"))
                        .ForIndex(1, s => s.SetPublicId("O7CLF"))
                        .ForIndex(2, s => s.SetPublicId("7zXob"))
                        .Generate(3)
                },
                nextVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    IndicatorMetas = DataFixture.DefaultIndicatorMeta()
                        .ForIndex(0, s => s.SetPublicId("7zXob"))
                        .ForIndex(1, s => s.SetPublicId("pTSoj"))
                        .ForIndex(2, s => s.SetPublicId("IzBzg"))
                        .Generate(3)
                });

            await CreateChanges(instanceId);

            var actualChanges = await GetDbContext<PublicDataDbContext>()
                .IndicatorMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == newVersion.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();

            Assert.Equal(4, actualChanges.Count);
            Assert.All(actualChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldIndicatorMetas = originalVersion.IndicatorMetas
                .ToDictionary(m => m.PublicId);

            Assert.Equal(oldIndicatorMetas["dP0Zw"].Id, actualChanges[0].PreviousStateId);
            Assert.Null(actualChanges[0].CurrentStateId);

            Assert.Equal(oldIndicatorMetas["O7CLF"].Id, actualChanges[1].PreviousStateId);
            Assert.Null(actualChanges[1].CurrentStateId);

            var newIndicatorMetas = newVersion.IndicatorMetas
                .ToDictionary(m => m.PublicId);

            Assert.Null(actualChanges[2].PreviousStateId);
            Assert.Equal(newIndicatorMetas["pTSoj"].Id, actualChanges[2].CurrentStateId);

            Assert.Null(actualChanges[3].PreviousStateId);
            Assert.Equal(newIndicatorMetas["IzBzg"].Id, actualChanges[3].CurrentStateId);
        }

        [Fact]
        public async Task IndicatorsUnchanged_ChangesAreEmpty()
        {
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    IndicatorMetas = DataFixture.DefaultIndicatorMeta()
                        .ForIndex(0, s => s.SetPublicId("dP0Zw"))
                        .ForIndex(1, s => s.SetPublicId("O7CLF"))
                        .ForIndex(2, s => s.SetPublicId("7zXob"))
                        .Generate(3)
                },
                nextVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    IndicatorMetas = DataFixture.DefaultIndicatorMeta()
                        .ForIndex(0, s => s.SetPublicId("dP0Zw"))
                        .ForIndex(1, s => s.SetPublicId("O7CLF"))
                        .ForIndex(2, s => s.SetPublicId("7zXob"))
                        .Generate(3)
                });

            await CreateChanges(instanceId);

            Assert.False(await GetDbContext<PublicDataDbContext>()
                .IndicatorMetaChanges
                .AnyAsync(c => c.DataSetVersionId == newVersion.Id));
        }

        [Fact]
        public async Task IndicatorsAdded_ChangesContainOnlyAdditions()
        {
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    IndicatorMetas = DataFixture.DefaultIndicatorMeta()
                        .WithPublicId("dP0Zw")
                        .Generate(1)
                },
                nextVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    IndicatorMetas = DataFixture.DefaultIndicatorMeta()
                        .ForIndex(0, s => s.SetPublicId("dP0Zw"))
                        .ForIndex(1, s => s.SetPublicId("O7CLF"))
                        .ForIndex(2, s => s.SetPublicId("7zXob"))
                        .Generate(3)
                });

            await CreateChanges(instanceId);

            var actualChanges = await GetDbContext<PublicDataDbContext>()
                .IndicatorMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == newVersion.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();

            Assert.Equal(2, actualChanges.Count);
            Assert.All(actualChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));
            Assert.All(actualChanges, c => Assert.Null(c.PreviousStateId));

            var newIndicatorMetas = newVersion.IndicatorMetas
                .ToDictionary(m => m.PublicId);

            Assert.Equal(newIndicatorMetas["O7CLF"].Id, actualChanges[0].CurrentStateId);
            Assert.Equal(newIndicatorMetas["7zXob"].Id, actualChanges[1].CurrentStateId);
        }

        [Fact]
        public async Task IndicatorsDeleted_ChangesContainOnlyDeletions()
        {
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    IndicatorMetas = DataFixture.DefaultIndicatorMeta()
                        .ForIndex(0, s => s.SetPublicId("dP0Zw"))
                        .ForIndex(1, s => s.SetPublicId("O7CLF"))
                        .ForIndex(2, s => s.SetPublicId("7zXob"))
                        .Generate(3)
                },
                nextVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    IndicatorMetas = DataFixture.DefaultIndicatorMeta()
                        .WithPublicId("dP0Zw")
                        .Generate(1)
                });

            await CreateChanges(instanceId);

            var actualChanges = await GetDbContext<PublicDataDbContext>()
                .IndicatorMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == newVersion.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();

            Assert.Equal(2, actualChanges.Count);
            Assert.All(actualChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));
            Assert.All(actualChanges, c => Assert.Null(c.CurrentStateId));

            var oldIndicatorMetas = originalVersion.IndicatorMetas
                .ToDictionary(m => m.PublicId);

            Assert.Equal(oldIndicatorMetas["O7CLF"].Id, actualChanges[0].PreviousStateId);
            Assert.Equal(oldIndicatorMetas["7zXob"].Id, actualChanges[1].PreviousStateId);
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

    private async Task CreateMeta(
        IEnumerable<FilterMeta>? filterMetas = null,
        IEnumerable<FilterOptionMetaLink>? filterOptionMetaLinks = null,
        IEnumerable<LocationMeta>? locationMetas = null,
        IEnumerable<LocationOptionMetaLink>? locationOptionMetaLinks = null,
        GeographicLevelMeta? geographicLevelMeta = null,
        IEnumerable<IndicatorMeta>? indicatorMetas = null,
        IEnumerable<TimePeriodMeta>? timePeriodMetas = null)
    {
        await AddTestData<PublicDataDbContext>(context =>
        {
            if (filterMetas is not null)
            {
                context.FilterMetas.AddRange(filterMetas);
            }

            if (filterOptionMetaLinks is not null)
            {
                context.FilterOptionMetaLinks.AddRange(filterOptionMetaLinks);
            }

            if (locationMetas is not null)
            {
                context.LocationMetas.AddRange(locationMetas);
            }

            if (locationOptionMetaLinks is not null)
            {
                context.LocationOptionMetaLinks.AddRange(locationOptionMetaLinks);
            }

            if (geographicLevelMeta is not null)
            {
                context.GeographicLevelMetas.Add(geographicLevelMeta);
            }

            if (indicatorMetas is not null)
            {
                context.IndicatorMetas.AddRange(indicatorMetas);
            }

            if (timePeriodMetas is not null)
            {
                context.TimePeriodMetas.AddRange(timePeriodMetas);
            }
        });
    }

    // These classes are to make setting up the tests easier and safer to change.
    // We define what changes we want the hypothetical new data set version to have, relative to its original version, and
    // the metadata and mappings are then worked out for us.
    private abstract record ParentChange<TParentIdentifier>
    {
        public required TParentIdentifier ParentIdentifier { get; init; }
        public required ChangeType ChangeType { get; init; }
        public required IReadOnlyList<OptionChange> OptionChanges { get; init; }
    }

    private record OptionChange
    {
        public required ChangeType ChangeType { get; init; }

        // This is the index of the corresponding option for the original data set version, or for the new data set version
        public required int OptionIndex { get; init; }
    }

    private record LocationChange : ParentChange<GeographicLevel>;

    private record FilterChange : ParentChange<string>
    {
        // This is the index of the corresponding filter for the original data set version, or for the new data set version
        public required int FilterIndex { get; init; }
    }

    private enum ChangeType
    {
        Unchanged, // This is essentially an option that has been auto-mapped
        Changed, // This is an option that has had its label changed, and has been manually mapped to the new option
        Deleted, // This is an option that had been deleted
        Added // This is an option that has been added
    }
}
