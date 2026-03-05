using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.FunctionApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.TestData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using FilterMeta = GovUk.Education.ExploreEducationStatistics.Public.Data.Model.FilterMeta;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

// ReSharper disable once ClassNeverInstantiated.Global
public class ProcessCompletionOfNextDataSetVersionFunctionsTestsFixture()
    : OptimisedPublicDataProcessorCollectionFixture(
        capabilities: [PublicDataProcessorIntegrationTestCapability.Postgres]
    )
{
    public ProcessCompletionOfNextDataSetVersionFunctions Function = null!;

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

        Function = lookups.GetService<ProcessCompletionOfNextDataSetVersionFunctions>();
    }
}

[CollectionDefinition(nameof(ProcessCompletionOfNextDataSetVersionFunctionsTestsFixture))]
public class ProcessCompletionOfNextDataSetVersionFunctionsTestsCollection
    : ICollectionFixture<ProcessCompletionOfNextDataSetVersionFunctionsTestsFixture>;

[Collection(nameof(ProcessCompletionOfNextDataSetVersionFunctionsTestsFixture))]
public abstract class ProcessCompletionOfNextDataSetVersionFunctionsTests(
    ProcessCompletionOfNextDataSetVersionFunctionsTestsFixture fixture
) : OptimisedFunctionAppIntegrationTestBase(fixture)
{
    private static readonly DataFixture DataFixture = new();

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
        TimePeriodsTable.ParquetFile,
    ];

    public abstract class CreateChangesTests(ProcessCompletionOfNextDataSetVersionFunctionsTestsFixture fixture)
        : ProcessCompletionOfNextDataSetVersionFunctionsTests(fixture)
    {
        protected const DataSetVersionImportStage Stage = DataSetVersionImportStage.CreatingChanges;

        protected async Task CreateChanges(Guid instanceId)
        {
            await fixture.Function.CreateChanges(instanceId, CancellationToken.None);
        }
    }

    public class CreateChangesFilterTests(ProcessCompletionOfNextDataSetVersionFunctionsTestsFixture fixture)
        : CreateChangesTests(fixture)
    {
        [Fact]
        public async Task FiltersAdded_ChangesContainOnlyAddedFilters()
        {
            var oldFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(1))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    )
                                    .Generate(1)
                            )
                )
                .GenerateList(1);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Filter and ALL options unchanged
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2)) // Filter added
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    ) // Filter Option added
                                    .Generate(1)
                            )
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(3)) // Filter added
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(3))
                                    ) // Filter Option added
                                    .Generate(1)
                            )
                )
                .GenerateList(3);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMetas: newFilterMeta
            );

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // 2 Filter additions
            Assert.Equal(2, filterMetaChanges.Count);
            Assert.All(filterMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            // No Filter Option additions
            Assert.Empty(filterOptionMetaChanges);

            var newFilterMetas = newVersion.FilterMetas.ToDictionary(m => m.PublicId);

            // Filter added
            AssertSingleFilterAdded(filterMetaChanges, newFilterMetas[SqidEncoder.Encode(2)]);

            // Filter added
            AssertSingleFilterAdded(filterMetaChanges, newFilterMetas[SqidEncoder.Encode(3)]);
        }

        [Fact]
        public async Task FiltersDeleted_ChangesContainOnlyDeletedFilters()
        {
            var oldFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(1))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    )
                                    .Generate(1)
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2)) // Filter and ALL options deleted
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    )
                                    .Generate(1)
                            )
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(3)) // Filter and ALL options deleted
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(3))
                                    )
                                    .Generate(1)
                            )
                )
                .GenerateList(3);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Filter and ALL options unchanged
                .GenerateList(1);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMetas: newFilterMeta
            );

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // 2 Filter deletions
            Assert.Equal(2, filterMetaChanges.Count);
            Assert.All(filterMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            // No Filter Option deletions
            Assert.Empty(filterOptionMetaChanges);

            var oldFilterMetas = originalVersion.FilterMetas.ToDictionary(m => m.PublicId);

            // Filter deleted
            AssertSingleFilterDeleted(filterMetaChanges, oldFilterMetas[SqidEncoder.Encode(2)]);

            // Filter deleted
            AssertSingleFilterDeleted(filterMetaChanges, oldFilterMetas[SqidEncoder.Encode(3)]);
        }

        [Fact]
        public async Task FiltersUpdatedOptionsAdded_ChangesContainOnlyUpdatedFiltersAndAddedOptions()
        {
            var oldFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(1))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    )
                                    .Generate(1)
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    )
                                    .Generate(1)
                            )
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(3))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(3))
                                    )
                                    .Generate(1)
                            )
                )
                .GenerateList(3);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Filter and ALL options unchanged
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2)) // Filter updated
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[1].OptionLinks[0])) // Filter Option unchanged
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(4))
                                    ) // Filter Option added
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(5))
                                    ) // Filter Option added
                                    .Generate(3)
                            )
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(3)) // Filter updated
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[2].OptionLinks[0])) // Filter Option unchanged
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(6))
                                    ) // Filter Option added
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(7))
                                    ) // Filter Option added
                                    .Generate(3)
                            )
                )
                .GenerateList(3);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMetas: newFilterMeta
            );

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // 2 Filter changes
            Assert.Equal(2, filterMetaChanges.Count);
            Assert.All(filterMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            // 4 Filter Option changes
            Assert.Equal(4, filterOptionMetaChanges.Count);
            Assert.All(filterOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas.ToDictionary(m => m.PublicId);

            var newFilterMetas = newVersion.FilterMetas.ToDictionary(
                m => m.PublicId,
                m => new { Meta = m, OptionLinks = m.OptionLinks.ToDictionary(l => l.PublicId) }
            );

            // Filter updated
            AssertSingleFilterUpdated(
                changes: filterMetaChanges,
                expectedOldFilter: oldFilterMetas[SqidEncoder.Encode(2)],
                expectedNewFilter: newFilterMetas[SqidEncoder.Encode(2)].Meta
            );

            // Filter updated
            AssertSingleFilterUpdated(
                changes: filterMetaChanges,
                expectedOldFilter: oldFilterMetas[SqidEncoder.Encode(3)],
                expectedNewFilter: newFilterMetas[SqidEncoder.Encode(3)].Meta
            );

            // Filter Option added
            AssertSingleFilterOptionAdded(
                filterOptionMetaChanges,
                newFilterMetas[SqidEncoder.Encode(2)].OptionLinks[SqidEncoder.Encode(4)]
            );

            // Filter Option added
            AssertSingleFilterOptionAdded(
                filterOptionMetaChanges,
                newFilterMetas[SqidEncoder.Encode(2)].OptionLinks[SqidEncoder.Encode(5)]
            );

            // Filter Option added
            AssertSingleFilterOptionAdded(
                filterOptionMetaChanges,
                newFilterMetas[SqidEncoder.Encode(3)].OptionLinks[SqidEncoder.Encode(6)]
            );

            // Filter Option added
            AssertSingleFilterOptionAdded(
                filterOptionMetaChanges,
                newFilterMetas[SqidEncoder.Encode(3)].OptionLinks[SqidEncoder.Encode(7)]
            );
        }

        [Fact]
        public async Task FiltersUpdatedOptionsDeleted_ChangesContainOnlyUpdatedFiltersAndDeletedOptions()
        {
            var oldFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(1))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    )
                                    .Generate(1)
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    )
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(3))
                                    ) // Filter Option deleted
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(4))
                                    ) // Filter Option deleted
                                    .Generate(3)
                            )
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(3))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(5))
                                    )
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(6))
                                    ) // Filter Option deleted
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(7))
                                    ) // Filter Option deleted
                                    .Generate(3)
                            )
                )
                .GenerateList(3);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Filter and ALL options unchanged
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2)) // Filter updated
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[1].OptionLinks[0])) // Filter Option unchanged
                                    .Generate(1)
                            )
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(3)) // Filter updated
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[2].OptionLinks[0])) // Filter Option unchanged
                                    .Generate(1)
                            )
                )
                .GenerateList(3);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMetas: newFilterMeta
            );

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // 2 Filter changes
            Assert.Equal(2, filterMetaChanges.Count);
            Assert.All(filterMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            // 4 Filter Option changes
            Assert.Equal(4, filterOptionMetaChanges.Count);
            Assert.All(filterOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas.ToDictionary(
                m => m.PublicId,
                m => new { Meta = m, OptionLinks = m.OptionLinks.ToDictionary(l => l.PublicId) }
            );

            var newFilterMetas = newVersion.FilterMetas.ToDictionary(m => m.PublicId);

            // Filter updated
            AssertSingleFilterUpdated(
                changes: filterMetaChanges,
                expectedOldFilter: oldFilterMetas[SqidEncoder.Encode(2)].Meta,
                expectedNewFilter: newFilterMetas[SqidEncoder.Encode(2)]
            );

            // Filter updated
            AssertSingleFilterUpdated(
                changes: filterMetaChanges,
                expectedOldFilter: oldFilterMetas[SqidEncoder.Encode(3)].Meta,
                expectedNewFilter: newFilterMetas[SqidEncoder.Encode(3)]
            );

            // Filter Option deleted
            AssertSingleFilterOptionDeleted(
                filterOptionMetaChanges,
                oldFilterMetas[SqidEncoder.Encode(2)].OptionLinks[SqidEncoder.Encode(3)]
            );

            // Filter Option deleted
            AssertSingleFilterOptionDeleted(
                filterOptionMetaChanges,
                oldFilterMetas[SqidEncoder.Encode(2)].OptionLinks[SqidEncoder.Encode(4)]
            );

            // Filter Option deleted
            AssertSingleFilterOptionDeleted(
                filterOptionMetaChanges,
                oldFilterMetas[SqidEncoder.Encode(3)].OptionLinks[SqidEncoder.Encode(6)]
            );

            // Filter Option deleted
            AssertSingleFilterOptionDeleted(
                filterOptionMetaChanges,
                oldFilterMetas[SqidEncoder.Encode(3)].OptionLinks[SqidEncoder.Encode(7)]
            );
        }

        [Fact]
        public async Task FiltersUpdatedOptionsUpdated_ChangesContainOnlyUpdatedFiltersAndUpdatedOptions()
        {
            var oldFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(1))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    )
                                    .Generate(1)
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    )
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(3))
                                    )
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(4))
                                    )
                                    .Generate(3)
                            )
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(3))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(5))
                                    )
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(6))
                                    )
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(7))
                                    )
                                    .Generate(3)
                            )
                )
                .GenerateList(3);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Filter and ALL options unchanged
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2)) // Filter updated
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[1].OptionLinks[0])) // Filter Option unchanged
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(3))
                                    ) // Filter Option updated
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(4))
                                    ) // Filter Option updated
                                    .Generate(3)
                            )
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(3)) // Filter updated
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[2].OptionLinks[0])) // Filter Option unchanged
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(6))
                                    ) // Filter Option updated
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(7))
                                    ) // Filter Option updated
                                    .Generate(3)
                            )
                )
                .GenerateList(3);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMetas: newFilterMeta
            );

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // 2 Filter changes
            Assert.Equal(2, filterMetaChanges.Count);
            Assert.All(filterMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            // 4 Filter Option changes
            Assert.Equal(4, filterOptionMetaChanges.Count);
            Assert.All(filterOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas.ToDictionary(
                m => m.PublicId,
                m => new { Meta = m, OptionLinks = m.OptionLinks.ToDictionary(l => l.PublicId) }
            );

            var newFilterMetas = newVersion.FilterMetas.ToDictionary(
                m => m.PublicId,
                m => new { Meta = m, OptionLinks = m.OptionLinks.ToDictionary(l => l.PublicId) }
            );

            // Filter updated
            AssertSingleFilterUpdated(
                changes: filterMetaChanges,
                expectedOldFilter: oldFilterMetas[SqidEncoder.Encode(2)].Meta,
                expectedNewFilter: newFilterMetas[SqidEncoder.Encode(2)].Meta
            );

            // Filter updated
            AssertSingleFilterUpdated(
                changes: filterMetaChanges,
                expectedOldFilter: oldFilterMetas[SqidEncoder.Encode(3)].Meta,
                expectedNewFilter: newFilterMetas[SqidEncoder.Encode(3)].Meta
            );

            // Filter Option updated
            AssertSingleFilterOptionUpdated(
                changes: filterOptionMetaChanges,
                expectedOldOptionLink: oldFilterMetas[SqidEncoder.Encode(2)].OptionLinks[SqidEncoder.Encode(3)],
                expectedNewOptionLink: newFilterMetas[SqidEncoder.Encode(2)].OptionLinks[SqidEncoder.Encode(3)]
            );

            // Filter Option updated
            AssertSingleFilterOptionUpdated(
                changes: filterOptionMetaChanges,
                expectedOldOptionLink: oldFilterMetas[SqidEncoder.Encode(2)].OptionLinks[SqidEncoder.Encode(4)],
                expectedNewOptionLink: newFilterMetas[SqidEncoder.Encode(2)].OptionLinks[SqidEncoder.Encode(4)]
            );

            // Filter Option updated
            AssertSingleFilterOptionUpdated(
                changes: filterOptionMetaChanges,
                expectedOldOptionLink: oldFilterMetas[SqidEncoder.Encode(3)].OptionLinks[SqidEncoder.Encode(6)],
                expectedNewOptionLink: newFilterMetas[SqidEncoder.Encode(3)].OptionLinks[SqidEncoder.Encode(6)]
            );

            // Filter Option updated
            AssertSingleFilterOptionUpdated(
                changes: filterOptionMetaChanges,
                expectedOldOptionLink: oldFilterMetas[SqidEncoder.Encode(3)].OptionLinks[SqidEncoder.Encode(7)],
                expectedNewOptionLink: newFilterMetas[SqidEncoder.Encode(3)].OptionLinks[SqidEncoder.Encode(7)]
            );
        }

        [Fact]
        public async Task FiltersUpdatedOptionsUnchanged_ChangesContainOnlyUpdatedFilters()
        {
            var oldFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(1))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    )
                                    .Generate(1)
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    )
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(3))
                                    )
                                    .Generate(2)
                            )
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(3))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(4))
                                    )
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(5))
                                    )
                                    .Generate(2)
                            )
                )
                .GenerateList(3);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Filter and ALL options unchanged
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2)) // Filter updated
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[1].OptionLinks[0])) // Filter Option unchanged
                                    .ForIndex(1, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[1].OptionLinks[1])) // Filter Option unchanged
                                    .Generate(2)
                            )
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(3)) // Filter updated
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[2].OptionLinks[0])) // Filter Option unchanged
                                    .ForIndex(1, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[2].OptionLinks[1])) // Filter Option unchanged
                                    .Generate(2)
                            )
                )
                .GenerateList(3);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMetas: newFilterMeta
            );

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // 2 Filter changes
            Assert.Equal(2, filterMetaChanges.Count);
            Assert.All(filterMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            // No Filter Option changes
            Assert.Empty(filterOptionMetaChanges);

            var oldFilterMetas = originalVersion.FilterMetas.ToDictionary(m => m.PublicId);

            var newFilterMetas = newVersion.FilterMetas.ToDictionary(m => m.PublicId);

            // Filter updated
            AssertSingleFilterUpdated(
                changes: filterMetaChanges,
                expectedOldFilter: oldFilterMetas[SqidEncoder.Encode(2)],
                expectedNewFilter: newFilterMetas[SqidEncoder.Encode(2)]
            );

            // Filter updated
            AssertSingleFilterUpdated(
                changes: filterMetaChanges,
                expectedOldFilter: oldFilterMetas[SqidEncoder.Encode(3)],
                expectedNewFilter: newFilterMetas[SqidEncoder.Encode(3)]
            );
        }

        [Fact]
        public async Task FiltersUnchangedOptionsAdded_ChangesContainOnlyAddedOptions()
        {
            var oldFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(1))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    )
                                    .Generate(1)
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    )
                                    .Generate(1)
                            )
                )
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(
                    0,
                    UnchangedFilterMetaSetter(
                        filterMeta: oldFilterMeta[0], // Filter unchanged
                        newOptionLinks: () =>
                            DataFixture
                                .DefaultFilterOptionMetaLink()
                                .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[0].OptionLinks[0])) // Filter Option unchanged
                                .ForIndex(
                                    1,
                                    s =>
                                        s.SetOption(DataFixture.DefaultFilterOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(3))
                                ) // Filter Option added
                                .ForIndex(
                                    2,
                                    s =>
                                        s.SetOption(DataFixture.DefaultFilterOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(4))
                                ) // Filter Option added
                                .Generate(3)
                    )
                )
                .ForIndex(
                    1,
                    UnchangedFilterMetaSetter(
                        filterMeta: oldFilterMeta[1], // Filter unchanged
                        newOptionLinks: () =>
                            DataFixture
                                .DefaultFilterOptionMetaLink()
                                .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[1].OptionLinks[0])) // Filter Option unchanged
                                .ForIndex(
                                    1,
                                    s =>
                                        s.SetOption(DataFixture.DefaultFilterOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(5))
                                ) // Filter Option added
                                .ForIndex(
                                    2,
                                    s =>
                                        s.SetOption(DataFixture.DefaultFilterOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(6))
                                ) // Filter Option added
                                .Generate(3)
                    )
                )
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMetas: newFilterMeta
            );

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // No Filter changes
            Assert.Empty(filterMetaChanges);

            // 4 Filter Option changes
            Assert.Equal(4, filterOptionMetaChanges.Count);
            Assert.All(filterOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var newFilterMetas = newVersion.FilterMetas.ToDictionary(
                m => m.PublicId,
                m => m.OptionLinks.ToDictionary(l => l.PublicId)
            );

            // Filter Option added
            AssertSingleFilterOptionAdded(
                filterOptionMetaChanges,
                newFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(3)]
            );

            // Filter Option added
            AssertSingleFilterOptionAdded(
                filterOptionMetaChanges,
                newFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(4)]
            );

            // Filter Option added
            AssertSingleFilterOptionAdded(
                filterOptionMetaChanges,
                newFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(5)]
            );

            // Filter Option added
            AssertSingleFilterOptionAdded(
                filterOptionMetaChanges,
                newFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(6)]
            );
        }

        [Fact]
        public async Task FiltersUnchangedOptionsDeleted_ChangesContainOnlyDeletedOptions()
        {
            var oldFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(1))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    )
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    ) // Filter Option deleted
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(3))
                                    ) // Filter Option deleted
                                    .Generate(3)
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(4))
                                    )
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(5))
                                    ) // Filter Option deleted
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(6))
                                    ) // Filter Option deleted
                                    .Generate(3)
                            )
                )
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(
                    0,
                    UnchangedFilterMetaSetter(
                        filterMeta: oldFilterMeta[0], // Filter unchanged
                        newOptionLinks: () =>
                            DataFixture
                                .DefaultFilterOptionMetaLink()
                                .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[0].OptionLinks[0])) // Filter Option unchanged
                                .Generate(1)
                    )
                )
                .ForIndex(
                    1,
                    UnchangedFilterMetaSetter(
                        filterMeta: oldFilterMeta[1], // Filter unchanged
                        newOptionLinks: () =>
                            DataFixture
                                .DefaultFilterOptionMetaLink()
                                .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[1].OptionLinks[0])) // Filter Option unchanged
                                .Generate(1)
                    )
                )
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMetas: newFilterMeta
            );

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // No Filter changes
            Assert.Empty(filterMetaChanges);

            // 4 Filter Option changes
            Assert.Equal(4, filterOptionMetaChanges.Count);
            Assert.All(filterOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas.ToDictionary(
                m => m.PublicId,
                m => m.OptionLinks.ToDictionary(l => l.PublicId)
            );

            // Filter Option deleted
            AssertSingleFilterOptionDeleted(
                filterOptionMetaChanges,
                oldFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(2)]
            );

            // Filter Option deleted
            AssertSingleFilterOptionDeleted(
                filterOptionMetaChanges,
                oldFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(3)]
            );

            // Filter Option deleted
            AssertSingleFilterOptionDeleted(
                filterOptionMetaChanges,
                oldFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(5)]
            );

            // Filter Option deleted
            AssertSingleFilterOptionDeleted(
                filterOptionMetaChanges,
                oldFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(6)]
            );
        }

        [Fact]
        public async Task FiltersUnchangedOptionsUpdated_ChangesContainOnlyUpdatedOptions()
        {
            var oldFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(1))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    )
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    )
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(3))
                                    )
                                    .Generate(3)
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(4))
                                    )
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(5))
                                    )
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(6))
                                    )
                                    .Generate(3)
                            )
                )
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(
                    0,
                    UnchangedFilterMetaSetter(
                        filterMeta: oldFilterMeta[0], // Filter unchanged
                        newOptionLinks: () =>
                            DataFixture
                                .DefaultFilterOptionMetaLink()
                                .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[0].OptionLinks[0])) // Filter Option unchanged
                                .ForIndex(
                                    1,
                                    s =>
                                        s.SetOption(DataFixture.DefaultFilterOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(2))
                                ) // Filter Option updated
                                .ForIndex(
                                    2,
                                    s =>
                                        s.SetOption(DataFixture.DefaultFilterOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(3))
                                ) // Filter Option updated
                                .Generate(3)
                    )
                )
                .ForIndex(
                    1,
                    UnchangedFilterMetaSetter(
                        filterMeta: oldFilterMeta[1], // Filter unchanged
                        newOptionLinks: () =>
                            DataFixture
                                .DefaultFilterOptionMetaLink()
                                .ForIndex(0, UnchangedFilterOptionMetaLinkSetter(oldFilterMeta[1].OptionLinks[0])) // Filter Option unchanged
                                .ForIndex(
                                    1,
                                    s =>
                                        s.SetOption(DataFixture.DefaultFilterOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(5))
                                ) // Filter Option updated
                                .ForIndex(
                                    2,
                                    s =>
                                        s.SetOption(DataFixture.DefaultFilterOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(6))
                                ) // Filter Option updated
                                .Generate(3)
                    )
                )
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMetas: newFilterMeta
            );

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // No Filter changes
            Assert.Empty(filterMetaChanges);

            // 4 Filter Option changes
            Assert.Equal(4, filterOptionMetaChanges.Count);
            Assert.All(filterOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas.ToDictionary(
                m => m.PublicId,
                m => m.OptionLinks.ToDictionary(l => l.PublicId)
            );

            var newFilterMetas = newVersion.FilterMetas.ToDictionary(
                m => m.PublicId,
                m => m.OptionLinks.ToDictionary(l => l.PublicId)
            );

            // Filter Option updated
            AssertSingleFilterOptionUpdated(
                changes: filterOptionMetaChanges,
                expectedOldOptionLink: oldFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(2)],
                expectedNewOptionLink: newFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(2)]
            );

            // Filter Option updated
            AssertSingleFilterOptionUpdated(
                changes: filterOptionMetaChanges,
                expectedOldOptionLink: oldFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(3)],
                expectedNewOptionLink: newFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(3)]
            );

            // Filter Option updated
            AssertSingleFilterOptionUpdated(
                changes: filterOptionMetaChanges,
                expectedOldOptionLink: oldFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(5)],
                expectedNewOptionLink: newFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(5)]
            );

            // Filter Option updated
            AssertSingleFilterOptionUpdated(
                changes: filterOptionMetaChanges,
                expectedOldOptionLink: oldFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(6)],
                expectedNewOptionLink: newFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(6)]
            );
        }

        [Fact]
        public async Task FiltersUnchangedOptionsUnchanged_ChangesAreEmpty()
        {
            var oldFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(1))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    )
                                    .Generate(1)
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    )
                                    .Generate(1)
                            )
                )
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(0, UnchangedFilterMetaSetter(oldFilterMeta[0])) // Filter and ALL options unchanged
                .ForIndex(1, UnchangedFilterMetaSetter(oldFilterMeta[1])) // Filter and ALL options unchanged
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMetas: newFilterMeta
            );

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);
            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // No Filter changes
            Assert.Empty(filterMetaChanges);

            // No Filter Option changes
            Assert.Empty(filterOptionMetaChanges);
        }

        [Fact]
        public async Task FiltersAddedAndDeletedAndUpdated_ChangesInsertedIntoDatabaseInCorrectOrder()
        {
            var oldFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(1)) // Filter deleted
                            .SetLabel("f")
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2)) // Filter deleted
                            .SetLabel("a")
                )
                .ForIndex(2, s => s.SetPublicId(SqidEncoder.Encode(3)).SetLabel("e"))
                .ForIndex(3, s => s.SetPublicId(SqidEncoder.Encode(4)).SetLabel("b"))
                .GenerateList(4);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(0, s => s.SetPublicId(SqidEncoder.Encode(3))) // Filter updated
                .ForIndex(1, s => s.SetPublicId(SqidEncoder.Encode(4))) // Filter updated
                .ForIndex(
                    2,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(5)) // Filter added
                            .SetLabel("d")
                )
                .ForIndex(
                    3,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(6)) // Filter added
                            .SetLabel("c")
                )
                .GenerateList(4);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMetas: newFilterMeta
            );

            await CreateChanges(instanceId);

            var filterMetaChanges = await GetFilterMetaChanges(newVersion);

            // 6 Filter changes
            Assert.Equal(6, filterMetaChanges.Count);
            Assert.All(filterMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas.ToDictionary(m => m.PublicId);

            var newFilterMetas = newVersion.FilterMetas.ToDictionary(m => m.PublicId);

            // The changes should be inserted into each database table ordered alphabetically by 'Label'.
            // They should also be ordered such that all deletions come first, updates next, and additions last.

            // Therefore, the expected order of Filter changes are (as per their Public IDs):
            // Sqid 2 deleted
            // Sqid 1 deleted
            // Sqid 4 updated
            // Sqid 3 updated
            // Sqid 6 added
            // Sqid 5 added

            AssertFilterDeleted(expectedFilter: oldFilterMetas[SqidEncoder.Encode(2)], change: filterMetaChanges[0]);
            AssertFilterDeleted(expectedFilter: oldFilterMetas[SqidEncoder.Encode(1)], change: filterMetaChanges[1]);
            AssertFilterUpdated(
                expectedOldFilter: oldFilterMetas[SqidEncoder.Encode(4)],
                expectedNewFilter: newFilterMetas[SqidEncoder.Encode(4)],
                change: filterMetaChanges[2]
            );
            AssertFilterUpdated(
                expectedOldFilter: oldFilterMetas[SqidEncoder.Encode(3)],
                expectedNewFilter: newFilterMetas[SqidEncoder.Encode(3)],
                change: filterMetaChanges[3]
            );
            AssertFilterAdded(expectedFilter: newFilterMetas[SqidEncoder.Encode(6)], change: filterMetaChanges[4]);
            AssertFilterAdded(expectedFilter: newFilterMetas[SqidEncoder.Encode(5)], change: filterMetaChanges[5]);
        }

        [Fact]
        public async Task FiltersOptionsAddedAndDeletedAndUpdated_ChangesInsertedIntoDatabaseInCorrectOrder()
        {
            var oldFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(1))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta().WithLabel("d"))
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    ) // Filter Option deleted
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta().WithLabel("a"))
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    ) // Filter Option deleted
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta().WithLabel("b"))
                                                .SetPublicId(SqidEncoder.Encode(3))
                                    )
                                    .ForIndex(
                                        3,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta().WithLabel("k"))
                                                .SetPublicId(SqidEncoder.Encode(4))
                                    )
                                    .Generate(4)
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2))
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta().WithLabel("c"))
                                                .SetPublicId(SqidEncoder.Encode(5))
                                    ) // Filter Option deleted
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta().WithLabel("h"))
                                                .SetPublicId(SqidEncoder.Encode(6))
                                    ) // Filter Option deleted
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta().WithLabel("f"))
                                                .SetPublicId(SqidEncoder.Encode(7))
                                    )
                                    .ForIndex(
                                        3,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultFilterOptionMeta().WithLabel("i"))
                                                .SetPublicId(SqidEncoder.Encode(8))
                                    )
                                    .Generate(4)
                            )
                )
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldFilterMeta);

            var newFilterMeta = DataFixture
                .DefaultFilterMeta()
                .ForIndex(
                    0,
                    UnchangedFilterMetaSetter(
                        filterMeta: oldFilterMeta[0],
                        newOptionLinks: () =>
                            DataFixture
                                .DefaultFilterOptionMetaLink()
                                .ForIndex(
                                    0,
                                    s =>
                                        s.SetOption(DataFixture.DefaultFilterOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(3))
                                ) // Filter Option updated
                                .ForIndex(
                                    1,
                                    s =>
                                        s.SetOption(DataFixture.DefaultFilterOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(4))
                                ) // Filter Option updated
                                .ForIndex(
                                    2,
                                    s =>
                                        s.SetOption(DataFixture.DefaultFilterOptionMeta().WithLabel("j"))
                                            .SetPublicId(SqidEncoder.Encode(9))
                                ) // Filter Option added
                                .ForIndex(
                                    3,
                                    s =>
                                        s.SetOption(DataFixture.DefaultFilterOptionMeta().WithLabel("e"))
                                            .SetPublicId(SqidEncoder.Encode(10))
                                ) // Filter Option added
                                .Generate(4)
                    )
                )
                .ForIndex(
                    1,
                    UnchangedFilterMetaSetter(
                        filterMeta: oldFilterMeta[1],
                        newOptionLinks: () =>
                            DataFixture
                                .DefaultFilterOptionMetaLink()
                                .ForIndex(
                                    0,
                                    s =>
                                        s.SetOption(DataFixture.DefaultFilterOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(7))
                                ) // Filter Option updated
                                .ForIndex(
                                    1,
                                    s =>
                                        s.SetOption(DataFixture.DefaultFilterOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(8))
                                ) // Filter Option updated
                                .ForIndex(
                                    2,
                                    s =>
                                        s.SetOption(DataFixture.DefaultFilterOptionMeta().WithLabel("g"))
                                            .SetPublicId(SqidEncoder.Encode(11))
                                ) // Filter Option added
                                .ForIndex(
                                    3,
                                    s =>
                                        s.SetOption(DataFixture.DefaultFilterOptionMeta().WithLabel("l"))
                                            .SetPublicId(SqidEncoder.Encode(12))
                                ) // Filter Option added
                                .Generate(4)
                    )
                )
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                filterMetas: newFilterMeta
            );

            await CreateChanges(instanceId);

            var filterOptionMetaChanges = await GetFilterOptionMetaChanges(newVersion);

            // 12 Filter Option changes
            Assert.Equal(12, filterOptionMetaChanges.Count);
            Assert.All(filterOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldFilterMetas = originalVersion.FilterMetas.ToDictionary(
                m => m.PublicId,
                m => m.OptionLinks.ToDictionary(l => l.PublicId)
            );

            var newFilterMetas = newVersion.FilterMetas.ToDictionary(
                m => m.PublicId,
                m => m.OptionLinks.ToDictionary(l => l.PublicId)
            );

            // The changes should be inserted into each database table ordered alphabetically by 'Label'.
            // They should also be ordered such that all deletions come first, updates next, and additions last.

            // Therefore, the expected order of Filter Option changes are (as per their Public IDs):
            // Sqid 2 in filter with Sqid 1 deleted
            // Sqid 5 in filter with Sqid 2 deleted
            // Sqid 1 in filter with Sqid 1 deleted
            // Sqid 6 in filter with Sqid 2 deleted
            // Sqid 3 in filter with Sqid 1 updated
            // Sqid 7 in filter with Sqid 2 updated
            // Sqid 8 in filter with Sqid 2 updated
            // Sqid 4 in filter with Sqid 1 updated
            // Sqid 10 in filter with Sqid 1 added
            // Sqid 11 in filter with Sqid 2 added
            // Sqid 9 in filter with Sqid 1 added
            // Sqid 12 in filter with Sqid 2 added

            AssertFilterOptionDeleted(
                expectedOptionLink: oldFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(2)],
                change: filterOptionMetaChanges[0]
            );
            AssertFilterOptionDeleted(
                expectedOptionLink: oldFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(5)],
                change: filterOptionMetaChanges[1]
            );
            AssertFilterOptionDeleted(
                expectedOptionLink: oldFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(1)],
                change: filterOptionMetaChanges[2]
            );
            AssertFilterOptionDeleted(
                expectedOptionLink: oldFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(6)],
                change: filterOptionMetaChanges[3]
            );
            AssertFilterOptionUpdated(
                expectedOldOptionLink: oldFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(3)],
                expectedNewOptionLink: newFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(3)],
                change: filterOptionMetaChanges[4]
            );
            AssertFilterOptionUpdated(
                expectedOldOptionLink: oldFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(7)],
                expectedNewOptionLink: newFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(7)],
                change: filterOptionMetaChanges[5]
            );
            AssertFilterOptionUpdated(
                expectedOldOptionLink: oldFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(8)],
                expectedNewOptionLink: newFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(8)],
                change: filterOptionMetaChanges[6]
            );
            AssertFilterOptionUpdated(
                expectedOldOptionLink: oldFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(4)],
                expectedNewOptionLink: newFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(4)],
                change: filterOptionMetaChanges[7]
            );
            AssertFilterOptionAdded(
                expectedOptionLink: newFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(10)],
                change: filterOptionMetaChanges[8]
            );
            AssertFilterOptionAdded(
                expectedOptionLink: newFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(11)],
                change: filterOptionMetaChanges[9]
            );
            AssertFilterOptionAdded(
                expectedOptionLink: newFilterMetas[SqidEncoder.Encode(1)][SqidEncoder.Encode(9)],
                change: filterOptionMetaChanges[10]
            );
            AssertFilterOptionAdded(
                expectedOptionLink: newFilterMetas[SqidEncoder.Encode(2)][SqidEncoder.Encode(12)],
                change: filterOptionMetaChanges[11]
            );
        }

        private static void AssertSingleFilterDeleted(
            IReadOnlyList<FilterMetaChange> changes,
            FilterMeta expectedFilter
        )
        {
            Assert.Single(
                changes,
                ([UsedImplicitly] c) => c.PreviousStateId == expectedFilter.Id && c.CurrentStateId is null
            );
        }

        private static void AssertSingleFilterAdded(IReadOnlyList<FilterMetaChange> changes, FilterMeta expectedFilter)
        {
            Assert.Single(
                changes,
                ([UsedImplicitly] c) => c.PreviousStateId is null && c.CurrentStateId == expectedFilter.Id
            );
        }

        private static void AssertSingleFilterUpdated(
            IReadOnlyList<FilterMetaChange> changes,
            FilterMeta expectedOldFilter,
            FilterMeta expectedNewFilter
        )
        {
            Assert.Single(
                changes,
                ([UsedImplicitly] c) =>
                    c.PreviousStateId == expectedOldFilter.Id && c.CurrentStateId == expectedNewFilter.Id
            );
        }

        [UsedImplicitly]
        private static void AssertFilterDeleted(FilterMeta expectedFilter, FilterMetaChange change)
        {
            Assert.Equal(expectedFilter.Id, change.PreviousStateId);
            Assert.Null(change.CurrentStateId);
        }

        private static void AssertFilterAdded(FilterMeta expectedFilter, FilterMetaChange change)
        {
            Assert.Null(change.PreviousStateId);
            Assert.Equal(expectedFilter.Id, change.CurrentStateId);
        }

        private static void AssertFilterUpdated(
            FilterMeta expectedOldFilter,
            FilterMeta expectedNewFilter,
            FilterMetaChange change
        )
        {
            Assert.Equal(expectedOldFilter.Id, change.PreviousStateId);
            Assert.Equal(expectedNewFilter.Id, change.CurrentStateId);
        }

        private static void AssertSingleFilterOptionDeleted(
            IReadOnlyList<FilterOptionMetaChange> changes,
            FilterOptionMetaLink expectedOptionLink
        )
        {
            Assert.Single(
                changes,
                ([UsedImplicitly] c) =>
                    c.PreviousState!.PublicId == expectedOptionLink.PublicId
                    && c.PreviousState.MetaId == expectedOptionLink.MetaId
                    && c.PreviousState.OptionId == expectedOptionLink.OptionId
                    && c.CurrentState is null
            );
        }

        private static void AssertSingleFilterOptionAdded(
            IReadOnlyList<FilterOptionMetaChange> changes,
            FilterOptionMetaLink expectedOptionLink
        )
        {
            Assert.Single(
                changes,
                ([UsedImplicitly] c) =>
                    c.PreviousState is null
                    && c.CurrentState!.PublicId == expectedOptionLink.PublicId
                    && c.CurrentState.MetaId == expectedOptionLink.MetaId
                    && c.CurrentState.OptionId == expectedOptionLink.OptionId
            );
        }

        private static void AssertSingleFilterOptionUpdated(
            IReadOnlyList<FilterOptionMetaChange> changes,
            FilterOptionMetaLink expectedOldOptionLink,
            FilterOptionMetaLink expectedNewOptionLink
        )
        {
            Assert.Single(
                changes,
                ([UsedImplicitly] c) =>
                    c.PreviousState!.PublicId == expectedOldOptionLink.PublicId
                    && c.PreviousState.MetaId == expectedOldOptionLink.MetaId
                    && c.PreviousState.OptionId == expectedOldOptionLink.OptionId
                    && c.CurrentState!.PublicId == expectedNewOptionLink.PublicId
                    && c.CurrentState.MetaId == expectedNewOptionLink.MetaId
                    && c.CurrentState.OptionId == expectedNewOptionLink.OptionId
            );
        }

        private static void AssertFilterOptionDeleted(
            FilterOptionMetaLink expectedOptionLink,
            FilterOptionMetaChange change
        )
        {
            Assert.Equal(expectedOptionLink.PublicId, change.PreviousState!.PublicId);
            Assert.Equal(expectedOptionLink.MetaId, change.PreviousState.MetaId);
            Assert.Equal(expectedOptionLink.OptionId, change.PreviousState.OptionId);
            Assert.Null(change.CurrentState);
        }

        private static void AssertFilterOptionAdded(
            FilterOptionMetaLink expectedOptionLink,
            FilterOptionMetaChange change
        )
        {
            Assert.Null(change.PreviousState);
            Assert.Equal(expectedOptionLink.PublicId, change.CurrentState!.PublicId);
            Assert.Equal(expectedOptionLink.MetaId, change.CurrentState.MetaId);
            Assert.Equal(expectedOptionLink.OptionId, change.CurrentState.OptionId);
        }

        private static void AssertFilterOptionUpdated(
            FilterOptionMetaLink expectedOldOptionLink,
            FilterOptionMetaLink expectedNewOptionLink,
            FilterOptionMetaChange change
        )
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
            Func<IEnumerable<FilterOptionMetaLink>>? newOptionLinks = null
        )
        {
            return s =>
                s.SetPublicId(filterMeta.PublicId)
                    .SetColumn(filterMeta.Column)
                    .SetLabel(filterMeta.Label)
                    .SetHint(filterMeta.Hint)
                    .SetOptionLinks(
                        newOptionLinks ??= () =>
                        {
                            return filterMeta.OptionLinks.Select(l =>
                                DataFixture
                                    .DefaultFilterOptionMetaLink()
                                    .ForInstance(UnchangedFilterOptionMetaLinkSetter(l))
                                    .Generate()
                            );
                        }
                    );
        }

        private static Action<InstanceSetters<FilterOptionMetaLink>> UnchangedFilterOptionMetaLinkSetter(
            FilterOptionMetaLink filterOptionMetaLink
        )
        {
            return s => s.SetPublicId(filterOptionMetaLink.PublicId).SetOptionId(filterOptionMetaLink.OptionId);
        }

        private async Task<IReadOnlyList<FilterMetaChange>> GetFilterMetaChanges(DataSetVersion version)
        {
            return await fixture
                .GetPublicDataDbContext()
                .FilterMetaChanges.AsNoTracking()
                .Where(c => c.DataSetVersionId == version.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        private async Task<IReadOnlyList<FilterOptionMetaChange>> GetFilterOptionMetaChanges(DataSetVersion version)
        {
            return await fixture
                .GetPublicDataDbContext()
                .FilterOptionMetaChanges.AsNoTracking()
                .Where(c => c.DataSetVersionId == version.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        private async Task<(DataSetVersion originalVersion, Guid instanceId)> CreateDataSetInitialVersion(
            List<FilterMeta> filterMetas
        )
        {
            return await CommonTestDataUtils.CreateDataSetInitialVersion(
                publicDataDbContext: fixture.GetPublicDataDbContext(),
                dataSetStatus: DataSetStatus.Published,
                dataSetVersionStatus: DataSetVersionStatus.Published,
                importStage: DataSetVersionImportStage.Completing,
                meta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    FilterMetas = filterMetas,
                }
            );
        }

        private async Task<(DataSetVersion nextVersion, Guid instanceId)> CreateDataSetNextVersion(
            DataSetVersion originalVersion,
            List<FilterMeta> filterMetas
        )
        {
            return await CommonTestDataUtils.CreateDataSetNextVersion(
                publicDataDbContext: fixture.GetPublicDataDbContext(),
                initialVersion: originalVersion,
                status: DataSetVersionStatus.Mapping,
                importStage: Stage.PreviousStage(),
                meta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    FilterMetas = filterMetas,
                }
            );
        }
    }

    public class CreateChangesLocationTests(ProcessCompletionOfNextDataSetVersionFunctionsTestsFixture fixture)
        : CreateChangesTests(fixture)
    {
        [Fact]
        public async Task LocationsAdded_ChangesContainOnlyAddedLocations()
        {
            var oldLocationMeta = DataFixture
                .DefaultLocationMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetLevel(GeographicLevel.LocalAuthority)
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    )
                                    .Generate(1)
                            )
                )
                .GenerateList(1);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldLocationMeta);

            var newLocationMeta = DataFixture
                .DefaultLocationMeta()
                .ForIndex(0, UnchangedLocationMetaSetter(oldLocationMeta[0])) // Location and ALL options unchanged
                .ForIndex(
                    1,
                    s =>
                        s.SetLevel(GeographicLevel.School) // Location added
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    ) // Location Option added
                                    .Generate(1)
                            )
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetLevel(GeographicLevel.RscRegion) // Location added
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationRscRegionOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(3))
                                    ) // Location Option added
                                    .Generate(1)
                            )
                )
                .GenerateList(3);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                locationMetas: newLocationMeta
            );

            await CreateChanges(instanceId);

            var locationMetaChanges = await GetLocationMetaChanges(newVersion);
            var locationOptionMetaChanges = await GetLocationOptionMetaChanges(newVersion);

            // 2 Location additions
            Assert.Equal(2, locationMetaChanges.Count);
            Assert.All(locationMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            // No Location Option additions
            Assert.Empty(locationOptionMetaChanges);

            var newLocationMetas = newVersion.LocationMetas.ToDictionary(m => m.Level);

            // Location added
            AssertSingleLocationAdded(locationMetaChanges, newLocationMetas[GeographicLevel.School]);

            // Location added
            AssertSingleLocationAdded(locationMetaChanges, newLocationMetas[GeographicLevel.RscRegion]);
        }

        [Fact]
        public async Task LocationsDeleted_ChangesContainOnlyDeletedLocations()
        {
            var oldLocationMeta = DataFixture
                .DefaultLocationMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetLevel(GeographicLevel.LocalAuthority)
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    )
                                    .Generate(1)
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetLevel(GeographicLevel.School) // Location and ALL options deleted
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    )
                                    .Generate(1)
                            )
                )
                .ForIndex(
                    2,
                    s =>
                        s.SetLevel(GeographicLevel.RscRegion) // Location and ALL options deleted
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationRscRegionOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(3))
                                    )
                                    .Generate(1)
                            )
                )
                .GenerateList(3);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldLocationMeta);

            var newLocationMeta = DataFixture
                .DefaultLocationMeta()
                .ForIndex(0, UnchangedLocationMetaSetter(oldLocationMeta[0])) // Location and ALL options unchanged
                .GenerateList(1);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                locationMetas: newLocationMeta
            );

            await CreateChanges(instanceId);

            var locationMetaChanges = await GetLocationMetaChanges(newVersion);
            var locationOptionMetaChanges = await GetLocationOptionMetaChanges(newVersion);

            // 2 Location deletions
            Assert.Equal(2, locationMetaChanges.Count);
            Assert.All(locationMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            // No Location Option deletions
            Assert.Empty(locationOptionMetaChanges);

            var oldLocationMetas = originalVersion.LocationMetas.ToDictionary(m => m.Level);

            // Location deleted
            AssertSingleLocationDeleted(locationMetaChanges, oldLocationMetas[GeographicLevel.School]);

            // Location deleted
            AssertSingleLocationDeleted(locationMetaChanges, oldLocationMetas[GeographicLevel.RscRegion]);
        }

        [Fact]
        public async Task LocationsUnchangedOptionsAdded_ChangesContainOnlyAddedOptions()
        {
            var oldLocationMeta = DataFixture
                .DefaultLocationMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetLevel(GeographicLevel.LocalAuthority)
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    )
                                    .Generate(1)
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetLevel(GeographicLevel.School)
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    )
                                    .Generate(1)
                            )
                )
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldLocationMeta);

            var newLocationMeta = DataFixture
                .DefaultLocationMeta()
                .ForIndex(
                    0,
                    UnchangedLocationMetaSetter(
                        locationMeta: oldLocationMeta[0], // Location unchanged
                        newOptionLinks: () =>
                            DataFixture
                                .DefaultLocationOptionMetaLink()
                                .ForIndex(0, UnchangedLocationOptionMetaLinkSetter(oldLocationMeta[0].OptionLinks[0])) // Location Option unchanged
                                .ForIndex(
                                    1,
                                    s =>
                                        s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(3))
                                ) // Location Option added
                                .ForIndex(
                                    2,
                                    s =>
                                        s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(4))
                                ) // Location Option added
                                .Generate(3)
                    )
                )
                .ForIndex(
                    1,
                    UnchangedLocationMetaSetter(
                        locationMeta: oldLocationMeta[1], // Location unchanged
                        newOptionLinks: () =>
                            DataFixture
                                .DefaultLocationOptionMetaLink()
                                .ForIndex(0, UnchangedLocationOptionMetaLinkSetter(oldLocationMeta[1].OptionLinks[0])) // Location Option unchanged
                                .ForIndex(
                                    1,
                                    s =>
                                        s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(5))
                                ) // Location Option added
                                .ForIndex(
                                    2,
                                    s =>
                                        s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(6))
                                ) // Location Option added
                                .Generate(3)
                    )
                )
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                locationMetas: newLocationMeta
            );

            await CreateChanges(instanceId);

            var locationMetaChanges = await GetLocationMetaChanges(newVersion);
            var locationOptionMetaChanges = await GetLocationOptionMetaChanges(newVersion);

            // No Location changes
            Assert.Empty(locationMetaChanges);

            // 4 Location Option changes
            Assert.Equal(4, locationOptionMetaChanges.Count);
            Assert.All(locationOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var newLocationMetas = newVersion.LocationMetas.ToDictionary(
                m => m.Level,
                m => m.OptionLinks.ToDictionary(l => l.PublicId)
            );

            // Location Option added
            AssertSingleLocationOptionAdded(
                locationOptionMetaChanges,
                newLocationMetas[GeographicLevel.LocalAuthority][SqidEncoder.Encode(3)]
            );

            // Location Option added
            AssertSingleLocationOptionAdded(
                locationOptionMetaChanges,
                newLocationMetas[GeographicLevel.LocalAuthority][SqidEncoder.Encode(4)]
            );

            // Location Option added
            AssertSingleLocationOptionAdded(
                locationOptionMetaChanges,
                newLocationMetas[GeographicLevel.School][SqidEncoder.Encode(5)]
            );

            // Location Option added
            AssertSingleLocationOptionAdded(
                locationOptionMetaChanges,
                newLocationMetas[GeographicLevel.School][SqidEncoder.Encode(6)]
            );
        }

        [Fact]
        public async Task LocationsUnchangedOptionsDeleted_ChangesContainOnlyDeletedOptions()
        {
            var oldLocationMeta = DataFixture
                .DefaultLocationMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetLevel(GeographicLevel.LocalAuthority)
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    )
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    ) // Location Option deleted
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(3))
                                    ) // Location Option deleted
                                    .Generate(3)
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetLevel(GeographicLevel.School)
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(4))
                                    )
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(5))
                                    ) // Location Option deleted
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(6))
                                    ) // Location Option deleted
                                    .Generate(3)
                            )
                )
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldLocationMeta);

            var newLocationMeta = DataFixture
                .DefaultLocationMeta()
                .ForIndex(
                    0,
                    UnchangedLocationMetaSetter(
                        locationMeta: oldLocationMeta[0], // Location unchanged
                        newOptionLinks: () =>
                            DataFixture
                                .DefaultLocationOptionMetaLink()
                                .ForIndex(0, UnchangedLocationOptionMetaLinkSetter(oldLocationMeta[0].OptionLinks[0])) // Location Option unchanged
                                .Generate(1)
                    )
                )
                .ForIndex(
                    1,
                    UnchangedLocationMetaSetter(
                        locationMeta: oldLocationMeta[1], // Location unchanged
                        newOptionLinks: () =>
                            DataFixture
                                .DefaultLocationOptionMetaLink()
                                .ForIndex(0, UnchangedLocationOptionMetaLinkSetter(oldLocationMeta[1].OptionLinks[0])) // Location Option unchanged
                                .Generate(1)
                    )
                )
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                locationMetas: newLocationMeta
            );

            await CreateChanges(instanceId);

            var locationMetaChanges = await GetLocationMetaChanges(newVersion);
            var locationOptionMetaChanges = await GetLocationOptionMetaChanges(newVersion);

            // No Location changes
            Assert.Empty(locationMetaChanges);

            // 4 Location Option changes
            Assert.Equal(4, locationOptionMetaChanges.Count);
            Assert.All(locationOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldLocationMetas = originalVersion.LocationMetas.ToDictionary(
                m => m.Level,
                m => m.OptionLinks.ToDictionary(l => l.PublicId)
            );

            // Location Option deleted
            AssertSingleLocationOptionDeleted(
                locationOptionMetaChanges,
                oldLocationMetas[GeographicLevel.LocalAuthority][SqidEncoder.Encode(2)]
            );

            // Location Option deleted
            AssertSingleLocationOptionDeleted(
                locationOptionMetaChanges,
                oldLocationMetas[GeographicLevel.LocalAuthority][SqidEncoder.Encode(3)]
            );

            // Location Option deleted
            AssertSingleLocationOptionDeleted(
                locationOptionMetaChanges,
                oldLocationMetas[GeographicLevel.School][SqidEncoder.Encode(5)]
            );

            // Location Option deleted
            AssertSingleLocationOptionDeleted(
                locationOptionMetaChanges,
                oldLocationMetas[GeographicLevel.School][SqidEncoder.Encode(6)]
            );
        }

        [Fact]
        public async Task LocationsUnchangedOptionsUpdated_ChangesContainOnlyUpdatedOptions()
        {
            var oldLocationMeta = DataFixture
                .DefaultLocationMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetLevel(GeographicLevel.LocalAuthority)
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    )
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    )
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(3))
                                    )
                                    .Generate(3)
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetLevel(GeographicLevel.School)
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(4))
                                    )
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(5))
                                    )
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(6))
                                    )
                                    .Generate(3)
                            )
                )
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldLocationMeta);

            var newLocationMeta = DataFixture
                .DefaultLocationMeta()
                .ForIndex(
                    0,
                    UnchangedLocationMetaSetter(
                        locationMeta: oldLocationMeta[0], // Location unchanged
                        newOptionLinks: () =>
                            DataFixture
                                .DefaultLocationOptionMetaLink()
                                .ForIndex(0, UnchangedLocationOptionMetaLinkSetter(oldLocationMeta[0].OptionLinks[0])) // Location Option unchanged
                                .ForIndex(
                                    1,
                                    s =>
                                        s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(2))
                                ) // Location Option updated
                                .ForIndex(
                                    2,
                                    s =>
                                        s.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(3))
                                ) // Location Option updated
                                .Generate(3)
                    )
                )
                .ForIndex(
                    1,
                    UnchangedLocationMetaSetter(
                        locationMeta: oldLocationMeta[1], // Location unchanged
                        newOptionLinks: () =>
                            DataFixture
                                .DefaultLocationOptionMetaLink()
                                .ForIndex(0, UnchangedLocationOptionMetaLinkSetter(oldLocationMeta[1].OptionLinks[0])) // Location Option unchanged
                                .ForIndex(
                                    1,
                                    s =>
                                        s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(5))
                                ) // Location Option updated
                                .ForIndex(
                                    2,
                                    s =>
                                        s.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(6))
                                ) // Location Option updated
                                .Generate(3)
                    )
                )
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                locationMetas: newLocationMeta
            );

            await CreateChanges(instanceId);

            var locationMetaChanges = await GetLocationMetaChanges(newVersion);
            var locationOptionMetaChanges = await GetLocationOptionMetaChanges(newVersion);

            // No Location changes
            Assert.Empty(locationMetaChanges);

            // 4 Location Option changes
            Assert.Equal(4, locationOptionMetaChanges.Count);
            Assert.All(locationOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldLocationMetas = originalVersion.LocationMetas.ToDictionary(
                m => m.Level,
                m => m.OptionLinks.ToDictionary(l => l.PublicId)
            );

            var newLocationMetas = newVersion.LocationMetas.ToDictionary(
                m => m.Level,
                m => m.OptionLinks.ToDictionary(l => l.PublicId)
            );

            // Location Option updated
            AssertSingleLocationOptionUpdated(
                changes: locationOptionMetaChanges,
                expectedOldOptionLink: oldLocationMetas[GeographicLevel.LocalAuthority][SqidEncoder.Encode(2)],
                expectedNewOptionLink: newLocationMetas[GeographicLevel.LocalAuthority][SqidEncoder.Encode(2)]
            );

            // Location Option updated
            AssertSingleLocationOptionUpdated(
                changes: locationOptionMetaChanges,
                expectedOldOptionLink: oldLocationMetas[GeographicLevel.LocalAuthority][SqidEncoder.Encode(3)],
                expectedNewOptionLink: newLocationMetas[GeographicLevel.LocalAuthority][SqidEncoder.Encode(3)]
            );

            // Location Option updated
            AssertSingleLocationOptionUpdated(
                changes: locationOptionMetaChanges,
                expectedOldOptionLink: oldLocationMetas[GeographicLevel.School][SqidEncoder.Encode(5)],
                expectedNewOptionLink: newLocationMetas[GeographicLevel.School][SqidEncoder.Encode(5)]
            );

            // Location Option updated
            AssertSingleLocationOptionUpdated(
                changes: locationOptionMetaChanges,
                expectedOldOptionLink: oldLocationMetas[GeographicLevel.School][SqidEncoder.Encode(6)],
                expectedNewOptionLink: newLocationMetas[GeographicLevel.School][SqidEncoder.Encode(6)]
            );
        }

        [Fact]
        public async Task LocationsUnchangedOptionsUnchanged_ChangesAreEmpty()
        {
            var oldLocationMeta = DataFixture
                .DefaultLocationMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetLevel(GeographicLevel.LocalAuthority)
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationLocalAuthorityOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    )
                                    .Generate(1)
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetLevel(GeographicLevel.School)
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationSchoolOptionMeta())
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    )
                                    .Generate(1)
                            )
                )
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldLocationMeta);

            var newLocationMeta = DataFixture
                .DefaultLocationMeta()
                .ForIndex(0, UnchangedLocationMetaSetter(oldLocationMeta[0])) // Location and ALL options unchanged
                .ForIndex(1, UnchangedLocationMetaSetter(oldLocationMeta[1])) // Location and ALL options unchanged
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                locationMetas: newLocationMeta
            );

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
            var oldLocationMeta = DataFixture
                .DefaultLocationMeta()
                .ForIndex(0, s => s.SetLevel(GeographicLevel.School)) // Location deleted
                .ForIndex(1, s => s.SetLevel(GeographicLevel.LocalAuthority)) // Location deleted
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldLocationMeta);

            var newLocationMeta = DataFixture
                .DefaultLocationMeta()
                .ForIndex(0, s => s.SetLevel(GeographicLevel.RscRegion)) // Location added
                .ForIndex(1, s => s.SetLevel(GeographicLevel.Provider)) // Location added
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                locationMetas: newLocationMeta
            );

            await CreateChanges(instanceId);

            var locationMetaChanges = await GetLocationMetaChanges(newVersion);

            // 4 Location changes
            Assert.Equal(4, locationMetaChanges.Count);
            Assert.All(locationMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldLocationMetas = originalVersion.LocationMetas.ToDictionary(m => m.Level);

            var newLocationMetas = newVersion.LocationMetas.ToDictionary(m => m.Level);

            // The changes should be inserted into each database table ordered alphabetically by 'Level'.
            // They should also be ordered such that all additions come last.

            // Therefore, the expected order of Location changes are (as per their Geographic Level):
            // LocalAuthority deleted
            // School deleted
            // Provider added
            // RscRegion added

            AssertLocationDeleted(
                expectedLocation: oldLocationMetas[GeographicLevel.LocalAuthority],
                change: locationMetaChanges[0]
            );
            AssertLocationDeleted(
                expectedLocation: oldLocationMetas[GeographicLevel.School],
                change: locationMetaChanges[1]
            );
            AssertLocationAdded(
                expectedLocation: newLocationMetas[GeographicLevel.Provider],
                change: locationMetaChanges[2]
            );
            AssertLocationAdded(
                expectedLocation: newLocationMetas[GeographicLevel.RscRegion],
                change: locationMetaChanges[3]
            );
        }

        [Fact]
        public async Task LocationsOptionsAddedAndDeletedAndUpdated_ChangesInsertedIntoDatabaseInCorrectOrder()
        {
            var oldLocationMeta = DataFixture
                .DefaultLocationMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetLevel(GeographicLevel.EnglishDevolvedArea)
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationCodedOptionMeta().WithLabel("d"))
                                                .SetPublicId(SqidEncoder.Encode(1))
                                    ) // Location Option deleted
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationCodedOptionMeta().WithLabel("a"))
                                                .SetPublicId(SqidEncoder.Encode(2))
                                    ) // Location Option deleted
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationCodedOptionMeta().WithLabel("b"))
                                                .SetPublicId(SqidEncoder.Encode(3))
                                    )
                                    .ForIndex(
                                        3,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationCodedOptionMeta().WithLabel("k"))
                                                .SetPublicId(SqidEncoder.Encode(4))
                                    )
                                    .Generate(4)
                            )
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetLevel(GeographicLevel.Country)
                            .SetOptionLinks(() =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForIndex(
                                        0,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationCodedOptionMeta().WithLabel("c"))
                                                .SetPublicId(SqidEncoder.Encode(5))
                                    ) // Location Option deleted
                                    .ForIndex(
                                        1,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationCodedOptionMeta().WithLabel("h"))
                                                .SetPublicId(SqidEncoder.Encode(6))
                                    ) // Location Option deleted
                                    .ForIndex(
                                        2,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationCodedOptionMeta().WithLabel("f"))
                                                .SetPublicId(SqidEncoder.Encode(7))
                                    )
                                    .ForIndex(
                                        3,
                                        ls =>
                                            ls.SetOption(DataFixture.DefaultLocationCodedOptionMeta().WithLabel("i"))
                                                .SetPublicId(SqidEncoder.Encode(8))
                                    )
                                    .Generate(4)
                            )
                )
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldLocationMeta);

            var newLocationMeta = DataFixture
                .DefaultLocationMeta()
                .ForIndex(
                    0,
                    UnchangedLocationMetaSetter(
                        locationMeta: oldLocationMeta[0],
                        newOptionLinks: () =>
                            DataFixture
                                .DefaultLocationOptionMetaLink()
                                .ForIndex(
                                    0,
                                    s =>
                                        s.SetOption(DataFixture.DefaultLocationCodedOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(3))
                                ) // Location Option updated
                                .ForIndex(
                                    1,
                                    s =>
                                        s.SetOption(DataFixture.DefaultLocationCodedOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(4))
                                ) // Location Option updated
                                .ForIndex(
                                    2,
                                    s =>
                                        s.SetOption(DataFixture.DefaultLocationCodedOptionMeta().WithLabel("j"))
                                            .SetPublicId(SqidEncoder.Encode(9))
                                ) // Location Option added
                                .ForIndex(
                                    3,
                                    s =>
                                        s.SetOption(DataFixture.DefaultLocationCodedOptionMeta().WithLabel("e"))
                                            .SetPublicId(SqidEncoder.Encode(10))
                                ) // Location Option added
                                .Generate(4)
                    )
                )
                .ForIndex(
                    1,
                    UnchangedLocationMetaSetter(
                        locationMeta: oldLocationMeta[1],
                        newOptionLinks: () =>
                            DataFixture
                                .DefaultLocationOptionMetaLink()
                                .ForIndex(
                                    0,
                                    s =>
                                        s.SetOption(DataFixture.DefaultLocationCodedOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(7))
                                ) // Location Option updated
                                .ForIndex(
                                    1,
                                    s =>
                                        s.SetOption(DataFixture.DefaultLocationCodedOptionMeta())
                                            .SetPublicId(SqidEncoder.Encode(8))
                                ) // Location Option updated
                                .ForIndex(
                                    2,
                                    s =>
                                        s.SetOption(DataFixture.DefaultLocationCodedOptionMeta().WithLabel("g"))
                                            .SetPublicId(SqidEncoder.Encode(11))
                                ) // Location Option added
                                .ForIndex(
                                    3,
                                    s =>
                                        s.SetOption(DataFixture.DefaultLocationCodedOptionMeta().WithLabel("l"))
                                            .SetPublicId(SqidEncoder.Encode(12))
                                ) // Location Option added
                                .Generate(4)
                    )
                )
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                locationMetas: newLocationMeta
            );

            await CreateChanges(instanceId);

            var locationOptionMetaChanges = await GetLocationOptionMetaChanges(newVersion);

            // 12 Location Option changes
            Assert.Equal(12, locationOptionMetaChanges.Count);
            Assert.All(locationOptionMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldLocationMetas = originalVersion.LocationMetas.ToDictionary(
                m => m.Level,
                m => m.OptionLinks.ToDictionary(l => l.PublicId)
            );

            var newLocationMetas = newVersion.LocationMetas.ToDictionary(
                m => m.Level,
                m => m.OptionLinks.ToDictionary(l => l.PublicId)
            );

            // The changes should be inserted into each database table ordered alphabetically by 'Label'.
            // They should also be ordered such that all deletions come first, updates next, and additions last.

            // Therefore, the expected order of Location Option changes are (as per their Public IDs):
            // Sqid 2 in Location with Level EnglishDevolvedArea deleted
            // Sqid 5 in Location with Level Country deleted
            // Sqid 1 in Location with Level EnglishDevolvedArea deleted
            // Sqid 6 in Location with Level Country deleted
            // Sqid 3 in Location with Level EnglishDevolvedArea updated
            // Sqid 7 in Location with Level Country updated
            // Sqid 8 in Location with Level Country updated
            // Sqid 4 in Location with Level EnglishDevolvedArea updated
            // Sqid 10 in Location with Level EnglishDevolvedArea added
            // Sqid 11 in Location with Level Country added
            // Sqid 9 in Location with Level EnglishDevolvedArea added
            // Sqid 12 in Location with Level Country added

            AssertLocationOptionDeleted(
                expectedOptionLink: oldLocationMetas[GeographicLevel.EnglishDevolvedArea][SqidEncoder.Encode(2)],
                change: locationOptionMetaChanges[0]
            );
            AssertLocationOptionDeleted(
                expectedOptionLink: oldLocationMetas[GeographicLevel.Country][SqidEncoder.Encode(5)],
                change: locationOptionMetaChanges[1]
            );
            AssertLocationOptionDeleted(
                expectedOptionLink: oldLocationMetas[GeographicLevel.EnglishDevolvedArea][SqidEncoder.Encode(1)],
                change: locationOptionMetaChanges[2]
            );
            AssertLocationOptionDeleted(
                expectedOptionLink: oldLocationMetas[GeographicLevel.Country][SqidEncoder.Encode(6)],
                change: locationOptionMetaChanges[3]
            );
            AssertLocationOptionUpdated(
                expectedOldOptionLink: oldLocationMetas[GeographicLevel.EnglishDevolvedArea][SqidEncoder.Encode(3)],
                expectedNewOptionLink: newLocationMetas[GeographicLevel.EnglishDevolvedArea][SqidEncoder.Encode(3)],
                change: locationOptionMetaChanges[4]
            );
            AssertLocationOptionUpdated(
                expectedOldOptionLink: oldLocationMetas[GeographicLevel.Country][SqidEncoder.Encode(7)],
                expectedNewOptionLink: newLocationMetas[GeographicLevel.Country][SqidEncoder.Encode(7)],
                change: locationOptionMetaChanges[5]
            );
            AssertLocationOptionUpdated(
                expectedOldOptionLink: oldLocationMetas[GeographicLevel.Country][SqidEncoder.Encode(8)],
                expectedNewOptionLink: newLocationMetas[GeographicLevel.Country][SqidEncoder.Encode(8)],
                change: locationOptionMetaChanges[6]
            );
            AssertLocationOptionUpdated(
                expectedOldOptionLink: oldLocationMetas[GeographicLevel.EnglishDevolvedArea][SqidEncoder.Encode(4)],
                expectedNewOptionLink: newLocationMetas[GeographicLevel.EnglishDevolvedArea][SqidEncoder.Encode(4)],
                change: locationOptionMetaChanges[7]
            );
            AssertLocationOptionAdded(
                expectedOptionLink: newLocationMetas[GeographicLevel.EnglishDevolvedArea][SqidEncoder.Encode(10)],
                change: locationOptionMetaChanges[8]
            );
            AssertLocationOptionAdded(
                expectedOptionLink: newLocationMetas[GeographicLevel.Country][SqidEncoder.Encode(11)],
                change: locationOptionMetaChanges[9]
            );
            AssertLocationOptionAdded(
                expectedOptionLink: newLocationMetas[GeographicLevel.EnglishDevolvedArea][SqidEncoder.Encode(9)],
                change: locationOptionMetaChanges[10]
            );
            AssertLocationOptionAdded(
                expectedOptionLink: newLocationMetas[GeographicLevel.Country][SqidEncoder.Encode(12)],
                change: locationOptionMetaChanges[11]
            );
        }

        private static void AssertSingleLocationDeleted(
            IReadOnlyList<LocationMetaChange> changes,
            LocationMeta expectedLocation
        )
        {
            Assert.Single(
                changes,
                ([UsedImplicitly] c) => c.PreviousStateId == expectedLocation.Id && c.CurrentStateId is null
            );
        }

        private static void AssertSingleLocationAdded(
            IReadOnlyList<LocationMetaChange> changes,
            LocationMeta expectedLocation
        )
        {
            Assert.Single(
                changes,
                ([UsedImplicitly] c) => c.PreviousStateId is null && c.CurrentStateId == expectedLocation.Id
            );
        }

        [UsedImplicitly]
        private static void AssertLocationDeleted(LocationMeta expectedLocation, LocationMetaChange change)
        {
            Assert.Equal(expectedLocation.Id, change.PreviousStateId);
            Assert.Null(change.CurrentStateId);
        }

        private static void AssertLocationAdded(LocationMeta expectedLocation, LocationMetaChange change)
        {
            Assert.Null(change.PreviousStateId);
            Assert.Equal(expectedLocation.Id, change.CurrentStateId);
        }

        private static void AssertSingleLocationOptionDeleted(
            IReadOnlyList<LocationOptionMetaChange> changes,
            LocationOptionMetaLink expectedOptionLink
        )
        {
            Assert.Single(
                changes,
                ([UsedImplicitly] c) =>
                    c.PreviousState!.PublicId == expectedOptionLink.PublicId
                    && c.PreviousState.MetaId == expectedOptionLink.MetaId
                    && c.PreviousState.OptionId == expectedOptionLink.OptionId
                    && c.CurrentState is null
            );
        }

        private static void AssertSingleLocationOptionAdded(
            IReadOnlyList<LocationOptionMetaChange> changes,
            LocationOptionMetaLink expectedOptionLink
        )
        {
            Assert.Single(
                changes,
                ([UsedImplicitly] c) =>
                    c.PreviousState is null
                    && c.CurrentState!.PublicId == expectedOptionLink.PublicId
                    && c.CurrentState.MetaId == expectedOptionLink.MetaId
                    && c.CurrentState.OptionId == expectedOptionLink.OptionId
            );
        }

        private static void AssertSingleLocationOptionUpdated(
            IReadOnlyList<LocationOptionMetaChange> changes,
            LocationOptionMetaLink expectedOldOptionLink,
            LocationOptionMetaLink expectedNewOptionLink
        )
        {
            Assert.Single(
                changes,
                ([UsedImplicitly] c) =>
                    c.PreviousState!.PublicId == expectedOldOptionLink.PublicId
                    && c.PreviousState.MetaId == expectedOldOptionLink.MetaId
                    && c.PreviousState.OptionId == expectedOldOptionLink.OptionId
                    && c.CurrentState!.PublicId == expectedNewOptionLink.PublicId
                    && c.CurrentState.MetaId == expectedNewOptionLink.MetaId
                    && c.CurrentState.OptionId == expectedNewOptionLink.OptionId
            );
        }

        private static void AssertLocationOptionDeleted(
            LocationOptionMetaLink expectedOptionLink,
            LocationOptionMetaChange change
        )
        {
            Assert.Equal(expectedOptionLink.PublicId, change.PreviousState!.PublicId);
            Assert.Equal(expectedOptionLink.MetaId, change.PreviousState.MetaId);
            Assert.Equal(expectedOptionLink.OptionId, change.PreviousState.OptionId);
            Assert.Null(change.CurrentState);
        }

        private static void AssertLocationOptionAdded(
            LocationOptionMetaLink expectedOptionLink,
            LocationOptionMetaChange change
        )
        {
            Assert.Null(change.PreviousState);
            Assert.Equal(expectedOptionLink.PublicId, change.CurrentState!.PublicId);
            Assert.Equal(expectedOptionLink.MetaId, change.CurrentState.MetaId);
            Assert.Equal(expectedOptionLink.OptionId, change.CurrentState.OptionId);
        }

        private static void AssertLocationOptionUpdated(
            LocationOptionMetaLink expectedOldOptionLink,
            LocationOptionMetaLink expectedNewOptionLink,
            LocationOptionMetaChange change
        )
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
            Func<IEnumerable<LocationOptionMetaLink>>? newOptionLinks = null
        )
        {
            return s =>
                s.SetLevel(locationMeta.Level)
                    .SetOptionLinks(
                        newOptionLinks ??= () =>
                            locationMeta.OptionLinks.Select(l =>
                                DataFixture
                                    .DefaultLocationOptionMetaLink()
                                    .ForInstance(UnchangedLocationOptionMetaLinkSetter(l))
                                    .Generate()
                            )
                    );
        }

        private static Action<InstanceSetters<LocationOptionMetaLink>> UnchangedLocationOptionMetaLinkSetter(
            LocationOptionMetaLink locationOptionMetaLink
        )
        {
            return s => s.SetPublicId(locationOptionMetaLink.PublicId).SetOptionId(locationOptionMetaLink.OptionId);
        }

        private async Task<IReadOnlyList<LocationMetaChange>> GetLocationMetaChanges(DataSetVersion version)
        {
            return await fixture
                .GetPublicDataDbContext()
                .LocationMetaChanges.AsNoTracking()
                .Where(c => c.DataSetVersionId == version.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        private async Task<IReadOnlyList<LocationOptionMetaChange>> GetLocationOptionMetaChanges(DataSetVersion version)
        {
            return await fixture
                .GetPublicDataDbContext()
                .LocationOptionMetaChanges.AsNoTracking()
                .Where(c => c.DataSetVersionId == version.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        private async Task<(DataSetVersion originalVersion, Guid instanceId)> CreateDataSetInitialVersion(
            List<LocationMeta> locationMetas
        )
        {
            return await CommonTestDataUtils.CreateDataSetInitialVersion(
                publicDataDbContext: fixture.GetPublicDataDbContext(),
                dataSetStatus: DataSetStatus.Published,
                dataSetVersionStatus: DataSetVersionStatus.Published,
                importStage: DataSetVersionImportStage.Completing,
                meta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    LocationMetas = locationMetas,
                }
            );
        }

        private async Task<(DataSetVersion nextVersion, Guid instanceId)> CreateDataSetNextVersion(
            DataSetVersion originalVersion,
            List<LocationMeta> locationMetas
        )
        {
            return await CommonTestDataUtils.CreateDataSetNextVersion(
                publicDataDbContext: fixture.GetPublicDataDbContext(),
                initialVersion: originalVersion,
                status: DataSetVersionStatus.Mapping,
                importStage: Stage.PreviousStage(),
                meta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    LocationMetas = locationMetas,
                }
            );
        }
    }

    public class CreateChangesGeographicLevelTests(ProcessCompletionOfNextDataSetVersionFunctionsTestsFixture fixture)
        : CreateChangesTests(fixture)
    {
        [Fact]
        public async Task GeographicLevelsAddedAndDeleted_ChangeExists()
        {
            var (originalVersion, newVersion, instanceId) =
                await CommonTestDataUtils.CreateDataSetInitialAndNextVersion(
                    publicDataDbContext: fixture.GetPublicDataDbContext(),
                    nextVersionStatus: DataSetVersionStatus.Mapping,
                    nextVersionImportStage: Stage.PreviousStage(),
                    initialVersionMeta: new DataSetVersionMeta
                    {
                        GeographicLevelMeta = DataFixture
                            .DefaultGeographicLevelMeta()
                            .WithLevels([
                                GeographicLevel.Country,
                                GeographicLevel.Region,
                                GeographicLevel.LocalAuthority,
                            ]),
                    },
                    nextVersionMeta: new DataSetVersionMeta
                    {
                        GeographicLevelMeta = DataFixture
                            .DefaultGeographicLevelMeta()
                            .WithLevels([
                                GeographicLevel.LocalAuthority,
                                GeographicLevel.LocalAuthorityDistrict,
                                GeographicLevel.School,
                            ]),
                    }
                );

            await CreateChanges(instanceId);

            var actualChange = await fixture
                .GetPublicDataDbContext()
                .GeographicLevelMetaChanges.AsNoTracking()
                .SingleOrDefaultAsync(c => c.DataSetVersionId == newVersion.Id);

            Assert.NotNull(actualChange);
            Assert.Equal(newVersion.Id, actualChange.DataSetVersionId);
            Assert.Equal(originalVersion.GeographicLevelMeta!.Id, actualChange.PreviousStateId);
            Assert.Equal(newVersion.GeographicLevelMeta!.Id, actualChange.CurrentStateId);
        }

        [Fact]
        public async Task GeographicLevelsUnchanged_ChangeIsNull()
        {
            var (_, newVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialAndNextVersion(
                publicDataDbContext: fixture.GetPublicDataDbContext(),
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture
                        .DefaultGeographicLevelMeta()
                        .WithLevels([GeographicLevel.Country, GeographicLevel.Region, GeographicLevel.LocalAuthority]),
                },
                nextVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture
                        .DefaultGeographicLevelMeta()
                        .WithLevels([GeographicLevel.Country, GeographicLevel.Region, GeographicLevel.LocalAuthority]),
                }
            );

            await CreateChanges(instanceId);

            var actualChange = await fixture
                .GetPublicDataDbContext()
                .GeographicLevelMetaChanges.AsNoTracking()
                .SingleOrDefaultAsync(c => c.DataSetVersionId == newVersion.Id);

            Assert.Null(actualChange);
        }

        [Fact]
        public async Task GeographicLevelsAdded_ChangeExists()
        {
            var (originalVersion, newVersion, instanceId) =
                await CommonTestDataUtils.CreateDataSetInitialAndNextVersion(
                    publicDataDbContext: fixture.GetPublicDataDbContext(),
                    nextVersionStatus: DataSetVersionStatus.Mapping,
                    nextVersionImportStage: Stage.PreviousStage(),
                    initialVersionMeta: new DataSetVersionMeta
                    {
                        GeographicLevelMeta = DataFixture
                            .DefaultGeographicLevelMeta()
                            .WithLevels([GeographicLevel.Country]),
                    },
                    nextVersionMeta: new DataSetVersionMeta
                    {
                        GeographicLevelMeta = DataFixture
                            .DefaultGeographicLevelMeta()
                            .WithLevels([
                                GeographicLevel.Country,
                                GeographicLevel.Region,
                                GeographicLevel.LocalAuthority,
                            ]),
                    }
                );

            await CreateChanges(instanceId);

            var actualChange = await fixture
                .GetPublicDataDbContext()
                .GeographicLevelMetaChanges.AsNoTracking()
                .SingleOrDefaultAsync(c => c.DataSetVersionId == newVersion.Id);

            Assert.NotNull(actualChange);
            Assert.Equal(newVersion.Id, actualChange.DataSetVersionId);
            Assert.Equal(originalVersion.GeographicLevelMeta!.Id, actualChange.PreviousStateId);
            Assert.Equal(newVersion.GeographicLevelMeta!.Id, actualChange.CurrentStateId);
        }

        [Fact]
        public async Task GeographicLevelsDeleted_ChangeExists()
        {
            var (originalVersion, newVersion, instanceId) =
                await CommonTestDataUtils.CreateDataSetInitialAndNextVersion(
                    publicDataDbContext: fixture.GetPublicDataDbContext(),
                    nextVersionStatus: DataSetVersionStatus.Mapping,
                    nextVersionImportStage: Stage.PreviousStage(),
                    initialVersionMeta: new DataSetVersionMeta
                    {
                        GeographicLevelMeta = DataFixture
                            .DefaultGeographicLevelMeta()
                            .WithLevels([
                                GeographicLevel.Country,
                                GeographicLevel.Region,
                                GeographicLevel.LocalAuthority,
                            ]),
                    },
                    nextVersionMeta: new DataSetVersionMeta
                    {
                        GeographicLevelMeta = DataFixture
                            .DefaultGeographicLevelMeta()
                            .WithLevels([GeographicLevel.Country]),
                    }
                );

            await CreateChanges(instanceId);

            var actualChange = await fixture
                .GetPublicDataDbContext()
                .GeographicLevelMetaChanges.AsNoTracking()
                .SingleOrDefaultAsync(c => c.DataSetVersionId == newVersion.Id);

            Assert.NotNull(actualChange);
            Assert.Equal(newVersion.Id, actualChange.DataSetVersionId);
            Assert.Equal(originalVersion.GeographicLevelMeta!.Id, actualChange.PreviousStateId);
            Assert.Equal(newVersion.GeographicLevelMeta!.Id, actualChange.CurrentStateId);
        }
    }

    public class CreateChangesIndicatorTests(ProcessCompletionOfNextDataSetVersionFunctionsTestsFixture fixture)
        : CreateChangesTests(fixture)
    {
        [Fact]
        public async Task IndicatorsAdded_ChangesContainOnlyAddedIndicators()
        {
            var oldIndicatorMeta = DataFixture
                .DefaultIndicatorMeta()
                .ForIndex(0, s => s.SetPublicId(SqidEncoder.Encode(1)))
                .GenerateList(1);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldIndicatorMeta);

            var newIndicatorMeta = DataFixture
                .DefaultIndicatorMeta()
                .ForIndex(0, UnchangedIndicatorMetaSetter(oldIndicatorMeta[0])) // Indicator unchanged
                .ForIndex(1, s => s.SetPublicId(SqidEncoder.Encode(2))) // Indicator added
                .ForIndex(2, s => s.SetPublicId(SqidEncoder.Encode(3))) // Indicator added
                .GenerateList(3);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                indicatorMetas: newIndicatorMeta
            );

            await CreateChanges(instanceId);

            var changes = await GetIndicatorMetaChanges(newVersion);

            // 2 Indicator changes
            Assert.Equal(2, changes.Count);
            Assert.All(changes, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var newIndicatorMetas = newVersion.IndicatorMetas.ToDictionary(m => m.PublicId);

            // Indicator added
            AssertSingleIndicatorAdded(changes, newIndicatorMetas[SqidEncoder.Encode(2)]);

            // Indicator added
            AssertSingleIndicatorAdded(changes, newIndicatorMetas[SqidEncoder.Encode(3)]);
        }

        [Fact]
        public async Task IndicatorsDeleted_ChangesContainOnlyDeletedIndicators()
        {
            var oldIndicatorMeta = DataFixture
                .DefaultIndicatorMeta()
                .ForIndex(0, s => s.SetPublicId(SqidEncoder.Encode(1)))
                .ForIndex(1, s => s.SetPublicId(SqidEncoder.Encode(2))) // Indicator deleted
                .ForIndex(2, s => s.SetPublicId(SqidEncoder.Encode(3))) // Indicator deleted
                .GenerateList(3);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldIndicatorMeta);

            var newIndicatorMeta = DataFixture
                .DefaultIndicatorMeta()
                .ForIndex(0, UnchangedIndicatorMetaSetter(oldIndicatorMeta[0])) // Indicator unchanged
                .GenerateList(1);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                indicatorMetas: newIndicatorMeta
            );

            await CreateChanges(instanceId);

            var changes = await GetIndicatorMetaChanges(newVersion);

            // 2 Indicator changes
            Assert.Equal(2, changes.Count);
            Assert.All(changes, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldIndicatorMetas = originalVersion.IndicatorMetas.ToDictionary(m => m.PublicId);

            // Indicator deleted
            AssertSingleIndicatorDeleted(changes, oldIndicatorMetas[SqidEncoder.Encode(2)]);

            // Indicator deleted
            AssertSingleIndicatorDeleted(changes, oldIndicatorMetas[SqidEncoder.Encode(3)]);
        }

        [Fact]
        public async Task IndicatorsUnchanged_ChangesAreEmpty()
        {
            var oldIndicatorMeta = DataFixture
                .DefaultIndicatorMeta()
                .ForIndex(0, s => s.SetPublicId(SqidEncoder.Encode(1)))
                .ForIndex(1, s => s.SetPublicId(SqidEncoder.Encode(2)))
                .GenerateList(2);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldIndicatorMeta);

            var newIndicatorMeta = DataFixture
                .DefaultIndicatorMeta()
                .ForIndex(0, UnchangedIndicatorMetaSetter(oldIndicatorMeta[0])) // Indicator unchanged
                .ForIndex(1, UnchangedIndicatorMetaSetter(oldIndicatorMeta[1])) // Indicator unchanged
                .GenerateList(2);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                indicatorMetas: newIndicatorMeta
            );

            await CreateChanges(instanceId);

            var changes = await GetIndicatorMetaChanges(newVersion);

            // No Indicator changes
            Assert.Empty(changes);
        }

        [Fact]
        public async Task IndicatorsAddedAndDeletedAndUpdated_ChangesInsertedIntoDatabaseInCorrectOrder()
        {
            var oldIndicatorMeta = DataFixture
                .DefaultIndicatorMeta()
                .ForIndex(
                    0,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(1)) // Indicator deleted
                            .SetLabel("f")
                )
                .ForIndex(
                    1,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(2)) // Indicator deleted
                            .SetLabel("a")
                )
                .ForIndex(2, s => s.SetPublicId(SqidEncoder.Encode(3)).SetLabel("e"))
                .ForIndex(3, s => s.SetPublicId(SqidEncoder.Encode(4)).SetLabel("b"))
                .GenerateList(4);

            var (originalVersion, _) = await CreateDataSetInitialVersion(oldIndicatorMeta);

            var newIndicatorMeta = DataFixture
                .DefaultIndicatorMeta()
                .ForIndex(0, s => s.SetPublicId(SqidEncoder.Encode(3))) // Indicator updated
                .ForIndex(1, s => s.SetPublicId(SqidEncoder.Encode(4))) // Indicator updated
                .ForIndex(
                    2,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(5)) // Indicator added
                            .SetLabel("d")
                )
                .ForIndex(
                    3,
                    s =>
                        s.SetPublicId(SqidEncoder.Encode(6)) // Indicator added
                            .SetLabel("c")
                )
                .GenerateList(4);

            var (newVersion, instanceId) = await CreateDataSetNextVersion(
                originalVersion: originalVersion,
                indicatorMetas: newIndicatorMeta
            );

            await CreateChanges(instanceId);

            var indicatorMetaChanges = await GetIndicatorMetaChanges(newVersion);

            // 6 Indicator changes
            Assert.Equal(6, indicatorMetaChanges.Count);
            Assert.All(indicatorMetaChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var oldIndicatorMetas = originalVersion.IndicatorMetas.ToDictionary(m => m.PublicId);

            var newIndicatorMetas = newVersion.IndicatorMetas.ToDictionary(m => m.PublicId);

            // The changes should be inserted into each database table ordered alphabetically by 'Label'.
            // They should also be ordered such that all deletions come first, updates next, and additions last.

            // Therefore, the expected order of Indicator changes are (as per their Public IDs):
            // Sqid 2 deleted
            // Sqid 1 deleted
            // Sqid 4 updated
            // Sqid 3 updated
            // Sqid 6 added
            // Sqid 5 added

            AssertIndicatorDeleted(
                expectedIndicator: oldIndicatorMetas[SqidEncoder.Encode(2)],
                change: indicatorMetaChanges[0]
            );
            AssertIndicatorDeleted(
                expectedIndicator: oldIndicatorMetas[SqidEncoder.Encode(1)],
                change: indicatorMetaChanges[1]
            );
            AssertIndicatorUpdated(
                expectedOldIndicator: oldIndicatorMetas[SqidEncoder.Encode(4)],
                expectedNewIndicator: newIndicatorMetas[SqidEncoder.Encode(4)],
                change: indicatorMetaChanges[2]
            );
            AssertIndicatorUpdated(
                expectedOldIndicator: oldIndicatorMetas[SqidEncoder.Encode(3)],
                expectedNewIndicator: newIndicatorMetas[SqidEncoder.Encode(3)],
                change: indicatorMetaChanges[3]
            );
            AssertIndicatorAdded(
                expectedIndicator: newIndicatorMetas[SqidEncoder.Encode(6)],
                change: indicatorMetaChanges[4]
            );
            AssertIndicatorAdded(
                expectedIndicator: newIndicatorMetas[SqidEncoder.Encode(5)],
                change: indicatorMetaChanges[5]
            );
        }

        private static void AssertSingleIndicatorDeleted(
            IReadOnlyList<IndicatorMetaChange> changes,
            IndicatorMeta expectedIndicator
        )
        {
            Assert.Single(
                changes,
                ([UsedImplicitly] c) => c.PreviousStateId == expectedIndicator.Id && c.CurrentStateId is null
            );
        }

        private static void AssertSingleIndicatorAdded(
            IReadOnlyList<IndicatorMetaChange> changes,
            IndicatorMeta expectedIndicator
        )
        {
            Assert.Single(
                changes,
                ([UsedImplicitly] c) => c.PreviousStateId is null && c.CurrentStateId == expectedIndicator.Id
            );
        }

        [UsedImplicitly]
        private static void AssertIndicatorDeleted(IndicatorMeta expectedIndicator, IndicatorMetaChange change)
        {
            Assert.Equal(expectedIndicator.Id, change.PreviousStateId);
            Assert.Null(change.CurrentStateId);
        }

        private static void AssertIndicatorAdded(IndicatorMeta expectedIndicator, IndicatorMetaChange change)
        {
            Assert.Null(change.PreviousStateId);
            Assert.Equal(expectedIndicator.Id, change.CurrentStateId);
        }

        private static void AssertIndicatorUpdated(
            IndicatorMeta expectedOldIndicator,
            IndicatorMeta expectedNewIndicator,
            IndicatorMetaChange change
        )
        {
            Assert.Equal(expectedOldIndicator.Id, change.PreviousStateId);
            Assert.Equal(expectedNewIndicator.Id, change.CurrentStateId);
        }

        private static Action<InstanceSetters<IndicatorMeta>> UnchangedIndicatorMetaSetter(IndicatorMeta indicatorMeta)
        {
            return s =>
                s.SetPublicId(indicatorMeta.PublicId)
                    .SetColumn(indicatorMeta.Column)
                    .SetLabel(indicatorMeta.Label)
                    .SetUnit(indicatorMeta.Unit)
                    .SetDecimalPlaces(indicatorMeta.DecimalPlaces);
        }

        private async Task<IReadOnlyList<IndicatorMetaChange>> GetIndicatorMetaChanges(DataSetVersion version)
        {
            return await fixture
                .GetPublicDataDbContext()
                .IndicatorMetaChanges.AsNoTracking()
                .Where(c => c.DataSetVersionId == version.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        private async Task<(DataSetVersion originalVersion, Guid instanceId)> CreateDataSetInitialVersion(
            List<IndicatorMeta> indicatorMetas
        )
        {
            return await CommonTestDataUtils.CreateDataSetInitialVersion(
                publicDataDbContext: fixture.GetPublicDataDbContext(),
                dataSetStatus: DataSetStatus.Published,
                dataSetVersionStatus: DataSetVersionStatus.Published,
                importStage: DataSetVersionImportStage.Completing,
                meta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    IndicatorMetas = indicatorMetas,
                }
            );
        }

        private async Task<(DataSetVersion nextVersion, Guid instanceId)> CreateDataSetNextVersion(
            DataSetVersion originalVersion,
            List<IndicatorMeta> indicatorMetas
        )
        {
            return await CommonTestDataUtils.CreateDataSetNextVersion(
                publicDataDbContext: fixture.GetPublicDataDbContext(),
                initialVersion: originalVersion,
                status: DataSetVersionStatus.Mapping,
                importStage: Stage.PreviousStage(),
                meta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    IndicatorMetas = indicatorMetas,
                }
            );
        }
    }

    public class CreateChangesTimePeriodTests(ProcessCompletionOfNextDataSetVersionFunctionsTestsFixture fixture)
        : CreateChangesTests(fixture)
    {
        [Fact]
        public async Task TimePeriodsAddedAndDeleted_ChangesContainAdditionsAndDeletions()
        {
            var (originalVersion, newVersion, instanceId) =
                await CommonTestDataUtils.CreateDataSetInitialAndNextVersion(
                    publicDataDbContext: fixture.GetPublicDataDbContext(),
                    nextVersionStatus: DataSetVersionStatus.Mapping,
                    nextVersionImportStage: Stage.PreviousStage(),
                    initialVersionMeta: new DataSetVersionMeta
                    {
                        GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                        TimePeriodMetas = DataFixture
                            .DefaultTimePeriodMeta()
                            .WithCode(TimeIdentifier.AcademicYear)
                            .ForIndex(0, s => s.SetPeriod("2020"))
                            .ForIndex(1, s => s.SetPeriod("2021"))
                            .ForIndex(2, s => s.SetPeriod("2022"))
                            .Generate(3),
                    },
                    nextVersionMeta: new DataSetVersionMeta
                    {
                        GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                        TimePeriodMetas = DataFixture
                            .DefaultTimePeriodMeta()
                            .WithCode(TimeIdentifier.AcademicYear)
                            .ForIndex(0, s => s.SetPeriod("2022"))
                            .ForIndex(1, s => s.SetPeriod("2023"))
                            .ForIndex(2, s => s.SetPeriod("2024"))
                            .Generate(3),
                    }
                );

            await CreateChanges(instanceId);

            var actualChanges = await fixture
                .GetPublicDataDbContext()
                .TimePeriodMetaChanges.AsNoTracking()
                .Where(c => c.DataSetVersionId == newVersion.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();

            Assert.Equal(4, actualChanges.Count);
            Assert.All(actualChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));

            var originalTimePeriodMetas = originalVersion.TimePeriodMetas.ToDictionary(m => (m.Code, m.Period));

            Assert.Equal(
                originalTimePeriodMetas[(TimeIdentifier.AcademicYear, "2020")].Id,
                actualChanges[0].PreviousStateId
            );
            Assert.Null(actualChanges[0].CurrentStateId);

            Assert.Equal(
                originalTimePeriodMetas[(TimeIdentifier.AcademicYear, "2021")].Id,
                actualChanges[1].PreviousStateId
            );
            Assert.Null(actualChanges[1].CurrentStateId);

            var newTimePeriodMetas = newVersion.TimePeriodMetas.ToDictionary(m => (m.Code, m.Period));

            Assert.Null(actualChanges[2].PreviousStateId);
            Assert.Equal(newTimePeriodMetas[(TimeIdentifier.AcademicYear, "2023")].Id, actualChanges[2].CurrentStateId);

            Assert.Null(actualChanges[3].PreviousStateId);
            Assert.Equal(newTimePeriodMetas[(TimeIdentifier.AcademicYear, "2024")].Id, actualChanges[3].CurrentStateId);
        }

        [Fact]
        public async Task TimePeriodsUnchanged_ChangesAreEmpty()
        {
            var (_, newVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialAndNextVersion(
                publicDataDbContext: fixture.GetPublicDataDbContext(),
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    TimePeriodMetas = DataFixture
                        .DefaultTimePeriodMeta()
                        .WithCode(TimeIdentifier.AcademicYear)
                        .ForIndex(0, s => s.SetPeriod("2019").SetCode(TimeIdentifier.AcademicYear))
                        .ForIndex(1, s => s.SetPeriod("2020").SetCode(TimeIdentifier.CalendarYear))
                        .ForIndex(2, s => s.SetPeriod("2021").SetCode(TimeIdentifier.January))
                        .Generate(3),
                },
                nextVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    TimePeriodMetas = DataFixture
                        .DefaultTimePeriodMeta()
                        .ForIndex(0, s => s.SetPeriod("2019").SetCode(TimeIdentifier.AcademicYear))
                        .ForIndex(1, s => s.SetPeriod("2020").SetCode(TimeIdentifier.CalendarYear))
                        .ForIndex(2, s => s.SetPeriod("2021").SetCode(TimeIdentifier.January))
                        .Generate(3),
                }
            );

            await CreateChanges(instanceId);

            Assert.False(
                await fixture
                    .GetPublicDataDbContext()
                    .TimePeriodMetaChanges.AnyAsync(c => c.DataSetVersionId == newVersion.Id)
            );
        }

        [Fact]
        public async Task TimePeriodsAdded_ChangesContainOnlyAdditions()
        {
            var (_, newVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialAndNextVersion(
                publicDataDbContext: fixture.GetPublicDataDbContext(),
                nextVersionStatus: DataSetVersionStatus.Mapping,
                nextVersionImportStage: Stage.PreviousStage(),
                initialVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    TimePeriodMetas = DataFixture
                        .DefaultTimePeriodMeta()
                        .WithCode(TimeIdentifier.CalendarYear)
                        .WithPeriod("2020")
                        .Generate(1),
                },
                nextVersionMeta: new DataSetVersionMeta
                {
                    GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                    TimePeriodMetas =
                    [
                        .. DataFixture
                            .DefaultTimePeriodMeta()
                            .WithCode(TimeIdentifier.CalendarYear)
                            .WithPeriod("2020")
                            .Generate(1),
                        .. DataFixture
                            .DefaultTimePeriodMeta()
                            .WithCode(TimeIdentifier.AcademicYear)
                            .ForIndex(0, s => s.SetPeriod("2019"))
                            .ForIndex(1, s => s.SetPeriod("2020"))
                            .ForIndex(2, s => s.SetPeriod("2021"))
                            .Generate(3),
                    ],
                }
            );

            await CreateChanges(instanceId);

            var actualChanges = await fixture
                .GetPublicDataDbContext()
                .TimePeriodMetaChanges.AsNoTracking()
                .Where(c => c.DataSetVersionId == newVersion.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();

            Assert.Equal(3, actualChanges.Count);
            Assert.All(actualChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));
            Assert.All(actualChanges, c => Assert.Null(c.PreviousStateId));

            var newTimePeriodMetas = newVersion.TimePeriodMetas.ToDictionary(m => (m.Code, m.Period));

            Assert.Equal(newTimePeriodMetas[(TimeIdentifier.AcademicYear, "2019")].Id, actualChanges[0].CurrentStateId);
            Assert.Equal(newTimePeriodMetas[(TimeIdentifier.AcademicYear, "2020")].Id, actualChanges[1].CurrentStateId);
            Assert.Equal(newTimePeriodMetas[(TimeIdentifier.AcademicYear, "2021")].Id, actualChanges[2].CurrentStateId);
        }

        [Fact]
        public async Task TimePeriodsDeleted_ChangesContainOnlyDeletions()
        {
            var (originalVersion, newVersion, instanceId) =
                await CommonTestDataUtils.CreateDataSetInitialAndNextVersion(
                    publicDataDbContext: fixture.GetPublicDataDbContext(),
                    nextVersionStatus: DataSetVersionStatus.Mapping,
                    nextVersionImportStage: Stage.PreviousStage(),
                    initialVersionMeta: new DataSetVersionMeta
                    {
                        GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                        TimePeriodMetas =
                        [
                            .. DataFixture
                                .DefaultTimePeriodMeta()
                                .WithCode(TimeIdentifier.CalendarYear)
                                .WithPeriod("2020")
                                .Generate(1),
                            .. DataFixture
                                .DefaultTimePeriodMeta()
                                .WithCode(TimeIdentifier.AcademicYear)
                                .ForIndex(0, s => s.SetPeriod("2019"))
                                .ForIndex(1, s => s.SetPeriod("2020"))
                                .ForIndex(2, s => s.SetPeriod("2021"))
                                .Generate(3),
                        ],
                    },
                    nextVersionMeta: new DataSetVersionMeta
                    {
                        GeographicLevelMeta = DataFixture.DefaultGeographicLevelMeta(),
                        TimePeriodMetas = DataFixture
                            .DefaultTimePeriodMeta()
                            .WithCode(TimeIdentifier.CalendarYear)
                            .WithPeriod("2020")
                            .Generate(1),
                    }
                );

            await CreateChanges(instanceId);

            var actualChanges = await fixture
                .GetPublicDataDbContext()
                .TimePeriodMetaChanges.AsNoTracking()
                .Where(c => c.DataSetVersionId == newVersion.Id)
                .OrderBy(c => c.Id)
                .ToListAsync();

            Assert.Equal(3, actualChanges.Count);
            Assert.All(actualChanges, c => Assert.Equal(newVersion.Id, c.DataSetVersionId));
            Assert.All(actualChanges, c => Assert.Null(c.CurrentStateId));

            var oldTimePeriodMetas = originalVersion.TimePeriodMetas.ToDictionary(m => (m.Code, m.Period));

            Assert.Equal(
                oldTimePeriodMetas[(TimeIdentifier.AcademicYear, "2019")].Id,
                actualChanges[0].PreviousStateId
            );
            Assert.Equal(
                oldTimePeriodMetas[(TimeIdentifier.AcademicYear, "2020")].Id,
                actualChanges[1].PreviousStateId
            );
            Assert.Equal(
                oldTimePeriodMetas[(TimeIdentifier.AcademicYear, "2021")].Id,
                actualChanges[2].PreviousStateId
            );
        }
    }

    public class CompleteNextDataSetVersionImportProcessingTests(
        ProcessCompletionOfNextDataSetVersionFunctionsTestsFixture fixture
    ) : ProcessCompletionOfNextDataSetVersionFunctionsTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.Completing;

        [Fact]
        public async Task Success()
        {
            var (dataSetVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                publicDataDbContext: fixture.GetPublicDataDbContext(),
                Stage.PreviousStage()
            );

            Directory.CreateDirectory(fixture.GetDataSetVersionPathResolver().DirectoryPath(dataSetVersion));

            await CompleteNextDataSetVersionImportProcessing(instanceId);

            var savedImport = await fixture
                .GetPublicDataDbContext()
                .DataSetVersionImports.Include(i => i.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);

            Assert.Equal(Stage, savedImport.Stage);
            savedImport.Completed.AssertUtcNow();

            Assert.Equal(DataSetVersionStatus.Draft, savedImport.DataSetVersion.Status);
        }

        [Fact]
        public async Task DuckDbFileIsDeleted()
        {
            var (dataSetVersion, instanceId) = await CommonTestDataUtils.CreateDataSetInitialVersion(
                publicDataDbContext: fixture.GetPublicDataDbContext(),
                Stage.PreviousStage()
            );

            // Create empty data set version files for all file paths
            var directoryPath = fixture.GetDataSetVersionPathResolver().DirectoryPath(dataSetVersion);
            Directory.CreateDirectory(directoryPath);
            foreach (var filename in AllDataSetVersionFiles)
            {
                await File.Create(Path.Combine(directoryPath, filename)).DisposeAsync();
            }

            await CompleteNextDataSetVersionImportProcessing(instanceId);

            // Ensure the duck db database file is the only file that was deleted
            CommonTestDataUtils.AssertDataSetVersionDirectoryContainsOnlyFiles(
                dataSetVersionPathResolver: fixture.GetDataSetVersionPathResolver(),
                dataSetVersion,
                AllDataSetVersionFiles.Where(file => file != DataSetFilenames.DuckDbDatabaseFile).ToArray()
            );
        }

        private async Task CompleteNextDataSetVersionImportProcessing(Guid instanceId)
        {
            await fixture.Function.CompleteNextDataSetVersionImportProcessing(instanceId, CancellationToken.None);
        }
    }
}
