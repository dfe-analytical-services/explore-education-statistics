namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Exceptions
{
    public class InvalidMetaRowException : ImporterException
    {
        public InvalidMetaRowException(long subjectId, int rowNumber) : base(subjectId, $"Invalid meta row found at row {rowNumber}")
        {
        }
    }
}