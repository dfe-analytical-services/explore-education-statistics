using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class ValidatorService : IValidatorService
    {
        private readonly IBatchService _batchService;

        public ValidatorService(IBatchService batchService)
        {
            _batchService = batchService;
        }

        public bool IsDataValid(ImportMessage message, SubjectData subjectData)
        {
            List<string> errors = new List<string>();
            
            ValidateMetaHeader(subjectData.GetMetaLines().First(), message, errors);
            ValidateMetaRows(subjectData.GetMetaLines(), message, errors);
            ValidateObservations(subjectData.GetCsvLines(), message, errors);

            if (errors.Count > 0)
            {
                _batchService.FailBatch(
                    message.Release.Id.ToString(), 
                    errors, 
                    message.DataFileName).Wait();
                return false;
            }

            return true;
        }

        private void ValidateMetaHeader(string header, ImportMessage message, List<string> errors)
        {
            if (RowContainsQuotes(header))
            {
                errors.Add("Meta header contains quotes");
            }
            
            // Check for unexpected column names
            Array.ForEach<string>( Enum.GetNames(typeof(MetaColumns)), (string col) => {
                if (!header.Contains(col))
                { 
                    errors.Add("Meta file has unexpected columns");
                }
            } );
        }
        
        private void ValidateMetaRows(IEnumerable<string> lines, ImportMessage message, List<string> errors)
        {
            var idx = 2;
            var headers = lines.First().Split(',').ToList();
            
            foreach (var line in lines.Skip(1))
            {
                ValidateMetaRow(line, idx++, headers.Count, message, errors);  
            }
        }
        
        private void ValidateObservations(IEnumerable<string> lines, ImportMessage message, List<string> errors)
        {
            var idx = 2;
            var headers = lines.First().Split(',').ToList();

            foreach (var line in lines.Skip(1))
            {
                ValidateObservationRow(line, idx++, headers.Count, message, errors);
            }
        }

        private void ValidateMetaRow(string row, int rowNumber, int numExpectedColumns, ImportMessage message, List<string> errors)
        {
            if (RowContainsQuotes(row))
            {
                errors.Add($"Meta file contains quotes at row {rowNumber}");
            }

            if (HasUnexpectedNumberOfColumns(row, numExpectedColumns))
            {
                errors.Add($"Meta file has invalid number of columns at row {rowNumber}");
            }
        }
        
        private void ValidateObservationRow(string row, int rowNumber, int numExpectedColumns, ImportMessage message, List<string> errors)
        {
            if (RowContainsQuotes(row))
            {
                errors.Add($"Datafile file contains quotes at row {rowNumber}");
            }

            if (HasUnexpectedNumberOfColumns(row, numExpectedColumns))
            {
                errors.Add( $"Data file has invalid number of columns at row {rowNumber}");
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