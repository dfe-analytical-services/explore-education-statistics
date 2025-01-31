namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Options;

public class OpenIdConnectOptions
{
    public const string Section = "OpenIdConnect";

    public Guid ClientId { get; init; }

    public Guid TenantId { get; init; }
}
