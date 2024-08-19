namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;

public class RequestTimeoutOptions
{
    public const string Section = "RequestTimeouts";

    public int? RequestTimeoutMilliseconds { get; init; }
}
