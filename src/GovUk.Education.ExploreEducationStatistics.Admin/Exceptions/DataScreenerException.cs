namespace GovUk.Education.ExploreEducationStatistics.Admin.Exceptions;

public class DataScreenerException : Exception
{
    public DataScreenerException() { }

    public DataScreenerException(string message)
        : base(message) { }

    public DataScreenerException(string message, Exception inner)
        : base(message, inner) { }
}
