#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

public class CdnOptions
{
    public const string Section = "Cdn";

    public string NextJsBuildId { get; init; } = string.Empty;
    public string EndpointResourceId { get; init; } = string.Empty;
    public string EesDomain { get; init; } = string.Empty;
    public string DefaultAfdDomain { get; init; } = string.Empty;
}
