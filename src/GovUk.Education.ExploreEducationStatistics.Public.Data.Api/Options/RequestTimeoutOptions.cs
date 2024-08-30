namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;

public class RequestTimeoutOptions
{
    public const string Section = "RequestTimeout";

    public int? TimeoutMilliseconds { get; init; }
}
