using System;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public class ValidatorService : IValidatorService
    {
        public void ValidateMetaHeader(long subjectId, string header)
        {
            if (header.Contains("\""))
            {
                throw new InvalidMetaHeaderException(subjectId);
            }
            Array.ForEach<string>( Enum.GetNames(typeof(MetaColumns)), (string col) => {
                if (!header.Contains(col))  throw new InvalidMetaHeaderException(subjectId);
            } );
        }
        
        public void ValidateMetaRow(long subjectId, string row, int rowNumber, int numExpectedColumns)
        {
            if (row.Contains("\""))
            {
                throw new InvalidMetaRowException(subjectId, rowNumber);
            }

            if (row.Split(',').Length != numExpectedColumns)
            {
                throw new InvalidMetaRowException(subjectId, rowNumber);  
            }
        }
    }
}