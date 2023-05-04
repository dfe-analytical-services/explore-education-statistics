#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static System.StringComparison;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ImporterService : IImporterService
    {
        private readonly IGuidGenerator _guidGenerator;
        private readonly ImporterLocationService _importerLocationService;
        private readonly IImporterMetaService _importerMetaService;
        private readonly IDataImportService _dataImportService;
        private readonly ILogger<ImporterService> _logger;
        private readonly IDatabaseHelper _databaseHelper;
        private readonly IObservationBatchImporter _observationBatchImporter;

        private const int Stage2RowCheck = 1000;

        public ImporterService(
            IGuidGenerator guidGenerator,
            ImporterLocationService importerLocationService,
            IImporterMetaService importerMetaService,
            IDataImportService dataImportService, 
            ILogger<ImporterService> logger, 
            IDatabaseHelper databaseHelper, 
            IObservationBatchImporter? observationBatchImporter = null)
        {
            _guidGenerator = guidGenerator;
            _importerLocationService = importerLocationService;
            _importerMetaService = importerMetaService;
            _dataImportService = dataImportService;
            _logger = logger;
            _databaseHelper = databaseHelper;
            _observationBatchImporter = observationBatchImporter ?? new StoredProcedureObservationBatchImporter();
        }

        public Task<SubjectMeta> ImportMeta(
            List<string> metaFileCsvHeaders,
            List<List<string>> metaFileRows, 
            Subject subject,
            StatisticsDbContext context)
        {
            return _importerMetaService.Import(metaFileCsvHeaders, metaFileRows, subject, context);
        }

        private record FilterItemMeta(Guid FilterId, string FilterGroupLabel, string FilterItemLabel)
        {
            public virtual bool Equals(FilterItemMeta? other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return FilterId.Equals(other.FilterId) 
                       && string.Equals(FilterGroupLabel, other.FilterGroupLabel, CurrentCultureIgnoreCase) 
                       && string.Equals(FilterItemLabel, other.FilterItemLabel, CurrentCultureIgnoreCase);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(FilterId, FilterGroupLabel.ToLower(), FilterItemLabel.ToLower());
            }
        }

        private record FilterGroupMeta(Guid FilterId, string FilterGroupLabel)
        {
            public virtual bool Equals(FilterGroupMeta? other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return FilterId.Equals(other.FilterId) 
                       && string.Equals(FilterGroupLabel, other.FilterGroupLabel, CurrentCultureIgnoreCase);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(FilterId, FilterGroupLabel.ToLower());
            }
        }

        public async Task ImportFiltersAndLocations(
            DataImport dataImport,
            Func<Task<Stream>> dataFileStreamProvider,
            SubjectMeta subjectMeta,
            StatisticsDbContext context)
        {
            var csvHeaders = await CsvUtils.GetCsvHeaders(dataFileStreamProvider);
            var soleGeographicLevel = dataImport.HasSoleGeographicLevel();

            var filterItemsFromCsv = new HashSet<FilterItemMeta>();
            var filterGroupsFromCsv = new HashSet<FilterGroupMeta>();
            var locations = new HashSet<Location>();

            var filterAndIndicatorReader = new FilterAndIndicatorValuesReader(csvHeaders, subjectMeta);
            var fixedInformationReader = new FixedInformationDataFileReader(csvHeaders);
            
            await CsvUtils.ForEachRow(dataFileStreamProvider, async (rowValues, index, _) =>
            {
                if (index % Stage2RowCheck == 0)
                {
                    var currentStatus = await _dataImportService.GetImportStatus(dataImport.Id);

                    if (currentStatus.IsFinishedOrAborting())
                    {
                        _logger.LogInformation(
                            "Import for {FileName} has finished or is being aborted, " +
                            "so finishing importing Filters and Locations early", dataImport.File.Filename);
                        return false;
                    }

                    await _dataImportService.UpdateStatus(dataImport.Id,
                        DataImportStatus.STAGE_2,
                        (double) (index + 1) / dataImport.TotalRows!.Value * 100);
                }

                if (IsRowAllowed(soleGeographicLevel, rowValues, fixedInformationReader))
                {
                    foreach (var filterMeta in subjectMeta.Filters)
                    {
                        var filterItemLabel = filterAndIndicatorReader.GetFilterItemLabel(rowValues, filterMeta.Filter.Id);
                        var filterGroupLabel = filterAndIndicatorReader.GetFilterGroupLabel(rowValues, filterMeta.Filter.Id);

                        filterGroupsFromCsv.Add(new FilterGroupMeta(filterMeta.Filter.Id, filterGroupLabel));
                        filterItemsFromCsv.Add(new FilterItemMeta(filterMeta.Filter.Id, filterGroupLabel, filterItemLabel));
                    }

                    locations.Add(fixedInformationReader.GetLocation(rowValues));
                }

                return true;
            });

            var filterGroups = filterGroupsFromCsv
                .Select(filterGroupMeta => new FilterGroup(filterGroupMeta.FilterId, filterGroupMeta.FilterGroupLabel))
                .ToList();

            var filterItems = filterItemsFromCsv
                .Select(filterItemMeta =>
                {
                    var (filterId, filterGroupLabel, filterItemLabel) = filterItemMeta;
                    
                    var filterGroup = filterGroups.Single(fg =>
                        fg.FilterId.Equals(filterId)
                        && string.Equals(fg.Label, filterGroupLabel, CurrentCultureIgnoreCase));

                    return new FilterItem(filterItemLabel, filterGroup.Id);
                })
                .ToList();
                    
            await _databaseHelper.DoInTransaction(context, async ctxDelegate =>
            {
                await ctxDelegate.FilterGroup.AddRangeAsync(filterGroups);
                await ctxDelegate.FilterItem.AddRangeAsync(filterItems);
                await ctxDelegate.SaveChangesAsync();
            });

            // Add any new Locations that are being introduced by this import process exclusively.  Other concurrent 
            // import processes reaching this stage will wait before adding their own new Locations, if any.
            //
            // This ensures that we do not end up with duplicate Locations being added by separate processes, or let one 
            // process continue on to the next stage of import before all Locations that it relies on have been
            // persisted successfully by whichever import process introduced it first.
            //
            // This also lets us take advantage of the performance gains of saving new Locations in a single batch
            // rather than in separate SaveChanges() calls, which introduces quite a penalty.
            await _databaseHelper.ExecuteWithExclusiveLock(
                context, 
                "Importer_AddNewLocations", 
                ctxDelegate => _importerLocationService.CreateIfNotExistsAndCache(ctxDelegate, locations.ToList()));
        }

        public async Task ImportObservations(DataImport import,
            Func<Task<Stream>> dataFileStreamProvider,
            Func<Task<Stream>> metaFileStreamProvider,
            Subject subject,
            StatisticsDbContext context)
        {
            var importObservationsBatchSize = GetRowsPerBatch();
            var soleGeographicLevel = import.HasSoleGeographicLevel();
            var csvHeaders = await CsvUtils.GetCsvHeaders(dataFileStreamProvider);
            var totalBatches = Math.Ceiling((decimal) import.TotalRows!.Value / importObservationsBatchSize);
            var importedRowsSoFar = import.ImportedRows;
            var lastProcessedRowIndex = import.LastProcessedRowIndex ?? -1;
            var startingRowIndex = lastProcessedRowIndex + 1;
            var startingBatchIndex = startingRowIndex / importObservationsBatchSize;
            
            var metaFileCsvHeaders = await CsvUtils.GetCsvHeaders(metaFileStreamProvider);
            var metaFileCsvRows = await CsvUtils.GetCsvRows(metaFileStreamProvider);

            var subjectMeta = await _importerMetaService.GetSubjectMeta(
                metaFileCsvHeaders,
                metaFileCsvRows,
                subject,
                context);

            var fixedInformationReader = new FixedInformationDataFileReader(csvHeaders);
            var filterAndIndicatorReader = new FilterAndIndicatorValuesReader(csvHeaders, subjectMeta);
            
            await CsvUtils.Batch(
                dataFileStreamProvider,
                importObservationsBatchSize,
                async (batchOfRows, batchIndex) =>
                {
                    var currentImportStatus = await _dataImportService.GetImportStatus(import.Id);

                    if (currentImportStatus.IsFinishedOrAborting())
                    {
                        _logger.LogInformation(
                            "Import for {FileName} has finished or is being aborted, " +
                            "so finishing importing Observations early", import.File.Filename);
                        return false;
                    }
                    
                    _logger.LogInformation(
                        "Importing Observation batch {BatchNumber} of {TotalBatches}", 
                        batchIndex + 1, 
                        totalBatches);

                    // Find the subset of this batch that hasn't yet been processed. We can use the
                    // lastProcessedRowIndex to work out which rows in this batch have not yet been processed.
                    // A scenario whereby a given batch of rows could already be partially imported can occur 
                    // if the import process was stopped mid-import and the "RowsPerBatch" configuration value
                    // changed, so that there is now some overlap between the rows in a new batch and the end of
                    // a previous batch using the old batch size. 
                    var startOfBatchRowIndex = batchIndex * importObservationsBatchSize;
                    var firstRowIndexOfBatchToProcess = 
                        Math.Max(startOfBatchRowIndex, lastProcessedRowIndex + 1) - startOfBatchRowIndex;
                    var unprocessedRows = batchOfRows.GetRange(
                        firstRowIndexOfBatchToProcess, batchOfRows.Count - firstRowIndexOfBatchToProcess);

                    if (unprocessedRows.Count != batchOfRows.Count)
                    {
                        _logger.LogInformation(
                            "Skipping first {SkippedRowCount} rows of batch {BatchNumber} as it is already " +
                            "partially imported", 
                            batchOfRows.Count() - unprocessedRows.Count,
                            batchIndex + 1);
                    }
                    
                    var allowedRows = unprocessedRows.Select((cells, rowIndex) =>
                    {
                        if (IsRowAllowed(soleGeographicLevel, cells, fixedInformationReader))
                        {
                            var csvRow = startOfBatchRowIndex + firstRowIndexOfBatchToProcess + rowIndex + 2;
                            
                            return ObservationFromCsv(
                                cells,
                                subject,
                                subjectMeta
                                    .Filters
                                    .Select(meta => meta.Filter)
                                    .ToList(),
                                fixedInformationReader,
                                filterAndIndicatorReader,
                                csvRow);
                        }

                        return null;
                    }).WhereNotNull();

                    await _databaseHelper.DoInTransaction(context, async contextDelegate => 
                        await _observationBatchImporter.ImportObservationBatch(contextDelegate, allowedRows)
                    );

                    importedRowsSoFar += allowedRows.Count();
                    lastProcessedRowIndex += unprocessedRows.Count;

                    await _dataImportService.Update(
                        import.Id, 
                        importedRows: importedRowsSoFar,
                        lastProcessedRowIndex: lastProcessedRowIndex);
                    
                    var percentageComplete = (double) ((batchIndex + 1) / totalBatches) * 100;

                    await _dataImportService.UpdateStatus(import.Id, DataImportStatus.STAGE_3, percentageComplete);

                    return true;
                },
                startingBatchIndex);
        }

        private static int GetRowsPerBatch()
        {
            return Int32.Parse(Environment.GetEnvironmentVariable("RowsPerBatch") 
                               ?? throw new InvalidOperationException("RowsPerBatch variable must be specified"));
        }

        /// <summary>
        /// Determines if a row should be imported based on geographic level.
        /// If a file contains a sole level then any row is allowed, otherwise rows for 'solo' importable levels are ignored.
        /// </summary>
        private static bool IsRowAllowed(bool soleGeographicLevel,
            IReadOnlyList<string> rowValues,
            FixedInformationDataFileReader fixedInformationReader)
        {
            return soleGeographicLevel || !fixedInformationReader.GetGeographicLevel(rowValues).IsSoloImportableLevel();
        }

        private Observation ObservationFromCsv(
            List<string> rowValues,
            Subject subject,
            List<Filter> filters,
            FixedInformationDataFileReader fixedInformationReader,
            FilterAndIndicatorValuesReader filterAndIndicatorReader,
            int csvRowNum)
        {
            var observationId = _guidGenerator.NewGuid();

            return new Observation
            {
                Id = observationId,
                FilterItems = GetFilterItems(rowValues, filters, filterAndIndicatorReader, observationId),
                LocationId = GetLocationId(rowValues, fixedInformationReader),
                Measures = filterAndIndicatorReader.GetMeasures(rowValues),
                SubjectId = subject.Id,
                TimeIdentifier = fixedInformationReader.GetTimeIdentifier(rowValues),
                Year = fixedInformationReader.GetYear(rowValues),
                CsvRow = csvRowNum
            };
        }

        private List<ObservationFilterItem> GetFilterItems(
            IReadOnlyList<string> rowValues,
            List<Filter> filters,
            FilterAndIndicatorValuesReader filterAndIndicatorReader,
            Guid observationId)
        {
            return filters.Select(filter =>
            {
                var filterItem = filterAndIndicatorReader.GetFilterItem(rowValues, filter);

                return new ObservationFilterItem
                {
                    ObservationId = observationId,
                    FilterItemId = filterItem.Id,
                    FilterId = filter.Id
                };
            }).ToList();
        }

        private Guid GetLocationId(IReadOnlyList<string> rowValues, FixedInformationDataFileReader fixedInformationReader)
        {
            var location = fixedInformationReader.GetLocation(rowValues);
            return _importerLocationService.Get(location).Id;
        }
    }

    public class StoredProcedureObservationBatchImporter : IObservationBatchImporter
    {
        public async Task ImportObservationBatch(StatisticsDbContext context, IEnumerable<Observation> observations)
        {
            var observationsTable = new DataTable();
            observationsTable.Columns.Add("Id", typeof(Guid));
            observationsTable.Columns.Add("SubjectId", typeof(Guid));
            observationsTable.Columns.Add("LocationId", typeof(Guid));
            observationsTable.Columns.Add("Year", typeof(int));
            observationsTable.Columns.Add("TimeIdentifier", typeof(string));
            observationsTable.Columns.Add("Measures", typeof(string));
            observationsTable.Columns.Add("CsvRow", typeof(long));

            var observationsFilterItemsTable = new DataTable();
            observationsFilterItemsTable.Columns.Add("ObservationId", typeof(Guid));
            observationsFilterItemsTable.Columns.Add("FilterItemId", typeof(Guid));
            observationsFilterItemsTable.Columns.Add("FilterId", typeof(Guid));

            foreach (var o in observations)
            {
                observationsTable.Rows.Add(
                    o.Id,
                    o.SubjectId,
                    o.LocationId,
                    o.Year,
                    o.TimeIdentifier.GetEnumValue(),
                    "{" + string.Join(",", o.Measures.Select(x => $"\"{x.Key}\":\"{x.Value}\"")) + "}",
                    o.CsvRow
                );

                foreach (var item in o.FilterItems)
                {
                    observationsFilterItemsTable.Rows.Add(
                        item.ObservationId,
                        item.FilterItemId,
                        item.FilterId
                    );
                }
            }

            var parameter = new SqlParameter("@Observations", SqlDbType.Structured)
            {
                Value = observationsTable, TypeName = "[dbo].[ObservationType]"
            };

            await context.Database.ExecuteSqlRawAsync("EXEC [dbo].[InsertObservations] @Observations", parameter);

            parameter = new SqlParameter("@ObservationFilterItems", SqlDbType.Structured)
            {
                Value = observationsFilterItemsTable, TypeName = "[dbo].[ObservationFilterItemType]"
            };

            await context.Database.ExecuteSqlRawAsync(
                "EXEC [dbo].[InsertObservationFilterItems] @ObservationFilterItems", parameter);
        }
    }

    public interface IObservationBatchImporter
    {
        Task ImportObservationBatch(StatisticsDbContext context, IEnumerable<Observation> observations);
    }
}
