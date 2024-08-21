using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
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
using static GovUk.Education.ExploreEducationStatistics.Public.Data.Model.MappingKeyFunctions;
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

    public abstract class GenerateChangelogTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessCompletionOfNextDataSetVersionImportFunctionTests(fixture)
    {
        protected const DataSetVersionImportStage Stage = DataSetVersionImportStage.GeneratingChangelog;

        protected async Task GenerateChangelog(Guid instanceId)
        {
            var function = GetRequiredService<ProcessCompletionOfNextDataSetVersionFunction>();
            await function.GenerateChangelog(instanceId, CancellationToken.None);
        }
    }

    // When we say 'Not mapped' we actually mean it is auto-mapped to the SAME candidate key

    // WHAT TO TEST FOR:
    // Entire FILTER deleted, with all options deleted. ===> Should just have 1 changelog for the deletion of the FILTER.
    // Entire FILTER added, with new options. ===> Should have 1 changelog for the FILTER, and 1 changelog for EACH of the new OPTIONS.
    // filter NOT mapped, NO options mapped. ===> Should have NO changelogs.
    // filter NOT mapped, with some deleted OPTIONS and some new OPTIONS. ===> Should have 1 changelog EACH of the deleted OPTIONS and EACH of the new OPTIONS.
    // FILTER mapped, NO options mapped. ===> Should just have 1 changelog for the FILTER.
    // filter NOT mapped, SOME OPTIONS mapped. ===> Should have 1 changelog for EACH of the mapped OPTIONS.
    // FILTER mapped + SOME OPTIONS mapped. ===> Should have 1 changelog for the FILTER, and 1 changelog for EACH of the OPTIONS.

    public class GenerateChangelogTestsLocationTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : GenerateChangelogTests(fixture)
    {
        [Fact]
        public async Task Success()
        {
            // SCENARIOS WE TEST:
            // Entire LOCATION deleted, with all options deleted. ===> Should just have 1 changelog for the deletion of the LOCATION.
            // Entire LOCATION added, with new options. ===> Should have 1 changelog for the LOCATION, and 1 changelog for EACH of the new OPTIONS.
            // location UNCHANGED, NO options changed. ===> Should have NO changelogs.
            // location UNCHANGED, with some deleted OPTIONS and some new OPTIONS. ===> Should have 1 changelog for EACH of the deleted OPTIONS and EACH of the new OPTIONS.
            // location UNCHANGED, SOME OPTIONS changed. ===> Should have 1 changelog for EACH of the changed OPTIONS.

            var (instanceId, originalVersion, newVersion) = await CreateNextDataSetVersionWithMeta(
                importStage: Stage.PreviousStage());

            // Here we define what changes have occurred between the original and new data set versions.
            // The metadata and mappings are then automatically calculated and stored from this, which can then be used
            // to generate the changelogs.
            List<ParentChange<GeographicLevel>> allChanges =
                [
                    new ParentChange<GeographicLevel> {
                        ParentIdentifier = GeographicLevel.LocalAuthority,
                        ChangeType = ChangeType.Deleted,
                        OptionChanges =
                            [
                                new OptionChange {
                                    ChangeType = ChangeType.Deleted,
                                    OptionIndex = 0
                                },
                                new OptionChange {
                                    ChangeType = ChangeType.Deleted,
                                    OptionIndex = 1
                                }
                            ]
                    },
                    new ParentChange<GeographicLevel> {
                        ParentIdentifier = GeographicLevel.School,
                        ChangeType = ChangeType.Added,
                        OptionChanges =
                            [
                                new OptionChange {
                                    ChangeType = ChangeType.Added,
                                    OptionIndex = 0
                                },
                                new OptionChange {
                                    ChangeType = ChangeType.Added,
                                    OptionIndex = 1
                                }
                            ]
                    },
                    new ParentChange<GeographicLevel> {
                        ParentIdentifier = GeographicLevel.Country,
                        ChangeType = ChangeType.Unchanged,
                        OptionChanges =
                            [
                                new OptionChange {
                                    ChangeType = ChangeType.Unchanged,
                                    OptionIndex = 0
                                },
                                new OptionChange {
                                    ChangeType = ChangeType.Unchanged,
                                    OptionIndex = 1
                                }
                            ]
                    },
                    new ParentChange<GeographicLevel> {
                        ParentIdentifier = GeographicLevel.RscRegion,
                        ChangeType = ChangeType.Unchanged,
                        OptionChanges =
                            [
                                new OptionChange {
                                    ChangeType = ChangeType.Unchanged,
                                    OptionIndex = 0
                                },
                                new OptionChange {
                                    ChangeType = ChangeType.Deleted,
                                    OptionIndex = 1
                                },
                                new OptionChange {
                                    ChangeType = ChangeType.Added,
                                    OptionIndex = 1
                                }
                            ]
                    },
                    new ParentChange<GeographicLevel> {
                        ParentIdentifier = GeographicLevel.Provider,
                        ChangeType = ChangeType.Unchanged,
                        OptionChanges =
                            [
                                new OptionChange {
                                    ChangeType = ChangeType.Unchanged,
                                    OptionIndex = 0
                                },
                                new OptionChange {
                                    ChangeType = ChangeType.Changed,
                                    OptionIndex = 1
                                },
                                new OptionChange {
                                    ChangeType = ChangeType.Changed,
                                    OptionIndex = 2
                                }
                            ]
                    }
                ];

            var (originalVersionLocationMetasWithLinksByLevel, newVersionLocationMetasWithLinksByLevel) = await CreateMeta(
                allChanges: allChanges,
                originalVersion: originalVersion,
                newVersion: newVersion);

            await CreateMappings(
                allChanges: allChanges,
                originalVersion: originalVersion,
                newVersion: newVersion,
                originalVersionLocationMetasWithLinksByLevel: originalVersionLocationMetasWithLinksByLevel,
                newVersionLocationMetasWithLinksByLevel: newVersionLocationMetasWithLinksByLevel);

            await GenerateChangelog(instanceId);

            var newVersionWithChangelogs = await GetVersionWithChangelogs(newVersion);

            // Should only have 2 LocationMetaChanges:
            // 1 for the DELETED 'LocalAuthority' level
            // 1 for the ADDED 'School' level.
            Assert.Equal(2, newVersionWithChangelogs.LocationMetaChanges.Count);

            // Should have 6 LocationOptionMetaChanges:
            // 2 for the 2 ADDED 'School' options
            // 1 for the ADDED 'RscRegion' option
            // 1 for the DELETED 'RscRegion' option
            // 2 for the 2 CHANGED 'Provider' options
            Assert.Equal(6, newVersionWithChangelogs.LocationOptionMetaChanges.Count);

            // All changelogs should be for the new data set version
            Assert.All(newVersionWithChangelogs.LocationMetaChanges, lmc => Assert.Equal(newVersionWithChangelogs.Id, lmc.DataSetVersionId));
            Assert.All(newVersionWithChangelogs.LocationOptionMetaChanges, lomc => Assert.Equal(newVersionWithChangelogs.Id, lomc.DataSetVersionId));

            foreach (var parentChange in allChanges)
            {
                originalVersionLocationMetasWithLinksByLevel.TryGetValue(parentChange.ParentIdentifier, out var originalLocationMeta);
                newVersionLocationMetasWithLinksByLevel.TryGetValue(parentChange.ParentIdentifier, out var newLocationMeta);

                // If the parent level has been DELETED, there should just be 1 changelog for this but none for the options - hence, we exit early
                if (parentChange.ChangeType is ChangeType.Deleted)
                {
                    Assert.Single(newVersionWithChangelogs.LocationMetaChanges, lmc =>
                        lmc.PreviousStateId == originalLocationMeta!.Id
                        && lmc.CurrentStateId == null);

                    continue;
                }

                // If the parent level has been ADDED, there should be 1 changelog for this. There will also be changelogs for the options - hence, we DON'T exit early
                if (parentChange.ChangeType is ChangeType.Added)
                {
                    Assert.Single(newVersionWithChangelogs.LocationMetaChanges, lmc =>
                        lmc.PreviousStateId == null
                        && lmc.CurrentStateId == newLocationMeta!.Id);
                }

                foreach (var optionChange in parentChange.OptionChanges)
                {
                    // If the option is UNCHANGED, there should be no changelog for this
                    if (optionChange.ChangeType is ChangeType.Unchanged)
                    {
                        continue;
                    }

                    var originalOptionLink = originalLocationMeta?.OptionLinks[optionChange.OptionIndex];
                    var newOptionLink = newLocationMeta?.OptionLinks[optionChange.OptionIndex];

                    switch (optionChange.ChangeType)
                    {
                        // If the option has been DELETED, ADDED or CHANGED, there should be 1 changelog for this
                        case ChangeType.Deleted:
                            Assert.Single(newVersionWithChangelogs.LocationOptionMetaChanges, lmc =>
                                lmc.PreviousState?.MetaId == originalOptionLink!.MetaId
                                && lmc.PreviousState?.OptionId == originalOptionLink.OptionId
                                && lmc.PreviousState?.PublicId == originalOptionLink.PublicId
                                && lmc.CurrentState == null);
                            break;
                        case ChangeType.Added:
                            Assert.Single(newVersionWithChangelogs.LocationOptionMetaChanges, lmc =>
                                lmc.PreviousState == null
                                && lmc.CurrentState?.MetaId == newOptionLink!.MetaId
                                && lmc.CurrentState?.OptionId == newOptionLink.OptionId
                                && lmc.CurrentState?.PublicId == newOptionLink.PublicId);
                            break;
                        case ChangeType.Changed:
                            Assert.Single(newVersionWithChangelogs.LocationOptionMetaChanges, lmc =>
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

        private async Task<(IReadOnlyDictionary<GeographicLevel, LocationMeta> originalVersionLocationMetasWithLinksByLevel, IReadOnlyDictionary<GeographicLevel, LocationMeta> newVersionLocationMetasWithLinksByLevel)> CreateMeta(
            IReadOnlyList<ParentChange<GeographicLevel>> allChanges,
            DataSetVersion originalVersion,
            DataSetVersion newVersion)
        {
            var oldLocationMetasByLevel = allChanges
                .Where(pc => pc.ChangeType is ChangeType.Unchanged or ChangeType.Deleted)
                .Select(pc => DataFixture
                    .DefaultLocationMeta()
                    .WithDataSetVersionId(originalVersion.Id)
                    .WithLevel(pc.ParentIdentifier)
                    .Generate())
                .ToDictionary(lm => lm.Level);

            await CreateMeta(locationMetas: oldLocationMetasByLevel.Values);

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

                originalVersionLocationMetasWithLinksByLevel.TryGetValue(parentChange.ParentIdentifier, out var originalLocationMeta);
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
                .ToDictionary(lm => lm.Level); // Refetching to get the links back this time

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
            List<ParentChange<GeographicLevel>> allChanges,
            DataSetVersion originalVersion, 
            DataSetVersion newVersion, 
            IReadOnlyDictionary<GeographicLevel, LocationMeta> originalVersionLocationMetasWithLinksByLevel,
            IReadOnlyDictionary<GeographicLevel, LocationMeta> newVersionLocationMetasWithLinksByLevel)
        {
            var mappedCandidateOptionKeysBySourceOptionKey = allChanges
                .SelectMany(
                    pc => pc.OptionChanges,
                    (pc, oc) => new { GeographicLevel = pc.ParentIdentifier, OptionChange = oc })
                .Where(a => a.OptionChange.ChangeType is ChangeType.Unchanged or ChangeType.Changed) // Only 'Unchanged' and 'Changed' change types correspond to an option which can be mapped
                .ToDictionary(
                    a => LocationOptionMetaKeyGenerator(originalVersionLocationMetasWithLinksByLevel[a.GeographicLevel].OptionLinks[a.OptionChange.OptionIndex].Option),
                    a => LocationOptionMetaKeyGenerator(newVersionLocationMetasWithLinksByLevel[a.GeographicLevel].OptionLinks[a.OptionChange.OptionIndex].Option));

            DataSetVersionMapping mappings = DataFixture
                .DefaultDataSetVersionMapping()
                .WithSourceDataSetVersionId(originalVersion.Id)
                .WithTargetDataSetVersionId(newVersion.Id)
                .WithLocationMappingPlan(
                    DataFixture.LocationMappingPlanFromLocationMeta(
                        sourceLocations: [.. originalVersionLocationMetasWithLinksByLevel.Values],
                        targetLocations: [.. newVersionLocationMetasWithLinksByLevel.Values],
                        mappedCandidateOptionKeysBySourceOptionKey: mappedCandidateOptionKeysBySourceOptionKey));

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

    public class UpdateFileStoragePathTests(
        ProcessorFunctionsIntegrationTestFixture fixture)
        : ProcessCompletionOfNextDataSetVersionImportFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.ManualMapping;

        [Fact]
        public async Task Success_PathUpdated()
        {
            var (initialDataSetVersion, _) = await CreateDataSet(
                importStage: DataSetVersionImportStage.Completing,
                status: DataSetVersionStatus.Published);

            var defaultNextVersion = initialDataSetVersion.DefaultNextVersion();

            var (nextDataSetVersion, instanceId) = await CreateDataSetVersionAndImport(
                dataSetId: initialDataSetVersion.DataSetId,
                importStage: Stage,
                versionMajor: defaultNextVersion.Major,
                versionMinor: defaultNextVersion.Minor);

            var dataSetVersionPathResolver = GetRequiredService<IDataSetVersionPathResolver>();
            var originalStoragePath = dataSetVersionPathResolver.DirectoryPath(nextDataSetVersion);
            Directory.CreateDirectory(originalStoragePath);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            nextDataSetVersion.VersionMajor++;

            publicDataDbContext.DataSetVersions.Attach(nextDataSetVersion);
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
            var (initialDataSetVersion, _) = await CreateDataSet(
                importStage: DataSetVersionImportStage.Completing,
                status: DataSetVersionStatus.Published);

            var defaultNextVersion = initialDataSetVersion.DefaultNextVersion();

            var (nextDataSetVersion, instanceId) = await CreateDataSetVersionAndImport(
                dataSetId: initialDataSetVersion.DataSetId,
                importStage: Stage,
                versionMajor: defaultNextVersion.Major,
                versionMinor: defaultNextVersion.Minor);

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
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

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
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

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

    private async Task<(Guid instanceId, DataSetVersion initialVersion, DataSetVersion nextVersion)>
        CreateNextDataSetVersionWithMeta(
        DataSetVersionImportStage importStage,
        IEnumerable<FilterMeta>? oldFilterMetas = null,
        IEnumerable<LocationMeta>? oldLocationMetas = null,
        GeographicLevelMeta? oldGeographicLevelMeta = null,
        IEnumerable<IndicatorMeta>? oldIndicatorMetas = null,
        IEnumerable<TimePeriodMeta>? oldTimePeriodMetas = null,
        IEnumerable<FilterMeta>? newFilterMetas = null,
        IEnumerable<LocationMeta>? newLocationMetas = null,
        GeographicLevelMeta? newGeographicLevelMeta = null,
        IEnumerable<IndicatorMeta>? newIndicatorMetas = null,
        IEnumerable<TimePeriodMeta>? newTimePeriodMetas = null)
    {
        var (initialDataSetVersion, _) = await CreateDataSet(
            importStage: DataSetVersionImportStage.Completing,
            status: DataSetVersionStatus.Published,
            filterMetas: oldFilterMetas,
            locationMetas: oldLocationMetas,
            geographicLevelMeta: oldGeographicLevelMeta ?? DataFixture.DefaultGeographicLevelMeta(),
            indicatorMetas: oldIndicatorMetas,
            timePeriodMetas: oldTimePeriodMetas);

        var (nextDataSetVersion, instanceId) = await CreateDataSetVersionAndImport(
            dataSetId: initialDataSetVersion.DataSetId,
            importStage: importStage,
            versionMajor: 1,
            versionMinor: 1,
            filterMetas: newFilterMetas,
            locationMetas: newLocationMetas,
            geographicLevelMeta: newGeographicLevelMeta ?? DataFixture.DefaultGeographicLevelMeta(),
            indicatorMetas: newIndicatorMetas,
            timePeriodMetas: newTimePeriodMetas);

        return (instanceId, initialDataSetVersion, nextDataSetVersion);
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
                context.GeographicLevelMetas.AddRange(geographicLevelMeta);
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

    private async Task<DataSetVersion> GetVersionWithChangelogs(DataSetVersion version)
    {
        return await GetDbContext<PublicDataDbContext>()
            .DataSetVersions
            .Include(dsv => dsv.FilterMetaChanges)
            .Include(dsv => dsv.FilterOptionMetaChanges)
            .Include(dsv => dsv.LocationMetaChanges)
            .Include(dsv => dsv.LocationOptionMetaChanges)
            .Include(dsv => dsv.GeographicLevelMetaChange)
            .Include(dsv => dsv.IndicatorMetaChanges)
            .Include(dsv => dsv.TimePeriodMetaChanges)
            .SingleAsync(dsv => dsv.Id == version.Id);
    }

    // This is to make setting up the tests easier and safer to change.
    // We define what changes we want the hypothetical new data set version to have, relative to it's original version, and
    // the metadata and mappings are then worked out for us.
    private class ParentChange<TParentIdentifier>
    {
        public TParentIdentifier ParentIdentifier { get; init; }
        public ChangeType ChangeType { get; init; }
        public IReadOnlyList<OptionChange> OptionChanges { get; init; } = [];
    }

    private class OptionChange
    {
        public ChangeType ChangeType { get; init; }
        public int OptionIndex { get; init; } // This is the index of the corresponding option for the original data set version, or for the new data set version
    }

    private enum ChangeType
    {
        Unchanged, // This is essentially an option that has been auto-mapped
        Changed, // This is an option that has had it's label changed, and has been manually mapped to the new option
        Deleted, // This is an option that had been deleted
        Added // This is an option that has been added
    }
}
