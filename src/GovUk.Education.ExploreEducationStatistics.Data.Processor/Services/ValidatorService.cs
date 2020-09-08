using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Storage.Blob;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.FileTypeValidationUtils;

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
        private readonly IFileTypeService _fileTypeService;

        public ValidatorService(ILogger<IValidatorService> logger, IFileTypeService fileTypeService)
        {
            _logger = logger;
            _fileTypeService = fileTypeService;
        }

        public ValidatorService(ILogger<IValidatorService> logger)
        {
            _logger = logger;
        }

        private static readonly List<string> MandatoryObservationColumns = new List<string>
        {
            "time_identifier",
            "time_period",
            "geographic_level"
        };

        public async Task<Either<IEnumerable<ValidationError>, ProcessorStatistics>> 
            Validate(SubjectData subjectData, ExecutionContext executionContext, ImportMessage message)
        {
            _logger.LogInformation($"Validating Datafile: {message.OrigDataFileName}");

            return await ValidateCsvFile(subjectData.DataBlob, false)
                .OnSuccess(async () => await ValidateCsvFile(subjectData.MetaBlob, true)
                .OnSuccess(() =>
                {
                    var csvTable = subjectData.GetCsvTable();
                    var metaTable = subjectData.GetMetaTable();
                    
                    return ValidateMetaHeader(metaTable.Columns)
                        .OnSuccess(() => ValidateMetaRows(metaTable.Columns, metaTable.Rows))
                        .OnSuccess(() => ValidateObservationHeaders(csvTable.Columns))
                        .OnSuccess(() =>
                            ValidateAndCountObservations(csvTable.Columns, csvTable.Rows, executionContext)
                                .OnSuccess(result =>
                                {
                                    _logger.LogInformation(
                                        $"Validation of Datafile: {message.OrigDataFileName} complete");
                                    return result;
                                }));
                }));
        }
        
        private async Task<Either<IEnumerable<ValidationError>, Unit>> ValidateCsvFile(CloudBlob blob, bool isMetaFile)
        {
            var errors = new List<ValidationError>();
            
            if (!await IsCsvFile(blob))
            {
                errors.Add(isMetaFile ? new ValidationError($"{MetaFileMustBeCsvFile.GetEnumLabel()}") :
                    new ValidationError($"{DataFileMustBeCsvFile.GetEnumLabel()}"));
            }
            else
            {
                using var reader = new StreamReader(await blob.OpenReadAsync());
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
                        errors.Add(isMetaFile ? new ValidationError($"error at row {idx}: {MetaFileHasInvalidNumberOfColumns.GetEnumLabel()}") :
                            new ValidationError($"error at row {idx}: {DataFileHasInvalidNumberOfColumns.GetEnumLabel()}"));
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

        private static async Task<Either<IEnumerable<ValidationError>, Unit>> ValidateMetaHeader(DataColumnCollection header)
        {
            var errors = new List<ValidationError>();
            // Check for unexpected column names
            Array.ForEach(Enum.GetNames(typeof(MetaColumns)), col =>
            {
                if (!header.Contains(col))
                {
                    errors.Add(new ValidationError($"{MetaFileMissingExpectedColumn.GetEnumLabel()} : {col}"));
                }
            });

            if (errors.Count > 0)
            {
                return errors;
            }

            return Unit.Instance;
        }

        private static async Task<Either<IEnumerable<ValidationError>, Unit>> ValidateMetaRows(
            DataColumnCollection cols, DataRowCollection rows)
        {
            var errors = new List<ValidationError>();
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
                    errors.Add(new ValidationError($"error at row {idx}: {MetaFileHasInvalidValues.GetEnumLabel()} : {e.Message}"));
                }
            }

            if (errors.Count > 0)
            {
                return errors;
            }

            return Unit.Instance;
        }
        
        private static async Task<Either<IEnumerable<ValidationError>, Unit>> ValidateObservationHeaders(DataColumnCollection cols)
        {
            var errors = new List<ValidationError>();
            
            foreach (var mandatoryCol in MandatoryObservationColumns)
            {
                if (!cols.Contains(mandatoryCol))
                {
                    errors.Add(new ValidationError($"{DataFileMissingExpectedColumn.GetEnumLabel()} : {mandatoryCol}"));
                }

                if (errors.Count == 100)
                {
                    errors.Add(new ValidationError(FirstOneHundredErrors.GetEnumLabel()));
                    break;
                }
            }

            if (errors.Count > 0)
            {
                return errors;
            }

            return Unit.Instance;
        }

        private static async Task<Either<IEnumerable<ValidationError>, ProcessorStatistics>>
            ValidateAndCountObservations(DataColumnCollection cols, DataRowCollection rows, ExecutionContext executionContext)
        {
            var idx = 0;
            var filteredRows = 0;
            var totalRows = 0;
            var errors = new List<ValidationError>();

            foreach (DataRow row in rows)
            {
                idx++;
                if (errors.Count == 100)
                {
                    errors.Add(new ValidationError(FirstOneHundredErrors.GetEnumLabel()));
                    break;
                }

                try
                {
                    var rowValues = CsvUtil.GetRowValues(row);
                    var colValues = CsvUtil.GetColumnValues(cols);

                    ImporterService.GetGeographicLevel(rowValues, colValues);
                    ImporterService.GetTimeIdentifier(rowValues, colValues);
                    ImporterService.GetYear(rowValues, colValues);
                    
                    if (!IsGeographicLevelIgnored(rowValues, colValues))
                    {
                        filteredRows++;
                    }
                }
                catch (Exception e)
                {
                    errors.Add(new ValidationError($"error at row {idx}: {e.Message}"));
                }
                
                totalRows++;
            }

            if (errors.Count > 0)
            {
                return errors;
            }

            var rowsPerBatch = Convert.ToInt32(LoadAppSettings(executionContext).GetValue<string>("RowsPerBatch"));

            return new ProcessorStatistics
            {
                FilteredObservationCount = filteredRows,
                RowsPerBatch = rowsPerBatch,
                NumBatches = FileStorageUtils.GetNumBatches(totalRows, rowsPerBatch)
            };
        }

        private static bool IsGeographicLevelIgnored(IReadOnlyList<string> line, List<string> headers)
        {
            var geographicLevel = ImporterService.GetGeographicLevel(line, headers);
            return ImporterService.IgnoredGeographicLevels.Contains(geographicLevel);
        }
        
        private static IConfigurationRoot LoadAppSettings(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables()
                .Build();
        }
        
        private async Task<bool> IsCsvFile(CloudBlob blob)
        {
            return await _fileTypeService.HasMatchingMimeType(blob, AllowedMimeTypesByFileType[ReleaseFileTypes.Data]) 
                   && await _fileTypeService.HasMatchingEncodingType(blob, CsvEncodingTypes);
        }
    }
}