#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class DataBlockMigrationService : IDataBlockMigrationService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IPersistenceHelper<StatisticsDbContext> _statisticsPersistenceHelper;
        private readonly ILocationRepository _locationRepository;

        public DataBlockMigrationService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper,
            ILocationRepository locationRepository)
        {
            _contentDbContext = contentDbContext;
            _contentPersistenceHelper = contentPersistenceHelper;
            _statisticsPersistenceHelper = statisticsPersistenceHelper;
            _locationRepository = locationRepository;
        }

        public async Task<Either<ActionResult, Unit>> Migrate(Guid id)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<DataBlock>(id)
                .OnSuccess(dataBlock =>
                {
                    return _statisticsPersistenceHelper.CheckEntityExists<Subject>(dataBlock.Query.SubjectId)
                        .OnSuccessVoid(async subject =>
                        {
                            var locationsMap = await GetLocationsMap(subject.Id);

                            _contentDbContext.DataBlocks.Update(dataBlock);
                            dataBlock.QueryMigrated = MigrateQuery(dataBlock, locationsMap);
                            dataBlock.ChartsMigrated = MigrateCharts(dataBlock, locationsMap);
                            dataBlock.TableMigrated = MigrateTableConfig(dataBlock, locationsMap);
                            dataBlock.LocationsMigrated = true;
                            await _contentDbContext.SaveChangesAsync();
                        });
                });
        }

        /// <summary>
        /// Get a map of all the locations for the subject.
        /// Locations are mapped by geographic level, and within that mapped by location attribute code.
        /// Locations are projected to their location id.
        /// Note that certain levels (Institution, Planning Area, Provider, and School) allow duplicate codes,
        /// so each code can potentially map to multiple id's.
        /// Example:
        /// {
        ///   GeographicLevel.LocalAuthority: {
        ///     "E09000003": [
        ///       188f6ea0-f778-4244-97f4-1ff6da1a0785
        ///     ]
        ///   },
        ///   GeographicLevel.PlanningArea : {
        ///     "8410001": [
        ///       8bb57f41-1ca9-4d87-8ea3-41f5efc1b440,
        ///       29ef4130-36d4-4a9c-b0c8-74f00767af5e
        ///     ],
        ///     "841002": [
        ///       f95200e0-9db8-4611-b0c1-fbcf77577013
        ///     ]
        ///   }
        /// }
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        private async Task<Dictionary<GeographicLevel, Dictionary<string, List<Guid>>>> GetLocationsMap(Guid subjectId)
        {
            var locationsForSubject = await _locationRepository.GetDistinctForSubject(subjectId);

            // First map all the locations for the subject by level
            var locationsByGeographicLevel = locationsForSubject
                .GroupBy(location => location.GeographicLevel)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping => grouping.ToList()
                );

            // Now map the locations within each level by the location attribute code
            // and project them to location id
            return locationsByGeographicLevel.ToDictionary(
                pair => pair.Key,
                pair => pair.Value
                    .GroupBy(location => location.ToLocationAttribute().GetCodeOrFallback())
                    .ToDictionary(
                        grouping => grouping.Key,
                        grouping => grouping.Select(location => location.Id).ToList()));
        }

        private static ObservationQueryContext MigrateQuery(
            DataBlock dataBlock,
            IReadOnlyDictionary<GeographicLevel, Dictionary<string, List<Guid>>> locationsMap)
        {
            if (!dataBlock.Query.LocationIds.IsNullOrEmpty())
            {
                throw new InvalidOperationException(
                    $"Expected data block '{dataBlock.Id}' query location id's to be empty");
            }

            var locationIds = new List<Guid>();

            if (dataBlock.Query.Locations != null)
            {
                // Locations has an array of codes in a field name corresponding with each geographic level.
                // Iterate over every level and lookup any codes that exist in that field 
                EnumUtil.GetEnumValues<GeographicLevel>().ForEach(geographicLevel =>
                {
                    var codes = GetQueryLocationCodesForLevel(dataBlock.Query.Locations, geographicLevel);
                    if (codes.Any())
                    {
                        var targets = locationsMap.GetValueOrDefault(geographicLevel) ??
                                      new Dictionary<string, List<Guid>>();
                        codes.ForEach(code =>
                        {
                            // Certain levels (Institution, Planning Area, Provider, and School) allow duplicate codes
                            // which means a code can map to more than one id which is why we expect a list of id's here.
                            if (targets.TryGetValue(code, out var idList))
                            {
                                locationIds.AddRange(idList);
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    $"Expected a mapping for data block '{dataBlock.Id}' query location with level '{geographicLevel}' and code '{code}'"
                                );
                            }
                        });
                    }
                });
            }

            // Deep clone the query
            var clonedQuery = dataBlock.Query.Clone();

            clonedQuery.LocationIds = locationIds;
            clonedQuery.Locations = null;
            return clonedQuery;
        }

        private static List<IChart> MigrateCharts(
            DataBlock dataBlock,
            IReadOnlyDictionary<GeographicLevel, Dictionary<string, List<Guid>>> locationsMap)
        {
            if (dataBlock.Charts == null)
            {
                return new List<IChart>();
            }

            // Deep clone the charts
            var chartsJson = JsonConvert.SerializeObject(dataBlock.Charts);
            var clonedCharts = JsonConvert.DeserializeObject<List<IChart>>(chartsJson);

            clonedCharts.ForEach(
                chart =>
                {
                    MigrateChartMajorAxisDataSets(
                        chart,
                        dataBlock.Id,
                        locationsMap
                    );
                    MigrateChartLegendDataSets(
                        chart,
                        dataBlock.Id,
                        locationsMap
                    );
                }
            );
            return clonedCharts;
        }

        private static void MigrateChartMajorAxisDataSets(
            IChart chart,
            Guid dataBlockId,
            IReadOnlyDictionary<GeographicLevel, Dictionary<string, List<Guid>>> locationsMap)
        {
            chart.Axes?.GetValueOrDefault("major")?.DataSets.ForEach(
                dataSet =>
                {
                    if (dataSet.Location != null)
                    {
                        if (Enum.TryParse(
                                value: dataSet.Location.Level,
                                ignoreCase: true,
                                out GeographicLevel geographicLevel))
                        {
                            var targets = locationsMap.GetValueOrDefault(geographicLevel) ??
                                          new Dictionary<string, List<Guid>>();
                            if (targets.TryGetValue(dataSet.Location.Value, out var idList))
                            {
                                // Check if this was a duplicate code (allowed for Provider/School level data)
                                if (idList.Count > 1)
                                {
                                    // A check of Prod data reveals there's no charts for Provider/School level data.
                                    // Fail the migration here with an exception so we can investigate.
                                    throw new InvalidOperationException(
                                        $"Only expecting one location id for data block '{dataBlockId}' chart major axis data set location with level '{dataSet.Location.Level}' and code '{dataSet.Location.Value}'"
                                    );
                                }

                                dataSet.Location.Value = idList[0].ToString();
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    $"Expected a mapping for data block '{dataBlockId}' chart major axis data set location with level '{dataSet.Location.Level}' and code '{dataSet.Location.Value}'"
                                );
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Could not parse data block '{dataBlockId}' chart major axis data set location level '{dataSet.Location.Level}' as {nameof(GeographicLevel)}'"
                            );
                        }
                    }
                }
            );
        }

        private static void MigrateChartLegendDataSets(
            IChart chart,
            Guid dataBlockId,
            IReadOnlyDictionary<GeographicLevel, Dictionary<string, List<Guid>>> locationsMap)
        {
            chart.Legend?.Items.ForEach(
                item =>
                {
                    var dataSet = item.DataSet;

                    if (dataSet.Location != null)
                    {
                        if (Enum.TryParse(
                                value: dataSet.Location.Level,
                                ignoreCase: true,
                                out GeographicLevel geographicLevel))
                        {
                            var targets = locationsMap.GetValueOrDefault(geographicLevel) ??
                                          new Dictionary<string, List<Guid>>();
                            if (targets.TryGetValue(dataSet.Location.Value, out var idList))
                            {
                                // Check if this was a duplicate code (allowed for Provider/School level data)
                                if (idList.Count > 1)
                                {
                                    // A check of Prod data reveals there's no charts for Provider/School level data.
                                    // Fail the migration here with an exception so we can investigate.
                                    throw new InvalidOperationException(
                                        $"Only expecting one location id for data block '{dataBlockId}' chart legend data set location with level '{dataSet.Location.Level}' and code '{dataSet.Location.Value}'"
                                    );
                                }

                                dataSet.Location.Value = idList[0].ToString();
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    $"Expected a mapping for data block '{dataBlockId}' chart legend data set location with level '{dataSet.Location.Level}' and code '{dataSet.Location.Value}'"
                                );
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Could not parse data block '{dataBlockId}' chart legend data set location level '{dataSet.Location.Level}' as {nameof(GeographicLevel)}'"
                            );
                        }
                    }
                }
            );
        }

        private static TableBuilderConfiguration? MigrateTableConfig(
            DataBlock dataBlock,
            IReadOnlyDictionary<GeographicLevel, Dictionary<string, List<Guid>>> locationsMap)
        {
            if (dataBlock.Table == null)
            {
                return null;
            }

            // Deep clone the table
            var clonedTable = dataBlock.Table.Clone();
            var tableHeaders = clonedTable.TableHeaders;

            // Column Groups
            tableHeaders.ColumnGroups = tableHeaders.ColumnGroups.Select(group =>
                MigrateTableHeaders(
                    tableHeaderField: TableHeaderField.ColumnGroups,
                    tableHeaders: group,
                    dataBlockId: dataBlock.Id,
                    locationsMap: locationsMap)
            ).ToList();

            // Columns
            tableHeaders.Columns = MigrateTableHeaders(
                tableHeaderField: TableHeaderField.Columns,
                tableHeaders: tableHeaders.Columns,
                dataBlockId: dataBlock.Id,
                locationsMap: locationsMap);

            // Row Groups
            tableHeaders.RowGroups = tableHeaders.RowGroups.Select(group =>
                MigrateTableHeaders(
                    tableHeaderField: TableHeaderField.RowGroups,
                    tableHeaders: group,
                    dataBlockId: dataBlock.Id,
                    locationsMap: locationsMap)
            ).ToList();

            // Rows
            tableHeaders.Rows = MigrateTableHeaders(
                tableHeaderField: TableHeaderField.Rows,
                tableHeaders: tableHeaders.Rows,
                dataBlockId: dataBlock.Id,
                locationsMap: locationsMap);

            return clonedTable;
        }

        private static List<TableHeader> MigrateTableHeaders(
            List<TableHeader> tableHeaders,
            TableHeaderField tableHeaderField,
            Guid dataBlockId,
            IReadOnlyDictionary<GeographicLevel, Dictionary<string, List<Guid>>> locationsMap)
        {
            // Use SelectMany as we allow a single header to be replaced with multiple
            var migrated = tableHeaders.SelectMany(tableHeader =>
            {
                if (tableHeader.Type != TableHeaderType.Location)
                {
                    // Don't touch any table headers that aren't locations
                    return ListOf(tableHeader);
                }

                if (tableHeader.Value.IsNullOrEmpty())
                {
                    throw new InvalidOperationException(
                        $"Expected a value for data block {dataBlockId} table header but found null or empty");
                }

                if (Enum.TryParse(
                        value: tableHeader.Level,
                        ignoreCase: true,
                        out GeographicLevel geographicLevel))
                {
                    var targets = locationsMap.GetValueOrDefault(geographicLevel) ??
                                  new Dictionary<string, List<Guid>>();

                    if (targets.TryGetValue(tableHeader.Value, out var idList))
                    {
                        switch (idList.Count)
                        {
                            // Should always have at least one id here since the map was built by grouping them
                            case 0:
                                throw new InvalidOperationException(
                                    $"Location map has empty id list for data block '{dataBlockId}' {tableHeaderField} table header with level '{geographicLevel}' and code '{tableHeader.Value}'"
                                );
                            // Happy path: In most cases we expect a location code to be replaced with a single id
                            case 1:
                                return ListOf(TableHeader.NewLocationHeader(geographicLevel, idList[0].ToString()));
                            // Unhappy path: Check if this was a duplicate code (allowed for Provider/School level data)
                            case > 1 when geographicLevel is GeographicLevel.Provider or GeographicLevel.School:
                                switch (tableHeaderField)
                                {
                                    case TableHeaderField.Rows:
                                    case TableHeaderField.RowGroups:
                                        // Transform the single header into multiple adjacent headers.
                                        // *** This is not completely safe so will need some manual testing after the migration ***
                                        // It's not safe to assume that where there's currently one merged row of locations with identical codes 
                                        // that afterwards the rendered table will have as many rows as there are distinct locations
                                        // (which is what this code will create).
                                        // At this point we only know that for this code there are multiple locations in the data file.
                                        // The combination of time period and filters which restrict the table result may mean that not all locations are displayed.
                                        // For example, given the following duplicate Provider data split between two years:
                                        // | 2015 | Provider A (1000000) |
                                        // | 2016 | Provider B (1000000) |
                                        // If the query is for 2015 to 2016 the rendered table currently has one merged row Provider A (1000000) / Provider B (1000000).
                                        // After the migration to id's it will render with two rows Provider A (1000000) and Provider B (1000000),
                                        // so one table row header needs to become two table row headers.
                                        // BUT
                                        // If the query is for 2015 OR 2016 (not both), the table is currently rendering one row,
                                        // either Provider A (1000000) for 2015 or Provider B (1000000) for 2016.
                                        // After the migration to id's it will continue to render one row and inserting new table headers would corrupt the table.

                                        // A check of Prod data reveals that where there's Provider level data with duplicates, that they are being used as table highlights
                                        // rather than embedded tables in content, and they are making all time periods and filters available.
                                        // Therefore proceed by inserting additional headers here but recommend these tables are
                                        // visually tested before and after we switch to using location id's.
                                        return idList.Select(id =>
                                                TableHeader.NewLocationHeader(geographicLevel, id.ToString()))
                                            .ToList();
                                    case TableHeaderField.Columns:
                                    case TableHeaderField.ColumnGroups:
                                        // A check of Prod data reveals there's no locations in Column/ColumnGroup headers for Provider/School level data.
                                        // Since what we do above for Rows/RowGroups is not completely safe let's limit creating extra headers for ColumnGroups / Columns.
                                        // Fail the migration here with an exception so we can investigate.
                                        throw new InvalidOperationException(
                                            $"Only expecting one location id for data block '{dataBlockId}' {tableHeaderField} table header with level '{geographicLevel}' and code '{tableHeader.Value}'"
                                        );
                                    default:
                                        throw new ArgumentOutOfRangeException(nameof(tableHeaderField),
                                            tableHeaderField, "Unexpected type of table header field");
                                }
                            case > 1:
                                // Only expecting multiple location id's for Provider/School level data
                                // Fail the migration here with an exception so we can investigate.
                                throw new InvalidOperationException(
                                    $"Only expecting one location id for data block '{dataBlockId}' {tableHeaderField} table header with level '{geographicLevel}' and code '{tableHeader.Value}'"
                                );
                        }
                    }

                    throw new InvalidOperationException(
                        $"Expected a mapping for data block '{dataBlockId}' {tableHeaderField} table header with level '{geographicLevel}' and code '{tableHeader.Value}'"
                    );
                }

                throw new InvalidOperationException(
                    $"Could not parse data block '{dataBlockId}' {tableHeaderField} table header level '{tableHeader.Level}' as {nameof(GeographicLevel)}'"
                );
            }).ToList();

            return migrated;
        }

        private static List<string> GetQueryLocationCodesForLevel(
            LocationQuery locations,
            GeographicLevel geographicLevel)
        {
            var queryProperty = typeof(LocationQuery).GetProperty(geographicLevel.ToString());
            if (queryProperty == null || queryProperty.GetMethod == null)
            {
                throw new ArgumentException(
                    $"{nameof(LocationQuery)} does not have a property {geographicLevel.ToString()} with get method");
            }

            var codes = (
                queryProperty.GetMethod.Invoke(locations, new object[] { }) as IEnumerable<string> ??
                new List<string>()
            ).ToList();

            return codes;
        }

        private enum TableHeaderField
        {
            ColumnGroups,
            Columns,
            RowGroups,
            Rows
        }
    }
}
