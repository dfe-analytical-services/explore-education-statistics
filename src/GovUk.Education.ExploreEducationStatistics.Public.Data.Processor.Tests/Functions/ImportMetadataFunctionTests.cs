using Dapper;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using InterpolatedSql.Dapper;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Tests.Functions;

public abstract class ImportMetadataFunctionTests(ProcessorFunctionsIntegrationTestFixture fixture)
    : ProcessorFunctionsIntegrationTest(fixture)
{
    public class ImportMetadataTests(ProcessorFunctionsIntegrationTestFixture fixture)
        : ImportMetadataFunctionTests(fixture)
    {
        private const DataSetVersionImportStage Stage = DataSetVersionImportStage.ImportingMetadata;

        public static readonly TheoryData<ProcessorTestData> Data = new()
        {
            ProcessorTestData.AbsenceSchool,
        };

        [Theory]
        [MemberData(nameof(Data))]
        public async Task Success(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var savedImport = await publicDataDbContext.DataSetVersionImports
                .Include(i => i.DataSetVersion)
                .SingleAsync(i => i.InstanceId == instanceId);
            var savedDataSetVersion = savedImport.DataSetVersion;

            Assert.Equal(Stage, savedImport.Stage);
            Assert.Equal(DataSetVersionStatus.Processing, savedDataSetVersion.Status);

            AssertDataSetVersionDirectoryContainsOnlyFiles(dataSetVersion,
            [
                DataSetFilenames.CsvDataFile,
                DataSetFilenames.CsvMetadataFile,
                DataSetFilenames.DuckDbDatabaseFile
            ]);

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
                        Period = firstExpectedTimePeriod.Period
                    },
                    End = new TimePeriodRangeBound
                    {
                        Code = firstExpectedTimePeriod.Code,
                        Period = lastExpectedTimePeriod.Period
                    }
                }
            };

            savedDataSetVersion.MetaSummary.AssertDeepEqualTo(expectedMetaSummary);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task DataSetVersionMeta_CorrectGeographicLevels(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var actualGeographicLevelMeta = await publicDataDbContext.GeographicLevelMetas
                .SingleAsync(glm => glm.DataSetVersionId == dataSetVersion.Id);

            var actualGeographicLevels = actualGeographicLevelMeta.Levels
                .OrderBy(EnumToEnumLabelConverter<GeographicLevel>.ToProvider)
                .ToList();

            Assert.Equal(testData.ExpectedGeographicLevels.Order(), actualGeographicLevels.Order());
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task DataSetVersionMeta_CorrectLocations(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var actualLocations = await publicDataDbContext.LocationMetas
                .Include(lm => lm.Options)
                .Where(lm => lm.DataSetVersionId == dataSetVersion.Id)
                .OrderBy(lm => lm.Level)
                .ToListAsync();

            // Locations are expected in order of level
            // Location options are expected in order of code(s) and then by label
            Assert.Equal(testData.ExpectedLocations.Count, actualLocations.Count);
            Assert.All(testData.ExpectedLocations,
                (expectedLocation, index) =>
                {
                    var actualLocation = actualLocations[index];
                    actualLocation.AssertDeepEqualTo(expectedLocation,
                        AssertExtensions.Except<LocationMeta>(
                            l => l.Id,
                            l => l.DataSetVersionId,
                            l => l.Options,
                            l => l.OptionLinks,
                            l => l.Created
                        ));

                    Assert.Equal(expectedLocation.Options.Count, actualLocation.Options.Count);
                    Assert.All(expectedLocation.Options,
                        (expectedOption, optionIndex) =>
                        {
                            var actualOption = actualLocation.Options[optionIndex];
                            actualOption.AssertDeepEqualTo(expectedOption,
                                AssertExtensions.Except<LocationOptionMeta>(
                                    o => o.Metas,
                                    o => o.MetaLinks
                                ));
                        });
                });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task DataSetVersionMeta_CorrectTimePeriods(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var actualTimePeriods = await publicDataDbContext.TimePeriodMetas
                .Where(tpm => tpm.DataSetVersionId == dataSetVersion.Id)
                .OrderBy(tpm => tpm.Period)
                .ToListAsync();

            Assert.Equal(testData.ExpectedTimePeriods.Count, actualTimePeriods.Count);
            Assert.All(testData.ExpectedTimePeriods,
                (expectedTimePeriod, index) =>
                {
                    var actualTimePeriod = actualTimePeriods[index];
                    actualTimePeriod.AssertDeepEqualTo(expectedTimePeriod,
                        AssertExtensions.Except<TimePeriodMeta>(
                            tpm => tpm.Id,
                            tpm => tpm.DataSetVersionId,
                            tpm => tpm.Created
                        ));
                });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task DataSetVersionMeta_CorrectFilters(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var actualFilters = await publicDataDbContext.FilterMetas
                .Include(filterMeta => filterMeta.Options.OrderBy(o => o.Label))
                .Where(fm => fm.DataSetVersionId == dataSetVersion.Id)
                .OrderBy(fm => fm.Label)
                .ToListAsync();

            Assert.Equal(testData.ExpectedFilters.Count, actualFilters.Count);
            Assert.All(testData.ExpectedFilters,
                (expectedFilter, index) =>
                {
                    var actualFilter = actualFilters[index];
                    actualFilter.AssertDeepEqualTo(expectedFilter,
                        AssertExtensions.Except<FilterMeta>(
                            fm => fm.Id,
                            fm => fm.DataSetVersionId,
                            fm => fm.Created,
                            fm => fm.Options,
                            fm => fm.OptionLinks
                        ));

                    Assert.Equal(expectedFilter.Options.Count, actualFilter.Options.Count);
                    Assert.All(expectedFilter.Options,
                        (expectedOption, optionIndex) =>
                        {
                            var actualOption = actualFilter.Options[optionIndex];
                            actualOption.AssertDeepEqualTo(expectedOption,
                                AssertExtensions.Except<FilterOptionMeta>(
                                    o => o.Id,
                                    o => o.Metas,
                                    o => o.MetaLinks
                                ));
                        });
                });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task DataSetVersionMeta_CorrectIndicators(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var publicDataDbContext = GetDbContext<PublicDataDbContext>();

            var actualIndicators = await publicDataDbContext.IndicatorMetas
                .Where(im => im.DataSetVersionId == dataSetVersion.Id)
                .OrderBy(im => im.Label)
                .ToListAsync();

            Assert.Equal(testData.ExpectedIndicators.Count, actualIndicators.Count);
            Assert.All(testData.ExpectedIndicators,
                (expectedIndicator, index) =>
                {
                    var actualIndicator = actualIndicators[index];
                    actualIndicator.AssertDeepEqualTo(expectedIndicator,
                        AssertExtensions.Except<IndicatorMeta>(
                            im => im.Id,
                            im => im.DataSetVersionId,
                            im => im.Created
                        ));
                });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task DuckDbMeta_CorrectLocationOptions(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var actualLocationOptions = (await duckDbConnection.SqlBuilder(
                $"""
                 SELECT *
                 FROM {LocationOptionsTable.TableName:raw}
                 ORDER BY {LocationOptionsTable.Cols.Label:raw}
                 """
            ).QueryAsync<ParquetLocationOption>()).AsList();

            var actualOptionsByLevel = actualLocationOptions
                .GroupBy(o => o.Level)
                .ToDictionary(g => EnumUtil.GetFromEnumValue<GeographicLevel>(g.Key),
                    g => g.ToList());

            Assert.Equal(testData.ExpectedLocations.Count, actualOptionsByLevel.Count);
            Assert.All(testData.ExpectedLocations,
                expectedLocation =>
                {
                    Assert.True(
                        actualOptionsByLevel.TryGetValue(expectedLocation.Level, out var actualOptions));
                    Assert.Equal(expectedLocation.Options.Count, actualOptions.Count);
                    Assert.All(expectedLocation.Options.OrderBy(o => o.Label),
                        (expectedOption, index) => expectedOption.AssertEqual(actualOptions[index]));
                });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task DuckDbMeta_CorrectTimePeriods(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var actualTimePeriods = (await duckDbConnection.SqlBuilder(
                $"""
                 SELECT *
                 FROM {TimePeriodsTable.TableName:raw}
                 ORDER BY {TimePeriodsTable.Cols.Period:raw}
                 """
            ).QueryAsync<ParquetTimePeriod>()).AsList();

            Assert.Equal(testData.ExpectedTimePeriods.Count, actualTimePeriods.Count);
            Assert.All(testData.ExpectedTimePeriods,
                (expectedTimePeriod, index) =>
                {
                    var actualTimePeriod = actualTimePeriods[index];
                    Assert.Equal(expectedTimePeriod.Code,
                        EnumUtil.GetFromEnumLabel<TimeIdentifier>(actualTimePeriod.Identifier));
                    Assert.Equal(expectedTimePeriod.Period, TimePeriodFormatter.FormatFromCsv(actualTimePeriod.Period));
                });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task DuckDbMeta_CorrectFilterOptions(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var actualFilterOptions = (await duckDbConnection.SqlBuilder(
                $"""
                 SELECT *
                 FROM {FilterOptionsTable.TableName:raw}
                 ORDER BY {FilterOptionsTable.Cols.Label:raw}
                 """
            ).QueryAsync<ParquetFilterOption>()).AsList();

            var actualOptionsByFilterId = actualFilterOptions
                .GroupBy(o => o.FilterId)
                .ToDictionary(g => g.Key, g => g.ToList());

            Assert.Equal(testData.ExpectedFilters.Count, actualOptionsByFilterId.Count);
            Assert.All(testData.ExpectedFilters,
                expectedFilter =>
                {
                    Assert.True(
                        actualOptionsByFilterId.TryGetValue(expectedFilter.PublicId, out var actualOptions));
                    Assert.Equal(expectedFilter.Options.Count, actualOptions.Count);
                    Assert.All(expectedFilter.Options,
                        (expectedOption, index) =>
                        {
                            var actualOption = actualOptions[index];
                            Assert.Equal(expectedOption.Label, actualOption.Label);
                            Assert.Equal(expectedFilter.PublicId, actualOption.FilterId);
                        });
                });
        }

        [Theory]
        [MemberData(nameof(Data))]
        public async Task DuckDbMeta_CorrectIndicators(ProcessorTestData testData)
        {
            var (dataSetVersion, instanceId) = await CreateDataSet(Stage.PreviousStage());

            await ImportMetadata(testData, dataSetVersion, instanceId);

            await using var duckDbConnection = GetDuckDbConnection(dataSetVersion);

            var actualIndicators = (await duckDbConnection.SqlBuilder(
                $"""
                 SELECT *
                 FROM {IndicatorsTable.TableName:raw}
                 ORDER BY {IndicatorsTable.Cols.Label:raw}
                 """
            ).QueryAsync<ParquetIndicator>()).AsList();

            Assert.Equal(testData.ExpectedIndicators.Count, actualIndicators.Count);
            Assert.All(testData.ExpectedIndicators,
                (expectedIndicator, index) =>
                {
                    var actualIndicator = actualIndicators[index];
                    Assert.Equal(expectedIndicator.PublicId, actualIndicator.Id);
                    Assert.Equal(expectedIndicator.Label, actualIndicator.Label);
                    Assert.Equal(expectedIndicator.DecimalPlaces, actualIndicator.DecimalPlaces);

                    if (expectedIndicator.Unit != null)
                    {
                        Assert.Equal(expectedIndicator.Unit,
                            EnumUtil.GetFromEnumLabel<IndicatorUnit>(actualIndicator.Unit));
                    }
                    else
                    {
                        Assert.Empty(actualIndicator.Unit);
                    }
                });
        }

        private async Task ImportMetadata(
            ProcessorTestData testData,
            DataSetVersion dataSetVersion,
            Guid instanceId)
        {
            SetupCsvDataFilesForDataSetVersion(testData, dataSetVersion);

            var function = GetRequiredService<ImportMetadataFunction>();
            await function.ImportMetadata(instanceId, CancellationToken.None);
        }
    }
}
