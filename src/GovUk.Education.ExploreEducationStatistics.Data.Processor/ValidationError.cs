namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public class ValidationError
    {
        public string Message { get; set; }

        public ValidationError(string message)
        {
            Message = message;
        }
    }
}