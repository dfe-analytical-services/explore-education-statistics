using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
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

    public class CreateChangesTestsFilterTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : CreateChangesTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            // SCENARIOS WE TEST:
            // filter DELETED, with all options DELETED. ===> Should just have 1 change for the DELETED filter.
            // filter ADDED, with ADDED options. ===> Should have 1 change for the ADDED filter, and 1 change for EACH of the ADDED options.
            // filter UNCHANGED, options UNCHANGED. ===> Should have NO changes.
            // filter UNCHANGED, with some DELETED options and some ADDED options. ===> Should have 1 change for EACH of the DELETED options and EACH of the ADDED options.
            // filter UNCHANGED, SOME options CHANGED. ===> Should have 1 change for EACH of the CHANGED options.
            // filter CHANGED, options UNCHANGED. ===> Should just have 1 change for the CHANGED filter.
            // filter CHANGED + SOME options CHANGED. ===> Should have 1 change for the CHANGED filter, and 1 change for EACH of the CHANGED options.

            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialVersionAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage());

            // Here we define what changes have occurred between the original and new data set versions.
            // The metadata and mappings are then automatically calculated and stored from this, which can then be used
            // to create the changes.
            List<FilterChange> allChanges =
            [
                new()
                {
                    ParentIdentifier = "filter 1",
                    ChangeType = ChangeType.Deleted,
                    FilterIndex = 0,
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
                    ParentIdentifier = "filter 2",
                    ChangeType = ChangeType.Added,
                    FilterIndex = 0,
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
                    ParentIdentifier = "filter 3",
                    ChangeType = ChangeType.Unchanged,
                    FilterIndex = 1,
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
                    ParentIdentifier = "filter 4",
                    ChangeType = ChangeType.Unchanged,
                    FilterIndex = 2,
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
                    ParentIdentifier = "filter 5",
                    ChangeType = ChangeType.Unchanged,
                    FilterIndex = 3,
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
                },
                new()
                {
                    ParentIdentifier = "filter 6",
                    ChangeType = ChangeType.Changed,
                    FilterIndex = 4,
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
                    ParentIdentifier = "filter 7",
                    ChangeType = ChangeType.Changed,
                    FilterIndex = 5,
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

            var (originalVersionLocationMetasWithLinksByPublicId, newVersionLocationMetasWithLinksByPublicId) =
                await CreateMeta(
                    allChanges: allChanges,
                    originalVersion: originalVersion,
                    newVersion: newVersion);

            await CreateMappings(
                allChanges: allChanges,
                originalVersion: originalVersion,
                newVersion: newVersion,
                originalVersionFilterMetasWithLinksByPublicId: originalVersionLocationMetasWithLinksByPublicId,
                newVersionFilterMetasWithLinksByPublicId: newVersionLocationMetasWithLinksByPublicId);

            await CreateChanges(instanceId);

            var newVersionWithChanges = await GetVersionWithChanges(newVersion);

            // Should have 2 FilterMetaChanges:
            // 1 for the DELETED 'filter 1'
            // 1 for the ADDED 'filter 2'.
            // NOTE: At the moment we are unable to detect FILTER mappings due to their public ID also being their key.
            // We also don't currently have the functionality to map top-level filters either. So here, we are just checking for 2 changes.
            // In the future, when we allow for top-level filter mappings, this will check for the other 2 changes from 'filter 6' and
            // 'filter 7' too.
            Assert.Equal(2, newVersionWithChanges.FilterMetaChanges.Count);

            // Should have 8 FilterOptionMetaChanges:
            // 2 for the 2 ADDED 'filter 2' options
            // 1 for the ADDED 'filter 4' option
            // 1 for the DELETED 'filter 4' option
            // 2 for the 2 CHANGED 'filter 5' options
            // 2 for the 2 CHANGED 'filter 7' options
            Assert.Equal(8, newVersionWithChanges.FilterOptionMetaChanges.Count);

            // All changes should be for the new data set version
            Assert.All(newVersionWithChanges.FilterMetaChanges,
                c => Assert.Equal(newVersionWithChanges.Id, c.DataSetVersionId));
            Assert.All(newVersionWithChanges.FilterOptionMetaChanges,
                c => Assert.Equal(newVersionWithChanges.Id, c.DataSetVersionId));

            foreach (var parentChange in allChanges)
            {
                originalVersionLocationMetasWithLinksByPublicId.TryGetValue(parentChange.ParentIdentifier,
                    out var originalFilterMeta);
                newVersionLocationMetasWithLinksByPublicId.TryGetValue(parentChange.ParentIdentifier,
                    out var newFilterMeta);

                // If the parent level has been DELETED, there should just be 1 change for this but none for the options - hence, we exit early
                if (parentChange.ChangeType is ChangeType.Deleted)
                {
                    Assert.Single(newVersionWithChanges.FilterMetaChanges,
                        lmc =>
                            lmc.PreviousStateId == originalFilterMeta!.Id
                            && lmc.CurrentStateId == null);

                    continue;
                }

                // If the parent level has been ADDED, there should be 1 change for this. There will also be changes for the options - hence, we DON'T exit early
                if (parentChange.ChangeType is ChangeType.Added)
                {
                    Assert.Single(newVersionWithChanges.FilterMetaChanges,
                        lmc =>
                            lmc.PreviousStateId == null
                            && lmc.CurrentStateId == newFilterMeta!.Id);
                }

                foreach (var optionChange in parentChange.OptionChanges)
                {
                    // If the option is UNCHANGED, there should be no change for this
                    if (optionChange.ChangeType is ChangeType.Unchanged)
                    {
                        continue;
                    }

                    var originalOptionLink = originalFilterMeta?.OptionLinks[optionChange.OptionIndex];
                    var newOptionLink = newFilterMeta?.OptionLinks[optionChange.OptionIndex];

                    switch (optionChange.ChangeType)
                    {
                        // If the option has been DELETED, ADDED or CHANGED, there should be 1 change for this
                        case ChangeType.Deleted:
                            Assert.Single(newVersionWithChanges.FilterOptionMetaChanges,
                                lmc =>
                                    lmc.PreviousState?.MetaId == originalOptionLink!.MetaId
                                    && lmc.PreviousState?.OptionId == originalOptionLink.OptionId
                                    && lmc.PreviousState?.PublicId == originalOptionLink.PublicId
                                    && lmc.CurrentState == null);
                            break;
                        case ChangeType.Added:
                            Assert.Single(newVersionWithChanges.FilterOptionMetaChanges,
                                lmc =>
                                    lmc.PreviousState == null
                                    && lmc.CurrentState?.MetaId == newOptionLink!.MetaId
                                    && lmc.CurrentState?.OptionId == newOptionLink.OptionId
                                    && lmc.CurrentState?.PublicId == newOptionLink.PublicId);
                            break;
                        case ChangeType.Changed:
                            Assert.Single(newVersionWithChanges.FilterOptionMetaChanges,
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
                IReadOnlyDictionary<string, FilterMeta> originalVersionFilterMetasWithLinksByPublicId,
                IReadOnlyDictionary<string, FilterMeta> newVersionFilterMetasWithLinksByPublicId)>
            CreateMeta(
                IReadOnlyList<FilterChange> allChanges,
                DataSetVersion originalVersion,
                DataSetVersion newVersion)
        {
            var oldGeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta()
                .WithDataSetVersionId(originalVersion.Id);
            await CreateMeta(geographicLevelMeta: oldGeographicLevelMeta);

            var oldFilterMetasByPublicId = allChanges
                .Where(pc => pc.ChangeType is ChangeType.Unchanged or ChangeType.Changed or ChangeType.Deleted)
                .Select(pc => CreateAddedFilter(
                    versionId: originalVersion.Id,
                    parentChange: pc))
                .ToDictionary(lm => lm.PublicId);

            var oldFilterMetas = oldFilterMetasByPublicId.Values.ToList();

            await CreateMeta(filterMetas: oldFilterMetas);

            var newGeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta()
                .WithDataSetVersionId(newVersion.Id);
            await CreateMeta(geographicLevelMeta: newGeographicLevelMeta);

            var newFilterMetasByPublicId = allChanges
                .Where(pc => pc.ChangeType is ChangeType.Unchanged or ChangeType.Changed or ChangeType.Added)
                .Select(pc =>
                {
                    if (pc.ChangeType is ChangeType.Added)
                    {
                        return CreateAddedFilter(
                            versionId: newVersion.Id,
                            parentChange: pc);
                    }

                    if (pc.ChangeType is ChangeType.Changed)
                    {
                        return CreateChangedFilter(
                            versionId: newVersion.Id,
                            oldFilterMetas: oldFilterMetas,
                            parentChange: pc);
                    }

                    return CreateUnchangedFilter(
                        versionId: newVersion.Id,
                        oldFilterMetas: oldFilterMetas,
                        parentChange: pc);
                })
                .ToDictionary(lm => lm.PublicId);

            await CreateMeta(filterMetas: newFilterMetasByPublicId.Values);

            // The above is SLIGHTLY different from that for locations

            var oldFilterOptionMetaLinks = new List<FilterOptionMetaLink>();

            foreach (var parentChange in allChanges)
            {
                foreach (var optionChange in parentChange.OptionChanges)
                {
                    if (optionChange.ChangeType
                        is ChangeType.Unchanged
                        or ChangeType.Changed
                        or ChangeType.Deleted)
                    {
                        var metaId = oldFilterMetasByPublicId[parentChange.ParentIdentifier].Id;

                        oldFilterOptionMetaLinks.Add(CreateAddedFilterOptionLink(metaId));
                    }
                }
            }

            await CreateMeta(filterOptionMetaLinks: oldFilterOptionMetaLinks);

            // The above could POTENTIALLY be refactored out from both filters and locations

            var originalVersionFilterMetasWithLinksByPublicId = (await GetFilterMeta(originalVersion))
                .ToDictionary(lm => lm.PublicId);

            var newFilterOptionMetaLinks = new List<FilterOptionMetaLink>();

            foreach (var parentChange in allChanges)
            {
                if (parentChange.ChangeType is ChangeType.Deleted)
                {
                    continue;
                }

                originalVersionFilterMetasWithLinksByPublicId.TryGetValue(parentChange.ParentIdentifier,
                    out var originalFilterMeta);
                var newFilterMeta = newFilterMetasByPublicId[parentChange.ParentIdentifier];

                foreach (var optionChange in parentChange.OptionChanges)
                {
                    if (optionChange.ChangeType is ChangeType.Deleted)
                    {
                        continue;
                    }

                    var metaId = newFilterMeta.Id;

                    if (optionChange.ChangeType is ChangeType.Added)
                    {
                        newFilterOptionMetaLinks.Add(CreateAddedFilterOptionLink(metaId));

                        continue;
                    }

                    if (optionChange.ChangeType is ChangeType.Changed)
                    {
                        newFilterOptionMetaLinks.Add(CreateChangedFilterOptionLink(
                            metaId: metaId,
                            originalFilterMeta: originalFilterMeta!,
                            optionChange: optionChange));

                        continue;
                    }

                    newFilterOptionMetaLinks.Add(CreateUnchangedFilterOptionLink(
                        metaId: metaId,
                        originalFilterMeta: originalFilterMeta!,
                        optionChange: optionChange));
                }
            }

            await CreateMeta(filterOptionMetaLinks: newFilterOptionMetaLinks);

            var newVersionFilterMetasWithLinksByPublicId = (await GetFilterMeta(newVersion))
                .ToDictionary(lm => lm.PublicId); // Refetching to get the links back this time

            return (originalVersionFilterMetasWithLinksByPublicId, newVersionFilterMetasWithLinksByPublicId);

            // This can 100% be reused across the filters and locations!
        }

        private FilterMeta CreateAddedFilter(
            Guid versionId,
            FilterChange parentChange)
        {
            return DataFixture
                .DefaultFilterMeta()
                .WithDataSetVersionId(versionId)
                .WithPublicId(parentChange.ParentIdentifier);
        }

        private FilterMeta CreateChangedFilter(
            Guid versionId,
            IReadOnlyList<FilterMeta> oldFilterMetas,
            FilterChange parentChange)
        {
            return DataFixture
                .DefaultFilterMeta()
                .WithDataSetVersionId(versionId)
                .WithPublicId(oldFilterMetas[parentChange.FilterIndex].PublicId);
        }

        private FilterMeta CreateUnchangedFilter(
            Guid versionId,
            IReadOnlyList<FilterMeta> oldFilterMetas,
            FilterChange parentChange)
        {
            return DataFixture
                .DefaultFilterMeta()
                .WithDataSetVersionId(versionId)
                .WithPublicId(oldFilterMetas[parentChange.FilterIndex].PublicId)
                .WithLabel(oldFilterMetas[parentChange.FilterIndex].Label)
                .WithHint(oldFilterMetas[parentChange.FilterIndex].Hint);
        }

        private FilterOptionMetaLink CreateAddedFilterOptionLink(int metaId)
        {
            return DataFixture
                .DefaultFilterOptionMetaLink()
                .WithOption(DataFixture.DefaultFilterOptionMeta())
                .WithMetaId(metaId);
        }

        private FilterOptionMetaLink CreateChangedFilterOptionLink(
            int metaId,
            FilterMeta originalFilterMeta,
            OptionChange optionChange)
        {
            return DataFixture
                .DefaultFilterOptionMetaLink()
                .WithOption(DataFixture.DefaultFilterOptionMeta())
                .WithMetaId(metaId)
                .WithPublicId(originalFilterMeta!.OptionLinks[optionChange.OptionIndex].PublicId);
        }

        private FilterOptionMetaLink CreateUnchangedFilterOptionLink(
            int metaId,
            FilterMeta originalFilterMeta,
            OptionChange optionChange)
        {
            return DataFixture
                .DefaultFilterOptionMetaLink()
                .WithMetaId(metaId)
                .WithOptionId(originalFilterMeta!.OptionLinks[optionChange.OptionIndex].OptionId)
                .WithPublicId(originalFilterMeta!.OptionLinks[optionChange.OptionIndex].PublicId);
        }

        private async Task CreateMappings(
            List<FilterChange> allChanges,
            DataSetVersion originalVersion,
            DataSetVersion newVersion,
            IReadOnlyDictionary<string, FilterMeta> originalVersionFilterMetasWithLinksByPublicId,
            IReadOnlyDictionary<string, FilterMeta> newVersionFilterMetasWithLinksByPublicId)
        {
            var mappedFilterCandidateKeysBySourceFilterKey = allChanges
                // Only 'Unchanged' and 'Changed' change types correspond to an option which can be mapped
                .Where(pc => pc.ChangeType is ChangeType.Unchanged or ChangeType.Changed)
                .ToDictionary(
                    pc => MappingKeyGenerators.Filter(
                        originalVersionFilterMetasWithLinksByPublicId[pc.ParentIdentifier]),
                    pc => MappingKeyGenerators.Filter(newVersionFilterMetasWithLinksByPublicId[pc.ParentIdentifier]));

            var mappedOptionCandidateKeysByOptionSourceKey = allChanges
                .SelectMany(
                    pc => pc.OptionChanges,
                    (pc, oc) => new
                    {
                        PublicId = pc.ParentIdentifier,
                        OptionChange = oc
                    })
                // Only 'Unchanged' and 'Changed' change types correspond to an option which can be mapped
                .Where(a => a.OptionChange.ChangeType is ChangeType.Unchanged or ChangeType.Changed)
                .ToDictionary(
                    a => MappingKeyGenerators.FilterOptionMetaLink(
                        originalVersionFilterMetasWithLinksByPublicId[a.PublicId]
                            .OptionLinks[a.OptionChange.OptionIndex]),
                    a => MappingKeyGenerators.FilterOptionMetaLink(newVersionFilterMetasWithLinksByPublicId[a.PublicId]
                        .OptionLinks[a.OptionChange.OptionIndex]));

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(newVersion.Id)
                .WithFilterMappingPlan(
                    DataFixture.FilterMappingPlanFromFilterMeta(
                        sourceFilters: [.. originalVersionFilterMetasWithLinksByPublicId.Values],
                        targetFilters: [.. newVersionFilterMetasWithLinksByPublicId.Values],
                        mappedFilterCandidateKeysBySourceFilterKey: mappedFilterCandidateKeysBySourceFilterKey,
                        mappedOptionCandidateKeysByOptionSourceKey: mappedOptionCandidateKeysByOptionSourceKey));

            await AddTestData<PublicDataDbContext>(context => context.DataSetVersionMappings.Add(mappings));
        }

        private async Task<IReadOnlyList<FilterMeta>> GetFilterMeta(DataSetVersion version)
        {
            return await GetDbContext<PublicDataDbContext>()
                .FilterMetas
                .Include(dsv => dsv.Options)
                .Include(dsv => dsv.OptionLinks)
                .Where(lm => lm.DataSetVersionId == version.Id)
                .ToListAsync();
        }
    }

    public class CreateChangesTestsLocationTests(
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

            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialVersionAndNextVersion(
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

            var newVersionWithChanges = await GetVersionWithChanges(newVersion);

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
                        .WithPublicId(originalLocationMeta!.OptionLinks[optionChange.OptionIndex].PublicId);
                case GeographicLevel.RscRegion:
                    return DataFixture
                        .DefaultLocationOptionMetaLink()
                        .WithOption(DataFixture.DefaultLocationRscRegionOptionMeta())
                        .WithMetaId(metaId)
                        .WithPublicId(originalLocationMeta!.OptionLinks[optionChange.OptionIndex].PublicId);
                case GeographicLevel.Provider:
                    return DataFixture
                        .DefaultLocationOptionMetaLink()
                        .WithOption(DataFixture.DefaultLocationProviderOptionMeta())
                        .WithMetaId(metaId)
                        .WithPublicId(originalLocationMeta!.OptionLinks[optionChange.OptionIndex].PublicId);
                default:
                    return DataFixture
                        .DefaultLocationOptionMetaLink()
                        .WithOption(DataFixture.DefaultLocationCodedOptionMeta())
                        .WithMetaId(metaId)
                        .WithPublicId(originalLocationMeta!.OptionLinks[optionChange.OptionIndex].PublicId);
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
                .WithOptionId(originalLocationMeta!.OptionLinks[optionChange.OptionIndex].OptionId)
                .WithPublicId(originalLocationMeta!.OptionLinks[optionChange.OptionIndex].PublicId);
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

    public class CreateChangesTestsGeographicLevelTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : CreateChangesTests(fixture)
    {
        [Fact]
        public async Task GeographicLevelsAddedAndDeleted_ChangeExists()
        {
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialVersionAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionGeographicLevelMeta: DataFixture.DefaultGeographicLevelMeta()
                    .WithLevels([GeographicLevel.Country, GeographicLevel.Region, GeographicLevel.LocalAuthority]),
                nextVersionGeographicLevelMeta: DataFixture.DefaultGeographicLevelMeta()
                    .WithLevels([GeographicLevel.LocalAuthority, GeographicLevel.LocalAuthorityDistrict, GeographicLevel.School]));

            await CreateDefaultCompleteMappings(
                sourceDataSetVersionId: originalVersion.Id,
                targetDataSetVersionId: newVersion.Id);

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
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialVersionAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionGeographicLevelMeta: DataFixture.DefaultGeographicLevelMeta()
                    .WithLevels([GeographicLevel.Country, GeographicLevel.Region, GeographicLevel.LocalAuthority]),
                nextVersionGeographicLevelMeta: DataFixture.DefaultGeographicLevelMeta()
                    .WithLevels([GeographicLevel.Country, GeographicLevel.Region, GeographicLevel.LocalAuthority]));

            await CreateDefaultCompleteMappings(
                sourceDataSetVersionId: originalVersion.Id,
                targetDataSetVersionId: newVersion.Id);

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
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialVersionAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionGeographicLevelMeta: DataFixture.DefaultGeographicLevelMeta()
                    .WithLevels([GeographicLevel.Country]),
                nextVersionGeographicLevelMeta: DataFixture.DefaultGeographicLevelMeta()
                    .WithLevels([GeographicLevel.Country, GeographicLevel.Region, GeographicLevel.LocalAuthority]));

            await CreateDefaultCompleteMappings(
                sourceDataSetVersionId: originalVersion.Id,
                targetDataSetVersionId: newVersion.Id);

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
        public async Task GeographicLevelsDeleted_ChangesExists()
        {
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialVersionAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionGeographicLevelMeta: DataFixture.DefaultGeographicLevelMeta()
                    .WithLevels([GeographicLevel.Country, GeographicLevel.Region, GeographicLevel.LocalAuthority]),
                nextVersionGeographicLevelMeta: DataFixture.DefaultGeographicLevelMeta()
                    .WithLevels([GeographicLevel.Country]));

            await CreateDefaultCompleteMappings(
                sourceDataSetVersionId: originalVersion.Id,
                targetDataSetVersionId: newVersion.Id);

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

    public class CreateChangesTestsTimePeriodTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : CreateChangesTests(fixture)
    {
        [Fact]
        public async Task TimePeriodsAddedAndDeleted_ChangesContainAdditionsAndDeletions()
        {
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialVersionAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionGeographicLevelMeta: DataFixture.DefaultGeographicLevelMeta(),
                nextVersionGeographicLevelMeta: DataFixture.DefaultGeographicLevelMeta(),
                initialVersionTimePeriodMetas: DataFixture.DefaultTimePeriodMeta()
                    .WithCode(TimeIdentifier.AcademicYear)
                    .ForIndex(0, s => s.SetPeriod("2020"))
                    .ForIndex(1, s => s.SetPeriod("2021"))
                    .ForIndex(2, s => s.SetPeriod("2022"))
                    .Generate(3),
                nextVersionTimePeriodMetas: DataFixture.DefaultTimePeriodMeta()
                    .WithCode(TimeIdentifier.AcademicYear)
                    .ForIndex(0, s => s.SetPeriod("2022"))
                    .ForIndex(1, s => s.SetPeriod("2023"))
                    .ForIndex(2, s => s.SetPeriod("2024"))
                    .Generate(3));

            await CreateDefaultCompleteMappings(
                sourceDataSetVersionId: originalVersion.Id,
                targetDataSetVersionId: newVersion.Id);

            await CreateChanges(instanceId);

            var actualChanges = await GetDbContext<PublicDataDbContext>()
                .TimePeriodMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == newVersion.Id)
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
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialVersionAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionGeographicLevelMeta: DataFixture.DefaultGeographicLevelMeta(),
                nextVersionGeographicLevelMeta: DataFixture.DefaultGeographicLevelMeta(),
                initialVersionTimePeriodMetas: DataFixture.DefaultTimePeriodMeta()
                    .WithCode(TimeIdentifier.AcademicYear)
                    .ForIndex(0, s => s.SetPeriod("2019").SetCode(TimeIdentifier.AcademicYear))
                    .ForIndex(1, s => s.SetPeriod("2020").SetCode(TimeIdentifier.CalendarYear))
                    .ForIndex(2, s => s.SetPeriod("2021").SetCode(TimeIdentifier.January))
                    .Generate(3),
                nextVersionTimePeriodMetas: DataFixture.DefaultTimePeriodMeta()
                    .ForIndex(0, s => s.SetPeriod("2019").SetCode(TimeIdentifier.AcademicYear))
                    .ForIndex(1, s => s.SetPeriod("2020").SetCode(TimeIdentifier.CalendarYear))
                    .ForIndex(2, s => s.SetPeriod("2021").SetCode(TimeIdentifier.January))
                    .Generate(3));

            await CreateDefaultCompleteMappings(
                sourceDataSetVersionId: originalVersion.Id,
                targetDataSetVersionId: newVersion.Id);

            await CreateChanges(instanceId);

            var actualChanges = await GetDbContext<PublicDataDbContext>()
                .TimePeriodMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == newVersion.Id)
                .ToListAsync();

            Assert.Empty(actualChanges);
        }

        [Fact]
        public async Task TimePeriodsAdded_ChangesContainOnlyAdditions()
        {
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialVersionAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionGeographicLevelMeta: DataFixture.DefaultGeographicLevelMeta(),
                nextVersionGeographicLevelMeta: DataFixture.DefaultGeographicLevelMeta(),
                initialVersionTimePeriodMetas: DataFixture.DefaultTimePeriodMeta()
                    .WithCode(TimeIdentifier.CalendarYear)
                    .WithPeriod("2020")
                    .Generate(1),
                nextVersionTimePeriodMetas:
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
                ]);

            await CreateDefaultCompleteMappings(
                sourceDataSetVersionId: originalVersion.Id,
                targetDataSetVersionId: newVersion.Id);

            await CreateChanges(instanceId);

            var actualChanges = await GetDbContext<PublicDataDbContext>()
                .TimePeriodMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == newVersion.Id)
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
            var (originalVersion, newVersion, instanceId) = await CreateDataSetInitialVersionAndNextVersion(
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionGeographicLevelMeta: DataFixture.DefaultGeographicLevelMeta(),
                nextVersionGeographicLevelMeta: DataFixture.DefaultGeographicLevelMeta(),
                initialVersionTimePeriodMetas:
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
                ],
                nextVersionTimePeriodMetas: DataFixture.DefaultTimePeriodMeta()
                    .WithCode(TimeIdentifier.CalendarYear)
                    .WithPeriod("2020")
                    .Generate(1));

            await CreateDefaultCompleteMappings(
                sourceDataSetVersionId: originalVersion.Id,
                targetDataSetVersionId: newVersion.Id);

            await CreateChanges(instanceId);

            var actualChanges = await GetDbContext<PublicDataDbContext>()
                .TimePeriodMetaChanges
                .AsNoTracking()
                .Where(c => c.DataSetVersionId == newVersion.Id)
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
            var (_, nextDataSetVersion, instanceId) = await CreateDataSetInitialVersionAndNextVersion(
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
            var (_, nextDataSetVersion, instanceId) = await CreateDataSetInitialVersionAndNextVersion(
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

    private async Task<DataSetVersion> GetVersionWithChanges(DataSetVersion version)
    {
        return await GetDbContext<PublicDataDbContext>()
            .DataSetVersions
            .AsNoTracking()
            .Include(dsv => dsv.FilterMetaChanges)
            .Include(dsv => dsv.FilterOptionMetaChanges)
            .Include(dsv => dsv.LocationMetaChanges)
            .Include(dsv => dsv.LocationOptionMetaChanges)
            .Include(dsv => dsv.GeographicLevelMetaChange)
            .Include(dsv => dsv.IndicatorMetaChanges)
            .Include(dsv => dsv.TimePeriodMetaChanges)
            .SingleAsync(dsv => dsv.Id == version.Id);
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
