namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Exceptions
{
    public class InvalidObservationException : ImporterException
    {
        public InvalidObservationException(long subjectId, int rowNumber, string message) : base(subjectId, $"Invalid observation row found at row {rowNumber} : {message}")
        {
        }
    }
}