#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.ValidationErrorMessages;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public enum ValidationErrorMessages
    {
        [EnumLabelValue("Metafile is missing expected column")]
        MetaFileMissingExpectedColumn,

        [EnumLabelValue("Metafile has invalid values")]
        MetaFileHasInvalidValues,

        [EnumLabelValue("Metafile has invalid number of columns")]
        MetaFileHasInvalidNumberOfColumns,

        [EnumLabelValue("Metafile must be a csv file")]
        MetaFileMustBeCsvFile,

        [EnumLabelValue("Datafile is missing expected column")]
        DataFileMissingExpectedColumn,

        [EnumLabelValue("Datafile has invalid number of columns")]
        DataFileHasInvalidNumberOfColumns,

        [EnumLabelValue("Datafile must be a csv file")]
        DataFileMustBeCsvFile,

        [EnumLabelValue("Only first 100 errors are shown")]
        FirstOneHundredErrors
    }

    public class ValidatorService : IValidatorService
    {
        private readonly ILogger<IValidatorService> _logger;
        private readonly IPrivateBlobStorageService _privateBlobStorageService;
        private readonly IFileTypeService _fileTypeService;
        private readonly IDataImportService _dataImportService;
        
        public ValidatorService(
            ILogger<ValidatorService> logger,
            IPrivateBlobStorageService privateBlobStorageService,
            IFileTypeService fileTypeService,
            IDataImportService dataImportService)
        {
            _logger = logger;
            _privateBlobStorageService = privateBlobStorageService;
            _fileTypeService = fileTypeService;
            _dataImportService = dataImportService;
        }

        /// <summary>
        /// Intervals at which to perform import status checks and updates.
        /// </summary>
        private const int Stage1RowCheck = 1000;
        
        private static readonly List<string> MandatoryObservationColumns = new()
        {
            "time_identifier",
            "time_period",
            "geographic_level"
        };
        
        public async Task<Either<List<DataImportError>, ProcessorStatistics>> Validate(Guid importId)
        {
            var import = await _dataImportService.GetImport(importId);

            _logger.LogInformation("Validating: {FileName}", import.File.Filename);

            await _dataImportService.UpdateStatus(import.Id, DataImportStatus.STAGE_1, 0);

            var dataFileStreamProvider = () => _privateBlobStorageService.StreamBlob(PrivateReleaseFiles, 
                import.File.Path());
                    
            var metaFileStreamProvider = () => _privateBlobStorageService.StreamBlob(PrivateReleaseFiles,
                import.MetaFile.Path());

            return await
                ValidateCsvFileType(import.MetaFile, metaFileStreamProvider, true)
                    .OnSuccess(() => ValidateCsvFileType(import.File, dataFileStreamProvider, false))
                    .OnSuccess(() => ValidateMetadataFile(import.MetaFile, metaFileStreamProvider, true))
                    .OnSuccess(async _ =>
                    {
                        var dataFileColumnHeaders = await CsvUtils.GetCsvHeaders(dataFileStreamProvider);
                        var dataFileTotalRows = await CsvUtils.GetTotalRows(dataFileStreamProvider);

                        return await 
                            ValidateObservationHeaders(dataFileColumnHeaders)
                            .OnSuccess(() => ValidateAndCountObservations(
                                import,
                                dataFileColumnHeaders,
                                dataFileTotalRows,
                                dataFileStreamProvider
                            )
                            .OnSuccessDo(async () =>
                                _logger.LogInformation("Validating: {FileName} complete", import.File.Filename)));
                    });
        }

        private async Task<Either<List<DataImportError>, Unit>> ValidateCsvFileType(
            File file,
            Func<Task<Stream>> fileStreamProvider,
            bool isMetaFile)
        {
            if (!await _fileTypeService.IsValidCsvFile(fileStreamProvider, file.Filename))
            {
                return ListOf(isMetaFile
                    ? new DataImportError(MetaFileMustBeCsvFile.GetEnumLabel())
                    : new DataImportError(DataFileMustBeCsvFile.GetEnumLabel()));
            }

            return Unit.Instance;
        }
        
        private async Task<Either<List<DataImportError>, (List<string> columnHeaders, int totalRows)>>
            ValidateMetadataFile(
                File file,
                Func<Task<Stream>> fileStreamProvider,
                bool isMetaFile)
        {
            _logger.LogDebug("Determining if CSV file {FileName} is correct shape", file.Filename);

            var csvHeaders = await CsvUtils.GetCsvHeaders(fileStreamProvider);

            var totalRows = 0;
            var errors = new List<DataImportError>();
            
            // Check for unexpected column names
            Array.ForEach(Enum.GetNames(typeof(MetaColumns)), col =>
            {
                if (!csvHeaders.Contains(col))
                {
                    errors.Add(new DataImportError($"{MetaFileMissingExpectedColumn.GetEnumLabel()} : {col}"));
                }
            });

            if (errors.Count > 0)
            {
                return errors;
            }

            var reader = new MetaDataFileReader(csvHeaders);

            await CsvUtils.ForEachRow(fileStreamProvider, (cells, index, _) =>
            {
                totalRows++;
                
                if (cells.Count != csvHeaders.Count)
                {
                    var errorCode = isMetaFile ? MetaFileHasInvalidNumberOfColumns : DataFileHasInvalidNumberOfColumns;
                    errors.Add(new DataImportError($"Error at data row {index + 1}: {errorCode.GetEnumLabel()}"));
                }
                
                try
                {
                    reader.GetMetaRow(cells);
                }
                catch (Exception e)
                {
                    errors.Add(new DataImportError($"error at data row {index + 1}: {MetaFileHasInvalidValues.GetEnumLabel()} : {e.Message}"));
                }
            });

            if (errors.Count > 0)
            {
                _logger.LogDebug("CSV metadata file {FileName} is invalid - {Errors}", file.Filename, errors.JoinToString());
                return errors;
            }
            
            _logger.LogDebug("CSV metadata file {FileName} is valid", file.Filename);
            
            return (csvHeaders, totalRows);
        }

        private static async Task<Either<List<DataImportError>, Unit>> ValidateObservationHeaders(List<string> cols)
        {
            var errors = new List<DataImportError>();   

            foreach (var mandatoryCol in MandatoryObservationColumns)
            {
                if (!cols.Contains(mandatoryCol))
                {
                    errors.Add(new DataImportError($"{DataFileMissingExpectedColumn.GetEnumLabel()} : {mandatoryCol}"));
                }

                if (errors.Count == 100)
                {
                    errors.Add(new DataImportError(FirstOneHundredErrors.GetEnumLabel()));
                    break;
                }
            }

            if (errors.Count > 0)
            {
                return errors;
            }

            return Unit.Instance;
        }

        private async Task<Either<List<DataImportError>, ProcessorStatistics>>
            ValidateAndCountObservations(
                DataImport import,
                List<string> csvHeaders,
                int totalRows,
                Func<Task<Stream>> dataFileStreamProvider)
        {
            var rowCountByGeographicLevel = new Dictionary<GeographicLevel, int>();
            var errors = new List<DataImportError>();
            
            var fixedInformationReader = new FixedInformationDataFileReader(csvHeaders);

            await CsvUtils.ForEachRow(dataFileStreamProvider, async (rowValues, index, _) =>
            {
                if (errors.Count == 100)
                {
                    errors.Add(new DataImportError(FirstOneHundredErrors.GetEnumLabel()));
                    return false;
                }
                
                if (rowValues.Count != csvHeaders.Count)
                {
                    errors.Add(new DataImportError($"Error at row {index + 1}: cell count {rowValues.Count} " +
                                                   $"does not match column header count of {csvHeaders.Count}"));
                    return true;
                }
                
                if (index % Stage1RowCheck == 0)
                {
                    var currentStatus = await _dataImportService.GetImportStatus(import.Id);
                    
                    if (currentStatus.IsFinishedOrAborting())
                    {
                        _logger.LogInformation(
                            "Import for {FileName} has finished or is being aborted, " +
                            "so finishing importing Filters and Locations early", import.File.Filename);
                        return false;
                    }
                }

                try
                {
                    fixedInformationReader.GetTimeIdentifier(rowValues);
                    fixedInformationReader.GetYear(rowValues);

                    var level = fixedInformationReader.GetGeographicLevel(rowValues);
                    
                    if (rowCountByGeographicLevel.ContainsKey(level))
                    {
                        rowCountByGeographicLevel[level]++;
                    }
                    else
                    {
                        rowCountByGeographicLevel.Add(level, 1);
                    }
                }
                catch (Exception e)
                {
                    errors.Add(new DataImportError($"Error at row {index + 1}: {e.Message}"));
                }

                if (index % Stage1RowCheck == 0)
                {
                    await _dataImportService.UpdateStatus(import.Id,
                        DataImportStatus.STAGE_1,
                        (double) index / totalRows * 100);
                }

                return true;
            });

            if (errors.Count > 0)
            {
                _logger.LogDebug("{ErrorCount} errors fond whilst validating {FileName}", 
                    errors.Count, import.File.Filename);
                return errors;
            }

            await _dataImportService.UpdateStatus(
                import.Id,
                DataImportStatus.STAGE_1,
                100);

            return new ProcessorStatistics
            (
                TotalRowCount: totalRows,
                ImportableRowCount: GetImportableRowCount(rowCountByGeographicLevel),
                GeographicLevels: rowCountByGeographicLevel.Keys.ToHashSet()
            );
        }

        private static int GetImportableRowCount(Dictionary<GeographicLevel, int> rowCountByGeographicLevel)
        {
            var geographicLevels = rowCountByGeographicLevel.Keys.ToList();

            if (geographicLevels.Count == 1)
            {
                return rowCountByGeographicLevel[geographicLevels.First()];
            }

            // Exclude the counts of any 'solo' levels.
            // Those rows will be ignored since they are not being imported exclusively.
            return rowCountByGeographicLevel.Sum(pair => pair.Key.IsSoloImportableLevel() ? 0 : pair.Value);
        }
    }
}
