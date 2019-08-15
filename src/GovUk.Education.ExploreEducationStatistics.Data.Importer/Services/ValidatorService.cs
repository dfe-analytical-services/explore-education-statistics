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
        }
        
        public void ValidateMetaRow(long subjectId, string row, int rowNumber)
        {
            if (row.Contains("\""))
            {
                throw new InvalidMetaRowException(subjectId, rowNumber);
            }
        }
    }
}