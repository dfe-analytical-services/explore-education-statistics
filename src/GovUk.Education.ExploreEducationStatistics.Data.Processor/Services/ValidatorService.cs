using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public enum ValidationErrorMessages
    {
        [EnumLabelValue("Metafile is missing expected column")]
        MetaFileMissingExpectedColumn,

        [EnumLabelValue("Metafile has invalid number of columns")]
        MetaFileHasInvalidNumberOfColumns,

        [EnumLabelValue("Metafile has invalid values")]
        MetaFileHasInvalidValues,

        [EnumLabelValue("Datafile has invalid number of columns")]
        DataFileHasInvalidNumberOfColumns,

        [EnumLabelValue("Datafile is missing expected column")]
        DataFileMissingExpectedColumn
    }

    public class ValidatorService : IValidatorService
    {
        private static readonly List<string> MandatoryObservationColumns = new List<string>
        {
            "time_identifier",
            "time_period",
            "geographic_level"
        };

        public Tuple<List<string>, int, int> ValidateAndCountRows(SubjectData subjectData)
        {
            var errors = new List<string>();
            var metaTable = subjectData.GetMetaTable();
           
            ValidateMetaHeader(metaTable.Columns, errors);

            // If the meta header not ok then stop error checks
            if (errors.Count != 0)
            {
                return new Tuple<List<string>, int, int>(errors, 0, 0);
            }

            ValidateMetaRows(metaTable.Columns, metaTable.Rows, errors);

            var csvTable = subjectData.GetCsvTable();
            
            ValidateObservationHeaders(csvTable.Columns, errors);

            if (errors.Count != 0)
            {
                return new Tuple<List<string>, int, int>(errors, 0, 0);
            }

            var (totalRows, filteredRows) = ValidateAndCountObservations(
                csvTable.Columns, csvTable.Rows, errors);

            return new Tuple<List<string>, int, int>(errors, totalRows, filteredRows);
            ;
        }

        private static void ValidateMetaHeader(DataColumnCollection header, List<string> errors)
        {
            // Check for unexpected column names
            {
                Array.ForEach(Enum.GetNames(typeof(MetaColumns)), col =>
                {
                    if (!header.Contains(col))
                    {
                        errors.Add(ValidationErrorMessages.MetaFileMissingExpectedColumn.GetEnumLabel() + " : " + col);
                    }
                });
            }
        }

        private static void ValidateMetaRows(DataColumnCollection cols, DataRowCollection rows, List<string> errors)
        {
            var idx = 0;
            foreach (DataRow row in rows)
            {
                ValidateMetaRow(row, idx++, cols, errors);
            }
        }

        private static Tuple<int, int> ValidateAndCountObservations(DataColumnCollection cols, DataRowCollection rows,
            List<string> errors)
        {
            var idx = 0;
            var filteredRows = 0;
            var totalRows = 0;

            foreach (DataRow row in rows)
            {
                if (errors.Count == 100)
                {
                    errors.Add("Only first 100 errors are returned");
                    break;
                }

                var rowValues = CsvUtil.GetRowValues(row);
                var columnValues = CsvUtil.GetColumnValues(cols);

                if (ValidateObservationRow(row, idx++, cols, errors) &&
                    !IsGeographicLevelIgnored(rowValues, columnValues))
                {
                    filteredRows++;
                }

                totalRows++;
            }

            return new Tuple<int, int>(totalRows, filteredRows);
        }

        private static void ValidateObservationHeaders(DataColumnCollection cols, List<string> errors)
        {
            foreach (var mandatoryCol in MandatoryObservationColumns)
            {
                if (!cols.Contains(mandatoryCol))
                {
                    errors.Add(ValidationErrorMessages.DataFileMissingExpectedColumn.GetEnumLabel() + " : " +
                               mandatoryCol);
                }

                if (errors.Count == 100)
                {
                    errors.Add("Only first 100 errors are returned");
                    break;
                }
            }
        }

        private static void ValidateMetaRow(DataRow row, int rowNumber, DataColumnCollection cols, List<string> errors)
        {
            {
                if (row.ItemArray.Count() != cols.Count)
                {
                    errors.Add($"error at row {rowNumber + 1}: " +
                               ValidationErrorMessages.MetaFileHasInvalidNumberOfColumns.GetEnumLabel());
                }

                try
                {
                    ImporterMetaService.GetMetaRow(CsvUtil.GetColumnValues(cols), row);
                }
                catch (Exception e)
                {
                    errors.Add($"error at row {rowNumber}: " +
                               ValidationErrorMessages.MetaFileHasInvalidValues.GetEnumLabel() + " : " + e.Message);
                }
            }
        }

        private static bool ValidateObservationRow(DataRow row, int rowNumber, DataColumnCollection cols,
            List<string> errors)
        {
            var valid = false;

            {
                if (row.ItemArray.Count() != cols.Count)
                {
                    errors.Add($"error at row {rowNumber}: " +
                               ValidationErrorMessages.DataFileHasInvalidNumberOfColumns.GetEnumLabel());
                }

                try
                {
                    var rowValues = CsvUtil.GetRowValues(row);
                    var colValues = CsvUtil.GetColumnValues(cols);

                    ImporterService.GetGeographicLevel(rowValues, colValues);
                    ImporterService.GetTimeIdentifier(rowValues, colValues);
                    ImporterService.GetYear(rowValues, colValues);
                    valid = true;
                }
                catch (Exception e)
                {
                    errors.Add($"error at row {rowNumber}: {e.Message}");
                }
            }

            return valid;
        }

        private static bool IsGeographicLevelIgnored(IReadOnlyList<string> line, List<string> headers)
        {
            var geographicLevel = ImporterService.GetGeographicLevel(line, headers);
            return ImporterService.IgnoredGeographicLevels.Contains(geographicLevel);
        }
    }
}