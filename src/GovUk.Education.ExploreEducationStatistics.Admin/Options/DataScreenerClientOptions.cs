using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

public class DataScreenerClientOptions : IAzureAuthenticationOptions
{
    public const string Section = "DataScreener";

    public string Url { get; init; }

    public Guid AppRegistrationClientId { get; init; }
}
