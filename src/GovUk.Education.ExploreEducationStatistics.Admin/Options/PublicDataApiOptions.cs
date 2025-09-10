#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

public class PublicDataApiOptions : IAzureAuthenticationOptions
{
    public const string Section = "PublicDataApi";

    public string PublicUrl { get; init; } = string.Empty;

    public string PrivateUrl { get; init; } = string.Empty;

    public string DocsUrl { get; init; } = string.Empty;

    public Guid AppRegistrationClientId { get; init; }
}
