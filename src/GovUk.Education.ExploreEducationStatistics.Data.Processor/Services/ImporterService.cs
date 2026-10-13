#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Options;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static System.StringComparison;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class ImporterService : IImporterService
{
    private readonly AppOptions _appOptions;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IImporterMetaService _importerMetaService;
    private readonly IDataImportService _dataImportService;
    private readonly ILogger<ImporterService> _logger;

    private const int Stage2RowCheck = 1000;

    public ImporterService(
        IOptions<AppOptions> appOptions,
        IGuidGenerator guidGenerator,
        IImporterMetaService importerMetaService,
        IDataImportService dataImportService,
        ILogger<ImporterService> logger
    )
    {
        _appOptions = appOptions.Value;
        _guidGenerator = guidGenerator;
        _importerMetaService = importerMetaService;
        _dataImportService = dataImportService;
        _logger = logger;
    }

    public Task<SubjectMeta> ImportMeta(
        List<string> metaFileCsvHeaders,
        List<List<string>> metaFileRows,
        IStorageDataSet dataSet,
        IStorageDataSetWriter dataSetWriter
    )
    {
        return _importerMetaService.Import(metaFileCsvHeaders, metaFileRows, dataSet, dataSetWriter);
    }

    private record FilterItemMeta(Guid FilterId, string FilterGroupLabel, string FilterItemLabel)
    {
        public virtual bool Equals(FilterItemMeta? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
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
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
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
        IStorageDataSetWriter dataSetWriter
    )
    {
        var csvHeaders = await CsvUtils.GetCsvHeaders(dataFileStreamProvider);
        var soleGeographicLevel = dataImport.HasSoleGeographicLevel();

        var filterItemsFromCsv = new HashSet<FilterItemMeta>();
        var filterGroupsFromCsv = new HashSet<FilterGroupMeta>();
        var locations = new HashSet<Location>();

        var filterAndIndicatorReader = new FilterAndIndicatorValuesReader(csvHeaders, subjectMeta);
        var fixedInformationReader = new FixedInformationDataFileReader(csvHeaders);

        await CsvUtils.ForEachRow(
            dataFileStreamProvider,
            async (rowValues, index, _) =>
            {
                if (index % Stage2RowCheck == 0)
                {
                    var currentStatus = await _dataImportService.GetImportStatus(dataImport.Id);

                    if (currentStatus.IsFinishedOrAborting())
                    {
                        _logger.LogInformation(
                            "Import for {FileName} has finished or is being aborted, "
                                + "so finishing importing Filters and Locations early",
                            dataImport.File.Filename
                        );
                        return false;
                    }

                    await _dataImportService.UpdateStatus(
                        dataImport.Id,
                        DataImportStatus.STAGE_2,
                        (double)(index + 1) / dataImport.TotalRows!.Value * 100
                    );
                }

                if (IsRowAllowed(soleGeographicLevel, rowValues, fixedInformationReader))
                {
                    foreach (var filterMeta in subjectMeta.Filters)
                    {
                        var filterItemLabel = filterAndIndicatorReader.GetFilterItemLabel(
                            rowValues,
                            filterMeta.Filter.Id
                        );
                        var filterGroupLabel = filterAndIndicatorReader.GetFilterGroupLabel(
                            rowValues,
                            filterMeta.Filter.Id
                        );

                        filterGroupsFromCsv.Add(new FilterGroupMeta(filterMeta.Filter.Id, filterGroupLabel));
                        filterItemsFromCsv.Add(
                            new FilterItemMeta(filterMeta.Filter.Id, filterGroupLabel, filterItemLabel)
                        );
                    }

                    locations.Add(fixedInformationReader.GetLocation(rowValues));
                }

                return true;
            }
        );

        var filterGroups = filterGroupsFromCsv
            .Select(filterGroupMeta => new FilterGroup(filterGroupMeta.FilterId, filterGroupMeta.FilterGroupLabel))
            .ToList();

        var filterItems = filterItemsFromCsv
            .Select(filterItemMeta =>
            {
                var (filterId, filterGroupLabel, filterItemLabel) = filterItemMeta;

                var filterGroup = filterGroups.Single(fg =>
                    fg.FilterId.Equals(filterId) && string.Equals(fg.Label, filterGroupLabel, CurrentCultureIgnoreCase)
                );

                return new FilterItem(filterItemLabel, filterGroup); // includes filterGroups objects in returned filterItems
            })
            .ToList();

        var filterIds = filterGroups.Select(group => group.FilterId).Distinct().ToList();

        var autoSelectFilterItemLabels = subjectMeta.Filters.ToDictionary(
            filterMeta => filterMeta.Filter.Id,
            filterMeta => filterMeta.Filter.AutoSelectFilterItemLabel
        );

        var filterIdToAutoSelectFilterItem = filterIds.ToDictionary(
            filterId => filterId,
            filterId =>
            {
                var autoSelectFilterItemLabel = autoSelectFilterItemLabels.GetValueOrDefault(filterId) ?? "Total"; // If meta file didn't specify a default, look for "Total"

                var filterItemsWithAutoSelectLabel = filterItems
                    .Where(item =>
                        item.FilterGroup.FilterId == filterId
                        && item.Label.Equals(autoSelectFilterItemLabel, OrdinalIgnoreCase)
                    )
                    .ToList();

                // There might be two or more filter items with the same label under different groups
                if (filterItemsWithAutoSelectLabel.Count > 1)
                {
                    // If so, does one belong to a filter group with the same label?
                    // If it does, this is the auto select filter item
                    return filterItemsWithAutoSelectLabel
                        .Where(fi => fi.FilterGroup.Label == "Total")
                        .SingleOrDefault(); // is the default "null", or does it break?

                    // NB. This implementation does not currently support filter hierarchies.
                    // TODO (EES-5884): Fix auto-select filter items for filter hierarchies
                }

                return filterItemsWithAutoSelectLabel.FirstOrDefault();
            }
        );

        await dataSetWriter.WriteFilterItems(filterItems, filterIdToAutoSelectFilterItem);

        await dataSetWriter.WriteLocations(locations);
    }

    public async Task ImportObservations(
        DataImport import,
        Func<Task<Stream>> dataFileStreamProvider,
        Func<Task<Stream>> metaFileStreamProvider,
        IStorageDataSet dataSet,
        IStorageDataSetWriter dataSetWriter
    )
    {
        var importObservationsBatchSize = _appOptions.RowsPerBatch;
        var soleGeographicLevel = import.HasSoleGeographicLevel();
        var csvHeaders = await CsvUtils.GetCsvHeaders(dataFileStreamProvider);
        var totalBatches = Math.Ceiling((decimal)import.TotalRows!.Value / importObservationsBatchSize);
        var importedRowsSoFar = import.ImportedRows;
        var lastProcessedRowIndex = import.LastProcessedRowIndex ?? -1;
        var startingRowIndex = lastProcessedRowIndex + 1;
        var startingBatchIndex = startingRowIndex / importObservationsBatchSize;

        var metaFileCsvHeaders = await CsvUtils.GetCsvHeaders(metaFileStreamProvider);
        var metaFileCsvRows = await CsvUtils.GetCsvRows(metaFileStreamProvider);

        var subjectMeta = await _importerMetaService.GetSubjectMeta(metaFileCsvHeaders, metaFileCsvRows, dataSet);

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
                        "Import for {FileName} has finished or is being aborted, "
                            + "so finishing importing Observations early",
                        import.File.Filename
                    );
                    return false;
                }

                _logger.LogInformation(
                    "Importing Observation batch {BatchNumber} of {TotalBatches}",
                    batchIndex + 1,
                    totalBatches
                );

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
                    firstRowIndexOfBatchToProcess,
                    batchOfRows.Count - firstRowIndexOfBatchToProcess
                );

                if (unprocessedRows.Count != batchOfRows.Count)
                {
                    _logger.LogInformation(
                        "Skipping first {SkippedRowCount} rows of batch {BatchNumber} as it is already "
                            + "partially imported",
                        batchOfRows.Count() - unprocessedRows.Count,
                        batchIndex + 1
                    );
                }

                var allowedRows = unprocessedRows
                    .Select(
                        (cells, rowIndex) =>
                        {
                            if (IsRowAllowed(soleGeographicLevel, cells, fixedInformationReader))
                            {
                                var csvRow = startOfBatchRowIndex + firstRowIndexOfBatchToProcess + rowIndex + 2;

                                return ObservationFromCsv(
                                    cells,
                                    dataSet.SubjectId,
                                    subjectMeta.Filters.Select(meta => meta.Filter).ToList(),
                                    fixedInformationReader,
                                    filterAndIndicatorReader,
                                    dataSetWriter,
                                    csvRow
                                );
                            }

                            return null;
                        }
                    )
                    .WhereNotNull()
                    .ToList();

                await dataSetWriter.WriteObservations(allowedRows);

                importedRowsSoFar += allowedRows.Count;
                lastProcessedRowIndex += unprocessedRows.Count;

                await _dataImportService.Update(
                    import.Id,
                    importedRows: importedRowsSoFar,
                    lastProcessedRowIndex: lastProcessedRowIndex
                );

                var percentageComplete = (double)((batchIndex + 1) / totalBatches) * 100;

                await _dataImportService.UpdateStatus(import.Id, DataImportStatus.STAGE_3, percentageComplete);

                return true;
            },
            startingBatchIndex
        );
    }

    /// <summary>
    /// Determines if a row should be imported based on geographic level.
    /// If a file contains a sole level then any row is allowed, otherwise rows for 'solo' importable levels are ignored.
    /// </summary>
    private static bool IsRowAllowed(
        bool soleGeographicLevel,
        IReadOnlyList<string> rowValues,
        FixedInformationDataFileReader fixedInformationReader
    )
    {
        return soleGeographicLevel || !fixedInformationReader.GetGeographicLevel(rowValues).IsSoloImportableLevel();
    }

    private Observation ObservationFromCsv(
        List<string> rowValues,
        Guid subjectId,
        List<Filter> filters,
        FixedInformationDataFileReader fixedInformationReader,
        FilterAndIndicatorValuesReader filterAndIndicatorReader,
        IStorageDataSetWriter dataSetWriter,
        int csvRowNum
    )
    {
        var observationId = _guidGenerator.NewGuid();

        return new Observation
        {
            Id = observationId,
            FilterItems = GetFilterItems(rowValues, filters, filterAndIndicatorReader, observationId),
            LocationId = dataSetWriter.GetLocationId(fixedInformationReader.GetLocation(rowValues)),
            Measures = filterAndIndicatorReader.GetMeasures(rowValues),
            SubjectId = subjectId,
            TimeIdentifier = fixedInformationReader.GetTimeIdentifier(rowValues),
            Year = fixedInformationReader.GetYear(rowValues),
            CsvRow = csvRowNum,
        };
    }

    private List<ObservationFilterItem> GetFilterItems(
        IReadOnlyList<string> rowValues,
        List<Filter> filters,
        FilterAndIndicatorValuesReader filterAndIndicatorReader,
        Guid observationId
    )
    {
        return filters
            .Select(filter =>
            {
                var filterItem = filterAndIndicatorReader.GetFilterItem(rowValues, filter);

                return new ObservationFilterItem
                {
                    ObservationId = observationId,
                    FilterItemId = filterItem.Id,
                    FilterId = filter.Id,
                };
            })
            .ToList();
    }
}
