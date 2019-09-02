using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public enum ValidationErrorMessages
    {
        [EnumLabelValue("Meta header contains quotes")]
        MetaHeaderContainsQuotes,
        
        [EnumLabelValue("Metafile has unexpected columns")]
        MetaFileHasUnexpectedColumns,
        
        [EnumLabelValue("Metafile contains quotes")]
        MetaFileContainsQuotes,
        
        [EnumLabelValue("Metafile has invalid number of columns")]
        MetaFileHasInvalidNumberOfColumns,
        
        [EnumLabelValue("Metafile has invalid values")]
        MetaFileHasInvalidValues,
        
        [EnumLabelValue("Datafile contains quotes")]
        DataFileContainsQuotes,
        
        [EnumLabelValue("Datafile has invalid number of columns")]
        DataFileHasInvalidNumberOfColumns,
        
        [EnumLabelValue("Datafile has invalid geographic level")]
        DataFileHasInvalidGeographicLevel,
        
        [EnumLabelValue("Datafile has invalid time identifier")]
        DataFileHasInvalidTimeIdentifier
    }
    
    public class ValidatorService : IValidatorService
    {
        public ValidatorService()
        {
        }

        public List<string> Validate(ImportMessage message, SubjectData subjectData)
        {
            var errors = new List<string>();

            var metaHeaders = subjectData.GetMetaLines().First();
            ValidateMetaHeader(metaHeaders, message, errors);
            ValidateMetaRows(subjectData.GetMetaLines(), message, errors);
            ValidateObservations(subjectData.GetCsvLines(), message, errors);

            return errors;
        }

        private static void ValidateMetaHeader(string header, ImportMessage message, List<string> errors)
        {
            if (RowContainsQuotes(header))
            {
                errors.Add(ValidationErrorMessages.MetaHeaderContainsQuotes.GetEnumLabel());
            }
            
            // Check for unexpected column names
            Array.ForEach<string>( Enum.GetNames(typeof(MetaColumns)), (string col) => {
                if (!header.Contains(col))
                { 
                    errors.Add(ValidationErrorMessages.MetaFileHasUnexpectedColumns.GetEnumLabel());
                }
            } );
        }
        
        private void ValidateMetaRows(IEnumerable<string> lines, ImportMessage message, List<string> errors)
        {
            var idx = 2;
            var headers = lines.First().Split(',').ToList();
            
            foreach (var line in lines.Skip(1))
            {
                ValidateMetaRow(line, idx++, headers, message, errors);  
            }
        }
        
        private void ValidateObservations(IEnumerable<string> lines, ImportMessage message, List<string> errors)
        {
            var idx = 2;
            var headers = lines.First().Split(',').ToList();

            foreach (var line in lines.Skip(1))
            {
                ValidateObservationRow(line, idx++, headers, message, errors);
            }
        }

        private void ValidateMetaRow(string row, int rowNumber, List<string> headers, ImportMessage message, List<string> errors)
        {
            int numExpectedColumns = headers.Count;
            
            if (RowContainsQuotes(row))
            {
                errors.Add($"error at row {rowNumber}: " + ValidationErrorMessages.MetaFileContainsQuotes.GetEnumLabel());
            }

            if (HasUnexpectedNumberOfColumns(row, numExpectedColumns))
            {
                errors.Add($"error at row {rowNumber}: " + ValidationErrorMessages.MetaFileHasInvalidNumberOfColumns.GetEnumLabel());
            }

            try
            {
                ImporterMetaService.GetMetaRow(row, headers);
            }
            catch (Exception e)
            {
                errors.Add($"error at row {rowNumber}: " + ValidationErrorMessages.MetaFileHasInvalidValues.GetEnumLabel() + " : " + e.Message);
            }
        }
        
        private void ValidateObservationRow(string row, int rowNumber, List<string> headers, ImportMessage message, List<string> errors)
        {
            if (RowContainsQuotes(row))
            {
                errors.Add($"error at row {rowNumber}: " + ValidationErrorMessages.DataFileContainsQuotes.GetEnumLabel());
            }

            if (HasUnexpectedNumberOfColumns(row, headers.Count))
            {
                errors.Add( $"error at row {rowNumber}: " + ValidationErrorMessages.DataFileHasInvalidNumberOfColumns.GetEnumLabel());
            }

            try
            {
                var line = row.Split(',');
                ImporterService.GetGeographicLevel(line, headers);
                ImporterService.GetTimeIdentifier(line, headers);
            }
            catch (InvalidGeographicLevelException e)
            {
                errors.Add( $"error at row {rowNumber}: " + ValidationErrorMessages.DataFileHasInvalidGeographicLevel.GetEnumLabel());
            }
            catch (InvalidTimeIdentifierException e)
            {
                errors.Add( $"error at row {rowNumber}: " + ValidationErrorMessages.DataFileHasInvalidTimeIdentifier.GetEnumLabel());
            }
        }

        private static bool RowContainsQuotes(string row)
        {
            return row.Contains("\"");
        }

        private static bool HasUnexpectedNumberOfColumns(string row, int numExpectedColumns)
        {
            return row.Split(',').Length != numExpectedColumns;
        }
    }
}