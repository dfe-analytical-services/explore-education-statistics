#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

internal class PublicDataProcessorOptions : IAzureAuthenticationOptions
{
    public const string Section = "PublicDataProcessor";

    public string Url { get; init; } = string.Empty;

    public Guid AppRegistrationClientId { get; init; }
}
