using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.FileTypeValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Models.SoloImportableLevels;
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
        private readonly IBlobStorageService _blobStorageService;
        private readonly IFileTypeService _fileTypeService;
        private readonly IDataImportService _dataImportService;
        private readonly IImporterService _importerService;
        
        public ValidatorService(
            ILogger<IValidatorService> logger,
            IBlobStorageService blobStorageService,
            IFileTypeService fileTypeService,
            IDataImportService dataImportService, 
            IImporterService importerService)
        {
            _logger = logger;
            _blobStorageService = blobStorageService;
            _fileTypeService = fileTypeService;
            _dataImportService = dataImportService;
            _importerService = importerService;
        }

        private const int Stage1RowCheck = 1000;

        private static readonly List<string> MandatoryObservationColumns = new()
        {
            "time_identifier",
            "time_period",
            "geographic_level"
        };
        
        public async Task<Either<List<DataImportError>, ProcessorStatistics>> Validate(
            Guid importId,
            ExecutionContext executionContext)
        {
            var import = await _dataImportService.GetImport(importId);

            _logger.LogInformation($"Validating: {import.File.Filename}");

            await _dataImportService.UpdateStatus(import.Id, DataImportStatus.STAGE_1, 0);

            return await ValidateCsvFile(import.File, false)
                .OnSuccessDo(async () => await ValidateCsvFile(import.MetaFile, true))
                .OnSuccess(async () =>
                    {
                        var dataFileStream = await _blobStorageService.StreamBlob(PrivateReleaseFiles, 
                            import.File.Path());
                        var dataFileTable = DataTableUtils.CreateFromStream(dataFileStream);

                        var metaFileStream = await _blobStorageService.StreamBlob(PrivateReleaseFiles,
                            import.MetaFile.Path());
                        var metaFileTable = DataTableUtils.CreateFromStream(metaFileStream);

                        return await ValidateMetaHeader(metaFileTable.Columns)
                            .OnSuccess(() => ValidateMetaRows(metaFileTable.Columns, metaFileTable.Rows))
                            .OnSuccess(() => ValidateObservationHeaders(dataFileTable.Columns))
                            .OnSuccess(
                                () =>
                                    ValidateAndCountObservations(dataFileTable.Columns, dataFileTable.Rows,
                                            executionContext, import.Id)
                                        .OnSuccess(
                                            result =>
                                            {
                                                _logger.LogInformation(
                                                    $"Validating: {import.File.Filename} complete");
                                                return result;
                                            }
                                        )
                            );
                });
        }

        private async Task<Either<List<DataImportError>, Unit>> ValidateCsvFile(File file, bool isMetaFile)
        {
            var errors = new List<DataImportError>();

            if (!await IsCsvFile(file))
            {
                errors.Add(isMetaFile ? new DataImportError($"{MetaFileMustBeCsvFile.GetEnumLabel()}") :
                    new DataImportError($"{DataFileMustBeCsvFile.GetEnumLabel()}"));
            }
            else
            {
                var stream = await _blobStorageService.StreamBlob(PrivateReleaseFiles, file.Path());

                using var reader = new StreamReader(stream);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                csv.Configuration.HasHeaderRecord = false;
                using var dr = new CsvDataReader(csv);
                var colCount = -1;
                var idx = 0;

                while (dr.Read())
                {
                    idx++;
                    if (colCount >= 0 && dr.FieldCount != colCount)
                    {
                        errors.Add(isMetaFile ? new DataImportError($"error at row {idx}: {MetaFileHasInvalidNumberOfColumns.GetEnumLabel()}") :
                            new DataImportError($"error at row {idx}: {DataFileHasInvalidNumberOfColumns.GetEnumLabel()}"));
                        break;
                    }
                    colCount = dr.FieldCount;
                }
            }

            if (errors.Count > 0)
            {
                return errors;
            }

            return Unit.Instance;
        }

        private static async Task<Either<List<DataImportError>, Unit>> ValidateMetaHeader(DataColumnCollection header)
        {
            var errors = new List<DataImportError>();
            // Check for unexpected column names
            Array.ForEach(Enum.GetNames(typeof(MetaColumns)), col =>
            {
                if (!header.Contains(col))
                {
                    errors.Add(new DataImportError($"{MetaFileMissingExpectedColumn.GetEnumLabel()} : {col}"));
                }
            });

            if (errors.Count > 0)
            {
                return errors;
            }

            return Unit.Instance;
        }

        private static async Task<Either<List<DataImportError>, Unit>> ValidateMetaRows(
            DataColumnCollection cols, DataRowCollection rows)
        {
            var errors = new List<DataImportError>();
            var idx = 0;
            foreach (DataRow row in rows)
            {
                idx++;

                try
                {
                    ImporterMetaService.GetMetaRow(CsvUtil.GetColumnValues(cols), row);
                }
                catch (Exception e)
                {
                    errors.Add(new DataImportError($"error at row {idx}: {MetaFileHasInvalidValues.GetEnumLabel()} : {e.Message}"));
                }
            }

            if (errors.Count > 0)
            {
                return errors;
            }

            return Unit.Instance;
        }

        private static async Task<Either<List<DataImportError>, Unit>> ValidateObservationHeaders(DataColumnCollection cols)
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
                DataColumnCollection cols,
                DataRowCollection rows,
                ExecutionContext executionContext,
                Guid importId)
        {
            var colValues = CsvUtil.GetColumnValues(cols);
            var totalRowCount = 0;
            var rowCountByGeographicLevel = new Dictionary<GeographicLevel, int>();
            var errors = new List<DataImportError>();
            var dataRows = rows.Count;

            foreach (DataRow row in rows)
            {
                totalRowCount++;
                if (errors.Count == 100)
                {
                    errors.Add(new DataImportError(FirstOneHundredErrors.GetEnumLabel()));
                    break;
                }

                try
                {
                    var rowValues = CsvUtil.GetRowValues(row);
                    _importerService.GetTimeIdentifier(rowValues, colValues);
                    _importerService.GetYear(rowValues, colValues);

                    var level = CsvUtil.GetGeographicLevel(rowValues, colValues);
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
                    errors.Add(new DataImportError($"error at row {totalRowCount}: {e.Message}"));
                }

                if (totalRowCount % Stage1RowCheck == 0)
                {
                    await _dataImportService.UpdateStatus(importId,
                        DataImportStatus.STAGE_1,
                        (double) totalRowCount / dataRows * 100);
                }
            }

            if (errors.Count > 0)
            {
                return errors;
            }

            await _dataImportService.UpdateStatus(importId,
                DataImportStatus.STAGE_1,
                100);

            var rowsPerBatch = Convert.ToInt32(LoadAppSettings(executionContext).GetValue<string>("RowsPerBatch"));

            return new ProcessorStatistics
            (
                totalRows: GetImportableRowCount(rowCountByGeographicLevel),
                rowsPerBatch: rowsPerBatch,
                numBatches: GetNumBatches(totalRowCount, rowsPerBatch),
                geographicLevels: rowCountByGeographicLevel.Keys.ToHashSet()
            );
        }

        private static IConfigurationRoot LoadAppSettings(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables()
                .Build();
        }

        private async Task<bool> IsCsvFile(File file)
        {
            var mimeTypeStream = await _blobStorageService.StreamBlob(PrivateReleaseFiles, file.Path());

            var hasMatchingMimeType = await _fileTypeService.HasMatchingMimeType(
                mimeTypeStream,
                AllowedMimeTypesByFileType[FileType.Data]
            );

            if (!hasMatchingMimeType)
            {
                return false;
            }

            var encodingStream = await _blobStorageService.StreamBlob(PrivateReleaseFiles, file.Path());
            return _fileTypeService.HasMatchingEncodingType(encodingStream, CsvEncodingTypes);
        }

        private static int GetNumBatches(int rows, int rowsPerBatch)
        {
            return (int) Math.Ceiling(rows / (double) rowsPerBatch);
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
