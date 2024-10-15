#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Options;

public class OpenIdConnectSpaClientOptions
{
    public const string Section = "OpenIdConnectSpaClient";

    public string ClientId { get; set; } = null!;

    public string Authority { get; set; } = null!;

    public string[] KnownAuthorities { get; set; } = null!;

    public string AdminApiScope { get; set; } = null!;

    public AuthorityMetadataOptions? AuthorityMetadata { get; set; }
}

public class AuthorityMetadataOptions
{
    public string AuthorizationEndpoint { get; set; } = null!;

    public string TokenEndpoint { get; set; } = null!;

    public string Issuer { get; set; } = null!;

    public string UserInfoEndpoint { get; set; } = null!;

    public string EndSessionEndpoint { get; set; } = null!;
}
