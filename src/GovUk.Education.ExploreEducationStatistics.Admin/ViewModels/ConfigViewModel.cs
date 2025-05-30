#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record ConfigViewModel
{
    public required string AppInsightsKey { get; init; }

    public required string PublicAppUrl { get; init; }

    public required string PublicApiUrl { get; init; }

    public required string PublicApiDocsUrl { get; init; }

    public required string[] PermittedEmbedUrlDomains { get; init; }

    public required OpenIdConnectSpaClientOptions Oidc { get; init; }

    public bool EnableReplacementOfPublicApiDataSets { get; init; }
}
